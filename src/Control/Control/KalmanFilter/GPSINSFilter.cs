#if !XBOX

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
using Microsoft.Xna.Framework;
using XNAMatrix = Microsoft.Xna.Framework.Matrix;

using Anj.Helpers.XNA;
using Sensors.Model;
using MathNet.Numerics.LinearAlgebra;
using Matrix = MathNet.Numerics.LinearAlgebra.Matrix;

#endregion

// ReSharper disable InconsistentNaming

namespace Control.KalmanFilter
{

    #region Filter structs

    public struct GPSINSInput
    {
        public Vector3 AccelerationWorld;
        public Quaternion Orientation;
//        public float RollRate;
//        public float PitchRate;
//        public float YawRate;
//        public float RollDelta;
//        public float PitchDelta;
//        public float YawDelta;
    }

    public struct GPSINSObservation
    {
        public Vector3 GPSPosition;
        public Vector3 GPSHVelocity;
        public bool GotGPSUpdate;
        public float RangeFinderHeightOverGround;
        //        public Vector3 Acceleration;
        public TimeSpan Time;
    }

    public struct GPSINSOutput
    {
        public Quaternion Orientation;
        public Vector3 Position;
        public Vector3 Velocity;
//        public float Pitch;
//        public float Roll;
//        public float Heading;
    }

    #endregion

    public class GPSINSFilter
    {
        #region Fields

        // Observable states (the number of states equals 'm')
        // Note: The order of the observation state fields needs to match the m first elements of the state variables in order for z = H*priX to work
        private const int ObsPositionX = 0;
        private const int ObsPositionY = 1;
        private const int ObsPositionZ = 2;
        private const int ObsVelocityX = 3;
        private const int ObsVelocityY = 4;
        private const int ObsVelocityZ = 5;
//        private const int ObsQuaternionX = 3;
//        private const int ObsQuaternionY = 4;
//        private const int ObsQuaternionZ = 5;
//        private const int ObsQuaternionW = 6;

        // State variables (the number of states equals 'n')
        private const int PositionX = 0;
        private const int PositionY = 1;
        private const int PositionZ = 2;
        private const int VelocityX = 3;
        private const int VelocityY = 4;
        private const int VelocityZ = 5;
        private const int QuaternionX = 6;
        private const int QuaternionY = 7;
        private const int QuaternionZ = 8;
        private const int QuaternionW = 9;

        /// <summary>
        /// Number of state variables. 
        /// 10 to represent position, velocity and orientation in 3D.
        /// </summary>
        private const int n = 10;

        /// <summary>
        /// Number of input variables. 
        /// </summary>
        private const int p = 7;

        /// <summary>
        /// Number of output variables (observed variables).
        /// </summary>
        private const int m = 6;

        /// <summary>
        /// H transposed
        /// </summary>
        private Matrix HT;

        /// <summary>
        /// State identity (n by n)
        /// </summary>
        private readonly Matrix I;

        /// <summary>
        /// Initial guessed error covariance (n by n)
        /// </summary>
        private readonly Matrix PostP0;

        /// <summary>
        /// Initial guessed state (n by 1)
        /// </summary>
        private readonly Matrix PostX0;

        /// <summary>
        /// Process noise covariance (n by n)
        /// </summary>
        private Matrix Q;

        /// <summary>
        /// Measurement noise covariance (m by m)
        /// </summary>
        private Matrix R;

        ///// <summary>
        ///// Input (p by 1)
        ///// </summary>
//        private Matrix u;

        /// <summary>
        /// Initial true state (n by 1)
        /// </summary>
        private readonly Matrix X0;

        private readonly Quaternion _orientation;
        private readonly GaussianRandom _rand;
        private readonly TimeSpan _startTime;
        private readonly Vector _stdDevV;
        private readonly Vector _stdDevW;
        private readonly SensorSpecifications _sensorSpecifications;

        /// <summary>
        /// State gain (n by n)
        /// </summary>
        private Matrix A;

        /// <summary>
        /// A transposed
        /// </summary>
        private Matrix AT;

        /// <summary>
        /// Input gain (n by p)
        /// </summary>
        private Matrix B;

        /// <summary>
        /// Output gain (m by n)
        /// </summary>
        private Matrix H;

        /// <summary>
        /// Result from the last iteration of this filter. Used in next iteration as a starting point.
        /// When this class is constructed this field is set to the initial state.
        /// </summary>
        private StepOutput<Matrix> _prevEstimate;

        /// <summary>
        /// Measurement noise (m by 1)
        /// </summary>
        private Matrix v;

        /// <summary>
        /// Process noise (n by 1)
        /// </summary>
        private Matrix w;

        #endregion

        #region Constructors

        public GPSINSFilter(TimeSpan startTime, Vector3 startPos, Vector3 startVelocity, Quaternion orientation,
                            SensorSpecifications sensorSpecifications)
        {
            _startTime = startTime;
            _orientation = orientation;
            _sensorSpecifications = sensorSpecifications;

            X0 = Matrix.Create(new double[n,1]
                                   {
                                       {startPos.X}, {startPos.Y}, {startPos.Z}, // Position
                                       {startVelocity.X}, {startVelocity.Y}, {startVelocity.Z}, // Velocity
                                       {_orientation.X}, {_orientation.Y}, {_orientation.Z}, {_orientation.W}, // Quaternion  
                                   });

            // Make sure we don't reference the same object, but rather copy its values.
            // It is possible to set PostX0 to a different state than X0, so that the initial guess
            // of state is wrong. 
            PostX0 = X0.Clone();


            // We use a very low initial estimate for error covariance, meaning the filter will
            // initially trust the model more and the sensors/observations less and gradually adjust on the way.
            // Note setting this to zero matrix will cause the filter to infinitely distrust all observations,
            // so always use a close-to-zero value instead.
            // Setting it to a diagnoal matrix of high values will cause the filter to trust the observations more in the beginning,
            // since we say that we think the current PostX0 estimate is unreliable.
            PostP0 = 0.001*Matrix.Identity(n, n);


            // Determine standard deviation of estimated process and observation noise variance
            // Process noise (acceleromters, gyros, etc..)
            _stdDevW = new Vector(new double[n]
                                      {
                                          _sensorSpecifications.AccelerometerStdDev.Forward,   
                                          _sensorSpecifications.AccelerometerStdDev.Right,
                                          _sensorSpecifications.AccelerometerStdDev.Up,
                                          0, 0, 0, 0, 0, 0, 0
                                      });

            // Observation noise (GPS inaccuarcy etc..)
            _stdDevV = new Vector(new double[m]
                                      {
                                          _sensorSpecifications.GPSPositionStdDev.X,
                                          _sensorSpecifications.GPSPositionStdDev.Y,
                                          _sensorSpecifications.GPSPositionStdDev.Z,
//                                          0.001000, 0.001000, 0.001000,
//                                          1000, 1000, 1000,
                                          _sensorSpecifications.GPSVelocityStdDev.X,
                                          _sensorSpecifications.GPSVelocityStdDev.Y,
                                          _sensorSpecifications.GPSVelocityStdDev.Z,
                                      });


            I = Matrix.Identity(n, n);
            
            _zeroMM = Matrix.Zeros(m);

            _rand = new GaussianRandom();
            _prevEstimate = GetInitialEstimate(X0, PostX0, PostP0);
        }

        #endregion

        #region Public methods

        public StepOutput<GPSINSOutput> Filter(GPSINSObservation obs, GPSINSInput input)
        {
            var inputFromPrev = new StepInput<Matrix>(_prevEstimate);
            StepOutput<Matrix> result = CalculateNext(inputFromPrev, obs, input);
            _prevEstimate = result;

            return ToFilterResult(result);
        }

        #endregion

        #region Private methods 

        private void UpdateTimeVaryingMatrices(TimeSpan timeDelta)
        {
            double dt = timeDelta.TotalSeconds;

            double posByV = dt; // p=vt
            double posByA = 0.5*dt*dt; // p=0.5at^2
            double velByA = dt; // v=at

            // World state transition matrix.
            // Update position and velocity from previous state.
            // Previous state acceleration is neglected since current acceleration only depends on current input.
            A = Matrix.Create(new double[n,n]
                                  {
                                      // Px Py Pz Vx Vy Vz Qx Qy Qz Qw
                                      {1, 0, 0, posByV, 0, 0, 0, 0, 0, 0}, // Px
                                      {0, 1, 0, 0, posByV, 0, 0, 0, 0, 0}, // Py
                                      {0, 0, 1, 0, 0, posByV, 0, 0, 0, 0}, // Pz
                                      {0, 0, 0, 1, 0, 0, 0, 0, 0, 0}, // Vx
                                      {0, 0, 0, 0, 1, 0, 0, 0, 0, 0}, // Vy
                                      {0, 0, 0, 0, 0, 1, 0, 0, 0, 0}, // Vz

                                      // We don't handle transition of quaternions here due to difficulties. Using B instead.
                                      {0, 0, 0, 0, 0, 0, 0, 0, 0, 0}, // Quaternion X 
                                      {0, 0, 0, 0, 0, 0, 0, 0, 0, 0}, // Quaternion Y
                                      {0, 0, 0, 0, 0, 0, 0, 0, 0, 0}, // Quaternion Z
                                      {0, 0, 0, 0, 0, 0, 0, 0, 0, 0}, // Quaternion W
                                  });
            AT = Matrix.Transpose(A);


            // Input gain matrix.
            // Acceleration forward/right/down
            // Angular Rate Roll/Pitch/Heading
            B = Matrix.Create(new double[n,p]
                                  {
                                      // Ax Ay Az Qx Qy Qz Qw
                                      {posByA, 0, 0, 0, 0, 0, 0}, // Px
                                      {0, posByA, 0, 0, 0, 0, 0}, // Py
                                      {0, 0, posByA, 0, 0, 0, 0}, // Pz
                                      {velByA, 0, 0, 0, 0, 0, 0}, // Vx
                                      {0, velByA, 0, 0, 0, 0, 0}, // Vy
                                      {0, 0, velByA, 0, 0, 0, 0}, // Vz

                                      // Simply set new orientation directly by quaternion input
                                      {0, 0, 0, 1, 0, 0, 0}, // Quaternion X  
                                      {0, 0, 0, 0, 1, 0, 0}, // Quaternion Y
                                      {0, 0, 0, 0, 0, 1, 0}, // Quaternion Z
                                      {0, 0, 0, 0, 0, 0, 1}, // Quaternion W
                                  });


            // TODO For simplicity we assume all acceleromter axes have identical standard deviations (although they don't)
            float accelStdDev = _sensorSpecifications.AccelerometerStdDev.Forward;
            float velocityStdDev = ((float)velByA) * accelStdDev;
            float positionStdDev = ((float)posByA) * accelStdDev;

            // Diagonal matrix with noise std dev
            Q = Matrix.Diagonal(new Vector(new double[n]
                                               {
                                                   positionStdDev, positionStdDev, positionStdDev,
                                                   velocityStdDev, velocityStdDev, velocityStdDev,
                                                   0, 0, 0, 0   // TODO Orientation has no noise, should be added later
                                               }));

            R = Matrix.Diagonal(_stdDevV);  // Diagonal matrix with noise std dev
            Q *= Matrix.Transpose(Q);       // Convert standard deviations to variances
            R *= Matrix.Transpose(R);       // Convert standard deviations to variances

            w = GetNoiseMatrix(_stdDevW, n);
            v = GetNoiseMatrix(_stdDevV, m);
        }

        /// <summary>
        /// Transform sensor measurements to output matrix
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        private static Matrix ToObservationMatrix(GPSINSObservation s)
        {
            return Matrix.Create(new double[m,1]
                                     {
                                         {s.GPSPosition.X},
                                         {s.GPSPosition.Y},
                                         {s.GPSPosition.Z},
                                         {s.GPSHVelocity.X},
                                         {s.GPSHVelocity.Y},
                                         {s.GPSHVelocity.Z},
                                     });
        }

        private static GPSINSOutput EstimateMatrixToFilterResult(Matrix x)
        {
            Vector stateVector = x.GetColumnVector(0);

            return new GPSINSOutput
                       {
                           Position = new Vector3(
                               (float) stateVector[PositionX],
                               (float) stateVector[PositionY],
                               (float) stateVector[PositionZ]),
                           Velocity = new Vector3(
                               (float) stateVector[VelocityX],
                               (float) stateVector[VelocityY],
                               (float) stateVector[VelocityZ]),
                           Orientation = GetOrientation(x),
                       };
        }

        private static GPSINSOutput ObservationMatrixToFilterResult(Matrix z)
        {
            Vector observationVector = z.GetColumnVector(0);
            return new GPSINSOutput
                       {
                           Position =
                               new Vector3((float) observationVector[ObsPositionX],
                                           (float) observationVector[ObsPositionY],
                                           (float) observationVector[ObsPositionZ]),
                       };
        }

        /// <summary></summary>
        /// <param name="x0">Initial system state</param>
        /// <param name="postX0">Initial guess of system state</param>
        /// <param name="postP0">Initial guess of a posteriori covariance</param>
        /// <returns></returns>
        private StepOutput<Matrix> GetInitialEstimate(Matrix x0, Matrix postX0, Matrix postP0)
        {
            var startingCondition = new StepInput<Matrix>(x0, postX0, postP0);

            // TODO Is it ok to use an observation for the initial state in this manner?
            var observation = new GPSINSObservation
                                  {
                                      GPSPosition = GetPosition(x0),
                                      Time = _startTime
                                  };
            return CalculateNext(startingCondition, observation, new GPSINSInput());
        }

        private static Vector3 GetPosition(Matrix x)
        {
            return new Vector3((float) x[PositionX, 0], (float) x[PositionX, 0], (float) x[PositionX, 0]);
        }


        private static StepOutput<GPSINSOutput> ToFilterResult(StepOutput<Matrix> step)
        {
            return new StepOutput<GPSINSOutput>
                       {
                           X = EstimateMatrixToFilterResult(step.X),
                           PriX = EstimateMatrixToFilterResult(step.PriX),
                           PostX = EstimateMatrixToFilterResult(step.PostX),
                           Z = ObservationMatrixToFilterResult(step.Z),
                           Time = step.Time,
                       };
        }

        private StepOutput<Matrix> CalculateNext(StepInput<Matrix> prev, GPSINSObservation observation,
                                                 GPSINSInput input)
        {
            TimeSpan time = observation.Time;
            TimeSpan timeDelta = time - _prevEstimate.Time;

            // Note: Use 0.0 and not 0 in order to use the correct constructor!
            // If no GPS update is available, then use a zero matrix to ignore the values in the observation.
            H = observation.GotGPSUpdate
                    ? Matrix.Identity(m, n)
                    : new Matrix(m, n, 0.0);
            HT = Matrix.Transpose(H);

            UpdateTimeVaryingMatrices(timeDelta);

            Matrix u = ToInputMatrix(input, time);

            // Ref equations: http://en.wikipedia.org/wiki/Kalman_filter#The_Kalman_filter
            // Calculate the state and the output
            Matrix x = A*prev.X + B*u; // +w; noise is already modelled in input data
            Matrix z = ToObservationMatrix(observation); //H * x + v;// m by 1

            // Predictor equations
            Matrix PriX = A*prev.PostX + B*u; // n by 1
            Matrix PriP = A*prev.PostP*AT + Q; // n by n
            Matrix residual = z - H*PriX; // m by 1
            Matrix residualP = H*PriP*HT + R; // m by m

            // If residualP is zero matrix then set its inverse to zero as well.
            // This occurs if all observation standard deviations are zero
            // and this workaround will cause the Kalman gain to trust the process model entirely.
            Matrix residualPInv = Matrix.AlmostEqual(residualP, _zeroMM) // m by m
                ? _zeroMM
                : residualP.Inverse(); 

            // Corrector equations
            Matrix K = PriP*Matrix.Transpose(H)*residualPInv; // n by m
            Matrix PostX = PriX + K*residual; // n by 1
            Matrix PostP = (I - K*H)*PriP; // n by n



            var tmpPosition = new Vector3((float)PostX[0, 0], (float)PostX[1, 0], (float)PostX[2, 0]);
//            var tmpPrevPosition = new Vector3((float)prev.PostX[0, 0], (float)prev.PostX[1, 0], (float)prev.PostX[2, 0]);
//            Vector3 positionChange = tmpPosition - tmpPrevPosition;
            Vector3 gpsPositionTrust = new Vector3((float)K[0, 0], (float)K[1, 1], (float)K[2, 2]);
            Vector3 gpsVelocityTrust = new Vector3((float)K[3, 3], (float)K[4, 4], (float)K[5, 5]);
//
//            Matrix tmpPriPGain = A*Matrix.Identity(10,10)*AT + Q;

            var tmpVelocity = new Vector3((float)x[3, 0], (float)x[4, 0], (float)x[5, 0]);
            var tmpAccel = new Vector3((float)x[6, 0], (float)x[7, 0], (float)x[8, 0]);

            return new StepOutput<Matrix>
                       {
                           K = K,
                           PriX = PriX,
                           PriP = PriP,
                           PostX = PostX,
                           PostP = PostP,
                           PostXError = PostX - x,
                           PriXError = PriX - x,
                           X = x,
                           Z = z,
                           W = w,
                           V = v,
                           Time = time,
                       };
        }


        private static Quaternion GetOrientation(Matrix state)
        {
            Vector stateVector = state.GetColumnVector(0);

            return new Quaternion(
                (float) stateVector[QuaternionX],
                (float) stateVector[QuaternionY],
                (float) stateVector[QuaternionZ],
                (float) stateVector[QuaternionW]);
        }

        private static readonly GaussianRandom _gaussRand = new GaussianRandom();
        private static TimeSpan _prevTimeOrientationNoiseGenerated = TimeSpan.Zero;
        private static Quaternion _noisyRotation = Quaternion.Identity;
        private readonly Matrix _zeroMM;

        private Matrix ToInputMatrix(GPSINSInput input, TimeSpan time)
        {
            Vector3 worldAcceleration = input.AccelerationWorld;

            var q = input.Orientation;
            var inputOrientation = new Quaternion(q.X, q.Y, q.Z, q.W);

            // Periodically set an orientation error that will increase the estimation error to 
            // make up for the missing orientation filtering. Perfect orientation hinders estimation error from accumulating significantly.
            // TODO Use simulation time instead
            if (time - _prevTimeOrientationNoiseGenerated > TimeSpan.FromSeconds(0.5f))
            {
                _prevTimeOrientationNoiseGenerated = time;

                float angleStdDev = MathHelper.ToRadians(_sensorSpecifications.OrientationAngleNoiseStdDev);
                _noisyRotation = Quaternion.CreateFromYawPitchRoll(_gaussRand.NextGaussian(0, angleStdDev),
                                                                   _gaussRand.NextGaussian(0, angleStdDev),
                                                                   _gaussRand.NextGaussian(0, angleStdDev));
            }

            var noisyOrientation = inputOrientation*_noisyRotation;

            return Matrix.Create(new double[p,1]
                                     {
                                         {worldAcceleration.X},
                                         {worldAcceleration.Y},
                                         {worldAcceleration.Z},
//                                         {input.Orientation.X},
//                                         {input.Orientation.Y},
//                                         {input.Orientation.Z},
//                                         {input.Orientation.W},
                                         {noisyOrientation.X},
                                         {noisyOrientation.Y},
                                         {noisyOrientation.Z},
                                         {noisyOrientation.W},
                                     });
        }

        private Matrix GetNoiseMatrix(Vector stdDev, int rows)
        {
            var matrixData = new double[rows,1];

            for (int r = 0; r < rows; r++)
                matrixData[r, 0] = _rand.NextGaussian(0, stdDev[r]);

            return Matrix.Create(matrixData);
        }

        #endregion

    }
}

// ReSharper restore InconsistentNaming

#endif