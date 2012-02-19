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
using System.IO;
using System.Linq;
using Control;
using Control.Common;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Simulator.Cameras;
using Simulator.Common;
using Simulator.Interfaces;
using Simulator.Scenarios;
using Simulator.Skydome;
using Simulator.StaticMeshes;
using Simulator.StaticMeshes.Trees;
using Simulator.Terrain;
using Simulator.Testing;
using Simulator.Utils;
using State.Model;
using Physics;
using Simulator.Parsers;
using NINFocusOnTerrain;
using System.Diagnostics;

#if !XBOX
using Tests.ControlGraphs;
using Tests.ControlGraphs.KalmanFilter;
#endif


#endregion

// Disable warnings about "code not reachable".
// I have a few of those because of hardcoded constant settings.
#pragma warning disable 0162

namespace Simulator
{
    /// <summary>
    ///   This is the main entry point for running the helicopter simulator.
    /// </summary>
    public class SimulatorGame : Game, ICameraProvider
    {
        /// <summary>
        /// Setting this to true would cause undefined behavior if Stereo rendering is enabled as well, since it requires windows to work.
        /// </summary>
        private const bool Fullscreen = false;

        /// <summary>
        /// Simulation speed. 1.0 is default.
        /// </summary>
        private const float SimulatorSpeed = 1.0f;

        private const bool DrawText = false;

        private static readonly Color ClearColor = Color.DarkGray;

#if !XBOX
        /// <summary>
        /// Eye distance for stereo rendering.
        /// </summary>
        private const float HalfEyeDistance = 0.05f;
        private readonly FlightLogWindow _logWindow;
        private readonly LiveFlightLogDataProvider _liveFlightLogger;
        private readonly IntPtr _stereoRightHandle;
#endif

        private readonly GraphicsDeviceManager _graphics;
        private readonly SimulationSettings _settings;
        private Scenario _scenario;
        private readonly int _screenHeight = 1024;
        private readonly int _screenWidth = 1280;
        private const bool ShowFPS = false;
        private readonly string _relativeOutputPath;
        private readonly TestConfiguration _testConfiguration;
        private TimeSpan _accumulatedSimulatorTime;
        private List<IGameComponent> _barrels;
        private BasicEffect _basicEffect;
        private SimpleModel _cockpitMesh;
        private SimpleModel _currentWaypoint;
        private Forest _forest;
        private FPS _fpsUtil;
        private HelicopterBase _helicopter;
        private SpriteFont _hudInfo1Font;
        private PIDSetup? _initialPIDSetup;
        private RenderTarget2D _leftEyeRender;
        private SoundEffect _music;
        private SoundEffectInstance _musicInst;
        private RenderTarget2D _rightEyeRender;
        private RenderTarget2D _screenshotTarget;
        private SkydomeComponent _skydome;
        private SpriteBatch _spriteBatch;
        private bool _takeScreenshot;

        private TerrainComponent _terrain;
        private readonly KeyboardEvents _keyEvents;
        private bool _isLoggingFlight;
        private FlatTexturedGround _texturedGround;
        private TerrainCollision _terrainCollision;
        private List<Scenario>.Enumerator _testScenarioIter;
        private NavigationMap _heightmap;
        private Dictionary<string, List<ScenarioTestResult>> _testResults;
        private ScenarioIntermediaryTestResult _intermediaryTestResult;
        private List<float>.Enumerator _testMaxHVelocityIter;
        private bool _isHelicopterCrashed;
        private RenderTarget2D _screenshotTargetLeft;
        private RenderTarget2D _screenshotTargetRight;


        /// <summary>
        /// Create the simulation object.
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="handle"></param>
        public SimulatorGame(SimulationSettings settings, IntPtr handle)
            : this(settings, handle, null, "Test Results")
        {
        }

        /// <summary>
        /// Create the simulation object using a test configuration run batch tests.
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="handle"></param>
        /// <param name="testConfiguration"></param>
        /// <param name="relativeOutputPath"></param>
        public SimulatorGame(SimulationSettings settings, IntPtr handle, TestConfiguration testConfiguration, string relativeOutputPath)
            : this()
        {
            _settings = settings;
            _relativeOutputPath = relativeOutputPath;
            _testConfiguration = testConfiguration ?? TestConfiguration.Default;
#if !XBOX
            _stereoRightHandle = handle;
#endif

        }

        /// <summary>
        ///   Create a simulator in default mode.
        /// </summary>
        private SimulatorGame()
        {
            Exiting += SimulatorGame_Exiting;

            Content.RootDirectory = "Content";

            // If not fullscreen, use desktop resolution
            // If fullscreen, use default values
            if (!Fullscreen)
            {
                _screenWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
                _screenHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
            }


            _graphics = new GraphicsDeviceManager(this)
                            {
                                PreferredDepthStencilFormat = SelectStencilMode(),
                                PreferredBackBufferWidth = _screenWidth,
                                PreferredBackBufferHeight = _screenHeight,
                                IsFullScreen = Fullscreen,
                                SynchronizeWithVerticalRetrace = false,
                            };

            _keyEvents = new KeyboardEvents(this);
            _keyEvents.KeyPressed += KeyPressed;

#if !XBOX
            _stereoRightHandle = IntPtr.Zero;
            _logWindow = new FlightLogWindow();
            _logWindow.Closing += LogWindowClosing;
            _liveFlightLogger = new LiveFlightLogDataProvider(_logWindow.PositionTopView.XYLineChart, _logWindow.AccelerometerView.XYLineChart, _logWindow.HeightView.XYLineChart);
#endif


            _barrels = new List<IGameComponent>();
        }

        public ICamera Camera { get; set; }

        private bool IsTestMode
        {
            get
            {
                return _scenario != null && !_scenario.HelicopterScenarios[0].PlayerControlled;
                //return _testConfiguration != null && 
                //    _testConfiguration != TestConfiguration.Default;
            }
        }


        private void KeyPressed(Keys key)
        {
#if !XBOX
            switch (key)
            {
                case Keys.L:
                    ToggleLiveFlightLogWindow();
                    break;
            }
#endif

        }

        


#if !XBOX
        private void ToggleLiveFlightLogWindow()
        {
            if (!_isLoggingFlight)
                ShowLiveFlightLogWindow();
            else
                HideLiveFlightLogWindow();
        }

        void LogWindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // We don't really want to close the window, just hide it
            e.Cancel = true;
            HideLiveFlightLogWindow();
        }

        private void ShowLiveFlightLogWindow()
        {
            _isLoggingFlight = true;
            _liveFlightLogger.Clear();
            _logWindow.Show();
        }

        private void HideLiveFlightLogWindow()
        {
            _isLoggingFlight = false;
            _logWindow.Hide();
        }

#endif

        private CameraType MyCameraType
        {
            get
            {
                // Default camera unless overridden in scenario.xml
                if (_scenario == null) return CameraType.Chase;

                return _scenario.CameraType;
            }
        }

        private void SimulatorGame_Exiting(object sender, EventArgs e)
        {
#if !XBOX
            if (IsTestMode)
            {
                ExportTestResultsToFile(@"test_results.txt", _testResults);
            }

            FlightLogXMLFile.Write(@"flightpath.xml", _helicopter.Log, _helicopter.Task.AllWaypoints);
#endif
        }

        private void ExportTestResultsToFile(string filename, Dictionary<string, List<ScenarioTestResult>> testResults)
        {
#if XBOX
            return;
#endif

            string testResultsDirectory = _relativeOutputPath;
            try
            {

                if (!Directory.Exists(testResultsDirectory))
                {
                    Directory.CreateDirectory(testResultsDirectory);
                    Console.WriteLine(@"Created folder.");
                }
                else
                {
                    string[] filesToDelete = Directory.GetFiles(testResultsDirectory);
                    foreach (var filenameToDelete in filesToDelete)
                    {
                        try
                        {
                            File.Delete(filenameToDelete);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                        }
                    }
                }

                string filepath = Path.Combine(testResultsDirectory, filename);

                using (var file = new StreamWriter(filepath))
                {
                    foreach (List<ScenarioTestResult> scenarioResults in testResults.Values)
                    {
                        foreach (ScenarioTestResult r in scenarioResults)
                        {
                            // TODO Multiple helicopters
                            string flightlogPath = Path.Combine(testResultsDirectory, GetFlightLogFilename(r));
                            FlightLogXMLFile.Write(flightlogPath, r.FlightLog, r.Scenario.HelicopterScenarios[0].Task.AllWaypoints);

                            file.WriteLine("Scenario: " + r.Scenario.Name);
                            file.WriteLine("Scenario ended by: " + r.EndTrigger);
                            file.WriteLine("Duration: {0} s", Math.Round(r.Duration.TotalSeconds, 1));
                            file.WriteLine("Sensors: \r\n" + r.Sensors);
                            file.WriteLine(r.Autopilot.ToString());
                            file.WriteLine();
                            file.WriteLine("MaxEstimatedPositionError: {0} m", Math.Round(r.MaxEstimatedPositionError, 2));
                            file.WriteLine("AvgEstimatedPositionError: {0} m", Math.Round(r.AvgEstimatedPositionError, 2));
                            file.WriteLine("MinEstimatedPositionError: {0} m", Math.Round(r.MinEstimatedPositionError, 2));

                            file.WriteLine("MaxHeightAboveGround: {0} m", Math.Round(r.MaxHeightAboveGround, 2));
                            file.WriteLine("AvgHeightAboveGround: {0} m", Math.Round(r.AvgHeightAboveGround, 2));
                            file.WriteLine("MinHeightAboveGround: {0} m", Math.Round(r.MinHeightAboveGround, 2));

                            file.WriteLine("MaxVelocity: {0} km/h", Math.Round(r.MaxVelocity * 3.6, 1));
                            file.WriteLine("AvgVelocity: {0} km/h", Math.Round(r.AvgVelocity * 3.6, 1));

                            file.WriteLine();
                            file.WriteLine();
                            file.WriteLine();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private static string GetFlightLogFilename(ScenarioTestResult r)
        {
            const string filenameBase = @"flightlog";
            const string ext = ".xml";

            // TODO Sensor configuration
            string dynamicPart = String.Format("{0}_{1}mps", r.Scenario.Name, r.Autopilot.MaxHVelocity);

            return String.Format("{0}_{1}{2}", filenameBase, dynamicPart, ext);
        }


        /// <summary>
        ///   Allows the game to perform any initialization it needs to before starting to run.
        ///   This is where it can query for any required services and load any non-graphic
        ///   related content.  Calling base.Initialize will enumerate through any components
        ///   and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            _basicEffect = CreateBasicEffect(this);

            #region Load scenario / test scenarios

            _testMaxHVelocityIter = _testConfiguration.MaxHVelocities.GetEnumerator();
            if (!_testMaxHVelocityIter.MoveNext())
                throw new Exception("No maxHVelocities defined in _testConfiguration.");

            _testScenarioIter = _testConfiguration.TestScenarios.GetEnumerator();

            if (IsTestMode)
                NextTestScenario();
            else
            {
                _testScenarioIter.MoveNext();
                _scenario = _testScenarioIter.Current;
            }

            if (_scenario == null)
                throw new Exception("Could not load scenario from XML.");

            #endregion

            #region Cameras

            // We must create a dummy camera target until the helicopter instance is created, to avoid exceptions
            var cameraOffset = new Vector3(0.0f, 0.5f, -0.5f);
            var startPos = _scenario.HelicopterScenarios[0].StartPosition + cameraOffset;
            _cockpitMesh = new SimpleModel("Models/apache-cockpit", this, this);
            InitCamera(MyCameraType, startPos + new Vector3(0, 0.5f, 4), new WorldDummy(Vector3.Zero));

            #endregion

            #region Sunlight, skydome, terrain and trees

            // Initialize skydome and sunlight parameters
            _skydome = new SkydomeComponent(this, this);
            _terrain = new TerrainComponent(this, this, _skydome.Parameters);

            UpdateTerrain(_scenario);
            

            // Initialize trees
            // TODO Forest component does not support dynamic loading/unloading of components yet
            if (_scenario.SceneElements.Contains("Forest"))
                _forest = new Forest(this, this, _terrain.Mesh, _skydome.Parameters);

            #endregion

            #region Misc components

            _fpsUtil = new FPS(this);
            _barrels = GetBarrels();
            _texturedGround = new FlatTexturedGround(this, this);

            _currentWaypoint = new WaypointMesh(this, this);
            _currentWaypoint.Position = new Vector3(0, 1, 0);


            #endregion

            #region Physics

            _terrainCollision = new TerrainCollision(this, this);

            // We implemented our own crash detection for whenever Jitter is disabled
            _terrainCollision.SetHeightValues(_heightmap.HeightValues);

            #endregion

            #region Helicopters

            
            // Initialize helicopter(s)
            InitHelicopters(_skydome.Parameters, _heightmap);
            Camera.LookAtTarget = _helicopter;
            _terrainCollision.Helicopter = _helicopter; // TODO Clean this up, non-intuitive usage
            _helicopter.Crashed += HelicopterCrashEventHandler;

            #endregion

            #region Test scenario results

            if (IsTestMode)
            {
                _testResults = new Dictionary<string, List<ScenarioTestResult>>();
            }

            #endregion


            // Add game components to the game loop
            AddGameComponents();

            Reset();

            base.Initialize();
        }

        private NavigationMap UpdateTerrain(Scenario scenario)
        {
            if (scenario.SceneElements.Contains("Terrain"))
            {
                // Build terrain and derive a heightmap from it
                TerrainInfo ti = scenario.TerrainInfo;
                _terrain.BuildTerrain(ti.Width, ti.MinHeight, ti.MaxHeight);
                _heightmap = new NavigationMap(HeightmapToValues(_terrain.Mesh.Heightmap));
            }
            else
            {
                // Assume flat ground if not using terrain
                _heightmap = new NavigationMap(new [,]
                                                   {
                                                       {0f, 0f,},
                                                       {0f, 0f,},
                                                   });
            }

            return _heightmap;
        }

        /// <summary>
        /// Returns a 2D array of [rows, cols].
        /// </summary>
        /// <param name="hm"></param>
        /// <returns></returns>
        private static float[,] HeightmapToValues(Heightmap hm)
        {
            var heightValues = new float[hm.Depth, hm.Width];
            for (int i = 0; i < hm.HeightValues.Length; i++)
            {
                int col = i % hm.Depth;
                int row = i / hm.Width;
                heightValues[row, col] = hm.HeightValues[i];
            }

            return heightValues;
        }


        private void HelicopterCrashEventHandler(GameTime gameTime)
        {
            // We must handle the crash in our Update method, so just flag it for now.
            _isHelicopterCrashed = true;
        }
        
        private void HandleTestScenarioEnded(GameTime gameTime, TestScenarioEndTrigger endTrigger)
        {
            UpdateTestResults(gameTime, endTrigger);

            // If we reached our destination or the user skipped this test scenario, then
            // go to next test scenario.
            if (endTrigger == TestScenarioEndTrigger.ReachedDestination)
            {
                Debug.WriteLine("We reached our destination.");
                NextTestScenario();
            }
            else if (endTrigger == TestScenarioEndTrigger.UserSkipped)
            {
                Debug.WriteLine("User skipped scenario."); 
                NextTestScenario();
            }
            else if (endTrigger == TestScenarioEndTrigger.TimedOut)
            {
                Debug.WriteLine("Test scenario timed out.");
                NextTestScenario();
            }
            else if (endTrigger == TestScenarioEndTrigger.Crashed)
            {
                // If helicopter crashed then try a slower velocity setting (if available)
                // or go to next test scenario (if available).
                if (_testMaxHVelocityIter.MoveNext())
                    Debug.WriteLine("Now testing maxHVelocity " + _testMaxHVelocityIter.Current);
                else
                {
                    Debug.WriteLine("Crashed at lowest velocity setting.");
                    NextTestScenario();
                }
            }
            else 
                throw new NotImplementedException("Unknown end trigger " + endTrigger);

            // Quit if no scenarios are left or reset to start new test scenario.
            if (_scenario == null)
                Exit();
            else
                Reset();
        }

        private void NextTestScenario()
        {
            if (!IsTestMode)
                return;

            // Proceed to next test scenario since we have tested all velocities at the current test scenario
            _testScenarioIter.MoveNext();
            _scenario = _testScenarioIter.Current;

            // Start over on the set of maxHVelocities to test
            _testMaxHVelocityIter = _testConfiguration.MaxHVelocities.GetEnumerator();
            _testMaxHVelocityIter.MoveNext();

            if (_scenario != null)
                Debug.WriteLine("Progressing to next scenario " + _scenario.Name + " at maxHVelocity " +
                                _testMaxHVelocityIter.Current + ".");
            else
                Debug.WriteLine("Completed all test scenarios!");
        }

        private void UpdateTestResults(GameTime gameTime, TestScenarioEndTrigger endTrigger)
        {
            if (!_testResults.ContainsKey(_scenario.Name))
                _testResults[_scenario.Name] = new List<ScenarioTestResult>();

            // Append result to list of results for this scenario name
            var r = new ScenarioTestResult(_intermediaryTestResult, _scenario, _helicopter, gameTime, endTrigger);
            _testResults[_scenario.Name].Add(r);
        }

        private void Reset()
        {
            // Note: We got an odd bug that increased the initial estimate of the kalman filter the longer the simulation had run
            // so that after running several scenarios in sequence, the initial estimate for each reset got worse.
            // Not sure why, but resetting the simulation time prevents this behavior. Must be some resetting that I have missed in
            // my sensor estimated state or sensors, but can't find where.
            _accumulatedSimulatorTime = TimeSpan.Zero;

            if (IsTestMode)
            {
                _intermediaryTestResult = new ScenarioIntermediaryTestResult
                                              {
                                                  StartTime = _accumulatedSimulatorTime,
                                              };
            }

            // Synchronize list of game components with currently selected scenario
            UpdateGameComponents();

            NavigationMap heightmap = UpdateTerrain(_scenario);
            HelicopterScenario heliScenario = _scenario.HelicopterScenarios[0];  // TODO Multiple helicopters

            // Setting new height values implicitly resets the physics engine
            _terrainCollision.SetHeightValues(heightmap.HeightValues);
//            _terrainCollision.Reset();

            _helicopter.Autopilot.MaxHVelocity = _testMaxHVelocityIter.Current;
            _helicopter.Reset(_accumulatedSimulatorTime, heliScenario, heightmap);
        }


        private void UpdateGameComponents()
        {
            // Remove all components
            Components.Clear();

            // Then add those components used for the current selected scenario
            AddGameComponents();
        }

        private void AddGameComponents()
        {
            foreach (string element in _scenario.SceneElements)
            {
                switch (element)
                {
                    case "Skydome":
                        Components.Add(_skydome);
                        break;
                    case "Terrain":
                        Components.Add(_terrain);
                        break;
                    case "Forest":
                        Components.Add(_forest);
                        break;
                    case "Ground":
//                        Components.Add(new Ground(this, _basicEffect));
                        Components.Add(_texturedGround);
                        break;
                    case "Barrels":
                        foreach (IGameComponent barrel in _barrels)
                            Components.Add(barrel);
                        break;
                    case "CurrentWaypoint":
                        Components.Add(_currentWaypoint);
                        break;
                }
            }

            // TODO Case this for terrain/flat ground
            Components.Add(_terrainCollision);

//            Components.Add(new WaypointMesh(this, _camera) { Position = Vector3.Zero } );

            // System game components, independent of render and update order etc..
            Components.Add(_fpsUtil);
            Components.Add(_keyEvents);

            // Helicopter 
            Components.Add(_helicopter);

            // TODO TEMP
//            var tree = new SunlitLTree(this, this, _skydome.Parameters, Vector3.Zero);
//            Components.Add(tree);

            // Only render the cockpit when in cockpit camera mode);
            if (Camera is CockpitCamera && !Components.Contains(_cockpitMesh))
                Components.Add(_cockpitMesh);
        }

        private List<IGameComponent> GetBarrels()
        {
            var barrels = new List<IGameComponent>();

            // Miscellaneous static objects
            barrels.Add(new SimpleModel("Models/barrel", this, this, _skydome.Parameters)
                            {
                                Position = 5*Vector3.Forward
                            });

//            var rand = new Random();
//            const int cols = 4, rows = 4, barrelsPerNode = 3;
//            const float gridLength = 8, clusterRadius = 0.4f;
//            for (int col = 0; col < cols; col++)
//            {
//                for (int row = 0; row < rows; row++)
//                {
//                    Each cluster is randomly translated and rotated
//                    var randomOffset = new Vector3((float) rand.NextDouble(), 0, (float) rand.NextDouble());
//                    double randomAngle = (float) rand.NextDouble()*2*Math.PI;
//                    for (int i = 0; i < barrelsPerNode; i++)
//                    {
//                        var clusterOffset = new Vector3((float) col/(cols - 1)*gridLength, 0,
//                                                        (float) row/(rows - 1)*gridLength);
//                        clusterOffset += randomOffset + new Vector3(-5, 0, -5);
//
//                        float x = (float) Math.Cos(randomAngle + 2*Math.PI*i/barrelsPerNode)*clusterRadius;
//                        float z = (float) Math.Sin(randomAngle + 2*Math.PI*i/barrelsPerNode)*clusterRadius;
//
//                        var barrel = new SimpleModel("barrel", this, _camera, _skydome.Parameters);
//                        barrel.Position = new Vector3(x, 0, z) + clusterOffset;
//                        barrels.Add(barrel);
//                    }
//                }
//            }

            return barrels;
        }


        private void InitCamera(CameraType type, Vector3 cameraPos, ICameraTarget target)
        {
            Components.Remove(_cockpitMesh);

            #region Handle different implementations

            if (type == CameraType.Fixed)
            {
                Camera = new FixedCamera
                             {
                                 FieldOfView = MathHelper.ToRadians(45),
                                 Position = cameraPos,
                             };
            }
            else if (type == CameraType.Chase)
            {
                Camera = new ChaseCamera
                             {
                                 DesiredPositionOffset = new Vector3(0.0f, 0.5f, 3.5f),
                                 LookAtOffset = new Vector3(0.0f, 0.0f, 0.0f),
                                 Damping = 600,
                                 Stiffness = 3000,
                                 IsElastic = true,
                                 FieldOfView = MathHelper.ToRadians(45),
                                 Position = cameraPos,
                             };
            }
            else if (type == CameraType.Free)
            {
                Camera = new FreeCamera(cameraPos, _screenWidth, _screenHeight)
                             {
                                 FieldOfView = MathHelper.ToRadians(45),
                                 Position = cameraPos,
                             };
            }
            else if (type == CameraType.Cockpit)
            {
                var offset = new Vector3(0, -0.1f, 0);
                Camera = new CockpitCamera(target, offset, _screenWidth, _screenHeight)
                             {
                                 FieldOfView = MathHelper.ToRadians(120),
                                 CockpitMesh = _cockpitMesh,
                             };
                Components.Add(_cockpitMesh);
            }
            else
                throw new NotImplementedException("Unknown camera type");

            #endregion


            if (target == null)
                Camera.LookAtTarget = new WorldDummy(Vector3.Zero);
            else
                Camera.LookAtTarget = target;


            Camera.NearPlane = 0.1f;
            Camera.FarPlane = 10000;
            Camera.AspectRatio = (float) _graphics.GraphicsDevice.Viewport.Width/
                                 _graphics.GraphicsDevice.Viewport.Height;
            Camera.Reset();
        }


        private void InitHelicopters(SunlightParameters skyParameters, NavigationMap heightmap)
        {
            // TODO 1 or more helicopters?
            if (_scenario.HelicopterScenarios == null || _scenario.HelicopterScenarios.Count == 0)
                throw new Exception("Could not find helicopter scenario.");

            HelicopterScenario heliScenario = _scenario.HelicopterScenarios[0];

            var startVelocity = new Vector3(); //new Vector3(-0.5f, 0, -1.0f)
            var startState = new PhysicalHeliState(
                Quaternion.Identity,
                heliScenario.StartPosition,
                startVelocity,
                Vector3.Zero);

            
            _helicopter = new HelicopterBase(this, _testConfiguration, _terrainCollision, this, _basicEffect, skyParameters, heliScenario, 
                                             heliScenario.EngineSound, heliScenario.PlayerControlled, DrawText);


            // If testing then we will continue to the next waypoint as soon as the true position is within
            // the radius of the waypoint. If not testing (non-simulated scenarios), we typically want to
            // progress only if the estimated state is belived to be within the radius.
            _helicopter.Autopilot.IsTestMode = IsTestMode;
            _helicopter.Autopilot.IsTruePositionWithinRadius = false;

            _helicopter.Autopilot.MaxHVelocity = _testMaxHVelocityIter.Current;

            // Optionally a PID setup may have been provided to use with the helicopters.
            // If not a default setup is used.
            if (_initialPIDSetup.HasValue)
                SetPIDSetup(_initialPIDSetup.Value);

            //            InitFormationHelicopters(startPos, skyParameters);

            _helicopter.Autopilot.Map = heightmap;
        }


        /// <summary>
        ///   LoadContent will be called once per game and is the place to load
        ///   all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            GraphicsDevice device = GraphicsDevice;

            _hudInfo1Font = Content.Load<SpriteFont>("HUDInfo1");
            _spriteBatch = new SpriteBatch(device);
            _screenshotTarget = new RenderTarget2D(device, _screenWidth, _screenHeight, 1, device.DisplayMode.Format);
            _screenshotTargetLeft = new RenderTarget2D(device, _screenWidth, _screenHeight, 1, device.DisplayMode.Format);
            _screenshotTargetRight = new RenderTarget2D(device, _screenWidth, _screenHeight, 1, device.DisplayMode.Format);

            if (_settings.RenderMode == RenderModes.StereoCrossConverged)
            {
                _leftEyeRender = new RenderTarget2D(device, _screenWidth, _screenHeight, 1, device.DisplayMode.Format);
                _rightEyeRender = new RenderTarget2D(device, _screenWidth, _screenHeight, 1, device.DisplayMode.Format);
            }

            if (!String.IsNullOrEmpty(_scenario.BackgroundMusic))
            {
                try
                {
                    _music = Content.Load<SoundEffect>(_scenario.BackgroundMusic);
                    _musicInst = _music.CreateInstance();
                    _musicInst.Volume = 1.0f;
                    _musicInst.IsLooped = true;

                    _musicInst.Stop(true);
                    _musicInst.Play();
                }
                catch (Exception e)
                {
                    Console.WriteLine(@"Could not load background music sound effect file. " + e);
                }
            }
        }

        /// <summary>
        ///   UnloadContent will be called once per game and is the place to unload
        ///   all content.
        /// </summary>
        protected override void UnloadContent()
        {
        }

        /// <summary>
        ///   Allows the game to run logic such as updating the world,
        ///   checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="doNotUseThisGameTime">
        ///   Provides a snapshot of timing values.
        /// </param>
        protected override void Update(GameTime doNotUseThisGameTime)
        {
            #region Manage our own simulation time

            // BUG? It seems there is a weird behavior for total game time.
            // First iteration: elapsed time = 0, game time = 0
            // Second iteration: elapsed time = 17ms, game time = 0 ??
            // Third iteration: elapsed time = 17ms, game time = 17ms and so on
            // So we keep track of our own game time to correct this, and also to be able to run simulation slower/faster than real time.
            TimeSpan elapsedSimulatorTime = TimeSpan.FromSeconds(doNotUseThisGameTime.ElapsedGameTime.TotalSeconds * SimulatorSpeed);
            _accumulatedSimulatorTime += elapsedSimulatorTime;


            var controlledGameTime = new GameTime(
                TimeSpan.FromSeconds(doNotUseThisGameTime.TotalRealTime.TotalSeconds * SimulatorSpeed),
                TimeSpan.FromSeconds(doNotUseThisGameTime.ElapsedRealTime.TotalSeconds * SimulatorSpeed),
                _accumulatedSimulatorTime,
                elapsedSimulatorTime,
                doNotUseThisGameTime.IsRunningSlowly);

            #endregion

            #region Handle keyboard input

            KeyboardState s = Keyboard.GetState();

            // Allows the game to exit)
            if (s.IsKeyDown(Keys.Escape))
            {
                Exit();
                return;
            }

            if (s.IsKeyDown(Keys.Space))
            {
                if (IsTestMode)
                {
                    HandleTestScenarioEnded(controlledGameTime, TestScenarioEndTrigger.UserSkipped);
                    if (_scenario == null)
                        return;
                }
                else 
                    Reset();
            }

            Vector3 defaultCameraPosition = _helicopter.Position - _helicopter.Axes.Forward;

            if (s.IsKeyDown(Keys.D1))
            {
                InitCamera(CameraType.Chase, defaultCameraPosition, _helicopter);
            }
            else if (s.IsKeyDown(Keys.D2))
            {
                InitCamera(CameraType.Cockpit, Vector3.Zero, _helicopter);
            }
            else if (s.IsKeyDown(Keys.D3))
            {
                InitCamera(CameraType.Fixed, defaultCameraPosition, _helicopter);
            }
            else if (s.IsKeyDown(Keys.D4))
            {
                InitCamera(CameraType.Free, defaultCameraPosition, _helicopter);
            }

            #endregion


            // Note This must be before base.Update() to ensure the autopilot uses the most recent value of this variable
            if (IsTestMode)
            {
                _helicopter.Autopilot.IsTruePositionWithinRadius =
                    _helicopter.Autopilot.Task.Current.IsWithinRadius(_helicopter.Position);
            }

            // TODO Only capture once per keypress, and not every loop when the key is pressed down
            // This has not proven to be a problem though, since the process takes a little while to compute
            _takeScreenshot = false;
            if (s.IsKeyDown(Keys.PrintScreen))
                _takeScreenshot = true;

            
            // Update all components of the simulator
            base.Update(controlledGameTime);

            if (_scenario == null)
            {
                Debug.WriteLine("Scenario is null so the last test scenario was probably just completed. Exiting Update() for application quit.");
                return;
            }

            // Update position of current waypoint (TODO use events to only update when it actually changes)
            Task task = _helicopter.Autopilot.Task;
            _currentWaypoint.Position = task.Current.Position;
            _currentWaypoint.Scale = Matrix.CreateScale(task.Current.Radius * 2);

            if (_isHelicopterCrashed)
            {
                HandleHelicopterCrash(controlledGameTime);
                return;
            }

#if !XBOX
            if (_isLoggingFlight)
                _liveFlightLogger.Add(_helicopter.Log.Last());
#endif

            if (IsTestMode)
            {
                UpdateIntermediaryTestResult();

                TimeSpan testScenarioDuration = controlledGameTime.TotalGameTime - _intermediaryTestResult.StartTime;

                // TODO Multiple helicopters
                if (_helicopter.Autopilot.IsAtDestination)
                {
                    HandleTestScenarioEnded(controlledGameTime, TestScenarioEndTrigger.ReachedDestination);
                }
                else if (_scenario.Timeout > TimeSpan.Zero && testScenarioDuration >= _scenario.Timeout)
                {
                    HandleTestScenarioEnded(controlledGameTime, TestScenarioEndTrigger.TimedOut);
                }
            }

            // Camera must be updated after the game objects to use the most recent
            // position when chasing or looking at objects
            // The free look camera keeps setting the mouse cursor to the centre of the centre for each update.
            // We want to disable this when ALT+TABbing out of the application even if it runs in the background.
#if !XBOX
            if (IsCapturingMouse)
                Camera.Update(controlledGameTime);
#else
            Camera.Update(controlledGameTime);
#endif

            // Use elastic follow camera as basic effect
            _basicEffect.View = Camera.View;
            _basicEffect.Projection = Camera.Projection;
        }

        private void HandleHelicopterCrash(GameTime gameTime)
        {
            if (IsTestMode)
            {
                HandleTestScenarioEnded(gameTime, TestScenarioEndTrigger.Crashed);
            }
            else
                Reset();

            _isHelicopterCrashed = false;
        }

        private void UpdateIntermediaryTestResult()
        {
            var currentLogEntry = _helicopter.Log.Last();
            PhysicalHeliState trueState = currentLogEntry.True;
            PhysicalHeliState estimatedState = currentLogEntry.Estimated;

            ScenarioIntermediaryTestResult i = _intermediaryTestResult;

            float estimationError = Vector3.Distance(estimatedState.Position, trueState.Position);
            float heightAboveGround = trueState.Position.Y - currentLogEntry.GroundAltitude;

            i.AccEstimatedPositionError += estimationError;
            i.AccHeightAboveGround += heightAboveGround;
            i.AccVelocity += trueState.Velocity.Length();
            i.MaxEstimatedPositionError = Math.Max(estimationError, i.MaxEstimatedPositionError);
            i.MinEstimatedPositionError = Math.Min(estimationError, i.MinEstimatedPositionError);
            i.MaxHeightAboveGround = Math.Max(heightAboveGround, i.MaxHeightAboveGround);
            i.MinHeightAboveGround = Math.Min(heightAboveGround, i.MinHeightAboveGround);
            i.MaxVelocity = Math.Max(trueState.Velocity.Length(), i.MaxVelocity);
            i.UpdateCount++;
            
            _intermediaryTestResult = i;
        }


        /// <summary>
        ///   This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">
        ///   Provides a snapshot of timing values.
        /// </param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice device = GraphicsDevice;

            // Render main screen either to screen or to screenshot buffer
            device.SetRenderTarget(0, (_takeScreenshot ? _screenshotTarget : null));


            _spriteBatch.Begin();

            switch (_settings.RenderMode)
            {
                case RenderModes.Normal:
                    DrawNormal(gameTime, GraphicsDevice, _spriteBatch);
                    break;

#if !XBOX
                case RenderModes.StereoCrossConverged:
                case RenderModes.Stereo:
                    DrawStereo(_settings.RenderMode, gameTime, GraphicsDevice, _spriteBatch);
                    break;
#endif


                default:
                    throw new NotImplementedException("Render mode not implemented");
            }

            _spriteBatch.End();

            // Render the camera HUD last to avoid skydome covering it
            Camera.DrawHUD(gameTime);

            // Restore render target to backbuffer
            device.SetRenderTarget(0, null);

            // Take screenshot if requested
            if (_takeScreenshot)
            {
                // TODO Screenshot for XBOX possible?
#if !XBOX
                if (_settings.RenderMode == RenderModes.Normal)
                {
                    string filename = GetScreenShotFileName();
                    _screenshotTarget.GetTexture().Save(filename, ImageFileFormat.Png);
                }
                else if (_settings.RenderMode == RenderModes.StereoCrossConverged)
                {
                    _screenshotTargetLeft.GetTexture().Save(GetScreenShotFileName("_left"), ImageFileFormat.Png);
                    _screenshotTargetRight.GetTexture().Save(GetScreenShotFileName("_right"), ImageFileFormat.Png);
                }
#endif

            }
        }

        private void DrawNormal(GameTime gameTime, GraphicsDevice device, SpriteBatch sb)
        {
            // Clear before next render pass
            device.Clear(ClearColor);

            // Render world
            base.Draw(gameTime);

            // Print FPS
            if (ShowFPS)
            {
                string fpsText = "FPS: " + _fpsUtil.Value;
                PrintInfo(sb, new Vector2(0.9f*_screenWidth, 0), fpsText);
            }

            PrintInfo(sb, new Vector2(0, 0), "Scenario: " + _scenario.Name);
        }


#if !XBOX
        private void DrawStereo(RenderModes mode, GameTime time, GraphicsDevice device, SpriteBatch sb)
        {
            // Find the X-vector in the camera view plane so that
            // a positive offset is towards the right eye and
            // a negative offset is towards the left.
            Vector3 originalPos = Camera.Position;
            Vector3 originalLookAt = Camera.LookAt;
            Vector3 cameraViewX = Vector3.Cross(Camera.LookAt - Camera.Position, Camera.Up);
            cameraViewX.Normalize();

            // Place eyes some cm apart
            Vector3 leftEyePosition = originalPos - cameraViewX*HalfEyeDistance;
            Vector3 rightEyePosition = originalPos + cameraViewX*HalfEyeDistance;

            if (_settings.SwapStereo)
            {
                Vector3 tmp = rightEyePosition;
                rightEyePosition = leftEyePosition;
                leftEyePosition = tmp;
            }

            // Render left eye
            DrawLeftEye(time, leftEyePosition, mode, device, sb);

            // Render right eye
            DrawRightEye(time, rightEyePosition, mode, device, sb);

            // Restore camera position and update its internal view matrices
            Camera.Position = originalPos;
            Camera.LookAt = originalLookAt;
            Camera.UpdateMatrices();


            // Special care must be taken to present the cross converged 
            // graphics (two sources) to a single backbuffer target
            if (mode == RenderModes.StereoCrossConverged)
                PresentCrossConverged(device, sb);
        }


        private void PresentCrossConverged(GraphicsDevice device, SpriteBatch sb)
        {
            // Restore output target to main backbuffer
            device.SetRenderTarget(0, null);

            device.Clear(ClearColor);

            var leftDestRect = new Rectangle(0, 0, _screenWidth/2, _screenHeight);
            var rightDestRect = new Rectangle(_screenWidth / 2, 0, _screenWidth / 2, _screenHeight);

            var leftSourceRect = new Rectangle(_screenWidth / 4, 0, _screenWidth/2, _screenHeight);
            var rightSourceRect = leftSourceRect;

            sb.Draw(_leftEyeRender.GetTexture(), leftDestRect, leftSourceRect, Color.White, 0, new Vector2(), SpriteEffects.None, 0);
            sb.Draw(_rightEyeRender.GetTexture(), rightDestRect, rightSourceRect, Color.White, 0, new Vector2(), SpriteEffects.None, 0);
        }

        private void DrawLeftEye(GameTime time, Vector3 eyePosition, RenderModes mode, GraphicsDevice device,
                                 SpriteBatch sb)
        {
            switch (mode)
            {
                case RenderModes.StereoCrossConverged:

                    // Render main screen either to screen or to screenshot buffer
                    device.SetRenderTarget(0, (_takeScreenshot ? _screenshotTargetLeft : _leftEyeRender));
                    break;
                case RenderModes.Stereo:
                    // Draw on the main window
                    device.Present();
                    break;
            }

            device.Clear(ClearColor);

            // Setup camera for left eye
            // Translate look at direction to new eye position
            const float focusDistance = 10.0f;
            Vector3 lookAtVector = Vector3.Normalize(Camera.LookAt - Camera.Position);
            Camera.LookAt = Camera.Position + focusDistance*lookAtVector;
            Camera.Position = eyePosition;
            Camera.UpdateMatrices();

            _basicEffect.View = Camera.View;


            base.Draw(time);

            // Print FPS
            if (ShowFPS)
            {
                string fpsText = "FPS: " + _fpsUtil.Value;
                PrintInfo(sb, new Vector2(0.9f*_screenWidth, 0), fpsText);
            }
        }

        private void DrawRightEye(GameTime time, Vector3 eyePosition, RenderModes mode, GraphicsDevice device,
                                  SpriteBatch sb)
        {
            switch (mode)
            {
                case RenderModes.StereoCrossConverged:
                    device.SetRenderTarget(0, (_takeScreenshot ? _screenshotTargetRight : _rightEyeRender));
                    break;
                case RenderModes.Stereo:
                    // Draw on the extra created window
                    device.Present(_stereoRightHandle);
                    break;
            }

            device.Clear(ClearColor);

            // Setup camera for right eye
            // Translate look at direction to new eye position
            const float focusDistance = 10.0f;
            Vector3 lookAtVector = Vector3.Normalize(Camera.LookAt - Camera.Position);
            Camera.LookAt = Camera.Position + focusDistance*lookAtVector;
            Camera.Position = eyePosition;
            Camera.UpdateMatrices();

            _basicEffect.View = Camera.View; 
            base.Draw(time);

            // Print FPS
            if (ShowFPS)
            {
                string fpsText = "FPS: " + _fpsUtil.Value;
                PrintInfo(sb, new Vector2(0.9f*_screenWidth, 0), fpsText);
            }
        }
#endif



        protected override void Dispose(bool disposing)
        {
            // Release allocated resources
            if (_leftEyeRender != null) _leftEyeRender.Dispose();
            if (_rightEyeRender != null) _rightEyeRender.Dispose();
            if (_screenshotTarget != null) _screenshotTarget.Dispose();
            if (_screenshotTargetLeft != null) _screenshotTargetLeft.Dispose();
            if (_screenshotTargetRight != null) _screenshotTargetRight.Dispose();

            _screenshotTarget = null;
            _screenshotTargetLeft = null;
            _screenshotTargetRight = null;
            _leftEyeRender = null;
            _rightEyeRender = null;

            base.Dispose(disposing);
        }

        private static string GetScreenShotFileName()
        {
            for (int i = 0; i < 100; i++)
            {
                string filename = "a2ds_" + String.Format("{0:000}", i) + ".png";
                if (!File.Exists(filename))
                    return filename;
            }

            // All names were taken, simply overwrite the first one..
            return "a2ds_001.png";
        }

        private static string GetScreenShotFileName(string postfix)
        {

            for (int i = 0; i < 100; i++)
            {
                string filename = "a2ds_" + String.Format("{0:000}", i) + postfix + ".png";
                if (!File.Exists(filename))
                    return filename;
            }

            // All names were taken, simply overwrite the first one..
            return "a2ds_001"+postfix+".png";
        }


        private static BasicEffect CreateBasicEffect(Game game)
        {
            // Use the world matrix to tilt the cube along x and y axes.
            Matrix worldMatrix = Matrix.Identity;
            Matrix viewMatrix = Matrix.CreateLookAt(new Vector3(0, 0.50f, 5.00f), Vector3.Zero, Vector3.Up);
            Matrix projectionMatrix = Matrix.CreatePerspectiveFieldOfView(
                MathHelper.ToRadians(45), // 45 degree angle
                (float) game.GraphicsDevice.Viewport.Width/game.GraphicsDevice.Viewport.Height,
                0.1f, 100.0f);

            var effect = new BasicEffect(game.GraphicsDevice, null)
                             {
                                 Alpha = 1.0f,
                                 DiffuseColor = new Vector3(1.0f),
                                 SpecularColor = new Vector3(0.25f),
                                 SpecularPower = 5.0f,
                                 AmbientLightColor = new Vector3(0.05f),
                                 DirectionalLight0 =
                                     {
                                         Direction = Vector3.Normalize(new Vector3(1.0f, -1.0f, -1.0f)),
                                         Enabled = true,
                                         DiffuseColor = Vector3.One,
                                         SpecularColor = Vector3.One,
                                     },
                                 DirectionalLight1 =
                                     {
                                         Enabled = true,
                                         DiffuseColor = new Vector3(0.5f),
                                         Direction = Vector3.Normalize(new Vector3(-1.0f, -1.0f, 1.0f)),
                                         SpecularColor = new Vector3(0.5f)
                                     },
                                 LightingEnabled = false,
                                 World = worldMatrix,
                                 View = viewMatrix,
                                 Projection = projectionMatrix,
                                 VertexColorEnabled = false,
                                 TextureEnabled = true
                             };

            return effect;
        }

        private static DepthFormat SelectStencilMode()
        {
            // Check stencil formats
            GraphicsAdapter adapter = GraphicsAdapter.DefaultAdapter;
            SurfaceFormat format = adapter.CurrentDisplayMode.Format;
            if (adapter.CheckDepthStencilMatch(DeviceType.Hardware, format, format, DepthFormat.Depth24Stencil8))
                return DepthFormat.Depth24Stencil8;
            if (adapter.CheckDepthStencilMatch(DeviceType.Hardware, format, format, DepthFormat.Depth24Stencil8Single))
                return DepthFormat.Depth24Stencil8Single;
            if (adapter.CheckDepthStencilMatch(DeviceType.Hardware, format, format, DepthFormat.Depth24Stencil4))
                return DepthFormat.Depth24Stencil4;
            if (adapter.CheckDepthStencilMatch(DeviceType.Hardware, format, format, DepthFormat.Depth15Stencil1))
                return DepthFormat.Depth15Stencil1;

            throw new InvalidOperationException("Could Not Find Stencil Buffer for Default Adapter");
        }

        private void PrintInfo(SpriteBatch batch, Vector2 pos, string text)
        {
            batch.DrawString(_hudInfo1Font, text, pos, Color.YellowGreen);
        }

        public void SetPIDSetup(PIDSetup pidSetup)
        {
            if (_helicopter != null) // && _helicopter.Autopilot != null && _helicopter.Autopilot.Output != null)
                _helicopter.Autopilot.Output.PIDSetup = pidSetup;
            else
                _initialPIDSetup = pidSetup;
        }

        #region Properties

        // We are capturing mouse by default
        private bool _isCapturingMouse = true;

        /// <summary>
        ///   Set to true to enable mouse control in the simulator, such as menus and camera.
        ///   It is often handy to disable this such as when ALT+TABbing out of the game, 
        ///   because the free look camera forces the mouse pointer to the centre of the screen.
        /// </summary>
        public bool IsCapturingMouse
        {
            get { return _isCapturingMouse; }
            set { _isCapturingMouse = value; }
        }

        #endregion
    }
}



