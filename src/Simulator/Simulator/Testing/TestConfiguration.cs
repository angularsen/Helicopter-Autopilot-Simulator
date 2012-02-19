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
using Microsoft.Xna.Framework;
using Sensors.Model;
using Simulator.Scenarios;
using State.Model;
using Simulator.Resources;

#endregion

namespace Simulator.Testing
{
    public class TestConfiguration
    {
        public bool FlyBySensors;
        public bool UsePerfectSensors;
        public bool UseGPS;
        public bool UseINS;
        public bool UseRangeFinder;

        public SensorSpecifications Sensors;
        public readonly List<Scenario> TestScenarios;
        public List<float> MaxHVelocities;


        public static TestConfiguration Default
        {
            get
            {
                var result = new TestConfiguration();
                result.TestScenarios.Add(SimulatorResources.GetPreSelectedScenario());
                return result;
            }
        }

        public TestConfiguration()
        {
            FlyBySensors = false;
            UsePerfectSensors = false;
            UseGPS = true;
            UseINS = true;
            UseRangeFinder = true;

            TestScenarios = new List<Scenario>();

            MaxHVelocities = new List<float> {DefaultAutopilotConfiguration.DefaultMaxHVelocity};
            Sensors = new SensorSpecifications
                          {
                              AccelerometerStdDev = DefaultAutopilotConfiguration.AccelerometerStdDev,
                              AccelerometerFrequency = 60,
                              GPSPositionStdDev = DefaultAutopilotConfiguration.GPSPositionStdDev,
                              GPSVelocityStdDev = DefaultAutopilotConfiguration.GPSVelocityStdDev,
                              OrientationAngleNoiseStdDev = 0,
                              
                          };
        }
    }

    public static class DefaultAutopilotConfiguration
    {
        public const float DefaultMaxHVelocity = 10;
        public const float WaypointRadius = 5;


        // Refs: 
        // http://www.romdas.com/technical/gps/gps-acc.htm
        // http://www.sparkfun.com/datasheets/GPS/FV-M8_Spec.pdf
        // http://onlinestatbook.com/java/normalshade.html 

        // Assuming GPS Sensor: FV-M8
        // Cold start: 41s
        // Hot start: 1s
        // Position accuracy: 3.3m CEP (horizontal circle, half the points within this radius centred on truth)
        // Position accuracy (DGPS): 2.6m CEP
        // Velocity accuracy: 0.1 knot RMS steady state (std. dev. is ~0.0514 m/s)


        // GPS position observation noise by standard deviation [meters]
        // Assume gaussian distribution, 2.45 x CEP is approx. 2dRMS (95%)
        // ref: http://www.gmat.unsw.edu.au/snap/gps/gps_survey/chap2/243.htm

        /// <summary>
        /// Found empirically by http://onlinestatbook.com/java/normalshade.html
        /// using area=0.5 and limits +- 3.3 meters => std dev of 4.8926m
        /// since our GPS is modelled with CEP (50%) as 3.3m.
        /// </summary>
        public static Vector3 GPSPositionStdDev
        {
            // Length of StdDev vector should be 4.8926f, so std dev
            // at each axis is then sqrt(4.8926*4.8926/3).
            get { return new Vector3(2.824743927f); }
        }

        // 
        /// <summary>
        /// Found by GPS sensor spec for sensor FV-M8.
        /// </summary>
        public static Vector3 GPSVelocityStdDev
        {
            // Length of StdDev 3D vector should be 0.0514444444f, so std dev
            // at each axis is then sqrt(radiusStdDev^2/3).
            get
            {
                //                const float radiusStdDev = 0.0514444444f;
                const float radiusStdDev = 10 * 0.0514444444f;
                return new Vector3(radiusStdDev * radiusStdDev / 3);
            }
        }

        /// <summary>
        /// ADXL330 noise density specifications
        /// X,Y: 280 micro-g / sqrt(Hz)
        ///   Z: 350 micro-g / sqrt(Hz)
        /// 
        /// ADXL330 pdf specs give this forumula:
        /// rms noise = noise density x sqrt(bandwidth x 1.6)
        ///
        /// Bandwidth: 0-400Hz low pass filter, as suggested by DIY UAV Platform pdf.
        /// 
        /// This gives the following standard deviations of accelerometer noise.
        /// X,Y noise RMS = 280E-6 x sqrt(1.6x400) = 7.08E-3 [g]
        ///   Z noise RMS = 350E-6 x sqrt(1.6x400) = 8.85E-3 [g]
        /// </summary>
        public static ForwardRightUp AccelerometerStdDev
        {
            get
            {
                // According to ADXL330 specs XYZ are mapped to Forward, Right and Up respectively 
                // (given this physical mounting of course)
                return new ForwardRightUp
                {
                    //                               Forward = 7.08E-3f,
                    //                               Right = 7.08E-3f,
                    //                               Up = 8.85E-3f
                    Forward = 7.08E-1f,
                    Right = 7.08E-1f,
                    Up = 8.85E-1f
                };
            }
        }
    }
}