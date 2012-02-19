/* -*- indent-tabs-mode:T; c-basic-offset:8; tab-width:8; -*- vi: set ts=8:
 * $Id: imu-1d.c,v 2.0 2002/09/22 02:10:18 tramm Exp $
 *
 * (c) Aaron Kahn <Aaron.Kahn@itt.com>
 * (c) Trammell Hudson <hudson@rotomotion.com>
 *
 * One dimensional IMU to compute pitch angle based on the readings
 * from an ADXL202 and a pitch gyro.
 * 
 *  plot [200:1000] \
 *	"kalman.log" using 1 title "Actual state" with lines, \
 *	"" using 2 title "Estimated state" with lines, \
 *	"" using 3 title "Error" with lines
 *
 *************
 *
 *  This file is part of the autopilot simulation package.
 *
 *  Autopilot is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2 of the License, or
 *  (at your option) any later version.
 *
 *  Autopilot is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with Autopilot; if not, write to the Free Software
 *  Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
 *
 */

#include <stdio.h>
#include <stdlib.h>
#include <time.h>
#include <math.h>

#define		pi		  3.14159
const double	dt		= 1.0 / 30.0;	/* 30 Hz sensor update */
const double	omega		= 0.5;		/* Max roll rate */
const double	Q		= 0.0001;	/* Noise weighting matrix */
const double	max_angle	= 30*pi/180.0;	/* Maximum pitch angle */
static double	theta		= 4*pi/180;	/* Our initial state estimate */
static double	R		= 1;		/* Measurement error weight */
static double	P		= 1;		/* Covariance matrix */


double
kalman(
	double			t,		/* Time */
	double			q,		/* Pitching gyro */
	double			ax,		/* X acceleration */
	double			ay		/* Y acceleration */
)
{
	double			Pdot;		/* Derivative of P */
	double			E;		/* ? */
	double			K;		/* ? */

	double			theta_m;	/* Our state measurement */

						/* A = 0 */
	Pdot = Q;				/* Pdot = A*P + P*A' + Q */
	P += Pdot * dt;

	/* Update our state estimate from the rate gyro */
	theta += q * dt;

	/* We only run the Kalman filter at a slower 10 Hz */
	if( (int)( t * 100 ) % 10 != 0 )
		return theta;

	/* Compute our measured state from the accelerometers */
	theta_m = atan2( -ay, ax );

	E = P + R;				/* E = CPC' + R */
	K = P / E;				/* K = PC'inv(E) */

	/* Update the state */
	theta += K * ( theta_m - theta );

	/* Covariance update */
	P *= ( 1 - K );

	return theta;
}

static inline double
noise( void )
{
	return 2.0 * drand48() - 1.0;
}


int main( void )
{
	double			t;

	srand48( time(0) );

	for( t=0 ; t<60.0 ; t+=dt )
	{
		/*
		 * Compute our actual state as a function of time.
		 */
		double real_q		= max_angle * omega * cos(omega*t);
		double real_theta	= max_angle * sin(omega*t);

		/*
		 * Fake our sensor readings by adding a little bit of noise
		 * to the system.
		 */
		double ax =  cos( real_theta ) + noise() * 0.9;
		double ay = -sin( real_theta ) + noise() * 0.9;
		double q  = real_q + noise() * 8 * pi / 180.0;

		/*
		 * Compute our new estimated state with the Kalman filter
		 */
		double theta = kalman( t, q, ax, ay );

		printf( "%f %f %f\n",
			real_theta,
			theta,
			real_theta - theta
		);
	}

	return 0;
}
