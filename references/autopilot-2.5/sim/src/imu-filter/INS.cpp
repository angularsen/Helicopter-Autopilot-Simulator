/* -*- indent-tabs-mode:T; c-basic-offset:8; tab-width:8; -*- vi: set ts=8:
 *  $Id: INS.cpp,v 1.16 2003/03/15 05:55:07 tramm Exp $
 *
 * (c) Aaron Kahn
 * (c) Trammell Hudson
 *
 * GPS aided INS object.
 *
 * Converted from Aaron's matlab code to use the C++ math library.
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

#include <INS.h>

#include <mat/Vector.h>
#include <mat/Matrix.h>
#include <mat/Matrix_Invert.h>
#include <mat/Quat.h>
#include <mat/Nav.h>
#include <mat/Conversions.h>
#include <mat/Kalman.h>

#include <iostream>
#include <fstream>
#include <cmath>
#include <cstdio>
#include <vector>
#include <string>


#include "macros.h"

namespace imufilter
{

using namespace util;
using namespace libmat;
using namespace std;



void
INS::make_a_matrix(
	Matrix<N,N> &		A,
	const Vector<3> &	uvw,
	const Vector<3> &	UNUSED( pqr ),
	const Matrix<3,3> &	DCM,
	const Matrix<4,4> &	Wxq,
	const Matrix<3,3> &	Wx
) const
{
	const double		q0 = this->q[0];
	const double		q1 = this->q[1];
	const double		q2 = this->q[2];
	const double		q3 = this->q[3];

	const double		u = uvw[0];
	const double		v = uvw[1];
	const double		w = uvw[2];

	const double		g = this->g;


	A.fill();

	/*
	 * xyz relative to uvw
	 */
	A.insert(  0,  3, DCM.transpose() );

	/*
	 * uvw relative to uvw
	 */
	A.insert(  3,  3, Wx );

	/*
	 * Q relative to Q
	 */
	A.insert(  6,  6, Wxq );


	/*
	 * XYZ relative to Q
	 * AC in Aaron's code
	 */
	A[0][6] =                - (2 * v * q3) + (2 * w * q2);	// dx/dq0
	A[0][7] =                  (2 * v * q2) + (2 * w * q3);	// dx/dq1
	A[0][8] = - (4 * u * q2) + (2 * v * q1) + (2 * w * q0);	// dx/dq2
	A[0][9] = - (4 * u * q3) - (2 * v * q0) + (2 * w * q1);	// dx/dq3

	A[1][6] =   (2 * u * q3)                - (2 * w * q1);	// dy/dq0
	A[1][7] =   (2 * u * q2) - (4 * v * q1) - (2 * w * q0);	// dy/dq1
	A[1][8] =   (2 * u * q1)                - (2 * w * q3);	// dy/dq2
	A[1][9] =   (2 * u * q0) - (4 * v * q3) + (2 * w * q2);	// dy/dq3

	A[2][6] = - (2 * u * q2) + (2 * v * q1);		// dz/dq0
	A[2][7] =   (2 * u * q3) + (2 * v * q0) - (4 * w * q1);	// dz/dq1
	A[2][8] = - (2 * u * q0) + (2 * v * q3) - (4 * w * q2);	// dz/dq2
	A[2][9] =   (2 * u * q1) + (2 * v * q2);		// dz/dq3

	/*
	 * Body frame velocity relative to Q
	 * AG in Aaron's code
	 */
	A[3][6] = -2 * g * q2;	// du/dq0
	A[3][7] =  2 * g * q3;	// du/dq1
	A[3][8] = -2 * g * q0;	// du/dq2
	A[3][9] =  2 * g * q1;	// du/dq3

	A[4][6] =  2 * g * q1;	// dv/dq0
	A[4][7] =  2 * g * q0;	// dv/dq1
	A[4][8] =  2 * g * q3;	// dv/dq2
	A[4][9] =  2 * g * q2;	// dv/dq3

      	A[5][6] =  0;		// dw/dq0
	A[5][7] = -4 * g * q1;	// dw/dq1
	A[5][8] = -4 * g * q2;	// dw/dq2
	A[5][9] =  0;		// dw/dq3

	/*
	 * Velocity in body frame relative to gravity
	 */
	A[3][10] = DCM[0][2];	// du/dG
	A[4][10] = DCM[1][2];	// dv/dG
	A[5][10] = DCM[2][2];	// dw/dG

	/*
	 * Gyro bias relative to the velocities
	 * dV = A + DCM * G - Wx * V
	 */
	A[3][11] =  0;		// du / d(phi bias)
	A[3][12] =  w;		// du / d(theta bias)
	A[3][13] = -v;		// du / d(psi bias)

	A[4][11] = -w;		// dv / d(phi bias)
	A[4][12] =  0;		// dv / d(theta bias)
	A[4][13] =  u;		// dv / d(psi bias)

	A[5][11] =  v;		// dw / d(phi bias)
	A[5][12] = -u;		// dw / d(theta bias)
	A[5][13] =  0;		// dw / d(psi bias)

	
	/*
	 * Gyro bias relative to quaternion state
	 */
	A[6][11] =  q1;		// dq0 / d(phi bias)
	A[6][12] =  q2;		// dq0 / d(theta bias)
	A[6][13] =  q3;		// dq0 / d(psi bias)

	A[7][11] = -q0;		// dq1 / d(phi bias)
	A[7][12] =  q3;		// dq1 / d(theta bias)
	A[7][13] = -q2;		// dq1 / d(psi bias)

	A[8][11] = -q3;		// dq2 / d(phi bias)
	A[8][12] = -q0;		// dq2 / d(theta bias)
	A[8][13] =  q1;		// dq2 / d(psi bias)

	A[9][11] =  q2;		// dq3 / d(phi bias)
	A[9][12] = -q1;		// dq3 / d(theta bias)
	A[9][13] = -q0;		// dq3 / d(psi bias)
}


void
INS::propagate_state(
	const Vector<3> &	uvw,
	const Vector<3> &	accel,
	const Vector<3> &	UNUSED( pqr ),
	const Matrix<3,3> &	DCM,
	const Matrix<4,4> &	Wxq,
	const Matrix<3,3> &	Wx
)
{
	/*
	 *  Propagate attitude state
	 */
	const Vector<4>		Qdot( Wxq * q );

	this->q += Qdot * this->dt;
	this->q.norm_self();


	/*
	 *  Propigate position state
	 */
	const Vector<3>		Xdot( DCM.transpose() * uvw );
	this->xyz += Xdot * this->dt;

	/*
	 *  Popagate velocity state
	 */
	const double		g( this->g );

	const Vector<3>		G(
		DCM[0][2] * g,
		DCM[1][2] * g,
		DCM[2][2] * g
	);

	const Vector<3>		Vdot( accel - Wx * uvw + G );
	this->uvw += Vdot * this->dt;
}


/*
 * We've split the covariance update into two stages to make it
 * more efficient on the iPAQ.  The covariance matrix should
 * change slowly, allowing us to run it at half the speed (2 * dt)
 * of the "continuous time" IMU samples.
 *
 * By default we have this on all the time now.
 *
 * A three or greater stage split covariance produced unstable
 * results.  The filter trace did not converge, so we're back to
 * two.
 */

#define SPLIT_COVARIANCE	0

void
INS::propagate_covariance()
{
#ifdef SPLIT_COVARIANCE
	switch( this->stage )
	{
	case 0:
		// The A matrix was generated by imu_update
		this->Pdot = this->Q;
		this->Pdot += this->A * this->Ptemp;
		this->stage = 1;
		break;

	case 1:
		this->Pdot += this->Ptemp * this->A.transpose();
		this->Pdot *= this->dt * 2.0;
		this->P += this->Pdot;
		this->trace = 0;

		for( int i=0 ; i<N ; i++ )
			this->trace += this->P[i][i];	

		/* Fall through */
	default:
		this->stage = 0;
	}
#else
	// The A matrix was generated by imu_update
	this->Pdot = this->Q;
	this->Pdot += this->A * this->Ptemp;
	this->Pdot += this->Ptemp * this->A.transpose();
	this->Pdot *= this->dt;
	this->P += this->Pdot;

	this->trace = 0;

	for( int i=0 ; i<N ; i++ )
		this->trace += this->P[i][i];	
#endif

}



template<
	int			m
>
void
INS::do_kalman(
	const Matrix<m,N> &	C,
	const Matrix<m,m> &	R,
	const Vector<m> &	eTHETA
)
{
	// We throw away the K result
	Matrix<N,m>		K;

	// Kalman() wants a vector, not an object.  Serialize the
	// state data into this vector, then extract it out again
	// once we're done with the loop.
	Vector<N>		X_vect;

	X_vect[0]	= this->xyz[0];
	X_vect[1]	= this->xyz[1];
	X_vect[2]	= this->xyz[2];

	X_vect[3]	= this->uvw[0];
	X_vect[4]	= this->uvw[1];
	X_vect[5]	= this->uvw[2];

	X_vect[6]	= this->q[0];
	X_vect[7]	= this->q[1];
	X_vect[8]	= this->q[2];
	X_vect[9]	= this->q[3];

	X_vect[10]	= this->g;

	X_vect[11]	= this->bias[0];
	X_vect[12]	= this->bias[1];
	X_vect[13]	= this->bias[2];

	Kalman(
		this->P,
		X_vect,
		C,
		R,
		eTHETA,
		K
	);

	this->xyz[0]	= X_vect[0];
	this->xyz[1]	= X_vect[1];
	this->xyz[2]	= X_vect[2];

	this->uvw[0]	= X_vect[3];
	this->uvw[1]	= X_vect[4];
	this->uvw[2]	= X_vect[5];

	this->q[0]	= X_vect[6];
	this->q[1]	= X_vect[7];
	this->q[2]	= X_vect[8];
	this->q[3]	= X_vect[9];

	this->g		= X_vect[10];

	this->bias[0]	= X_vect[11];
	this->bias[1]	= X_vect[12];
	this->bias[2]	= X_vect[13];

	this->q.norm_self();
}


void
INS::kalman_attitude_update(
	const Vector<3> &	UNUSED( pqr ),
	const Vector<3> &	accel,
	const Matrix<3,3> &	DCM,
	const Vector<3> &	THETAe
)
{
	double			err;
	const double		q0 = this->q[0];
	const double		q1 = this->q[1];
	const double		q2 = this->q[2];
	const double		q3 = this->q[3];

	const double		DCM_0_2( DCM[0][2] );
	const double		DCM_1_2( DCM[1][2] );
	const double		DCM_2_2( DCM[2][2] );


	// compute the euler angles from the accelerometers
	const Vector<3>		THETAm(
		accel2euler( accel, THETAe[2] )
	);

	// make the C matrix
	Matrix<2,N>		C;

	// PHI section
	err = 2.0 / ( sqr(DCM_2_2) + sqr(DCM_1_2) );

	C[0][6] = err * ( q1 * DCM_2_2 );
	C[0][7] = err * ( q0 * DCM_2_2 + 2.0 * q1 * DCM_1_2 );
	C[0][8] = err * ( q3 * DCM_2_2 + 2.0 * q2 * DCM_1_2 );
	C[0][9] = err * ( q2 * DCM_2_2 );

	// THETA section
	err = -1.0 / sqrt(1.0 - sqr(DCM_0_2) );

	C[1][6] = -2.0 * q2 * err;
	C[1][7] =  2.0 * q3 * err;
	C[1][8] = -2.0 * q0 * err;
	C[1][9] =  2.0 * q1 * err;


	// compute the error; this should be ( THETAm - THETAe ),
	// but we can only use the pitch and roll angles here
	Vector<2>		eTHETA;
	eTHETA[0] = THETAm[0] - THETAe[0];
	eTHETA[1] = THETAm[1] - THETAe[1];

	this->do_kalman(
		C,
		this->R_attitude,
		eTHETA
	);
}




INS::INS(
	double			dt
) :
	dt( dt )
{
	this->reset();
}


void
INS::reset()
{
	this->stage		= 0;
	this->P.fill();
	this->P[0][0]		= 1;
	this->P[1][1]		= 1;
	this->P[2][2]		= 1;

	Matrix<N,N> &		Q( this->Q );

	// Position estimate noise
	Q[0][0]	= 0;
	Q[1][1]	= 0;
	Q[2][2]	= 0;

	// Velocity estimate noise
	Q[3][3]	= 0.1;
	Q[4][4]	= 0.1;
	Q[5][5]	= 0.1;

	// Quaterion attitude estimate noise
	Q[6][6] = 0.0001;
	Q[7][7] = 0.0001;
	Q[8][8] = 0.0001;
	Q[9][9] = 0.0001;

	// Gravity
	Q[10][10]	= 0.001;

	// Gyro bias
	Q[11][11] = 0.03;
	Q[12][12] = 0.03;
	Q[13][13] = 0.03;

	this->R_attitude[0][0] = 0.3;	// phi
	this->R_attitude[1][1] = 0.3;	// theta

	this->R_heading[0][0] = 0.5;	// psi

	this->R_position[0][0] = 0.16;	// x
	this->R_position[1][1] = 0.16;	// y
	this->R_position[2][2] = 0.16;	// z

	this->R_position[3][3] = 0.01;	// u
	this->R_position[4][4] = 0.01;	// v
	this->R_position[5][5] = 0.01;	// w
}


/**
 *  We assume that the vehicle is still during the first sample
 * and use the values to help us determine the zero point for the
 * gyro bias and accelerometers.
 *
 * You must call this once you have the samples from the IMU
 * and compass.  Perhaps throw away the first few to let things
 * stabilize.
 */
void
INS::initialize(
	const Vector<3> &	position,
	const Vector<3> &	velocity,
	const Vector<3> &	accel,
	const Vector<3> &	pqr,
	double			heading
)
{
	this->xyz		= position;
	this->uvw		= velocity;
	this->q			= euler2quat( accel2euler( accel, heading ) );
	this->bias		= pqr;
	this->g			= 19.81;

	// Give the user an estimate of our orientation
	this->theta		= quat2euler( this->q );
}


void
INS::imu_update(
	const Vector<3> &	accel_measured,
	const Vector<3> &	pqr_raw
)
{
	const Vector<3>		pqr_measured( pqr_raw - this->bias );

	/* Compute the DCM and other values for the current estimate */
	const Matrix<3,3> 	DCM( quatDC( this->q ) );
	const Matrix<4,4>	Wxq( quatW( pqr_measured ) );
	const Matrix<3,3>	Wx( eulerWx( pqr_measured ) );


	if( this->stage == 0 )
	{
		this->make_a_matrix(
			this->A,
			this->uvw,
			pqr_measured,
			DCM,
			Wxq,
			Wx
		);

		this->Ptemp = this->P;
	}

	this->propagate_state(
		this->uvw,
		accel_measured,
		pqr_measured,
		DCM,
		Wxq,
		Wx
	);

	this->propagate_covariance();

	/* Compute the DCM and angle for the new estimate */
	const Matrix<3,3> 	new_DCM( quatDC( this->q ) );
	const Vector<3>		new_THETAe( quat2euler( this->q ) );

	this->kalman_attitude_update(
		pqr_measured,
		accel_measured,
		new_DCM,
		new_THETAe
	);

	/* compute angles from quaternions */
	this->theta	= quat2euler( this->q );

	/* Store our bias and converted angular rates */
	this->pqr	= pqr_raw - this->bias;
}


void
INS::compass_update(
	double			heading
)
{
	/* Should we reuse from the previous time step? */
	const Vector<3> &	THETAe( this->theta );

	const Matrix<3,3> 	DCM( quatDC( this->q ) );
	const double		DCM_0_0( DCM[0][0] );
	const double		DCM_0_1( DCM[0][1] );

	const double		q0 = this->q[0];
	const double		q1 = this->q[1];
	const double		q2 = this->q[2];
	const double		q3 = this->q[3];

	Matrix<1,N>		C( 0 );

	// PSI section
	const double		err = 2 / (sqr(DCM_0_0) + sqr(DCM_0_1));

	C[0][6] = err * ( q3 * DCM_0_0 );
	C[0][7] = err * ( q2 * DCM_0_0 );
	C[0][8] = err * ( q1 * DCM_0_0 + 2.0 * q2 * DCM_0_1 );
	C[0][9] = err * ( q0 * DCM_0_0 + 2.0 * q3 * DCM_0_1 );

	// Compute the error, which is the shortest way around the
	// compass to the current heading.
	Vector<1>		eTHETA;

	eTHETA[0] = heading - THETAe[2];
	if( eTHETA[0] > C_PI )
		eTHETA[0] -= 2.0 * C_PI;
	else
	if( eTHETA[0] < -C_PI )
		eTHETA[0] += 2.0 * C_PI;

	this->do_kalman(
		C,
		this->R_heading,
		eTHETA
	);
}


void
INS::gps_update(
	const Vector<3> &	ned,
	const Vector<3> &	uvw
)
{
	Matrix<6,N>		C;

	for( int i=0 ; i<6 ; i++ )
		C[i][i] = 1;

	Vector<6>		error;

	error[0]	= ned[0] - this->xyz[0];
	error[1]	= ned[1] - this->xyz[1];
	error[2]	= ned[2] - this->xyz[2];

	error[3]	= uvw[0] - this->uvw[0];
	error[4]	= uvw[1] - this->uvw[1];
	error[5]	= uvw[2] - this->uvw[2];

	this->do_kalman(
		C,
		this->R_position,
		error
	);
}



}
