/* -*- indent-tabs-mode:T; c-basic-offset:8; tab-width:8; -*- vi: set ts=8:
 * $Id: old-nav_funcs.cpp,v 2.0 2002/09/22 02:07:32 tramm Exp $
 *
 * This is the navigation related functions for the math library.
 *
 *	Author: Aaron Kahn, Suresh Kannan, Eric Johnson
 *	copyright 2001
 *	Portions (c) Trammell Hudson
 */

#include "old-nav_funcs.h"
#include "old-matlib.h"
#include <mat/Conversions.h>
#include "macros.h"
#include <cmath>

namespace matlib
{

using std::sin;
using std::cos;
using std::atan;
using std::atan2;

/*
 * This will convert from ECEF to local tangent plane coordinates
 * local(x,y,z) = Re2t*[X;Y;Z]
 * latitude and longitude are in radians
 */
void
ECEF2Tangent(
	double *		Local,
	const double *		ECEF,
	const double		latitude,
	const double		longitude
)
{
	double			clat = cos(latitude);
	double			clon = cos(longitude);
	double			slat = sin(latitude);
	double			slon = sin(longitude);

	double			Re2t[MAXSIZE][MAXSIZE] = {
		{ -slat*clon,	-slat*slon,	 clat },
		{ -slon,	clon,		 0.0  },
 		{ -clat*clon,	-clat*slon,	-slat },
	};

	MVmult( Re2t, ECEF, Local, 3, 3 );
}


/*
 * This will convert from local tangent plane coordinates to ECEF
 * ECEF[X,Y,Z] = Rt2e*local(x,y,z)
 *
 * latitude and longitude are in radians
 */
void
Tangent2ECEF(
	double *		ECEF,
	const double *		Local,
	const double		latitude,
	const double		longitude
)
{
	double			clat = cos(latitude);
	double			clon = cos(longitude);
	double			slat = sin(latitude);
	double			slon = sin(longitude);

	double			Rt2e[MAXSIZE][MAXSIZE] = {
	 	{ -slat*clon,	-slon,		-clat*clon },
	 	{ -slat*slon, 	 clon,		-clat*slon },
	 	{  clat,	0.0,		-slat },
	};

	MVmult( Rt2e, Local, ECEF, 3, 3 );
}


/*
 * This will convert from ECEF to geodedic latitude, longitude, altitude
 * based on the WGS-84 elipsoid.
 *
 *	latitude and longitude (rad), altitude (m)
 *	ECEF (m)
 *	ECEF = [X Y Z] (m)
 *	llh = [latitude longitude altitude] (rad)(m)
 */
void
ECEF2llh(
	const double *		ECEF,
	double *		llh
)
{
	double			X = ECEF[0];
	double			Y = ECEF[1];
	double			Z = ECEF[2];

	double			f = (C_WGS84_a-C_WGS84_b) / C_WGS84_a;
	double			e = sqrt(2*f - f*f);

	double			h = 0;
	double			N = C_WGS84_a;

	llh[1] = atan2(Y, X);	// longitude

	for( int n=0 ; n<50 ; ++n )
	{
		double			sin_lat = Z / (N*(1 - sqr(e)) + h);

		llh[0] = atan(
			(Z + e*e * N * sin_lat) / sqrt(X*X + Y*Y)
		);

		N = C_WGS84_a / sqrt( 1 - sqr(e)*sqr(sin(llh[0])) );
		h = sqrt(X*X + Y*Y) / cos( llh[0] ) - N;
	}

	llh[2] = h;
}


/*
 * This will convert from latitude, longitude, and altitude into
 * ECEF coordinates.  Note: altitude in + up
 *
 *	ECEF = [X Y Z] (m)
 *	llh = [latitude longitude altitude] (rad)(m)
 */
void
llh2ECEF(
	const double *		llh,
	double *		ECEF
)
{
	double			f = (C_WGS84_a - C_WGS84_b) / C_WGS84_a;
	double			e = sqrt( 2*f - f*f );
	double			N = C_WGS84_a/sqrt( 1 - e*e*sqr(sin(llh[0])));

	ECEF[0] = (N + llh[2]) * cos(llh[0]) * cos(llh[1]);
	ECEF[1] = (N + llh[2]) * cos(llh[0]) * sin(llh[1]);
	ECEF[2] = (N*(1-e*e) + llh[2]) * sin(llh[0]);
}


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
void
atmosphere(
	double			altitude,
	double *		density,
	double *		pressure,
	double *		temperature,
	double *		sp_sound
)
{
	double			temp;
	double			temp1;
	double			temp2;

	// compute density first
	temp = (1 - (0.68753 - 0.003264*altitude*1e-5)*altitude*1e-5);
	*density = pow( temp, 4.256)*0.0023769;

	// compute pressure
	temp = (1-(0.68753 - 0.003264*altitude*1e-5)*altitude*1e-5);
	temp1 = pow( temp, 4.256)*0.0023769;
	temp2 = 1716.5*(1 - 0.687532*altitude*1e-5 + 0.003298*sqr(altitude*1e-5))*518.69;
	*pressure = temp1*temp2;

	// compute temp
	*temperature = (1 - 0.687532*altitude*1e-5 + 0.003298*sqr(altitude*1e-5))*518.69;

	// compute speed of sound
	*sp_sound = 1116.45*sqrt( 1 - 0.687532*altitude*1e-5 + 0.003298*sqr(altitude*1e-5));
}			


}
