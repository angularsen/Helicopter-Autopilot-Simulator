/* -*- indent-tabs-mode:T; c-basic-offset:8; tab-width:8; -*- vi: set ts=8:
 * $Id: ahrs.c,v 2.0 2002/09/22 02:10:16 tramm Exp $
 *
 * (c) 2002 Trammell Hudson <hudson@swcp.com>
 *************
 *
 *  This file is part of the autopilot onboard code package.
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

#include <math.h>
#include "ahrs.h"
#include "../avr/vector.h"

/**
 *  Kalman weighting matrices
 */
static m_t Q = {
	{ 0.00015,	0.00000,	0.00000,	0.00000 },
	{ 0.00000,	0.00015,	0.00000,	0.00000 },
	{ 0.00000,	0.00000,	0.00015,	0.00000 },
	{ 0.00000,	0.00000,	0.00000,	0.00015 },
};

static m_t R = {
	{ 0.086,	0.000,		0.000 },
	{ 0.000,	0.086,		0.000 },
	{ 0.000,	0.000,		0.015 },
};


/*
 * Covariance matrix
 */
static m_t P;


/*
 * Position vector
 * Initial position:  Level, facing north
 *
 * Estimated position is derived from this.  Unless ahrs_init() is called
 * the filter is likely to spike.
 */
static v_t X = { 1.0, 0.0, 0.0, 0.0 };


/*
 *  F[3] is a temp vector of d( arcsin(X) ) to reduce the complexity
 * of the C matrix calculations.
 */
static void
compute_f(
	v_t			F,
	m_t 			DCM
)
{
	F[0] =  2.0 / ( sq(DCM[2][2]) + sq(DCM[1][2]) );
	F[1] = -1.0 / sqrt( 1.0 - sq(DCM[0][2]) );
	F[2] =  2.0 / ( sq(DCM[0][0]) + sq(DCM[0][1]) );
}


/*
 *  C is the measurement matrix that has the measured state
 * and the estimated state.  
 *
 * C = [
 *	[ dphi/dq0	dhpi/dq1	... ]
 *	[ dtheta/qd0	dtheta/dq1	... ]
 *	[ dpsi/dq0	dpsi/dq1	... ]
 *     ]
 */
static void
compute_c(
	m_t			C,
	v_t			X
)
{
	m_t			DCM;
	v_t			F;

	/* Compute the new DCM matrix from the quaternion position */
	quat2dcm( DCM, X );

	/* Build our temporary matrix */
	compute_f( F, DCM );

	/* Compute the estimated state */
	C[0][0] = F[0] * ( X[1] * DCM[2][2] );
	C[0][1] = F[0] * ( X[0] * DCM[2][2] + 2.0 * X[1] * DCM[1][2] );
	C[0][2] = F[0] * ( X[3] * DCM[2][2] + 2.0 * X[2] * DCM[1][2] );
	C[0][3] = F[0] * ( X[2] * DCM[2][2] );

	C[1][0] = F[1] * -2.0 * X[2];
	C[1][1] = F[1] *  2.0 * X[3];
	C[1][2] = F[1] * -2.0 * X[0];
	C[1][3] = F[1] *  2.0 * X[1];

	C[2][0] = F[2] * ( X[3] * DCM[0][0] );
	C[2][1] = F[2] * ( X[2] * DCM[0][0] );
	C[2][2] = F[2] * ( X[1] * DCM[0][0] + 2.0 * X[2] * DCM[0][1] );
	C[2][3] = F[2] * ( X[0] * DCM[0][0] + 2.0 * X[3] * DCM[0][1] );
}


/*
 * Kalman filter update
 *
 * P is [4,4] and holds the covariance matrix
 * R is [3,3] and holds the measurement weights
 * C is [4,3] and holds the euler to quat tranform
 * X is [4,1] and holds the estimated state as a quaterion
 * Xe is [3,1] and holds the euler angles of the estimated state
 * Xm is [3,1] and holds the measured euler angles
 */
static inline void
kalman_update(
	m_t			P,
	m_t			R,
	m_t			C,
	v_t			X,
	const v_t		Xe,
	const v_t		Xm
)
{
	m_t			T1;
	m_t			T2;
	m_t			E;
	m_t			K;
	v_t			err;
	v_t			temp;

	/* E[3,3] = C[3,4] P[4,4] C'[4,3] + R[3,3] */
	m_mult( T1, C, P, 3, 4, 4 );
	m_transpose( T2, C, 3, 4 );
	m_mult( E, T1, T2, 3, 4, 3 );
	m_add( E, R, E, 3, 3 );

	/* K[4,3] = P[4,4] C'[4,3] inv(E)[3,3] */
	m_mult( T1, P, T2, 4, 4, 3 );
	m33_inv( E, T2 );
	m_mult( K, T1, T2, 4, 3, 3 );

	/* X[4] = X[4] + K[4,3] ( Xm[3] - Xe[3] ) */
	v_sub( err, Xm, Xe, 3 );
	mv_mult( temp, K, err, 4, 3 );
	v_add( X, temp, X, 4 );

	/* P[4,4] = P[4,4] - K[4,3] C[3,4] P[4,4] */
	m_mult( T1, K, C, 4, 3, 4 );
	m_mult( T2, T1, P, 4, 4, 4 );
	m_sub( P, T2, P, 4, 4 );
}


/**
 *  ahrs_init() takes the initial values from the IMU and converts
 * them into the quaterion state vector.
 */
void
ahrs_init(
	f_t			ax,
	f_t			ay,
	f_t			az,
	f_t			heading
)
{
	v_t			euler_angles;

	accel2euler(
		euler_angles,
		ax,
		ay,
		az,
		heading
	);

	euler2quat(
		X,
		euler_angles[0],
		euler_angles[1],
		euler_angles[2]
	);
}

/**
 *  ahrs_step() takes the values from the IMU and produces the
 * new estimated attitude.
 */
void
ahrs_step(
	v_t			angles_out,
	f_t			dt,
	f_t			p,
	f_t			q,
	f_t			r,
	f_t			ax,
	f_t			ay,
	f_t			az,
	f_t			heading
)
{
	m_t			C;

	m_t			A;
	m_t			AT;
	v_t			X_dot;
	m_t			temp;

	v_t			Xm;
	v_t			Xe;

	/* Construct the quaternion omega matrix */
	rotate2omega( A, p, q, r );

	/* X_dot = AX */
	mv_mult( X_dot, A, X, 4, 4 );

	/* X += X_dot dt */
	v_scale( X_dot, X_dot, dt, 4 );

	v_add( X, X_dot, X, 4 );
	v_normq( X, X );

/*
	printf( "X =" );
	Vprint( X, 4 );
	printf( "\n" );
*/
	
	/* AT = transpose(A) */
	m_transpose( AT, A, 4, 4 );

	/* P = A * P * AT + Q */
	m_mult( temp, A, P, 4, 4, 4 );
	m_mult( P, temp, AT, 4, 4, 4 );
	m_add( P, Q, P, 4, 4 );
	
	
	compute_c( C, X );

	/* Compute the euler angles of the measured attitude */
	accel2euler(
		Xm,
		ax,
		ay,
		az,
		heading
	);

	/*
	 * Compute the euler angles of the estimated attitude
	 * Xe = quat2euler( X )
	 */
	quat2euler( Xe, X );

	/* Kalman filter update */
	kalman_update(
		P,
		R,
		C,
		X,
		Xe,
		Xm
	);

	/* Estimated attitude extraction */
	quat2euler( angles_out, X );
}


#ifndef AVR
#include <stdio.h>

int
main( void )
{
	v_t			angles;

	ahrs_init( 0, 0, 1, 0 );

	while(1)
	{
		ahrs_step( angles, 0.1, 0, 0, 0, 0, 0, 1, 0 );
	
		printf( "Angles: %lf %lf %lf\n",
			angles[0],
			angles[1],
			angles[2]
		);

		getchar();
	}

	return 0;
}
#else

#include "uart.h"
#include "timer.h"
#include "string.h"

static volatile uint16_t high_bits;

SIGNAL( SIG_OVERFLOW1 )
{
	high_bits++;
}

static inline uint32_t
high_time( void )
{
	uint32_t now = (uint32_t) high_bits;
	now <<= 16;
	now |= time();
	return now;
}

int
main( void )
{
	v_t			angles;
	uint32_t		start;
	uint32_t		stop;

	init_uart();
	init_timer();

	sbi( TIMSK, TOIE1 );
	sei();

	puts( "ahrs_init()\r\n" );
	ahrs_init( 0, 0, 1, 0 );

	while(1)
	{
		puts( "\r\nstep: " );
		start = high_time();
		ahrs_step( angles, 0.1, 0, 0, 0, 0, 0, 1, 0 );
		stop = high_time();

		put_uint16_t( (stop >> 16) & 0xFFFF );
		put_uint16_t( (stop >>  0) & 0xFFFF );
		puts( " - " );
		put_uint16_t( (start >> 16) & 0xFFFF );
		put_uint16_t( (start >>  0) & 0xFFFF );

		stop -= start;
		puts( " = " );
		put_uint16_t( (stop >> 16) & 0xFFFF );
		put_uint16_t( (stop >>  0) & 0xFFFF );
	}
}

#endif
