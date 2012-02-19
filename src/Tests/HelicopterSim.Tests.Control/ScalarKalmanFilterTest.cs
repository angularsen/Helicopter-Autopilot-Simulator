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

using System.Collections.Generic;
using Control.KalmanFilter;
using NUnit.Framework;

#endregion

namespace HelicopterSim.Tests.Control
{
    [TestFixture]
    public class ScalarKalmanFilterTest
    {
        [Test]
        public void Test()
        {
            var filter = new ScalarKF(1, 0, 3, 0, 1);
            IList<StepOutput<float>> result =
                filter.Filter(new List<float>(new float[] {1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1}));
            Assert.That(result.Count > 0);
        }
    }
}