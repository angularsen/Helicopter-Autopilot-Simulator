/* -*- indent-tabs-mode:T; c-basic-offset:8; tab-width:8; -*- vi: set ts=8:
 * $Id: old-vect.cpp,v 2.0 2002/09/22 02:07:32 tramm Exp $
 *
 * Vector manipulation routines
 *
 *	Author: Aaron Kahn, Suresh Kannan, Eric Johnson
 *	copyright 2001
 *	Portions (c) Trammell Hudson
 */

#include "old-vect.h"
#include <cmath>

namespace matlib
{

using std::sqrt;


/*
 * This will zero out the vector a
 * zeros(n,1) = a
 */
void
Vinit(
	double *		a,
	const int		n
)
{
	for( int i=0 ; i<n ; ++i )
		a[i] = 0.0;
}


/*
 * This will multiply scalar s will all the elements of vector a(n)
 * placing the result in vector b(n)
 * s.*a(n) = b(n)
 *
 * Ok for a == b.
 */
void
sVmult(
	const double		s,
	const double *		a,
	double *		b,
	const int		n
)
{
	for( int i=0 ; i<n ; ++i )
		b[i] = s*a[i];
}


/*
 * This will add vector a and vector b return vector c
 * a(n,1) + b(n,1) = c(n,1)
 *
 * Safe for c == a and c == b.
 */
void
VVadd(
	const double *		a,
	const double *		b,
	double *		c,
	const int		n
)
{
	for( int i=0 ;  i<n ;  ++i )
		c[i] = a[i] + b[i];
}


/*
 * This will subtract vector a(n) from vector b(n) placing result
 * in vector c(n)
 * a(n,1) - b(n,1) = c(n,1)
 *
 * Safe for c == a and c == b.
 */
void
VVsub(
	const double *		a,
	const double *		b,
	double *		c,
	const int		n
)
{
	for( int i=0 ; i<n ; ++i )
		c[i] = a[i] - b[i];
}



/*
 * This will take the dot product of two vectors a and b, and return the result.
 *       T
 * a(n,1) b(n,1) = c(1,1)
 */
double
dot(
	const double *		a,
	const double *		b,
	const int		n
)
{
	double			c = 0.0;

	for( int i=0 ; i<n ; ++i )
		c += a[i] * b[i];

	return c;
}


/*
 * This will take the cross product of two vectors a,b and return
 * in vector c
 * a(3,1) X b(3,1) = c(3,1)
 */
void
cross(
	const double *		a,
	const double *		b,
	double *		c
)
{

	c[0] = a[1]*b[2] - a[2]*b[1];
	c[1] = a[2]*b[0] - a[0]*b[2];
	c[2] = a[0]*b[1] - a[1]*b[0];
}


/*
 * This will compute the norm of a vector v(n,1)
 * norm(v(n,1)) = r
 */
double
norm(
	const double *		v,
	const int		n
)
{
	double			r = 0.0;
	for( int i=0 ; i<n ; ++i )
		r += v[i]*v[i];

	return sqrt(r);
}

}
