#region Copyright

// A²DS - Autonomous Aerial Drone Simulator
// http://anjdreas.spaces.live.com/
//  
// A Master of Science thesis on autonomous flight at the 
// Norwegian University of Science and Technology (NTNU).
//  
// Copyright © 2009-2010 by Andreas Larsen.  All rights reserved.

#endregion

#region Using

using System;
using System.Collections.Generic;
using Anj.Helpers.XNA;

#if !XBOX
using Anj.XNA.Joysticks;
using Anj.XNA.Joysticks.Wizard;
#endif

using Control;
using Control.Common;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Physics;
using Physics.Common;
using Sensors;
using Sensors.Model;
using Simulator.Cameras;
using Simulator.Interfaces;
using Simulator.Resources;
using Simulator.Scenarios;
using Simulator.Skydome;
using State;
using State.Common;
using State.Model;
using Keys = Microsoft.Xna.Framework.Input.Keys;
using Simulator.Testing;

#endregion

// Disable warnings about "code not reachable".
// I have a few of those because of hardcoded constant settings.
#pragma warning disable 0162

namespace Simulator
{
    /// <summary>
    ///   Sources:
    ///   http://www.absoluteastronomy.com/topics/Helicopter_rotor
    /// </summary>
    public class HelicopterBase : DrawableGameComponent, ICameraTarget
    {
        /// <summary>
        /// Set to true if the autopilot should base its navigation on sensors, 
        /// and false if it should use perfect information.
        /// </summary>
        private readonly bool _flyBySensors = true;
        private const int MotionBlurRenderCount = 20;
        private const bool UseTerrainCollision = false;
        private const bool DrawShadows = false;
        private bool DrawGhost { get { return _flyBySensors && !_useJoystick && !IsPlayerControlled; } }

            public event Action<GameTime> Crashed;

        private readonly BasicEffect _basicEffect;
            private readonly bool _drawText;
        private readonly Game _game;
        private readonly TestConfiguration _testConfiguration;
        private readonly SensorSpecifications _sensorSpecifications;
        private readonly TerrainCollision _collision;
        private readonly bool _playEngineSound;
        private readonly SunlightParameters _skyParams;
        private HelicopterScenario _scenario;
        private ControlGoal _controlGoal;
        private SoundEffect _engineSound;
        private SoundEffectInstance _engineSoundInst;
        private HeliState _estimatedState;
        private PhysicalHeliState _estimatedStateError;
        private IStateProvider _estimatedStateProvider;
        private Model _helicopter;
        private SpriteFont _hudInfo1Font;
        private int _infoCount;
//        private Joystick _joystick;
//        private JoystickEnum _joystickType;
        private PerfectState _perfectStateProvider;
        private PhysicalHeliState _physicalState;
        private IHeliPhysics _physics;
        private SensorModel _sensors;
        private Matrix _shadowTransform;
        private SimulationStepResults _simulationState;
        private SpriteBatch _spriteBatch;
        private SunlightEffect _sunlightEffect;
        private HeliState _trueState;

#if !XBOX
        private JoystickSystem _joystickSystem;
#endif


        /// <summary>
        ///   Set this to true, to override autopilot and use a joystick instead.
        /// </summary>
        private bool _useJoystick;

        private readonly ICameraProvider _cameraProvider;

        #region Constructors

        public HelicopterBase(Game game, TestConfiguration testConfiguration, TerrainCollision collision, 
            ICameraProvider cameraProvider, BasicEffect effect, SunlightParameters skyParams, HelicopterScenario scenario, 
            bool playEngineSound, bool isPlayerControlled, bool drawText)
            : base(game)
        {
            if (game == null || cameraProvider == null || effect == null || skyParams == null)
                throw new ArgumentNullException("", @"One or several of the necessary arguments were null!");

            

            _game = game;
            _testConfiguration = testConfiguration;
            _sensorSpecifications = testConfiguration.Sensors;
            _collision = collision;
            _flyBySensors = testConfiguration.FlyBySensors;

            if (_collision != null)
                _collision.CollidedWithTerrain += gameTime => Crashed(gameTime);
                

            _basicEffect = effect;
            _skyParams = skyParams;
            _scenario = scenario;
            _drawText = drawText;
            _cameraProvider = cameraProvider;
            IsPlayerControlled = isPlayerControlled;

            _playEngineSound = playEngineSound;

            _estimatedState = new HeliState();

            PIDSetup pidSetup = SimulatorResources.GetPIDSetups()[0];
            Autopilot = new Autopilot(_scenario.Task, pidSetup);
            Log = new List<HelicopterLogSnapshot>();
        }

        #endregion

        #region Properties

        private ICamera Camera { get { return _cameraProvider.Camera; } }

        /// <summary>
        ///   Returns either the estimated state or the true state depending on FlyBySensors.
        ///   This state should be fed to the autopilot to calculate control output.
        /// </summary>
        private HeliState AutopilotAwareState
        {
            get { return _flyBySensors ? _estimatedState : _trueState; }
        }


        protected bool IsPlayerControlled { get; set; }

        public IList<HelicopterLogSnapshot> Log { get; private set; }

        /// <summary>
        ///   Returns a reference to the autopilot component.
        /// </summary>
        public Autopilot Autopilot { get; private set; }

        public Task Task
        {
            get { return Autopilot.Task; }
            set { Autopilot.Task = value; }
        }

        public Axes Axes
        {
            get { return _physicalState.Axes; }
        }

        public SensorModel Sensors
        {
            get { return _sensors; }
        }

        public Matrix Rotation
        {
            get { return Matrix.CreateFromQuaternion(_physicalState.Orientation); }
        }

        public Vector3 Position
        {
            get { return _physicalState.Position; }
        }

        public Vector3 CameraUp
        {
            get { return Axes.Up; }
        }

        public Vector3 CameraForward
        {
            get { return Axes.Forward; }
        }

        #endregion

        #region Overrides

        public override void Initialize()
        {
#if !XBOX
            string[] joystickNames = Joystick.FindJoysticks();
            if (joystickNames == null || joystickNames.Length == 0)
            {
                //                MessageBox.Show("No joysticks found.");
            }
            else
            {
                IList<JoystickSetup> joystickSetups = SimulatorResources.GetJoystickSetups();
                if (joystickSetups == null || joystickSetups.Count == 0)
                    throw new Exception("No configured joysticks.");

                _joystickSystem = new JoystickSystem(_game.Window.Handle, joystickSetups);
                _useJoystick = true;
            }
#endif


            // Initialize stencil shadow transformation matrix
            Vector3 shadowLightDir = Vector3.Down;
            var shadowPlane = new Plane(Vector3.Zero, Vector3.Forward, Vector3.Right);
            _shadowTransform = Matrix.CreateShadow(shadowLightDir, shadowPlane)*Matrix.CreateTranslation(Vector3.Up/100);

            base.Initialize();
        }


        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _hudInfo1Font = _game.Content.Load<SpriteFont>("HUDInfo1");
            _helicopter = _game.Content.Load<Model>("Models/gunship");

            _engineSound = _game.Content.Load<SoundEffect>("Audio/full_throttle");
            _engineSoundInst = _engineSound.CreateInstance();
            _engineSoundInst.Volume = 0.7f;
            _engineSoundInst.IsLooped = true;

            _sunlightEffect = SunlightEffect.Create(GraphicsDevice, _game.Content);

            // TODO This is a hack for this helicopter mesh, which has only one texture.
            // Normally we would need to create a new sunlight effect for each mesh part.
            foreach (ModelMesh mesh in _helicopter.Meshes)
                foreach (ModelMeshPart part in mesh.MeshParts)
                {
                    if (_sunlightEffect != null)
                    {
                        // Reuse existing model texture in sunlight effect
                        if (part.Effect as BasicEffect != null)
                            _sunlightEffect.Texture = ((BasicEffect) part.Effect).Texture;

                        // Assign sunlight shader
                        part.Effect = _sunlightEffect;
                    }
                }


            base.LoadContent();

//            Reset(TimeSpan.Zero, _scenario, Autopilot.Map);
        }

        protected override void UnloadContent()
        {
#if !XBOX
            if (_joystickSystem != null) 
                _joystickSystem.Dispose();
#endif

            base.UnloadContent();
        }


        public override void Update(GameTime gameTime)
        {
            if (_playEngineSound && _engineSoundInst != null && _engineSoundInst.State != SoundState.Playing)
                _engineSoundInst.Play();


            long totalTicks = gameTime.TotalGameTime.Ticks;
            JoystickOutput output = GetJoystickInput(totalTicks);

            // Invert yaw because we want yaw rotation positive as clockwise seen from above
            output.Yaw *= -1;

            // Update physics and sensors + state estimators
            PhysicalHeliState trueState, observedState, estimatedState, blindEstimatedState;
            UpdatePhysicalState(gameTime, output, out trueState);
            UpdateStateEstimators(_simulationState, gameTime, output, out estimatedState, out blindEstimatedState, out observedState);

            float trueGroundAltitude = GetGroundAltitude(trueState.Position);
            float estimatedGroundAltitude = GetGroundAltitude(trueState.Position);
            float trueHeightAboveGround = GetHeightAboveGround(trueState.Position);
            float gpsinsHeightAboveGround = GetHeightAboveGround(estimatedState.Position);
            float rangefinderHeightAboveGround = _sensors.GroundRangeFinder.FlatGroundHeight;

            

            float estimatedHeightAboveGround;
            if (!_testConfiguration.UseGPS)
                throw new NotImplementedException();
            else if (!_testConfiguration.UseINS)
                throw new NotImplementedException();

            if (_testConfiguration.UseGPS && _testConfiguration.UseINS)
            {
                if (!_testConfiguration.UseRangeFinder)
                    estimatedHeightAboveGround = gpsinsHeightAboveGround;
                else
                {
                    // The range finder may be out of range (NaN) and typically requires <10 meters.
                    // In this case we need to fully trust INS/GPS estimate.
                    // Note GPS is easily out-bested by range finder, so no need to weight
                    estimatedHeightAboveGround = float.IsNaN(rangefinderHeightAboveGround)
                                                     ? gpsinsHeightAboveGround
                                                     : rangefinderHeightAboveGround;
                    //                                                   : 0.2f*gpsinsHeightAboveGround + 0.8f*rangefinderHeightAboveGround;
                }
            }
            else
                throw new NotImplementedException();


            // Override Kalman Filter estimate of altitude by the more accurate range finder.
            // However, there is a problem that the estimated map altitude depends on the estimated horizontal position; which is inaccurate.
            estimatedState.Position.Y = estimatedHeightAboveGround + estimatedGroundAltitude;

            _physicalState = trueState;
            _trueState = StateHelper.ToHeliState(trueState, trueHeightAboveGround, Autopilot.CurrentWaypoint, output);
            _estimatedState = StateHelper.ToHeliState(estimatedState, estimatedHeightAboveGround, Autopilot.CurrentWaypoint, output);

            

            // _observedState = observedState;

            // Calculate current error in estimated state
            _estimatedStateError = StateHelper.GetError(_physicalState, StateHelper.ToPhysical(_estimatedState));

            // Add current simulation step to log
            ForwardRightUp accelerometerData = _sensors.IMU.AccelerationLocal;
            Log.Add(new HelicopterLogSnapshot(trueState, observedState, estimatedState, blindEstimatedState, accelerometerData, trueGroundAltitude, gameTime.TotalGameTime));


            // Update visual appearances
            UpdateEffects();

            UpdateSound(output);

            // If we have disabled Jitter we must detect collisions ourselves for test scenarios
            if (!UseTerrainCollision)
            {
                if (IsCollidedWithTerrain())
                    if (Crashed != null) Crashed(gameTime);
            }

            // Handle crashes, user input etc.. that cause the helicopter to reset
            HandleResetting(gameTime);

            base.Update(gameTime);
        }

        private bool IsCollidedWithTerrain()
        {
            return GetHeightAboveGround(Position) < 0.1f;
        }


        public override void Draw(GameTime gameTime)
        {
            Vector3 topCamera = Position;
            topCamera.Y += 4;
            Vector3 sideCamera = Position;
            sideCamera.X += 4;
            Vector3 frontCamera = Position;
            frontCamera.Z += 4;

            DrawWorld(gameTime, DrawShadows);

            _spriteBatch.Begin();
            DrawText();
            _spriteBatch.End();

            base.Draw(gameTime);
        }

        #endregion

        #region Private

        private float GetHeightAboveGround(Vector3 position)
        {
            float heightAboveGround = position.Y - GetGroundAltitude(position);
            return heightAboveGround;
        }

        private float GetGroundAltitude(Vector3 position)
        {
            Vector2 mapPosition = VectorHelper.ToHorizontal(position);
            float groundAltitude = Autopilot.Map.GetAltitude(mapPosition);
            return groundAltitude;
        }

        private void UpdatePhysicalState(GameTime gameTime, JoystickOutput output, out PhysicalHeliState trueState)
        {
            _simulationState = _physics.PerformTimestep(_physicalState, output, gameTime.ElapsedGameTime,
                                                        gameTime.TotalGameTime);

            TimestepResult final = _simulationState.Result;

            // We need to use the second last simulation substep to obtain the current acceleration used
            // because the entire substep has constant acceleration and it makes no sense
            // to use the acceleration calculated for the state after the timestep because no animation will occur after it.
            // TODO Support substeps
            Vector3 currentAcceleration = _simulationState.StartingCondition.Acceleration;

            trueState = new PhysicalHeliState(final.Orientation, final.Position, final.Velocity, currentAcceleration);
        }

        private void UpdateStateEstimators(SimulationStepResults simulationState, GameTime gameTime, JoystickOutput output,
                                           out PhysicalHeliState estimatedState,
                                           out PhysicalHeliState blindEstimatedState,
                                           out PhysicalHeliState observedState)
        {
            var dtMillis = (long)gameTime.ElapsedGameTime.TotalMilliseconds;
            var totalMillis = (long)gameTime.TotalGameTime.TotalMilliseconds;

            //            TimeSpan totalSimulationTime = gameTime.TotalGameTime;

            // Update sensors prior to using the state provider, 
            // because they are typically dependent on the state of the sensors.
            Sensors.Update(_simulationState, output);

            // Update states
            ((SensorEstimatedState)_estimatedStateProvider).CheatingTrueState = simulationState; // TODO Temp cheating
            _estimatedStateProvider.Update(simulationState, dtMillis, totalMillis, output);

            // TODO Separate physical and navigational states
            _estimatedStateProvider.GetState(out estimatedState, out observedState, out blindEstimatedState);
        }

        private JoystickOutput GetJoystickInput(long totalTicks)
        {
            // Get user joystick input or autopilot input based on previous state
#if XBOX
            if (IsPlayerControlled)
                return GetUserInput();
#else
            if (_useJoystick && IsPlayerControlled)
                return GetUserInput();
#endif


            // Hover if button is pressed
            if (Keyboard.GetState().IsKeyDown(Keys.H))
                return Autopilot.GetHoverOutput(AutopilotAwareState, totalTicks, out _controlGoal);

            // Use assisted autopilot if enabled in settings to control joystick output
            if (_scenario.AssistedAutopilot)
                return Autopilot.GetAssistedOutput(GetUserInput(), AutopilotAwareState, totalTicks, out _controlGoal);

            // Use autopilot to control joystick output
            return Autopilot.GetOutput(AutopilotAwareState, totalTicks, out _controlGoal);
        }

        private void UpdateEffects()
        {
            if (_sunlightEffect != null)
                _sunlightEffect.Update(Camera, _skyParams);
        }

        private void UpdateSound(JoystickOutput output)
        {
            if (_playEngineSound)
            {
                // Volume from 50-100% depending on throttle
                _engineSoundInst.Pitch = MyMathHelper.Lerp(output.Throttle, 0, 1, -0.5f, 0.5f);
            }
        }

        private void HandleResetting(GameTime time)
        {
            // Reset the helicopter once upon key press, or continuously if the helicopter was created to
            // freeze on the starting state
            // Note removed because we want to try and handle this in SimulatorGame instead
            //            if (Keyboard.GetState().IsKeyDown(Keys.Space))
            //                ResetHelicopter(time.TotalGameTime);

            // Reset helicopter if it goes too far or crashes with ground
            //            const float maxRadius = 10.0f;
            //            if (Vector3.Distance(RealPosition, _startState.Position) > maxRadius || RealPosition.Y <= 0)
            //                ResetHelicopter();


            //            if (Position.Y <= 0)
            //                ResetHelicopter(time.TotalGameTime);
        }

        public void Reset(TimeSpan totalGameTime, HelicopterScenario scenario, NavigationMap heightmap)
        {
            Console.WriteLine(@"Resetting helicopter.");

            _scenario = scenario;
            

            // TODO We would rather want to do Autopilot.Reset() than this fugly code
            Autopilot.IsAtDestination = false;
            Autopilot = Autopilot.Clone();
            Autopilot.Task = scenario.Task.Clone();
            Autopilot.Map = heightmap;


            Vector3 startPosition = scenario.StartPosition;
            Vector3 startVelocity = Vector3.Zero;
            Vector3 startAcceleration = Vector3.Zero;
            Quaternion startOrientation = Quaternion.Identity;

            if (Task.HoldHeightAboveGround > 0)
                startPosition.Y = Autopilot.Map.GetAltitude(VectorHelper.ToHorizontal(startPosition)) + Task.HoldHeightAboveGround;

            var startPhysicalState = new PhysicalHeliState(
                startOrientation, startPosition, startVelocity, startAcceleration);
            var initialState = new SimulationStepResults(startPhysicalState, totalGameTime);

            _physicalState = startPhysicalState;

            // Re-create the state provider when resetting because some sensors will have to re-initialize.
            _physics = new HeliPhysics(_collision, UseTerrainCollision);
            _sensors = new SensorModel(_sensorSpecifications, Autopilot.Map, startPosition, startOrientation);
            _perfectStateProvider = new PerfectState();
            _estimatedStateProvider = new SensorEstimatedState(_sensors, startPhysicalState);

            // Wait for state to become stable.
//            while (!_perfectStateProvider.Ready)
//            {
                // TODO GPS will require N seconds of startup time
//                _perfectStateProvider.Update(initialState, 0, 0, new JoystickOutput());
//                Sensors.Update(initialState, new JoystickOutput());
                //                Thread.Sleep(100);
//            }

            // When resetting, use perfect state as starting point.
            // TODO It should not be necessary to create this waypoint since it should never be used for navigation! Delete if safe.
            // Use start position and start orientation instead.
            const float defaultWaypointRadius = 5;
            var startWaypoint = new Waypoint(startPosition, 0, WaypointType.Intermediate, defaultWaypointRadius);
            _trueState = StateHelper.ToHeliState(startPhysicalState, GetHeightAboveGround(startPhysicalState.Position), startWaypoint, new JoystickOutput());
            _estimatedState = _trueState;

            Log.Clear();
        }

        private JoystickOutput GetUserInput()
        {
            float pitch = 0, roll = 0, yaw = 0, throttle = 0;
#if XBOX
            Vector2 leftTS = GamePad.GetState(PlayerIndex.One).ThumbSticks.Left;
            Vector2 rightTS = GamePad.GetState(PlayerIndex.One).ThumbSticks.Right;
            pitch = -rightTS.Y;
            roll = rightTS.X;
            yaw = leftTS.X;
            throttle = MyMathHelper.Lerp(leftTS.Y, -1, 1, 0, 1);
#else
            if (_joystickSystem == null)
                throw new Exception("No joystick connected? Either \n1) Connect a properly configured joystick and restart. \n2) Modify Scenarios.xml to run an autopilot scenario that does not require joystick input.");

            _joystickSystem.Update();

            pitch = _joystickSystem.GetAxisValue(JoystickAxisAction.Pitch);
            roll = _joystickSystem.GetAxisValue(JoystickAxisAction.Roll);
            yaw = _joystickSystem.GetAxisValue(JoystickAxisAction.Yaw);
            throttle = _joystickSystem.GetAxisValue(JoystickAxisAction.Throttle);

            throttle = 0.5f + throttle/2; // -1 to +1 => 0 to +1

            // Try square exponential response (i^2) and a linear reduction of max
            // TODO Define response in terms of max angular rotation per second
            const float cyclicResponse = 0.7f; // 0 means no response, 1 means full response
            const float rudderResponse = 0.7f;

            roll = Math.Sign(roll)*cyclicResponse*roll*roll; 
            pitch = Math.Sign(pitch)*cyclicResponse*pitch*pitch;
            yaw = Math.Sign(yaw)*rudderResponse*yaw*yaw;
#endif


            // Dead zone to avoid jittery behavior at idle stick
            if (Math.Abs(roll) < 0.05) roll = 0;
            if (Math.Abs(pitch) < 0.05) pitch = 0;
            if (Math.Abs(yaw) < 0.05) yaw = 0;

            var result = new JoystickOutput {Roll = roll, Pitch = pitch, Yaw = yaw, Throttle = throttle};
            return result;
        }


        private void DrawLine(Vector3 origin, Vector3 target, Color color)
        {
            var lineStripIndices = new short[] {0, 1};
            var pointList = new[]
                                {
                                    new VertexPositionColor(origin, color),
                                    new VertexPositionColor(target, color)
                                };

            _basicEffect.World = Matrix.Identity;
            _basicEffect.VertexColorEnabled = true;
            _basicEffect.TextureEnabled = false;
            _basicEffect.Begin();
            foreach (EffectPass pass in _basicEffect.CurrentTechnique.Passes)
            {
                pass.Begin();

                GraphicsDevice.VertexDeclaration = new VertexDeclaration(_game.GraphicsDevice,
                                                                         VertexPositionColor.VertexElements);
                GraphicsDevice.DrawUserIndexedPrimitives(
                    PrimitiveType.LineStrip,
                    pointList,
                    0, // vertex buffer offset to add to each element of the index buffer
                    2, // number of vertices to draw
                    lineStripIndices,
                    0, // first index element to read
                    1 // number of primitives to draw
                    );

                pass.End();
            }
            _basicEffect.End();
        }


        private void DrawMesh(Matrix transform, ModelMesh mesh, float alpha)
        {
            GraphicsDevice.RenderState.AlphaBlendEnable = true;
            GraphicsDevice.RenderState.SourceBlend = Blend.SourceAlpha;
            GraphicsDevice.RenderState.DestinationBlend = Blend.InverseSourceAlpha;
            //            GraphicsDevice.RenderState.AlphaBlendOperation = BlendFunction.Add;

            //            foreach (BasicEffect effect in mesh.Effects)
            foreach (SunlightEffect effect in mesh.Effects)
            {
                //                effect.EnableDefaultLighting();

                effect.Alpha = alpha;
                effect.View = Camera.View;
                effect.Projection = Camera.Projection;
                effect.World = transform;
                effect.ApplyParameters();

//                SunlightEffect sunlight = effect;
//                if (sunlight != null)
//                {
//                }
            }


            mesh.Draw();

            GraphicsDevice.RenderState.AlphaBlendEnable = false;
        }

        private void DrawMeshGhost(Matrix transform, ModelMesh mesh, float alpha)
        {
            GraphicsDevice.RenderState.AlphaBlendEnable = true;
            GraphicsDevice.RenderState.SourceBlend = Blend.One;
            GraphicsDevice.RenderState.DestinationBlend = Blend.One;

            foreach (SunlightEffect effect in mesh.Effects)
            {
                effect.Alpha = alpha;
                effect.View = Camera.View;
                effect.Projection = Camera.Projection;
                effect.World = transform;
                effect.ApplyParameters();
            }

            mesh.Draw();

            GraphicsDevice.RenderState.AlphaBlendEnable = false;
        }

        private void DrawMeshShadow(Matrix transform, ModelMesh mesh, float alpha)
        {
            // Store original effects for mesh parts, before substituting them for shadow rendering
            var originalEffects = new Dictionary<ModelMeshPart, Effect>();
            foreach (ModelMeshPart part in mesh.MeshParts)
            {
                originalEffects[part] = part.Effect;
                part.Effect = _basicEffect;
            }

            foreach (BasicEffect effect in mesh.Effects)
            {
                effect.AmbientLightColor = Vector3.Zero;
                effect.Alpha = alpha;
                effect.DirectionalLight0.Enabled = false;
                effect.DirectionalLight1.Enabled = false;
                effect.DirectionalLight2.Enabled = false;
//                effect.View = _basicEffect.View;
//                effect.Projection = _basicEffect.Projection;
                effect.World = transform*_shadowTransform;
            }
            mesh.Draw();

            // Restore former effects for mesh parts
            foreach (ModelMeshPart part in mesh.MeshParts)
            {
                part.Effect = originalEffects[part];
            }
        }


        private void DrawHelicopter(GameTime gameTime, Model m, bool shadows)
        {
            // Draw a fully opaque version of the real helicopter
            DrawHelicopter(gameTime, m, 1f, Position, _physicalState.Orientation, DrawMesh);
            if (shadows)
            {
                GraphicsDevice.Clear(ClearOptions.Stencil, Color.Black, 0, 0);
                GraphicsDevice.RenderState.StencilEnable = true;
                // Draw on screen if 0 is the stencil buffer value           
                GraphicsDevice.RenderState.ReferenceStencil = 0;
                GraphicsDevice.RenderState.StencilFunction = CompareFunction.Equal;
                // Increment the stencil buffer if we draw
                GraphicsDevice.RenderState.StencilPass = StencilOperation.Increment;
                // Setup alpha blending to make the shadow semi-transparent
                GraphicsDevice.RenderState.AlphaBlendEnable = false;
                GraphicsDevice.RenderState.SourceBlend = Blend.SourceAlpha;
                GraphicsDevice.RenderState.DestinationBlend = Blend.InverseSourceAlpha;

                DrawHelicopter(gameTime, m, 0.5f, Position, _physicalState.Orientation, DrawMeshShadow);

                // turn stencilling off
                GraphicsDevice.RenderState.StencilEnable = false;
                // turn alpha blending off
                GraphicsDevice.RenderState.AlphaBlendEnable = false;
            }

            if (DrawGhost)
            {
                // Draw a silhouette of the estimated helicopter position and orientation
                Vector3 estimatedPosition = _estimatedState.Position;
                Quaternion estimatedOrientation = StateHelper.ToPhysical(_estimatedState).Orientation;
                // TODO Shouldn't orientation be stored in estimated helicopter state?
                DrawHelicopter(gameTime, m, 1f, estimatedPosition, estimatedOrientation, DrawMeshGhost);
            }
        }


        private static void DrawHelicopter(GameTime gameTime, Model m, float alpha, Vector3 position,
                                           Quaternion rotation,
                                           DrawMeshMethod renderer)
        {
            var transforms = new Matrix[m.Bones.Count];
            m.CopyAbsoluteBoneTransformsTo(transforms);
            foreach (ModelMesh mesh in m.Meshes)
            {
                if (mesh.Name.Equals("Rotor"))
                {
                    // Rotate rotor
                    float rotorBaseAngle = (float) gameTime.TotalGameTime.TotalSeconds*4*MathHelper.TwoPi;

                    for (int i = 0; i < MotionBlurRenderCount; i++)
                    {
                        Axes axes = VectorHelper.GetAxes(rotation);
                        Quaternion rotorAxisRotation = Quaternion.CreateFromAxisAngle(Vector3.Up,
                                                                                      rotorBaseAngle + 0.0471f*i);
                        Quaternion rotorRotation = rotation*rotorAxisRotation; //Matrix.Transform(rotation, rotorAxis);

                        Matrix transform = transforms[mesh.ParentBone.Index]*
                                           Matrix.CreateScale(1/100.0f)*
                                           Matrix.CreateFromQuaternion(rotorRotation)*
                                           Matrix.CreateTranslation(position);

                        renderer(transform, mesh,
                                 alpha*MathHelper.Lerp(0.2f, 1.0f, (float) i/(MotionBlurRenderCount - 1)));
                    }
                }
                else
                {
                    Matrix transform = transforms[mesh.ParentBone.Index]*
                                       Matrix.CreateScale(1/100.0f)*
                                       Matrix.CreateFromQuaternion(rotation)*
                                       Matrix.CreateTranslation(position);

                    renderer(transform, mesh, alpha);
                }
            }
        }


        private void PrintInfo(SpriteBatch batch, string text)
        {
            const int lineSpacing = 40;
            const int infoX = 800;
            batch.DrawString(_hudInfo1Font, text, new Vector2(infoX, _infoCount++*lineSpacing), Color.YellowGreen);
        }

        private void DrawText()
        {
            if (!_drawText) return;

            _infoCount = 0;

            Angles deg = _estimatedState.Degrees;
            Waypoint wp = _estimatedState.Waypoint;
            Angles rad = _estimatedState.Radians;

            PrintInfo(_spriteBatch, "Speed: " + Convert.ToInt32(_physicalState.Velocity.Length()*3.6) + " km/h");
            //            PrintInfo(_spriteBatch, String.Format("Pitch {0}' / {1}' = {2}%",
            //                                                   deg.PitchAngle.ToString("#0"),
            //                                                   MathHelper.ToDegrees(_controlGoal.PitchAngle).ToString("#0"),
            //                                                   (int)(100 - MathHelper.Distance(rad.HeadingAngle, _controlGoal.PitchAngle) / MathHelper.TwoPi * 100)));
            //
            //            PrintInfo(_spriteBatch, String.Format("Roll {0} / {1}' = {2}%",
            //                                                   deg.RollAngle.ToString("#0"),
            //                                                   MathHelper.ToDegrees(_controlGoal.RollAngle).ToString("#0"),
            //                                                   (int)(100 - MathHelper.Distance(rad.RightAngle, _controlGoal.RollAngle) / MathHelper.TwoPi * 100)));
            //
            //            PrintInfo(_spriteBatch, String.Format("Yaw {0} / {1}' => {2}'",
            //                                                   deg.HeadingAngle.ToString("#0"),
            //                                                   MathHelper.ToDegrees(_controlGoal.HeadingAngle).ToString("#0"), 0));
            //            //                (int)_State.Degrees.HeadingErrorAngle));

            PrintInfo(_spriteBatch, String.Format("Throttle {0}%", (_estimatedState.Output.Throttle*100).ToString("#0")));

            PrintInfo(_spriteBatch, String.Format("Real pos: {0}, {1}, {2}m to {3}, {4}, {5} => {6}m",
                                                  Math.Round(Position.X, 1), Math.Round(Position.Y, 1), Math.Round(Position.Z, 1),
                                                  Math.Round(wp.Position.X, 1), Math.Round(wp.Position.Y, 1),
                                                  Math.Round(wp.Position.Z, 1),
                                                  Math.Round(Vector3.Distance(wp.Position, Position), 1)
                                        ));

            PrintInfo(_spriteBatch, String.Format("Est. pos: {0}, {1}, {2}m - error {3}m",
                                                  Math.Round(_estimatedState.Position.X, 1),
                                                  Math.Round(_estimatedState.Position.Y, 1),
                                                  Math.Round(_estimatedState.Position.Z, 1),
                                                  Math.Round(_estimatedStateError.Position.Length(), 1)
                                        ));

            PrintInfo(_spriteBatch, String.Format("Pitch ({0}) Roll({1}) Yaw({2})",
                                                  Convert.ToInt32(_trueState.Degrees.PitchAngle),
                                                  Convert.ToInt32(_trueState.Degrees.RollAngle),
                                                  Convert.ToInt32(_trueState.Degrees.HeadingAngle)));

            PrintInfo(_spriteBatch, String.Format("Height Above Ground: {0}m", Math.Round(_sensors.GroundRangeFinder.FlatGroundHeight, 1)));
            //
            //            PrintInfo(_spriteBatch, String.Format("Nav: {0}", Autopilot.Navigation));
            //            PrintInfo(_spriteBatch, String.Format("Actions: {0}", Autopilot.ActionsToString(Autopilot.Actions)));
            //            PrintInfo(_spriteBatch, String.Format("State: "));
        }


        private void DrawWorld(GameTime time, bool shadows)
        {
            _game.GraphicsDevice.RenderState.DepthBufferWriteEnable = true;
            _game.GraphicsDevice.RenderState.DepthBufferEnable = true;
            _game.GraphicsDevice.RenderState.AlphaBlendEnable = false;
            _game.GraphicsDevice.RenderState.AlphaTestEnable = false;

            if (Camera is CockpitCamera == false)
                DrawHelicopter(time, _helicopter, shadows);

            //            DrawLine(Position, Position + 0.5f * _State.Up, Color.Black);
            //            DrawLine(Position, Position + 1.0f * hVelocity, Color.White);
            //            DrawLine(Position, Position + 0.5f * _State.LiftDir, Color.White);
            //            DrawLine(Position, Position + 0.5f * _State.Forward, Color.Yellow);
            //            DrawLine(Position, Position + 0.5f * _State.Right, Color.Red);
            //DrawLine(Position, Autopilot.CurrentWaypoint.Position, Color.Pink);
        }

        #endregion

        #region Nested type: DrawMeshMethod

        private delegate void DrawMeshMethod(Matrix transform, ModelMesh mesh, float alpha);

        #endregion
    }
}