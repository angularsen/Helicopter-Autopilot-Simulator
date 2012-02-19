/* -*- indent-tabs-mode:T; c-basic-offset:8; tab-width:8; -*- vi: set ts=8:
 *  $Id: GPSINS.cpp,v 2.0 2002/09/22 02:07:30 tramm Exp $
 *
 * (c) Trammell Hudson
 * (c) Aaron Kahn
 *
 * GPS aided INS object.
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
#include <gpsins/GPSINS.h>

#include <mat/Quat.h>
#include <mat/Vector.h>
#include <mat/Matrix.h>
#include <mat/Matrix_Invert.h>
#include <mat/Nav.h>

#include "macros.h"

namespace gpsins
{

using namespace libmat;
using namespace util;

GPSINS::GPSINS(
) :
	imu_samples(0),
	compass_samples(0),
	gps_samples(0),
	Q( 0.0 ),
	P( eye<11,double>() ),
	compass_sd( 0.2 ),
	position_sd( 0.4 ),
	velocity_sd( 0.1 )
	
{
	// Initial state estimate is level with gravity down
	// Position XYZ
	this->X[ X_index ]	= 0;
	this->X[ Y_index ]	= 0;
	this->X[ Z_index ]	= 0;

	// Velocity UVW
	this->X[ U_index ]	= 0;
	this->X[ V_index ]	= 0;
	this->X[ W_index ]	= 0;

	// Quaternion attitude
	this->X[ Q0_index ]	= 1;
	this->X[ Q1_index ]	= 0;
	this->X[ Q2_index ]	= 0;
	this->X[ Q3_index ]	= 0;

	// Gravity estimate
	this->X[ G_index ]	= 9.78;

	// Process noise matrix for some sensors
	Q[3][3] = 0.1;
	Q[4][4] = 0.1;
	Q[5][5] = 0.1;

	Q[6][6] = 0.00001;
	Q[7][7] = 0.00001;
	Q[8][8] = 0.00001;
	Q[9][9] = 0.00001;

	Q[10][10] = 0.001;
}


void
GPSINS::propagate_state(
	const Vector<3> &	accel,
	const Vector<3> &	pqr,
	double			dt
)
{
	const Vector<3>		uvw( this->uvw() );
	Vector<4>		q( this->q() );

	const Matrix<4,4>	Wxq( quatW( pqr ) );
	const Matrix<3,3>	Wx( eulerWx( pqr ) );
	const Matrix<3,3>	dcm( quatDC( q ) );

	const Vector<3>		XYZdot( dcm.transpose() * uvw );
	const Vector<3>		UVWdot(
		accel + dcm * Vector<3>(0,0,this->g()) - Wx * uvw
	);
	const Vector<4>		Qdot( Wxq * q );

	// Update our state vector with the new values
	this->X[ X_index ]	+= XYZdot[0] * dt;
	this->X[ Y_index ]	+= XYZdot[1] * dt;
	this->X[ Z_index ]	+= XYZdot[2] * dt;

	this->X[ U_index ]	+= UVWdot[0] * dt;
	this->X[ V_index ]	+= UVWdot[1] * dt;
	this->X[ W_index ]	+= UVWdot[2] * dt;

	// Normalize our attitude estimate before proceding
	q += Qdot * dt;
	q.norm_self();

	insert( this->X, Q0_index, q );
}
	

void
GPSINS::propagate_covariance(
	const Vector<3> &	pqr,
	double			dt
)
{
	Matrix<11,11>		A;

	this->make_a_matrix( A, pqr );

	const Matrix<11,11>	Pdot(
		this->Q + A * this->P + this->P * A.transpose()
	);

	this->P += Pdot * dt;
}


/**
 *  Hand translated from Aaron's matlab code.
 * I'm not at all clear on what this does...
 */
void
GPSINS::make_a_matrix(
	Matrix<11,11> &		A,
	const Vector<3> &	pqr
)
{
	const Vector<3>		uvw( this->uvw() );
	const double		u( uvw[0] );
	const double		v( uvw[1] );
	const double		w( uvw[2] );

	const Vector<4>		q( this->q() );
	const double		q0( q[0] );
	const double		q1( q[1] );
	const double		q2( q[2] );
	const double		q3( q[3] );

	const double		g( this->g() );

	const Matrix<3,3>	C( quatDC( q ) );
	const Matrix<4,4>	Wxq( quatW( pqr ) );
	const Matrix<3,3>	Wx( eulerWx( pqr ) );
	
	A.fill( 0.0 );

	A.insert(   C, 0, 3 );
	A.insert( -Wx, 3, 3 );
	A.insert( Wxq, 6, 6 );

	A[0][6] =         - 2*v*q3 + 2*w*q2;
	A[0][7] =           2*v*q2 + 2*w*q3;
	A[0][8] = -4*u*q2 + 2*v*q1 + 2*w*q0;
	A[0][9] = -4*u*q3 - 2*v*q0 + 2*w*q1;

	A[1][6] = 2*u*q3          - 2*w*q1;
	A[1][7] = 2*u*q2 - 4*v*q1 - 2*w*q0;
	A[1][8] = 2*u*q1          + 2*w*q3;
	A[1][9] = 2*u*q0 - 4*v*q3 + 2*w*q2;

	A[2][6] = -2*u*q2 + 2*v*q1;
	A[2][7] =  2*u*q3 + 2*v*q0 - 4*w*q1;
	A[2][8] = -2*u*q0 + 2*v*q3 - 4*w*q2;
	A[2][9] =  2*u*q1 + 2*v*q2;

	A[3][6] = -2*g*q2;
	A[3][7] =  2*g*q3;
	A[3][8] = -2*g*q0;
	A[3][9] =  2*g*q1;

	A[4][6] =  2*g*q1;
	A[4][7] =  2*g*q0;
	A[4][8] =  2*g*q3;
	A[4][9] =  2*g*q2;

	A[5][6] = 0;
	A[5][7] = -4*g*q1;
	A[5][8] = -4*g*q2;
	A[5][9] = 0;

	A[3][10] = C[0][2];
	A[4][10] = C[1][2];
	A[5][10] = C[2][2];
}


void
GPSINS::imu_init(
	const Vector<3> &	accel,
	const Vector<3> &	UNUSED( pqr )
)
{
	const Vector<4>		q( euler2quat( accel2euler( accel, 0 ) ) );
	insert( this->X, Q0_index, q );
}


void
GPSINS::compass_init(
	double			heading
)
{
	Vector<3>		euler( quat2euler( this->q() ) );
	euler[2] = heading;
	
	insert( this->X, Q0_index, euler2quat( euler ) );
}

void
GPSINS::gps_init(
	const Vector<3> &	xyz,
	const Vector<3> &	uvw
)
{
	this->X[ X_index ] = xyz[0];
	this->X[ Y_index ] = xyz[1];
	this->X[ Z_index ] = xyz[2];

	this->X[ U_index ] = uvw[0];
	this->X[ V_index ] = uvw[1];
	this->X[ W_index ] = uvw[2];
}


void
GPSINS::imu_update(
	const Vector<3> &	accel,
	const Vector<3> &	pqr,
	double			dt
)
{
	if( this->imu_samples++ == 0 )
	{
		this->imu_init( accel, pqr );
		return;
	}

	this->propagate_state(
		accel,
		pqr,
		dt
	);

	this->propagate_covariance(
		pqr,
		dt
	);
}


void
GPSINS::gps_update(
	const Vector<3> &	ned,
	const Vector<3> &	uvw,
	double			UNUSED( dt )
)
{
	if( this->gps_samples++ == 0 )
	{
		this->gps_init( ned, uvw );
		return;
	}

	Matrix<6,6>		R;
	const double		pos_sd = sqr( this->position_sd );
	const double		vel_sd = sqr( this->velocity_sd );

	R[0][0] = pos_sd;
	R[1][1] = pos_sd;
	R[2][2] = pos_sd;
	R[3][3] = vel_sd;
	R[4][4] = vel_sd;
	R[5][5] = vel_sd;

	Matrix<6,11>		C( 0.0 );
	C[0][0] = 1;
	C[1][1] = 1;
	C[2][2] = 1;
	C[3][3] = 1;
	C[4][4] = 1;
	C[5][5] = 1;

	const Matrix<11,6>	C_transpose( C.transpose() );

	const Matrix<6,6>	E( R + mult3( C, P, C_transpose ) );
	const Matrix<11,6>	K( mult3( P, C_transpose, invert( E ) ) );

	Vector<6>		measured;
	measured[0] = ned[0];
	measured[1] = ned[1];
	measured[2] = ned[2];
	measured[3] = uvw[0];
	measured[4] = uvw[1];
	measured[5] = uvw[2];
	
	// Update our state
	this->X += K * ( measured - C * this->X );

	// Update the covariance matrix
	this->P -= mult3( K, C, P );
}


void
GPSINS::compass_update(
	const double		heading,
	double			UNUSED( dt )
)
{
	if( this->compass_samples++ == 0 )
	{
		this->compass_init( heading );
		return;
	}

	const Vector<4>		q( this->q() );

	Matrix<1,1>		R;
	R[0][0] = sqr( this->compass_sd );

	// Build our estimate matrix
	Matrix<1,11>		C( 0.0 );
	const Vector<4>		dpsi( dpsi_dq( q ) );

	for( int i=0; i<4 ; i++ )
		C[0][6+i] = dpsi[i];

	// Compute the E matrix
	const Matrix<11,1>	C_transpose( C.transpose() );
	const Matrix<1,1>	E( R + mult3( C, this->P, C_transpose ) );

	// Compute the Kalman gain
	const Matrix<11,1>	K( mult3( this->P, C_transpose, invert( E ) ) );

	// Update the state vector
	const Vector<3>		eul( quat2euler( q ) );

	this->X += (K * ( heading - eul[2] ) ).col(0);

	// Update the covariance matrix
	this->P -= mult3( K, C, P );
}

}

