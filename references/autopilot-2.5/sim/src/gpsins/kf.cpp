/* -*- indent-tabs-mode:T; c-basic-offset:8; tab-width:8; -*- vi: set ts=8:
 * $Id: kf.cpp,v 2.0 2002/09/22 02:07:30 tramm Exp $
 *
 * (c) Trammell Hudson
 *
 * Translation of Aaron Kahn's GPS+INS code.
 *
 **************
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


#include <mat/Matrix.h>
#include <mat/Matrix_Invert.h>
#include <mat/Vector.h>
#include <mat/Quat.h>
#include <mat/Constants.h>
#include <mat/Conversions.h>
#include <cmath>
#include <cstdlib>

using namespace libmat;
using namespace std;

#include "sixdofe.h"
#include "propigate.h"
#include "gena.h"
#include "gps_update.h"
#include "compass_update.h"



/*
 *  Our actual states and Kalman filter variables
 */
static Matrix<11,11>	Q;
static Vector<11>	X_est;
static Vector<3>	XYZ_est;
static Vector<3>	UVW_est;
static Vector<4>	Q_est;
static double		G_est;

static Vector<12>	X_true;
static Vector<4>	inertia;
static double		m = 1.0;
static double		G = 32.2;

static Matrix<11,11>	P;


/*
 *  Routine to reset all the kalman filter variables back to
 * their initial states.
 */
static void
setup_kf( void )
{
	Q.fill( 0.0 );
	Q[ 0][ 0] = 0.0;
	Q[ 1][ 1] = 0.0;
	Q[ 2][ 2] = 0.0;

	Q[ 3][ 3] = 0.1;
	Q[ 4][ 4] = 0.1;
	Q[ 5][ 5] = 0.1;

	Q[ 6][ 6] = 0.00001;
	Q[ 7][ 7] = 0.00001;
	Q[ 8][ 8] = 0.00001;
	Q[ 9][ 9] = 0.00001;

	Q[10][10] = 0.00001;
	Q[11][11] = 0.001;

	// Our position estimate starts at the origin
	XYZ_est[0] = X_est[0] = 0.0;
	XYZ_est[1] = X_est[1] = 0.0;
	XYZ_est[2] = X_est[2] = 0.0;

	// Our velocity estimate starts at zero
	UVW_est[0] = X_est[3] = 0.0;
	UVW_est[1] = X_est[4] = 0.0;
	UVW_est[2] = X_est[5] = 0.0;

	// Our initial orientation is upright
	Q_est[0] = X_est[6] = 1.0;
	Q_est[1] = X_est[7] = 0.0;
	Q_est[2] = X_est[8] = 0.0;
	Q_est[3] = X_est[9] = 0.0;

	// Gravity starts as G.
	G_est = X_est[10] = 32.2;

	// Setup the covariance matrix
	P = eye<11,double>();

	// Zero our true position
	X_true.fill( 0.0 );

	inertia = Vector<4>(
		1.0,	// Ix,
		1.0,	// Iy,
		1.0,	// Iz,
		0.0	// Ixz
	);
}


static double
normalize(
	double		angle
)
{
	if( angle > pi && angle <= 2 * pi )
		return angle - 2 * pi;
	if( angle < -pi && angle > -2 * pi )
		return angle + 2 * pi;
	return angle;
}


/*
 *  Emulates the Matlab randn() call.  Returns a normally distributed
 * random vector of size n.
 */
template<
	const int	n
>
const Vector<n>
randn(
	double		sigma = 1.0
)
{
	Vector<n>	v;

	for( int i=0 ; i<n ; i++ )
		v[i] = 2.0 * sigma * drand48() - sigma;

	return v;
}


int
main( void )
{
	double		dt		= 0.01;
	const double	tf		= 90.0;
	const int	one_hz		= int(1.0 / dt);
	const int	five_hz		= int(0.2 / dt);
	int		step		= 0;

	setup_kf();

	for( double t = 0.0 ; t < tf ; t += dt, step++ )
	{
		// Simulate the truth
		const Vector<3>	true_euler( slice<6,3>( X_true ) );
		const Vector<3> weight( 0, 0, -G * m );
		const Vector<3> F( eulerDC( true_euler ) * weight );
		const Vector<3> next_F( F + Vector<3>(
			0.01,
			0.0 * cos( 0.3 * t ), 
			0.0 * sin( 0.3 * t )
		));

		const Vector<3> LMN(
			0.01 * sin( 0.3 * t ),
			0.01 * cos( 0.3 * t ),
			0.001
		);

		const Vector<12> Xtrue_dot( sixdofe(
			next_F,
			LMN,
			inertia,
			m,
			G,
			X_true
		) );


		// Update our true position
		X_true += Xtrue_dot * dt;
		X_true[6] = normalize( X_true[6] );
		X_true[7] = normalize( X_true[7] );
		X_true[8] = normalize( X_true[8] );


		// Add noise into true values to simulate the data
		const Vector<3> accel_true(
			slice<0,3>( Xtrue_dot )
			+ F
			+ eulerWx( slice<3,3>(X_true) ) * slice<0,3>(X_true)
			+ randn<3>( 2.5 )
		);

		const double DEG = 2 * C_DEG2RAD;

		const Vector<3> XYZ_true( slice<9,3>(X_true) + randn<3>() );
		const Vector<3> UVW_true( slice<0,3>(X_true) + randn<3>(0.01) );
		const Vector<3> PQR_true( slice<3,3>(X_true) + randn<3>(DEG) );
		const Vector<3> THETA_true( slice<6,3>(X_true) + randn<3>(2.5) );

		// Start the actual filter now
		Vector<11> Xest_dot;

		propigate_state(
			// Outputs
			X_est,
			Xest_dot,

			// Inputs
			XYZ_est,
			UVW_est,
			accel_true,
			PQR_true,
			Q_est,
			G_est,
			dt
		);

		XYZ_est = slice<0,3>( X_est );
		UVW_est = slice<3,3>( X_est );
		Q_est = slice<6,4>( X_est );
		Q_est.norm_self();
		G_est = X_est[10];


		// Generate the A matrix
		const Matrix<11,11> A( gena(
			UVW_est,
			PQR_true,
			Q_est,
			G_est
		) );


		// Propigate the covariance matrix P
		P += (A*P + P * A.transpose() + Q) * dt;

		// GPS update once per second
		if( step % one_hz == 0 )
		{
			gps_update(
				X_est,
				P,
				XYZ_true,
				UVW_true,
				XYZ_est,
				UVW_est,
				Q_est,
				G_est
			);

			XYZ_est = slice<0,3>( X_est );
			UVW_est = slice<3,3>( X_est );
			Q_est = slice<6,4>( X_est );
			Q_est.norm_self();
			G_est = X_est[10];
		}

		// Compass update five times per second
		if( step % five_hz == 0 )
		{
			compass_update(
				X_est,
				P,
				THETA_true[2],
				XYZ_est,
				UVW_est,
				Q_est,
				G_est
			);

			XYZ_est = slice<0,3>( X_est );
			UVW_est = slice<3,3>( X_est );
			Q_est = slice<6,4>( X_est );
			Q_est.norm_self();
			G_est = X_est[10];
		}

		cout
			<< XYZ_est[0] << " "
			<< XYZ_true[0] << " "
			<< X_true[9] << endl;
	}
}
