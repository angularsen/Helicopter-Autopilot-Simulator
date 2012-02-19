/* -*- indent-tabs-mode:T; c-basic-offset:8; tab-width:8; -*- vi: set ts=8:
 * $Id: old-nav_funcs.h,v 2.0 2002/09/22 02:07:32 tramm Exp $
 *
 * This is the navigation related functions for the math library.
 *
 *	Author: Aaron Kahn, Suresh Kannan, Eric Johnson
 *	copyright 2001
 *	Portions (c) Trammell Hudson
 */

#ifndef _NAV_FUNCS_H_
#define _NAV_FUNCS_H_

namespace matlib
{


/*
 * This will convert from ECEF to local tangent plane coordinates
 * local(x,y,z) = Re2t*[X;Y;Z]
 * latitude and longitude are in radians
 */
extern void
ECEF2Tangent(
	double *		Local,
	const double *		ECEF,
	const double		lattitude,
	const double		longitude
);


/*
 * This will convert from local tangent plane coordinates to ECEF
 * ECEF[X,Y,Z] = Rt2e*local(x,y,z)
 *
 * latitude and longitude are in radians
 */
extern void
Tangent2ECEF(
	double *		ECEF,
	const double *		Local,
	const double		latitude,
	const double		longitude
);


/*
 * This will convert from ECEF to geodedic latitude, longitude, altitude
 * based on the WGS-84 elipsoid.
 *
 *	latitude and longitude (rad), altitude (m)
 *	ECEF (m)
 *	ECEF = [X Y Z] (m)
 *	llh = [latitude longitude altitude] (rad)(m)
 */
extern void
ECEF2llh(
	const double *		ECEF,
	double *		llh
);


/*
 * This will convert from latitude, longitude, and altitude into
 * ECEF coordinates.  Note: altitude in + up
 *
 *	ECEF = [X Y Z] (m)
 *	llh = [latitude longitude altitude] (rad)(m)
 */
extern void
llh2ECEF(
	const double *		llh,
	double *		ECEF
);


/*
 * This will compute the current atmospheric properties given the
 * current altitude in feet above mean sea level (MSL).  The output
 * parameters are output with pointers with all of the useful information.
 *
 *	altitude	MSL altitude (ft) (NOTE: Only valid to 36,152 ft)
 *	density		density (slug/ft^3)
 *	pressure	pressure (lb/ft^2)
 *	temperature	temperature (degrees R)
 *	sp_sound	local speed of sound (ft/s)
 */
extern void
atmosphere(
	double			altitude,
	double *		density,
	double *		pressure,
	double *		temperature,
	double *		sp_sound
);

}

#endif
