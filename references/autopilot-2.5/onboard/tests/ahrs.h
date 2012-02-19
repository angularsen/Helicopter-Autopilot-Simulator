/* -*- indent-tabs-mode:T; c-basic-offset:8; tab-width:8; -*- vi: set ts=8:
 *  $Id: ahrs.h,v 2.0 2002/09/22 02:10:18 tramm Exp $
 *
 * Computes the attitude and heading from IMU-type readings.
 *
 * (c) 2002 Trammell Hudson <hudson@swcp.com>
 */
#ifndef _AHRS_H_
#define _AHRS_H_

#include <math.h>
#include "../avr/vector.h"

/**
 *  ahrs_init() takes the initial values from the IMU and converts
 * them into the quaterion state vector.
 */
extern void
ahrs_init(
	f_t			ax,
	f_t			ay,
	f_t			az,
	f_t			heading
);

/**
 *  ahrs_step() takes the values from the IMU and produces the
 * new estimated attitude.
 */
extern void
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
);


/**
 *  Vector helpers related to the AHRS computation
 */
static inline f_t
sq(
	f_t			d
)
{
	return d * d;
}

/*
 * This will normalize a quaternion vector q
 * q/norm(q)
 * q(4,1)
 */
static inline void
v_normq(
	v_t			q_out,
	const v_t		q
)
{
	v_scale(
		q_out,
		q,
		1.0 / sqrt( sq(q[0]) + sq(q[1]) + sq(q[2]) + sq(q[3]) ),
		4
	);
}


/*
 * This will convert from quaternions to euler angles
 * q(4,1) -> euler[phi;theta;psi] (rad)
 */
static inline void
quat2euler(
	v_t			euler,
	const v_t		q
)
{
	f_t			q0 = q[0];
	f_t			q1 = q[1];
	f_t			q2 = q[2];
	f_t			q3 = q[3];

	f_t			q02 = q0*q0;
	f_t			q12 = q1*q1;
	f_t			q22 = q2*q2;
	f_t			q32 = q3*q3;

	/* phi */
	euler[0] = atan2( 2.0 * (q2*q3 + q0*q1), 1.0 - 2.0 * (q12 + q22) );

	/* theta */
	euler[1] = -asin( 2.0 * (q1*q3 - q0*q2) );

	/* psi */
    	euler[2] = atan2( 2.0 * (q1*q2 + q0*q3), 1.0 - 2.0 * (q22 + q32) );
}
	
/*
 * This will convert from euler angles to quaternion vector
 * phi, theta, psi -> q(4,1)
 * euler angles in radians
 */
static inline void
euler2quat(
	v_t			q,
	f_t			phi,
	f_t			theta,
	f_t			psi
)
{
	f_t			sphi	= sin( 0.5 * phi );
	f_t			stheta	= sin( 0.5 * theta );
	f_t			spsi	= sin( 0.5 * psi );

	f_t			cphi	= cos( 0.5 * phi );
	f_t			ctheta	= cos( 0.5 * theta );
	f_t			cpsi	= cos( 0.5 * psi );

	q[0] =  cphi*ctheta*cpsi + sphi*stheta*spsi;
	q[1] = -cphi*stheta*spsi + sphi*ctheta*cpsi;
	q[2] =  cphi*stheta*cpsi + sphi*ctheta*spsi;
	q[3] =  cphi*ctheta*spsi - sphi*stheta*cpsi;
}



/*
 *  This will convert from accelerometer and compass readings into
 * [phi, theta, psi] (in radians).  The accelerometer readings are assumed
 * to be in m/s^2, but the actual units don't matter.
 */
static inline void
accel2euler(
	v_t			euler,
	f_t			ax,
	f_t			ay,
	f_t			az,
	f_t			compass
)
{
	f_t			g = sqrt( ax*ax + ay*ay + az*az );

	euler[0] = atan2( ay, -az );		/* Roll */
        euler[1] = asin( ax / -g );		/* Pitch */
        euler[2] = compass;			/* Yaw */
}



/*
 * This will construct a direction cosine matrix from 
 * quaternions in the standard rotation  sequence
 * [phi][theta][psi]
 *
 * body = tBL(3,3)*NED
 * q(4,1)
 */
static inline void
quat2dcm(
	m_t			dcm,
	const v_t		q
)
{
	f_t			q0	= q[0];
	f_t			q1	= q[1];
	f_t			q2	= q[2];
	f_t			q3	= q[3];

	f_t			q02	= q0 * q0;
	f_t			q12	= q1 * q1;
	f_t			q22	= q2 * q2;
	f_t			q32	= q3 * q3;

	dcm[0][0] = 1.0 - 2.0 * (  q22  +  q32  );
	dcm[0][1] =       2.0 * ( q1*q2 + q0*q3 );
	dcm[0][2] =       2.0 * ( q1*q3 - q0*q2 );

	dcm[1][0] =       2.0 * ( q1*q2 - q0*q3 );
	dcm[1][1] = 1.0 - 2.0 * (  q12  +  q32  );
	dcm[1][2] =       2.0 * ( q2*q3 + q0*q1 );

	dcm[2][0] =       2.0 * ( q1*q3 + q0*q2 );
	dcm[2][1] =       2.0 * ( q2*q3 - q0*q1 );
	dcm[2][2] = 1.0 - 2.0 * (  q12  +  q22  );
}


/*
 * This will construct the quaternion omega matrix
 * W(4,4)
 * p, q, r (rad/sec)
 */
void
rotate2omega(
	m_t			W,
	f_t			p,
	f_t			q,
	f_t			r
)
{
	W[0][0] = 0.0;
	W[0][1] = -p/2.0;
	W[0][2] = -q/2.0;
	W[0][3] = -r/2.0;

	W[1][0] =  p/2.0;
	W[1][1] = 0.0;
	W[1][2] =  r/2.0;
	W[1][3] = -q/2.0;

	W[2][0] =  q/2.0;
	W[2][1] = -r/2.0;
	W[2][2] = 0.0;
	W[2][3] =  p/2.0;

	W[3][0] =  r/2.0;
	W[3][1] =  q/2.0;
	W[3][2] = -p/2.0;
	W[3][3] = 0.0;
}


#endif
