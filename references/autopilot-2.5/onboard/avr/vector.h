/* -*- indent-tabs-mode:T; c-basic-offset:8; tab-width:8; -*- vi: set ts=8:
 * $Id: vector.h,v 2.0 2002/09/22 02:10:16 tramm Exp $
 *
 * Inlined minimal set of vector routines
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

#ifndef _VECTOR_H_
#define _VECTOR_H_

#ifdef AVR
#include <string-avr.h>
#include <io.h>
#else
#include <string.h>
#include <stdint.h>
#endif

/*
 * Timing numbers for double precision floating point values:
 *
 *  a *= b:		550 instructions
 *  a /= b:		520 instructions
 *  a += b:		1089 .. 1215 instructions
 *  a = sin(b):		5570 instructions
 *  c = atan2(a,b):	4460 instructions
 *
 * With the 4 Mhz clock, divide by 4 to get the number of microseconds
 * per operation.
 */

typedef double			f_t;
typedef f_t			v_t[4];
typedef v_t			m_t[4];
typedef uint8_t			iter_t;


static inline void
v_zero(
	v_t			c
)
{
	memset( c, 0, sizeof(v_t) );
}


static inline void
v_add(
	v_t 			c,
	const v_t 		a,
	const v_t 		b,
	iter_t			n
)
{
	do {
		--n;
		c[n] = a[n] + b[n];
	} while( n > 0 );
}


static inline void
v_sub(
	v_t 			c,
	const v_t 		a,
	const v_t 		b,
	iter_t			n
)
{
	do {
		--n;
		c[n] = a[n] - b[n];
	} while( n > 0 );
}


static inline void
v_scale(
	v_t			c,
	const v_t		a,
	f_t			s,
	iter_t			n
)
{
	do {
		--n;
		c[n] = a[n] * s;
	} while( n > 0 );
}



/**
 *  Matrix operations
 */
static inline void
m_zero(
	m_t			c
)
{
	memset( c, 0, sizeof(m_t) );
}


/*
 * Adds two matrices: c = a + b
 *
 * Safe to call with c == a and c == b
 * Requires 2300 instructions (580 useconds)
 */
static inline void
m_add(
	m_t 			c,
	m_t 			a,
	m_t			b,
	iter_t			m,
	iter_t			n
)
{
	do {
		n--;
		v_add( c[n], a[n], b[n], m );
	} while( n > 0 );
}


static inline void
m_sub(
	m_t 			c,
	m_t 			a,
	m_t			b,
	iter_t			m,
	iter_t			n
)
{
	do {
		n--;
		v_sub( c[n], a[n], b[n], m );
	} while( n > 0 );
}


static inline void
m_scale(
	m_t			c,
	m_t			a,
	f_t			s,
	iter_t			m,
	iter_t			n
)
{
	do {
		n--;
		v_scale( c[n], a[n], s, m );
	} while( n > 0 );
}



/**
 *  vc = ma * vb
 *
 * Unsafe to call with vc == vb
 */
static inline void
mv_mult(
	v_t			vc,
	m_t			ma,
	const v_t		vb,
	iter_t			m,
	iter_t			n
)
{
	iter_t			i;
	iter_t			j;

	for( j=0 ; j<m ; ++j )
	{
		f_t			sum = (f_t) 0;
		const f_t *		row = &ma[j][0];

		for( i=0 ; i<n ; ++i )
			sum += row[i] * vb[i];

		vc[j] = sum;
	}
}


/**
 *  vc = va * mb
 *
 * Unsafe to call with vc == va
 */
static inline void
vm_mult(
	v_t			vc,
	v_t			va,
	m_t			mb,
	iter_t			m,
	iter_t			n
)
{
	iter_t			i;
	iter_t			j;

	for( i=0 ; i<n ; ++i )
	{
		f_t			sum = (f_t) 0;

		for( j=0 ; j<m ; j++ )
			sum += va[j] * mb[j][i];

		vc[j] = sum;
	}
}


/**
 *  c = a * b
 *
 * Unsafe for c == a or c == b.
 *
 * Optimal form runs in 1/2 the time of the simple form due to
 * loop constant hoisting and others.
 *
 * Requires 14630 instructions (3650 usec)
 */
static inline void
m_mult(
	m_t			c,
	m_t			a,
	m_t			b,
	iter_t			m,
	iter_t			n,
	iter_t			p
)
{
	iter_t			i;
	iter_t			j;
	iter_t			k;

#ifdef SIMPLE
	m_zero( c );

	for( i=0 ; i<m ; ++i )
		for( j=0 ; j<p ; ++j )
			for( k=0 ; k<n ; ++k )
				c[i][j] += a[i][k] * b[k][j];
#else
	for( i=0 ; i<m ; ++i )
	{
		for( j=0 ; j<p ; ++j )
		{
			f_t		sum = (f_t) 0;
			const f_t *	A = &a[i][0];

			for( k=0 ; k<n ; ++k )
				sum += A[k] * b[k][j];
			c[i][j] = sum;
		}
	}
#endif

}


/**
 *  transpose is safe to call with c == a
 */
static inline void
m_transpose(
	m_t			c,
	m_t			a,
	iter_t			m,
	iter_t			n
)
{
	iter_t			i;
	iter_t			j;

	for( i=0 ; i<m ; ++i )
	{
		for( j=i ; j<n ; j++ )
		{
			f_t			u = a[i][j];
			f_t			l = a[j][i];

			c[j][i] = u;
			c[i][j] = l;
		}
	}
}


/*
 *  Produce the determinant of a 3x3 matrix
 */
static inline f_t
m33_det(
	m_t			m
)
{
	return	m[0][0] * m[1][1] * m[2][2]
	- 	m[0][0] * m[1][2] * m[2][1]
	-	m[0][1] * m[1][0] * m[2][2]
	+	m[0][1] * m[1][2] * m[2][0]
	+	m[0][2] * m[1][0] * m[2][1]
	-	m[0][2] * m[1][1] * m[2][0]
	;
}



/**
 *  m33_inv will invert a 3x3 matrix.
 *
 * Unsafe to call with c == a
 *
 * Details were copied from:
 *
 *	http://mathworld.wolfram.com/MatrixInverse.html
 *
 * Requires 16478 instructions (4120 useconds)
 */
static inline void
m33_inv(
	m_t			D,
	m_t			m
)
{
	D[0][0] = m[1][1] * m[2][2] - m[1][2] * m[2][1];
	D[0][1] = m[0][2] * m[2][1] - m[0][1] * m[2][2];
	D[0][2] = m[0][1] * m[1][2] - m[0][2] * m[1][1];

	D[1][0] = m[1][2] * m[2][0] - m[1][0] * m[2][2];
	D[1][1] = m[0][0] * m[2][2] - m[0][2] * m[2][0];
	D[1][2] = m[0][2] * m[1][0] - m[0][0] * m[1][2];

	D[2][0] = m[1][0] * m[2][1] - m[1][1] * m[2][0];
	D[2][1] = m[0][1] * m[2][0] - m[0][0] * m[2][1];
	D[2][2] = m[0][0] * m[1][1] - m[0][1] * m[1][0];

	m_scale( D, D, 1.0 / m33_det(m), 3, 3 );
}

#ifndef AVR
#include <stdio.h>

static inline void
m_print(
	const char *		label,
	m_t			a,
	iter_t			m,
	iter_t			n
)
{
	iter_t			i;
	iter_t			j;

	printf( "%s=", label );
	for( i=0 ; i<m ; i++ )
	{
		printf( "[" );
		for( j=0 ; j<n ; j++ )
			printf( " %lf", a[i][j] );
		printf( "]\n" );
	}
}

#endif

#endif
