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
using Control.Common;
using Microsoft.Xna.Framework;
using Sensors.Model;
using State.Model;
using Anj.Helpers.XNA;

#endregion

namespace Sensors
{
    public class SonicRangeFinder : SensorBase
    {
        private readonly NavigationMap _map;

        /// <summary>
        ///   Direction of range finding relative to the vehicle's orientation. 
        ///   Vector3.Forward means the direction the nose points in and Vector3.Down 
        ///   would typically point towards the ground.
        /// </summary>
        private readonly Vector3 _relativeRangeDirection;

        public SonicRangeFinder(SensorSpecifications sensorSpecifications, bool isPerfect, NavigationMap map)
            : base(isPerfect)
        {
            _map = map;
            _relativeRangeDirection = Vector3.Down;
        }

        /// <summary>
        /// If we assume a flat ground then this is the height above the ground.
        /// Obviously in most cases, in particular when tilting the sensor towards 90 degrees, this is not true but at least provides a rough estimate.
        /// </summary>
        public float FlatGroundHeight { get; private set; }

        #region Implementation of ISensor

        public override void Update(PhysicalHeliState startState, PhysicalHeliState endState, JoystickOutput output, TimeSpan startTime, TimeSpan endTime)
        {
            // Note: The sonic range finder is concerned with measuring at the new position in the timestep,
            // so always use endState here.
            PhysicalHeliState stateToMeasure = endState;
            Vector3 worldDirection = Vector3.Transform(_relativeRangeDirection, stateToMeasure.Orientation);
            worldDirection.Normalize();

            // Line equation, how long must line be to reach ground at y=y0 at given angle? 
            // y = ax + b 
            // y0 = worldDirection.Y * u + state.Position.Y =>
            // u = (y0-state.Position.Y)/worldDirection.Y
            Vector2 mapPosition = VectorHelper.ToHorizontal(stateToMeasure.Position);
            float y0 = _map.GetAltitude(mapPosition);

            if (stateToMeasure.Position.Y <= y0)
                FlatGroundHeight = stateToMeasure.Position.Y - y0;   // This will only be the case if we can fly through the terrain.. usually this should never happen
            else
            {
                const float maxRange = 10f;
                const float minDelta = 0.001f;    // Search resolution, returns distance when searching further does not improve the distance accuracy more than this
                float rayDistanceToGround = BinarySearchTerrainRayIntersection(0, maxRange, minDelta, stateToMeasure.Position, worldDirection);
                if (float.IsNaN(rayDistanceToGround))
                    FlatGroundHeight = float.NaN;
                else
                {
                    // A vector from the current position to the ground
                    Vector3 rangeVectorToGround = worldDirection*rayDistanceToGround;

                    // Invert vector and take its Y component to give flat ground height
                    FlatGroundHeight = Vector3.Negate(rangeVectorToGround).Y;
                }
            }
        }

        /// <summary>
        /// Finds distance from helicopter to ground along its down vector (sensor mounted to point down).
        /// This is implemented by a binary search and a minimum accuracy parameter, minDelta.
        /// </summary>
        /// <param name="minRange"></param>
        /// <param name="maxRange"></param>
        /// <param name="maxError"></param>
        /// <param name="position"></param>
        /// <param name="worldDirection"></param>
        /// <returns></returns>
        private float BinarySearchTerrainRayIntersection(float minRange, float maxRange, float maxError, Vector3 position, Vector3 worldDirection)
        {
            if (MathHelper.Distance(minRange, maxRange) <= maxError)
                return (minRange + maxRange)/2;

            float halfRange = minRange + (maxRange - minRange)/2;
            Vector3 rayStartPosition = position + minRange*worldDirection;
            Vector3 rayHalfPosition = position + halfRange * worldDirection;
            Vector3 rayEndPosition = position + maxRange * worldDirection;
            float groundAltitudeAtRayStart = _map.GetAltitude(VectorHelper.ToHorizontal(rayStartPosition));
            float groundAltitudeAtRayHalf = _map.GetAltitude(VectorHelper.ToHorizontal(rayHalfPosition));
            float groundAltitudeAtRayEnd = _map.GetAltitude(VectorHelper.ToHorizontal(rayEndPosition));

            float rayStartHeightAboveGround = rayStartPosition.Y - groundAltitudeAtRayStart;
            float rayHalfHeightAboveGround = rayHalfPosition.Y - groundAltitudeAtRayHalf;
            float rayEndHeightAboveGround = rayEndPosition.Y - groundAltitudeAtRayEnd;

            if (rayStartHeightAboveGround > 0 && rayHalfHeightAboveGround <= 0)
                return BinarySearchTerrainRayIntersection(minRange, halfRange, maxError, position, worldDirection);
            
            if (rayHalfHeightAboveGround > 0 && rayEndHeightAboveGround <= 0)
                return BinarySearchTerrainRayIntersection(halfRange, maxRange, maxError, position, worldDirection);

            return float.NaN;
        }

//        float BinarySearchTerrainRayIntersection(float minRange, float maxRange, float minDelta, Vector3 position, Vector3 worldDirection)
//        {
//            if (distance(minRange, maxRange) <= minDelta)
//                return (minRange + maxRange) / 2;
//
//            float halfRange = minRange + (maxRange - minRange) / 2;
//            Vector3 rayStartPosition = position + minRange * worldDirection;
//            Vector3 rayHalfPosition = position + halfRange * worldDirection;
//            Vector3 rayEndPosition = position + maxRange * worldDirection;
//            float startAltitude = GetAltitude(rayStartPosition);
//            float halfPointAltitude = GetAltitude(rayHalfPosition);
//            float endAltitude = GetAltitude(rayEndPosition);
//
            // Height Above Ground
//            float startHAG = rayStartPosition.Y - startAltitude;
//            float halfPointHAG = rayHalfPosition.Y - halfPointAltitude;
//            float endHAG = rayEndPosition.Y - endAltitude;
//
//            if (startHAG > 0 && halfPointHAG <= 0)
//                return BinarySearchTerrainRayIntersection(minRange, halfRange, minDelta, position, worldDirection);
//
//            if (halfPointHAG > 0 && endHAG <= 0)
//                return BinarySearchTerrainRayIntersection(halfRange, maxRange, minDelta, position, worldDirection);
//
//            return float.NaN;
//        }

        #endregion
    }
}