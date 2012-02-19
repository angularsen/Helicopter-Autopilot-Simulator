/* -*- indent-tabs-mode:T; c-basic-offset:8; tab-width:8; -*- vi: set ts=8:
 * $Id: ahrs.c,v 1.17 2002/12/05 03:11:31 tramm Exp $
 *
 * AHRS (Attitude and Heading Reference System)
 *
 * Optimized for smaller microcontrollers with limited memory.
 *
 * (c) 2002 Trammell Hudson <hudson@rotomotion.com>
 *
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

#include "ahrs.h"
#include "mat.h"
#include <string.h>		/* For memset */


#define			g		9.78f
#define			dt		0.060 /* Ideal: 0.032768f */


static float		A[6][6];
static float		P[6][6];
static float		Pdot[6][6];
static float		bias[2];

static float		quat[4];

float			ahrs_theta[2];
float			ahrs_pqr[2];
float			ahrs_accel[2];
float			ahrs_trace;
uint8_t			ahrs_stage;

#ifndef __AVR__

#include <stdio.h>

static void
print_matrix(
	const char *		name,
	const void *		M_p,
	index_t			n,
	index_t			m
)
{
	const float *		M = M_p;
	index_t			i;
	index_t			j;

	printf( "%s=\n", name );
	for( i=0 ; i<n ; i++ )
	{
		for( j=0 ; j<m ;j++ )
			printf( "% 3.8f ", M[i*m + j] );
		printf( "\n" );
	}
}
#endif

static void
generate_A()
{
	const float		p = ahrs_pqr[0] / 2.0;
	const float		q = ahrs_pqr[1] / 2.0;

        // A[0][0] =  0;
           A[0][1] = -p;
           A[0][2] = -q;
        // A[0][3] =  0;
           A[0][4] =  quat[1] / 2.0;
           A[0][5] =  quat[2] / 2.0;

           A[1][0] =  p;
        // A[1][1] =  0;
        // A[1][2] =  0;
           A[1][3] = -q;
           A[1][4] = -quat[0] / 2.0;
           A[1][5] =  quat[3] / 2.0;

           A[2][0] =  q;
        // A[2][1] =  0;
        // A[2][2] =  0;
           A[2][3] =  p;
           A[2][4] = -quat[3] / 2.0;
           A[2][5] = -quat[0] / 2.0;

        // A[3][0] =  0;
           A[3][1] =  q;
           A[3][2] = -p;
        // A[3][3] =  0;
           A[3][4] =  quat[2] / 2.0;
           A[3][5] = -quat[1] / 2.0;

        // A[4][0] = 0;
        // A[4][1] = 0;
        // A[4][2] = 0;
        // A[4][3] = 0;
        // A[4][4] = 0;
        // A[4][5] = 0;

        // A[5][0] = 0;
        // A[5][1] = 0;
        // A[5][2] = 0;
        // A[5][3] = 0;
        // A[5][4] = 0;
        // A[5][5] = 0;
}



static void
propagate_state( void )
{
	index_t			i;
	const float		p = ahrs_pqr[0] / 2.0;
	const float		q = ahrs_pqr[1] / 2.0;

	float			Qdot[4];

	Qdot[0] = (-p * quat[1] - q * quat[2]) * dt;
	Qdot[1] = ( p * quat[0] - q * quat[3]) * dt;
	Qdot[2] = ( p * quat[3] + q * quat[0]) * dt;
	Qdot[3] = (-p * quat[2] + q * quat[1]) * dt;

	for( i=0 ; i<4 ; i++ )
		quat[i] += Qdot[i];

	norm( quat );
}



/**
 *  Full covariance update is:
 *
 * Pdot = Q + A * P + P * A.transpose
 * P = P + Pdot * dt
 *
 */

static void
propagate_covariance0( void )
{
	memset( Pdot, 0, 6 * 6 * sizeof(float) );
	
	// Noise estimate for quaternion state (Q)
	Pdot[0][0] = 0.0001;
	Pdot[1][1] = 0.0001;
	Pdot[2][2] = 0.0001;
	Pdot[3][3] = 0.0001;

	// Noise estimate for gyro bias
	Pdot[4][4] = 0.03;
	Pdot[5][5] = 0.03;
}


static void
propagate_covariance1( void )
{
	mulNxM( Pdot, A, P, 6, 6, 6, 0, 1 );
}


static void
propagate_covariance2( void )
{
	mulNxM( Pdot, P, A, 6, 6, 6, 1, 1 );
}


static void
propagate_covariance3( void )
{
	index_t			i;
	index_t			j;

	ahrs_trace = 0;

	for( i=0 ; i<6 ; i++ )
	{
		for( j=0 ; j<6 ; j++ )
		{
			P[i][j] += Pdot[i][j] * dt;
			if( i == j )
				ahrs_trace += P[i][i] * P[i][i];
		}
	}
}




/**
 *  Actual code:
 *
 * E = R + C * P * C.transpose
 * K = P * C.transpose * invert(E)
 *
 * X = X + K * err
 * P = P - K * C * P
 *
 */
static void
kalman(
	float			C[2][6],
	const float 		err[2]
)
{
	float			E[2][2];
	float			K[6][2];
	float			temp[12];

	// E = R
	E[0][0] = 0.3;	// Pitch
	E[0][1] = 0;
	E[1][0] = 0;
	E[1][1] = 0.3;	// Roll

	// C * P
	mulNxM( temp, C, P, 2, 6, 6, 0, 0 );

	// E += (C*P) * C.transpose
	mulNxM( E, temp, C, 2, 6, 2, 1, 1 );

	// E = invert(E)
	invert2(E);

	// P * C.transpose
	mulNxM( temp, P, C, 6, 6, 2, 1, 0 );

	// K = (P*C.tranpose) * invert(E)
	mulNxM( K, temp, E, 6, 2, 2, 0, 0 );
	
	// X += K * err;
	quat[0]		+= K[0][0] * err[0] + K[0][1] * err[1];
	quat[1]		+= K[1][0] * err[0] + K[1][1] * err[1];
	quat[2]		+= K[2][0] * err[0] + K[2][1] * err[1];
	quat[3]		+= K[3][0] * err[0] + K[3][1] * err[1];
	bias[0]		+= K[4][0] * err[0] + K[4][1] * err[1];
	bias[1]		+= K[5][0] * err[0] + K[5][1] * err[1];

	//  C * P
	mulNxM( temp, C, P, 2, 6, 6, 0, 0 );

	// P -= K * (C * P)
	mulNxM( P, K, temp, 6, 2, 6, 0, -1 );
}


static void
attitude_update( void )
{
	float			err;
	float			C[2][6];
	
	/* Convert the acceleration measurement into angles */
	float			THETAm[2];

	/* Convert the quaternion estimate into euler angles */
	float			THETAe[2];

	/*
	 * For the 2D case we can use the reduced Direction Cosine Vector,
	 * which has only the [0][2], [1][2] and [2][2] values.
	 */
	float			DCV[3];

	accel2euler( THETAm, ahrs_accel );
	quat2dcv( DCV, quat );
	dcv2euler( THETAe, DCV );

	// Compute our error in the measurement
	THETAm[0] -= THETAe[0];
	THETAm[1] -= THETAe[1];

	err = 2.0 / ( DCV[2] * DCV[2] + DCV[1] * DCV[1] );

	C[0][0] = err * ( quat[1] * DCV[2] );
	C[0][1] = err * ( quat[0] * DCV[2] + 2.0 * quat[1] * DCV[1] );
	C[0][2] = err * ( quat[3] * DCV[2] + 2.0 * quat[2] * DCV[1] );
	C[0][3] = err * ( quat[2] * DCV[2] );
	C[0][4] = 0;
	C[0][5] = 0;


	// Fill in the THETA section of the C matrix
	err = -2.0 / sqrt( 1.0 - DCV[0] * DCV[0] );

	C[1][0] = -err * quat[2];
	C[1][1] =  err * quat[3];
	C[1][2] = -err * quat[0];
	C[1][3] =  err * quat[1];
	C[1][4] = 0;
	C[1][5] = 0;


	kalman( C, THETAm );
	norm( quat );
}


/**
 *  Caller should set pqr with the raw (biased) pqr values in
 * ahrs_pqr[] and the two raw accelerometer values in ahrs_accel[];
 */
void
ahrs_update( void )
{
	/* Unbias the values */
	ahrs_pqr[0] -= bias[0];
	ahrs_pqr[1] -= bias[1];


	switch( ahrs_stage )
	{
	case 0:
		generate_A();
		propagate_covariance0();
		ahrs_stage = 1;
		break;

	case 1:
		propagate_covariance1();
		ahrs_stage = 2;
		break;

	case 2:
		propagate_covariance2();
		ahrs_stage = 3;
		break;

	case 3:
		propagate_covariance3();

		/* Fall through */
	default:
		ahrs_stage = 0;
		break;
	}

	propagate_state();
	attitude_update();

	// Produce the output for the caller
	quat2euler( ahrs_theta, quat );
}



void
ahrs_init( void )
{
	index_t			i;

	// Covariance matrix is an I matrix to start
	memset( P, 0, 6 * 6 * sizeof(float) );
	for( i=0 ; i<6 ; i++ )
		P[i][i] = 1;

	// Zero out our other matrices
	memset( A, 0, 6 * 6 * sizeof(float) );

	// Transform our accelerometer values into an angle estimate
	accel2euler( ahrs_theta, ahrs_accel );
	euler2quat( quat, ahrs_theta );

	bias[0]	= ahrs_pqr[0];
	bias[1]	= ahrs_pqr[1];
}


#ifndef __AVR__
int
main( void )
{
	int			i;
	float			t = 0;

	ahrs_init();

	for( i=0 ; i<65536 ; i++ )
	{
		printf( "time=%3.4f "
			"angle=% 3.8f % 3.8f "
			"bias=% 3.8f % 3.8f\n",
			t += dt,
			ahrs_theta[0],
			ahrs_theta[1],
			bias[0],
			bias[1]
		);

		ahrs_pqr[0]	=  0.1;
		ahrs_pqr[1]	= -0.2;

		ahrs_accel[0]	= 0;
		ahrs_accel[1]	= 0;

		ahrs_update();

	}

	return 0;
}
#endif
