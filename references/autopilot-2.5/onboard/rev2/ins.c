#define AHRS_3D

#include "mat.h"

float			ins_pqr[3];

static float		A[7][7];
static float		P[7][7];
static float		DCM[3][3];
static float		Pdot[7][7];

/*
 * DCM must be generated before calling
 */
static void
generate_A( void )
{
	memset( A, 0, sizeof(A) );

	/* XYZ relative to XYZ is zero */
	// A[0..3][0..3] = 0;

	/* XYZ relative to UVW = DCM transpose */
	A[0][3] = DCM[0][0];
	A[0][4] = DCM[1][0];
	A[0][5] = DCM[2][0];

	A[1][3] = DCM[0][1];
	A[1][4] = DCM[1][1];
	A[1][5] = DCM[2][1];

	A[2][3] = DCM[0][2];
	A[2][4] = DCM[1][2];
	A[2][5] = DCM[2][2];

	/* XYZ relative to G is zero */
	// A[0..3][6] = 0;

	/* UVW relative to XYZ is zero */
	// A[3..5][0..3] = 0;

	/* UVW relative to UVW is Wx */
	//A[3][3] = 0;
	A[3][4] = -ins_pqr[2];
	A[3][5] =  ins_pqr[1];

	A[4][3] =  ins_pqr[2];
	//A[4][4] = 0;
	A[4][5] = -ins_pqr[0];

	A[5][3] = -ins_pqr[1];
	A[5][4] =  ins_pqr[0];
	//A[5][5] = 0;

	/* UVW relative to G is part of the DCM */
 	A[3][6] = DCM[0][2];
	A[4][6] = DCM[1][2];
	A[5][6] = DCM[2][2];

	/* G relative to everything is zero */
	// A[6][0..6] = 0;
}


/*
 * State propagation only needs to worry about the
 * position and velocity states.
 *
 * DCM must be filled in.
 */
static void
propagate_state( void )
{
	uint8_t			i;

	/* Attitude state is maintained by the AHRS */
	// ins_Q += Qdot * dt

	/* Position state: X += DCM.tranpose * uvw * dt */
	for( i=0 ; i<3 ; i++ )
		ins_XYZ[i] += dt * (
			  DCM[0][i] * ins_UVW[0]
			+ DCM[1][i] * ins_UVW[1]
			+ DCM[2][i] * ins_UVW[2]
		);

	/* Velocity state: V += (accel - Wx * uvw + G ) */
	float			dU;
	float			dV;
	float			dW;

	dU = ( ins_accel[0] + DCM[0][2] * G
		+ 0
		+ ins_pqr[2] * ins_UVW[1]
		- ins_pqr[1] * ins_UVW[2]
	);

	dV = ( ins_accel[1] + DCM[1][2] * G
		- ins_pqr[2] * ins_UVW[0]
		+ 0
		+ ins_pqr[0] * ins_UVW[2]
	);

	dW = ( ins_accel[2] + DCM[2][2] * G
		+ ins_pqr[1] * ins_UVW[0]
		- ins_pqr[0] * ins_UVW[1]
		+ 0
	);

	ins_UVW[0] += dU * dt;
	ins_UVW[1] += dV * dt;
	ins_UVW[2] += dW * dt;
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
	memset( Pdot, 0, sizeof( Pdot ) );
	
	// Noise estimate for position state (XYZ)
	// Pdot[0][0] = 0;
	// Pdot[1][1] = 0;
	// Pdot[2][2] = 0;

	// Noise estimate for velocity (UVW)
	Pdot[3][3] = 0.1;
	Pdot[4][4] = 0.1;
	Pdot[5][5] = 0.1;

	// Noise estimate for gravity (G)
	Pdot[6][6] = 0.001;
}


static void
propagate_covariance1( void )
{
	mulNxM( Pdot, A, P, 7, 7, 7, 0, 1 );
}


static void
propagate_covariance2( void )
{
	mulNxM( Pdot, P, A, 7, 7, 7, 1, 1 );
}


static void
propagate_covariance3( void )
{
	index_t			i;
	index_t			j;

	ahrs_trace = 0;

	for( i=0 ; i<7 ; i++ )
	{
		for( j=0 ; j<7 ; j++ )
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
	const uint8_t		n,
	float			E[n][n],
	float			C[][7],
	const float * 		err
)
{
	float			K[7][n];
	float			temp[7 * n];

/*
	// E = R
	E[0][0] = 0.3;	// Pitch
	E[0][1] = 0;
	E[1][0] = 0;
	E[1][1] = 0.3;	// Roll
*/

	// C * P
	mulNxM( temp, C, P, n, 7, 7, 0, 0 );

	// E += (C*P) * C.transpose
	mulNxM( E, temp, C, n, 7, n, 1, 1 );

	// E = invert(E)
	invert(E);

	// P * C.transpose
	mulNxM( temp, P, C, 7, 7, n, 1, 0 );

	// K = (P*C.tranpose) * invert(E)
	mulNxM( K, temp, E, 7, n, n, 0, 0 );
	
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




/*
 * ins_Q, ins_PQR, ins_accel, ins_XYZ, ins_UVW must be filled in
 * before calling ins_update().
 */
void
ins_update( void )
{
	static uint8_t		ins_stage;

	quat2dcm( DCM, ins_Q );

	switch( ins_stage )
	{
	case 0:
		generate_A();
		propagate_covariance0();
		ins_stage = 1;
		break;

	case 1:
		propagate_covariance1();
		ins_stage = 2;
		break;

	case 2:
		propagate_covariance2();
		ins_stage = 3;
		break;

	case 3:
		propagate_covariance3();
		/* Fall through */

	default:
		ins_stage = 0;
		break;
	}

	propagate_state();
	kalman_update();

	// Produce the output for the caller
	quat2euler( ins_theta, ins_Q );
}


void
ins_init( void )
{
	uint8_t			i;

	/* P = eye(7) */
	memset( P, 0, sizeof(P) );

	for( i=0 ; i<7 ; i++ )
		P[i][i] = 0;
}
