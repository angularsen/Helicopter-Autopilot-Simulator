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
using System.Linq;
using Anj.Helpers.XNA;

#endregion

namespace Control.KalmanFilter
{
    public interface IKalmanFilter<T>
    {
        IList<StepOutput<T>> Filter(IList<T> samples);
    }


    /// <summary>
    /// A scalar Kalman Filter.
    /// </summary>
    public class ScalarKF : IKalmanFilter<float>
    {
        private readonly float _a;
        private readonly float _b;
        private readonly float _h;
        private readonly float _q;
        private readonly float _r;
        private readonly GaussianRandom _rand;
        private readonly float _stdDevV;
        private readonly float _stdDevW;

        public ScalarKF(float gainA, float gainB, float gainH, float covarianceQ, float covarianceR)
        {
            // Starting with example
            _a = gainA;
            _b = gainB;
            _h = gainH;

            // Noise co-variances
            _q = covarianceQ;
            _r = covarianceR;

            // Determine standard deviation of estimated process and observation noise variance
            _stdDevW = (float) Math.Sqrt(_q);
            _stdDevV = (float) Math.Sqrt(_r);

            _rand = new GaussianRandom();
        }

        #region IKalmanFilter<float> Members

        public IList<StepOutput<float>> Filter(IList<float> samples)
        {
            var result = new List<StepOutput<float>>();

            // Initial state condition
            const float x0 = 1.0f;
            const float postX0 = 1.5f;
            const float postP0 = 1.0f;

            StepOutput<float> first = InitializeFirstStep(x0, postX0, postP0);
            result.Add(first);

            for (int i = 1; i < samples.Count; i++)
            {
                StepOutput<float> prev = result.Last();
                result.Add(CalculateNext(new StepInput<float>(prev)));
            }

            return result;
        }

        #endregion

        /// <summary></summary>
        /// <param name="x0">Initial system state</param>
        /// <param name="postX0">
        ///   Initial guess of system state
        /// </param>
        /// <param name="postP0">
        ///   Initial guess of a posteriori covariance
        /// </param>
        /// <returns></returns>
        private StepOutput<float> InitializeFirstStep(float x0, float postX0, float postP0)
        {
            var startingCondition = new StepInput<float>(x0, postX0, postP0);
            return CalculateNext(startingCondition);
        }

        private StepOutput<float> CalculateNext(StepInput<float> prev)
        {
            float w = _rand.NextGaussian(0, _stdDevW);
            float v = _rand.NextGaussian(0, _stdDevV);

            // Calculate the state and the output
            float x = _a*prev.X + w;
            float z = _h*x + v;

            // Predictor equations
            float priX = _a*prev.PostX;
            float residual = z - _h*priX;
            float priP = _a*_a*prev.PostP + _q;

            // Corrector equations
            float k = _h*priP/(_h*_h*priP + _r);
            float postP = priP*(1 - _h*k);
            float postX = priX + k*residual;

            return new StepOutput<float>
                       {
                           K = k,
                           PriX = priX,
                           PriP = priP,
                           PostX = postX,
                           PostP = postP,
                           PostXError = postX - x,
                           PriXError = priX - x,
                           X = x,
                           Z = z,
                           W = w,
                           V = v,
                       };
        }
    }
}