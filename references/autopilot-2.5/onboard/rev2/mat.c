/* -*- indent-tabs-mode:T; c-basic-offset:8; tab-width:8; -*- vi: set ts=8:
 * $Id: mat.c,v 1.6 2002/10/21 20:26:54 tramm Exp $
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

#include "mat.h"


void
norm(
	float *			q
)
{
	float			mag = 0;
	index_t			i;

	for( i=0 ; i<4 ; i++ )
		mag += q[i] * q[i];

	mag = sqrt(mag);

	for( i=0 ; i<4 ; i++ )
		q[i] /= mag;
}


void
mulNxM(
	void *			OUT_ptr,
	const void *		A_ptr,
	const void *		B_ptr,
	index_t			n,
	index_t			m,
	index_t			p,
	int8_t			transpose_B,
	int8_t			add
)
{
	index_t			i;
	index_t			j;
	index_t			k;

	float *			OUT = OUT_ptr;
	const float *		A = A_ptr;
	const float *		B = B_ptr;


	for( i=0 ; i<n ; i++ )
	{
		const float *		A_i = A + i * m;
		float *			O_i = OUT + i * p;

		for( j=0 ; j<p ; j++ )
		{
			float			s = 0;
			float *			O_i_j = O_i + j;

			for( k=0 ; k<m ; k++ )
			{
				const float *		a = A_i + k;
				const float *		b;

				if( is_zero( a ) )
					continue;

				if( transpose_B )
					b = B + j * m + k;
				else
					b = B + k * p + j;

				if( is_zero( b ) )
					continue;

				s += *a * *b;
			}


			if( add == 0 )
				*O_i_j = s;
			else
			if( add > 0 )
				*O_i_j += s;
			else
				*O_i_j -= s;
		}
	}
}



void
invert2(
	float			A[2][2]
)
{
	float			det =
		A[0][0] * A[1][1] - A[0][1] * A[1][0];

	float			temp;

	temp = A[0][0];
	A[0][0] = A[1][1] / det;
	A[1][1] = temp / det;

	A[0][1] /= -det;
	A[1][0] /= -det;
}


void
quat2dcv(
	float *			DCV,
	const float *		q
)
{
	DCV[0] =     2*(q[1]*q[3] - q[0]*q[2]);
	DCV[1] =     2*(q[2]*q[3] + q[0]*q[1]);
	DCV[2] = 1.0-2*(q[1]*q[1] + q[2]*q[2]);
}

/*
 * These are only used for the 3D AHRS, so we don't both
 * compiling them otherwise.
 */
#ifdef AHRS_3D
void
quat2dcm(
	float			DCM[3][3],
	const float *		q
)
{
	DCM[0][0] = 1.0-2*(q[2]*q[2] + q[3]*q[3]);
	DCM[0][1] =     2*(q[1]*q[2] + q[0]*q[3]);
	DCM[0][2] =     2*(q[1]*q[3] - q[0]*q[2]);

	DCM[1][0] =     2*(q[1]*q[2] - q[0]*q[3]);
	DCM[1][1] = 1.0-2*(q[1]*q[1] + q[3]*q[3]);
	DCM[1][2] =     2*(q[2]*q[3] + q[0]*q[1]);

	DCM[2][0] =     2*(q[1]*q[3] + q[0]*q[2]);
	DCM[2][1] =     2*(q[2]*q[3] - q[0]*q[1]);
	DCM[2][2] = 1.0-2*(q[1]*q[1] + q[2]*q[2]);
}


void
dcm2euler(
	float *			THETAe,
	float			DCM[3][3]
)
{
#ifdef __AVR__
	THETAe[0] = atan2( DCM[2][2], DCM[1][2] );
#else
	THETAe[0] = atan2( DCM[1][2], DCM[2][2] );
#endif

	THETAe[1] = -asin( DCM[0][2] );
#ifdef AHRS_3D
	THEATe[2] = atan2( DCM[0][1], DCM[0][0] );
#endif
}
#endif


void
dcv2euler(
	float *			THETAe,
	const float *		DCV
)
{
#ifdef __AVR__
	THETAe[0] = atan2( DCV[2], DCV[1] );
#else
	THETAe[0] = atan2( DCV[1], DCV[2] );
#endif

	THETAe[1] = -asin( DCV[0] );
}


void
quat2euler(
	float *			THETAe,
	const float *		quat
)
{
#ifdef __AVR__
	THETAe[0] = atan2(
		1 - 2 * (quat[1] * quat[1] + quat[2] * quat[2] ),
		    2 * (quat[2] * quat[3] + quat[0] * quat[1] )
	);
#else
	THETAe[0] = atan2(
		    2 * (quat[2] * quat[3] + quat[0] * quat[1] ),
		1 - 2 * (quat[1] * quat[1] + quat[2] * quat[2] )
	);
#endif

	THETAe[1] = -asin(
		    2 * (quat[1] * quat[3] - quat[0] * quat[2] )
	);

	/* 3D case has this extra term.  We ignore it for now */
#ifdef AHRS_3D
	THETAe[2] = atan2(
		    2 * (quat[1] * quat[2] + quat[0] * quat[3] ),
		1 - 2 * (quat[2] * quat[2] + quat[3] * quat[3] )
	);
#endif
}

	
static inline float
limit(
	const float		f,
	const float		min,
	const float		max
)
{
	if( f < min )
		return min;
	if( f > max )
		return max;
	return f;
}


void
accel2euler(
	float *			THETAm,
	const float *		accel
)
{
	static const float	g = 9.78;
		
	THETAm[0] = -asin( limit(  accel[1] / g, -1, 1 ) );
	THETAm[1] = -asin( limit( -accel[0] / g, -1, 1 ) );
#ifdef AHRS_3D
	THEATm[2] = heading;
#endif
}


void
euler2quat(
	float *			quat,
	const float *		euler
)
{
#ifdef AHRS_3D
	const float		phi	= euler[0] / 2.0;
	const float		theta	= euler[1] / 2.0;
	const float		psi     = euler[2] / 2.0;

	const float		shphi0	= sin( phi );
	const float		chphi0	= cos( phi );

	const float		shtheta0 = sin( theta );
	const float		chtheta0 = cos( theta );

	const float		shpsi0	= sin( psi );
	const float		chpsi0	= cos( psi );

	quat[0] =  chphi0 * chtheta0 * chpsi0 + shphi0 * shtheta0 * shpsi0;
	quat[1] = -chphi0 * shtheta0 * shpsi0 + shphi0 * chtheta0 * chpsi0;
	quat[2] =  chphi0 * shtheta0 * chpsi0 + shphi0 * chtheta0 * shpsi0;
	quat[2] =  chphi0 * chtheta0 * shpsi0 - shphi0 * shtheta0 * chpsi0;
#else
	const float		phi	= euler[0] / 2.0;
	const float		theta	= euler[1] / 2.0;

	const float		shphi0	= sin( phi );
	const float		chphi0	= cos( phi );

	const float		shtheta0 = sin( theta );
	const float		chtheta0 = cos( theta );

	quat[0] =  chphi0 * chtheta0;
	quat[1] =  shphi0 * chtheta0;
	quat[2] =  chphi0 * shtheta0;
	quat[2] = -shphi0 * shtheta0;
#endif
}

