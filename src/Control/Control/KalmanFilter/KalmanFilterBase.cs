#region Copyright

// A²DS - Autonomous Aerial Drone Simulator
// http://anjdreas.spaces.live.com/
//  
// A Master of Science thesis on autonomous flight at the 
// Norwegian University of Science and Technology (NTNU).
//  
// Copyright © 2009-2010 by Andreas Larsen.  All rights reserved.

#endregion

namespace Control.KalmanFilter
{
    //    public abstract class KalmanFilterBase<T> : IScalarKF<T>
    //    {
    //        private readonly T _a;
    //        private readonly T _b;
    //        private readonly T _h;
    //        private readonly T _q;
    //        private readonly T _r;
    //        private readonly GaussianRandom _rand;
    //        private readonly T _stdDevV;
    //        private readonly T _stdDevW;
    //
    //        public abstract IList<StepOutput<T>> Filter(IList<T> samples);
    //        protected abstract T GetStandardDeviation(T covarianceVector);
    //
    //        protected KalmanFilterBase(T gainA, T gainB, T gainH, T covarianceQ, T covarianceR, T stdDevW, T stdDevV)
    //        {
    // Starting with example
    //            _a = gainA;
    //            _b = gainB;
    //            _h = gainH;
    //
    // Noise co-variances
    //            _q = covarianceQ;
    //            _r = covarianceR;
    //
    // Determine standard deviation of estimated process and observation noise variance
    //            _stdDevW = stdDevW; // (T)Math.Sqrt(_q);
    //            _stdDevV = stdDevV; // (T)Math.Sqrt(_r);
    //
    //            _rand = new GaussianRandom();
    //        }
    //
    //        private StepOutput<T> CalculateNext(StepInput<T> prev)
    //        {
    //            float w = _rand.NextGaussian(0, _stdDevW);
    //            float v = _rand.NextGaussian(0, _stdDevV);
    //
    // Calculate the state and the output
    //            float x = _a * prev.X + w;
    //            float z = _h * x + v;
    //
    // Predictor equations
    //            float priX = _a * prev.PostX;
    //            float residual = z - _h * priX;
    //            float priP = _a * _a * prev.PostP + _q;
    //
    // Corrector equations
    //            float k = _h * priP / (_h * _h * priP + _r);
    //            float postP = priP * (1 - _h * k);
    //            float postX = priX + k * residual;
    //
    //            return new StepOutput<T>
    //            {
    //                K = k,
    //                PriX = priX,
    //                PriP = priP,
    //                PostX = postX,
    //                PostP = postP,
    //                PostXError = postX - x,
    //                PriXError = priX - x,
    //                X = x,
    //                Z = z,
    //                W = w,
    //                V = v,
    //            };
    //        }
    //    }
}