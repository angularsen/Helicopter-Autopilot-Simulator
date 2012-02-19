/* -*- indent-tabs-mode:T; c-basic-offset:8; tab-width:8; -*- vi: set ts=8:
 * $Id: mat.h,v 1.4 2002/10/26 03:17:05 tramm Exp $
 *
 * Matrix math library
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

#ifndef _avr_mat_h_
#define _avr_mat_h_

#include <math.h>

#ifdef __AVR__
#include <io.h>
#else
#include <stdint.h>
#endif

typedef int8_t		index_t;


/*
 * We can avoid expensive floating point operations if one of the
 * values in question is zero.  This provides an easy way to check
 * to see if it is actually a zero without using any floating point
 * code.
 */
static inline int8_t
is_zero(
	const float *		f
)
{
	const uint32_t *	x = (const uint32_t*) f;

	if( *x == 0 )
		return 1;
	return 0;
}


/*
 * Normalize a quaternion in place.
 */
extern void
norm(
	float *			q
);


/*
 * Perform the matrix multiplication A[n,m] * B[m,p], storing the
 * result in OUT[n,p].
 *
 * If transpose_B is set, B is assumed to be a [p,m] matrix that is
 * tranversed in column major order instead of row major.  This
 * has the effect of transposing B during the computation.
 *
 * If add == 0, OUT  = A * B.
 * If add >  0, OUT += A * B.
 * If add <  0, OUT -= A * B.
 */
extern void
mulNxM(
	void *			OUT_ptr,
	const void *		A_ptr,
	const void *		B_ptr,
	index_t			n,
	index_t			m,
	index_t			p,
	int8_t			transpose_B,
	int8_t			add
);


#ifndef AHRS_3D
/*
 * Invert a 2x2 matrix in place.
 */
extern void
invert2(
	float			A[2][2]
);
#endif


#ifdef AHRS_3D
/*
 * Compute the full direction cosine matrix for the quaternion quat.
 */
extern void
quat2dcm(
	float			DCM[3][3],
	const float *		quat
);

#else

/*
 * Compute the reduce direction cosine matrix for the quaternion.
 * This consists of only the third column, which is used for the
 * two-dimensional computations.
 */
extern void
quat2dcv(
	float *			DCV,
	const float *		quat
);
#endif

#ifdef AHRS_3D
/*
 * Convert a direction cosine matrix into an euler angle.  This is
 * less expensive than quat2euler if you have already computed the
 * quaternion DCM.
 */
extern void
dcm2euler(
	float *			THETAe,
	float			DCM[3][3]
);

#else

/*
 * Conver the reduced DCM to an euler angle.  As with dcm2euler, this
 * is less expensive than quat2euler if you have already computed the
 * DCV for the quaternion.
 */
extern void
dcv2euler(
	float *			THETAe,
	const float *		DCV
);
#endif


/*
 * Computes the euler angle representation of the quaternion.  If you
 * need both the DCM or DCV and the Euler angles, use quat2dcv and
 * then dcv2euler instead.
 */
extern void
quat2euler(
	float *			THETAe,
	const float *		quat
);


/*
 * Given two accelerometer readings, compute an estimated set of Euler
 * angles.
 */
extern void
accel2euler(
	float *			THETAm,
	const float *		accel
);


/*
 * Given an euler angle, compute the quaternion representation.  This
 * assumes a two dimensional vector with the heading always 0, so it
 * will run much faster in the 2D case.
 */
extern void
euler2quat(
	float *			quat,
	const float *		euler
);

#endif
