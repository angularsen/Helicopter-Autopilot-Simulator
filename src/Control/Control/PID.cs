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

#endregion

namespace Control
{
    /// <summary>
    /// PID configuration.
    /// P - Proportional factor. More difference in a value compared to its goal gives more output.
    /// I - Integral factor. If the difference holds over time, the output can be increased to hasten change.
    /// D - Derivative factor. If the difference is rapidly nearing towards the goal, the output can be decreased 
    ///                        for a smoother transition to the goal.
    /// 
    /// The three factors are relative, so they must be proportional to each other.
    /// 
    /// A proportional–integral–derivative controller (PID controller) is a generic control loop feedback mechanism (controller) 
    /// widely used in industrial control systems. A PID controller attempts to correct the error between a measured process variable 
    /// and a desired setpoint by calculating and then outputting a corrective action that can adjust the process accordingly 
    /// and rapidly, to keep the error minimal.
    /// 
    /// Reference: http://en.wikipedia.org/wiki/PID_controller
    /// </summary>
    public class PID
    {
        #region Fields

        /// <summary>
        /// The D coefficient as 1/D
        /// </summary>
        private float _d;

        private float _differentialError;

        /// <summary>
        /// The I coefficient as 1/I
        /// </summary>
        private float _i;

        private float _integralError;

        /// <summary>
        /// The P coefficient as 1/P
        /// </summary>
        private float _p;

        private ErrorSample _prevError;

        #endregion

        #region Constructors

        /// <summary>
        /// Construct a PID with parameters p, i and d.
        /// </summary>
        /// <param name="p"></param>
        /// <param name="i"></param>
        /// <param name="d"></param>
        public PID(float p, float i, float d)
        {
            Name = "Unnamed";
            P = p;
            I = i;
            D = d;
            _prevError = new ErrorSample(float.NaN, 0);
        }

        /// <summary>
        /// Construct a PID with zero coefficients.
        /// </summary>
        public PID() : this(0, 0, 0)
        {
        }

        #endregion

        #region Internal structures

        internal struct ErrorSample
        {
            public long TotalTicks;
            public float Value;

            public ErrorSample(float value, long totalTicks)
            {
                Value = value;
                TotalTicks = totalTicks;
            }
        }

        #endregion

        #region Properties

        /// <summary>
        /// Name of PID configuration
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Proportional error correction factor. For simplicity the internal value is implicitly set as: 1/value.
        /// </summary>
        public float P
        {
            // Avoid division by zero.
            get { return (_p == 0) ? 0 : 1/_p; }
            set { _p = (value == 0) ? 0 : 1/value; }
        }


        /// <summary>
        /// Integral error correction factor. For simplicity the internal value is implicitly set as: 1/value.
        /// </summary>
        public float I
        {
            // Avoid division by zero.
            get { return (_i == 0) ? 0 : 1/_i; }
            set { _i = (value == 0) ? 0 : 1/value; }
        }


        /// <summary>
        /// Differential error correction factor. For simplicity the internal value is implicitly set as: 1/value.
        /// </summary>
        public float D
        {
            // Avoid division by zero.
            get { return (_d == 0) ? 0 : 1/_d; }
            set { _d = (value == 0) ? 0 : 1/value; }
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Returns a PID output ranging from -1.0 to 1.0.
        /// </summary>
        /// <param name="proportionalError"></param>
        /// <param name="integralError"></param>
        /// <param name="differentialError"></param>
        /// <returns>Value between -1.0 and 1.0</returns>
        public float ComputeExplicit(float proportionalError, float integralError, float differentialError)
        {
            float pidSum = proportionalError*_p + integralError*_i + differentialError*_d;
            return MathHelper.Clamp(pidSum, -1, 1);
        }

        /// <summary>
        /// Returns a PID output ranging from 0.0 to 1.0.
        /// </summary>
        /// <param name="proportionalError"></param>
        /// <param name="integralError"></param>
        /// <param name="differentialError"></param>
        /// <returns>Value between 0.0 and 1.0</returns>
        public float ComputeExplicitPositive(float proportionalError, float integralError, float differentialError)
        {
            float pidSum = proportionalError*_p + integralError*_i + differentialError*_d;
            return MathHelper.Clamp(pidSum/2.0f + 0.5f, 0, 1);
        }

        /// <summary>
        /// Returns a PID-controller output ranging from 0.0 to 1.0.
        /// The integrated and differential errors are managed internally and implicitly included in the calculations.
        /// To omit P, I or D simply set their coefficients to 0 in the constructor.
        /// </summary>
        /// <param name="error"></param>
        /// <param name="totalTicks"></param>
        /// <returns></returns>
        public float ComputePositive(float error, long totalTicks)
        {
            UpdateDifferentialAndIntegralErrors(error, totalTicks);
            return ComputeExplicitPositive(error, _integralError, _differentialError);
        }

        /// <summary>
        /// Returns a PID-controller output ranging from -1.0 to 1.0.
        /// The integrated and differential errors are managed internally and implicitly included in the calculations.
        /// To omit P, I or D simply set their coefficients to 0 in the constructor.
        /// </summary>
        /// <param name="error"></param>
        /// <param name="totalTicks"></param>
        /// <returns></returns>
        public float Compute(float error, long totalTicks)
        {
            UpdateDifferentialAndIntegralErrors(error, totalTicks);
            return ComputeExplicit(error, _integralError, _differentialError);
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Call this method (only once) before each call to compute methods. 
        /// Note that this method should only be called for each main loop cycle, so make sure to not run Compute() more than once in the loop for each PID.
        /// </summary>
        /// <param name="error"></param>
        /// <param name="totalTicks">TimeSpan ticks (100ns).</param>
        private void UpdateDifferentialAndIntegralErrors(float error, long totalTicks)
        {
            long ticksDelta = totalTicks - _prevError.TotalTicks;

            // Avoid division by zero
            if (ticksDelta > 0)
            {
                var elapsedSeconds = (float) TimeSpan.FromTicks(ticksDelta).TotalSeconds;
                if (float.IsNaN(_prevError.Value)) _prevError.Value = error;

                _differentialError = (error - _prevError.Value)/elapsedSeconds;
                _integralError += (error*elapsedSeconds);
            }

            // Remember error for next timestep
            _prevError = new ErrorSample(error, totalTicks);
        }

        #endregion

        #region Overrides

        public override string ToString()
        {
            return Name;
        }

        #endregion

        /// <summary>
        /// Returns a new PID with equivalent P, I and D values, but without the state of the PID.
        /// </summary>
        /// <returns></returns>
        public PID Clone()
        {
            return new PID(P, I, D);
        }
    }
}