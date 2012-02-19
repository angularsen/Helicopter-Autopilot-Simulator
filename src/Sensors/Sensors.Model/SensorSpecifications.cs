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

using Microsoft.Xna.Framework;
using System.Text;
using State.Model;

#endregion

namespace Sensors.Model
{
    public class SensorSpecifications
    {
        public ForwardRightUp AccelerometerStdDev;
        public float AccelerometerFrequency;

        public Vector3 GPSVelocityStdDev;
        public Vector3 GPSPositionStdDev;
        public float OrientationAngleNoiseStdDev;

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendFormat("Accelerometer {1} Hz std.dev: FRU({0}) \r\n", AccelerometerStdDev, AccelerometerFrequency);
            sb.AppendFormat("GPS position std.dev: XYZ({0}) \r\n", GPSPositionStdDev);
            sb.AppendFormat("GPS velocity std.dev: XYZ({0}) \r\n", GPSVelocityStdDev);
            sb.AppendFormat("Orientation angle noise std.dev: {0} \r\n", OrientationAngleNoiseStdDev);
            return sb.ToString();
        }
    }
}