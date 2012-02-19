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
using Anj.Helpers.XNA;
using MathNet.Numerics.LinearAlgebra;
using Microsoft.Xna.Framework;
using Matrix = MathNet.Numerics.LinearAlgebra.Matrix;

#endregion

// ReSharper disable InconsistentNaming

namespace Control.KalmanFilter
{
    public struct GPSObservation
    {
        public Vector3 Position;
        //        public Vector3 Velocity;
        //        public Vector3 Acceleration;
        public TimeSpan Time;
    }

    public struct GPSFilter2DSample
    {
        public Vector3 Position;
    }

    public class GPSFilter2D //: IKalmanFilter<GPSObservation>
    {
        private const int PositionX = 0;
        private const int PositionY = 1;
        private const int PositionZ = 2;
        private const int VelocityX = 3;
        private const int VelocityY = 4;
        private const int VelocityZ = 5;
        private const int AccelerationX = 6;
        private const int AccelerationY = 7;
        private const int AccelerationZ = 8;

        /// <summary>
        /// Number of state variables. Typically 9 for position, velocity and acceleration variables in 3D.
        /// </summary>
        private const int n = 9;

        /// <summary>
        /// Number of input variables. 
        /// </summary>
        private const int p = 6;

        /// <summary>
        /// Number of output variables (observed variables).
        /// </summary>
        private const int m = 3;

        private const float ObservationNoiseStdDevY = 10.0f;
        //        private const int Time = 9;

        /// <summary>
        /// Output gain (m by n)
        /// </summary>
        private readonly Matrix H;

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
        private readonly Matrix Q;

        /// <summary>
        /// Measurement noise covariance (m by m)
        /// </summary>
        private readonly Matrix R;

        ///// <summary>
        ///// Input (p by 1)
        ///// </summary>
//        private Matrix u;

        /// <summary>
        /// Initial true state (n by 1)
        /// </summary>
        private readonly Matrix X0;

        private readonly GaussianRandom _rand;
        private readonly Vector _stdDevV;
        private readonly Vector _stdDevW;

        /// <summary>
        /// State gain (n by n)
        /// </summary>
        private Matrix A;

        /// <summary>
        /// Input gain (n by p)
        /// </summary>
        private Matrix B;

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


        public GPSFilter2D(GPSObservation startState)
        {
            X0 = Matrix.Create(new double[n,1]
                                   {
                                       {startState.Position.X}, {startState.Position.Y}, {startState.Position.Z},
                                       // Position
                                       {0}, {0}, {0}, // Velocity
                                       {0}, {0}, {0}, // Acceleration
                                   });

            PostX0 = X0.Clone();
            /* Matrix.Create(new double[n, 1]
                                   {
                                       {startState.Position.X}, {startState.Position.Y}, {startState.Position.Z}, // Position
                                       {1}, {0}, {0}, // Velocity
                                       {0}, {1}, {0}, // Acceleration
                                   });*/

            // Start by assuming no covariance between states, meaning position, velocity, acceleration and their three XYZ components
            // have no correlation and behave independently. This not entirely true.
            PostP0 = Matrix.Identity(n, n);


            // Refs: 
            // http://www.romdas.com/technical/gps/gps-acc.htm
            // http://www.sparkfun.com/datasheets/GPS/FV-M8_Spec.pdf
            // http://onlinestatbook.com/java/normalshade.html 

            // Assuming GPS Sensor: FV-M8
            // Cold start: 41s
            // Hot start: 1s
            // Position precision: 3.3m CEP (horizontal circle, half the points within this radius centred on truth)
            // Position precision (DGPS): 2.6m CEP


            //            const float coVarQ = ;
            //            _r = covarianceR;

            // Determine standard deviation of estimated process and observation noise variance
            // Position process noise
            _stdDevW = Vector.Zeros(n); //(float)Math.Sqrt(_q);

            _rand = new GaussianRandom();

            // Circle Error Probable (50% of the values are within this radius)
            //            const float cep = 3.3f;

            // GPS position observation noise by standard deviation [meters]
            // Assume gaussian distribution, 2.45 x CEP is approx. 2dRMS (95%)
            // ref: http://www.gmat.unsw.edu.au/snap/gps/gps_survey/chap2/243.htm

            // Found empirically by http://onlinestatbook.com/java/normalshade.html
            // using area=0.5 and limits +- 3.3 meters
            _stdDevV = new Vector(new double[m]
                                      {
                                          0,
                                          ObservationNoiseStdDevY,
                                          0,
//                                             0,
//                                             0,
//                                             0, 
//                                             0, 
//                                             0, 
//                                             0, 
//                                             0,
                                      });
            //Vector.Zeros(Observations);
            //2, 2, 4.8926f);


            H = Matrix.Identity(m, n);
            I = Matrix.Identity(n, n);
            Q = new Matrix(n, n);
            R = Matrix.Identity(m, m);


            _prevEstimate = GetInitialEstimate(X0, PostX0, PostP0);
        }


        private void UpdateMatrices(TimeSpan timeTotal, TimeSpan timeDelta)
        {
            double t = timeTotal.TotalSeconds;
            double dt = timeDelta.TotalSeconds;

            double posByV = dt; // p=vt
            double posByA = 0.5*dt*dt; // p=0.5at^2
            double velByA = t; // v=at

            // World state transition matrix
            A = Matrix.Create(new double[n,n]
                                  {
                                      {1, 0, 0, posByV, 0, 0, posByA, 0, 0}, // Px
                                      {0, 1, 0, 0, posByV, 0, 0, posByA, 0}, // Py
                                      {0, 0, 1, 0, 0, posByV, 0, 0, posByA}, // Pz
                                      {0, 0, 0, 1, 0, 0, velByA, 0, 0}, // Vx
                                      {0, 0, 0, 0, 1, 0, 0, velByA, 0}, // Vy
                                      {0, 0, 0, 0, 0, 1, 0, 0, velByA}, // Vz
                                      {0, 0, 0, 0, 0, 0, 0, 0, 0}, // Ax
                                      {0, 0, 0, 0, 0, 0, 0, 0, 0}, // Ay
                                      {0, 0, 0, 0, 0, 0, 0, 0, 0}, // Az
                                  });

            B = Matrix.Create(new double[n,p]
                                  {
                                      {posByV, 0, 0, posByA, 0, 0}, // Px
                                      {0, posByV, 0, 0, posByA, 0}, // Py
                                      {0, 0, posByV, 0, 0, posByA}, // Pz
                                      {1, 0, 0, velByA, 0, 0}, // Vx
                                      {0, 1, 0, 0, velByA, 0}, // Vy
                                      {0, 0, 1, 0, 0, velByA}, // Vz
                                      {0, 0, 0, 1, 0, 0}, // Ax
                                      {0, 0, 0, 0, 1, 0}, // Ay
                                      {0, 0, 0, 0, 0, 1}, // Az
                                  });

//            u = Matrix.Create(new double[p, 1]
//            {
//                {0},    // Px
//                {0},    // Py
//                {0},    // Pz
//                {0},    // Vx
//                {0},    // Vy
//                {0},    // Vz
//                {0},    // Ax
//                {0},    // Ay
//                {0},    // Az
//            });

            w = GetNoiseMatrix(_stdDevW, n);
            v = GetNoiseMatrix(_stdDevV, m);
        }

        /// <summary>
        /// Transform sensor measurements to output matrix
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        private static Matrix ToOutputMatrix(GPSObservation s)
        {
            return Matrix.Create(new double[m,1]
                                     {
                                         {s.Position.X},
                                         {s.Position.Y},
                                         {s.Position.Z},
//                                          {0}, {0}, {0},    // Velocity
//                                          {0}, {0}, {0},    // Acceleration
//                                          {s.Time.TotalSeconds},
                                     });
        }

//
        private static GPSFilter2DSample EstimateMatrixToFilterResult(Matrix x)
        {
            Vector stateVector = x.GetColumnVector(0);
            return new GPSFilter2DSample
                       {
                           Position =
                               new Vector3((float) stateVector[PositionX], (float) stateVector[PositionY],
                                           (float) stateVector[PositionZ]),
//                                                      Velocity = new Vector3((float)stateVector[VelocityX], (float)stateVector[VelocityY], (float)stateVector[VelocityZ]),
//                                                      Acceleration = new Vector3((float)stateVector[AccelerationX], (float)stateVector[AccelerationY], (float)stateVector[AccelerationZ]),
//                                                      Time = TimeSpan.FromSeconds(stateVector[Time]),
                       };
        }

        #region Implementation of IKalmanFilter<Matrix>

        /// <summary></summary>
        /// <param name="x0">Initial system state</param>
        /// <param name="postX0">
        ///   Initial guess of system state
        /// </param>
        /// <param name="postP0">
        ///   Initial guess of a posteriori covariance
        /// </param>
        /// <returns></returns>
        private StepOutput<Matrix> GetInitialEstimate(Matrix x0, Matrix postX0, Matrix postP0)
        {
            var startingCondition = new StepInput<Matrix>(x0, postX0, postP0);

            // TODO Is it ok to use an observation for the initial state in this manner?
            var observation = new GPSObservation
                                  {
                                      Position = GetPosition(x0),
                                      Time = TimeSpan.Zero,
                                      //GetTime(x0)
                                  };
            return CalculateNext(startingCondition, observation, new GPSFilter2DInput());
        }

        //        private static TimeSpan GetTime(Matrix x)
        //        {
        //            return TimeSpan.FromSeconds(x[Time, 0]);
        //        }

        private static Vector3 GetPosition(Matrix x)
        {
            return new Vector3((float) x[PositionX, 0], (float) x[PositionX, 0], (float) x[PositionX, 0]);
        }

        public StepOutput<GPSFilter2DSample> Filter(GPSObservation obs, GPSFilter2DInput input)
        {
            var inputFromPrev = new StepInput<Matrix>(_prevEstimate);
            StepOutput<Matrix> result = CalculateNext(inputFromPrev, obs, input);
            _prevEstimate = result;

            return ToFilterResult(result);
        }

        private static StepOutput<GPSFilter2DSample> ToFilterResult(StepOutput<Matrix> step)
        {
            return new StepOutput<GPSFilter2DSample>
                       {
                           X = new GPSFilter2DSample {Position = EstimateMatrixToFilterResult(step.X).Position,},
                           PriX = new GPSFilter2DSample {Position = EstimateMatrixToFilterResult(step.PriX).Position,},
                           PostX = new GPSFilter2DSample {Position = EstimateMatrixToFilterResult(step.PostX).Position,},
                           Z = new GPSFilter2DSample {Position = EstimateMatrixToFilterResult(step.Z).Position,},
                           Time = step.Time,
                       };
        }

        // Observations are never used.
//        public IEnumerable<StepOutput<GPSFilter2DSample>> Filter(IEnumerable<GPSObservation> samples, IEnumerable<GPSFilter2DInput> inputs)
//        {
//            return samples.Select(s => Filter(s, i));
//        }

        private StepOutput<Matrix> CalculateNext(StepInput<Matrix> prev, GPSObservation observation,
                                                 GPSFilter2DInput input)
        {
            TimeSpan time = observation.Time;
            TimeSpan timeDelta = time - _prevEstimate.Time;

            UpdateMatrices(time, timeDelta);

            Matrix u = ToInputMatrix(input);

            // Ref equations: http://en.wikipedia.org/wiki/Kalman_filter#The_Kalman_filter
            // Calculate the state and the output
            Matrix x = A*prev.X + B*u + w;
            Matrix z = H*x + v; //ToOutputMatrix(observation) + v;//H * x + v;

            // Predictor equations
            Matrix PriX = A*prev.PostX + B*u; // n by 1
            Matrix AT = Matrix.Transpose(A);
            Matrix PriP = A*prev.PostP*AT + Q; // n by n
            Matrix residual = z - H*PriX;
            Matrix residualP = H*PriP*Matrix.Transpose(H) + R;
            Matrix residualPInv = residualP.Inverse();

            // Corrector equations
            Matrix K = PriP*Matrix.Transpose(H)*residualPInv; // n by m

            // TODO Temp, experimenting with skipping measurements
            //            compileerror
            //            k[PositionY, PositionY] = 1.0;
            //            k[VelocityY, VelocityY] = 1.0;
            //            k[AccelerationY, AccelerationY] = 1.0;


            Matrix PostX = PriX + K*residual; // n by 1
            Matrix PostP = (I - K*H)*PriP; // n by n


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

        private static Matrix ToInputMatrix(GPSFilter2DInput input)
        {
            return Matrix.Create(new double[p,1]
                                     {
//                {0},    // Px
//                {0},    // Py
//                {0},    // Pz
                                         {input.Velocity.X}, // Vx
                                         {input.Velocity.Y}, // Vy
                                         {input.Velocity.Z}, // Vz
                                         {input.Acceleration.X}, // Ax
                                         {input.Acceleration.Y}, // Ay
                                         {input.Acceleration.Z}, // Az
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

    public struct GPSFilter2DInput
    {
        public Vector3 Acceleration;
        public Vector3 Velocity;
    }

    // ReSharper restore InconsistentNaming
}
#endif