/* -*- indent-tabs-mode:T; c-basic-offset:8; tab-width:8; -*- vi: set ts=8:
 * $Id: old-vect.h,v 2.0 2002/09/22 02:07:32 tramm Exp $
 *
 * Vector manipulation routines
 *
 *	Author: Aaron Kahn, Suresh Kannan, Eric Johnson
 *	copyright 2001
 *	Portions (c) Trammell Hudson
 */

#ifndef _VECT_H_
#define _VECT_H_

namespace matlib
{

/*
 * This will zero out the vector a
 * zeros(n,1) = a
 */
extern void
Vinit(
	double *		a,
	const int		n
);


/*
 * This will multiply scalar s will all the elements of vector a(n)
 * placing the result in vector b(n)
 * s.*a(n) = b(n)
 *
 * Ok for a == b.
 */
extern void
sVmult(
	double			s,
	const double *		a,
	double *		b,
	const int		n
);


/*
 * This will add vector a and vector b return vector c
 * a(n,1) + b(n,1) = c(n,1)
 *
 * Safe for c == a and c == b.
 */
extern void
VVadd(
	const double *		a,
	const double *		b,
	double *		c,
	const int		n
);


/*
 * This will subtract vector a(n) from vector b(n) placing result
 * in vector c(n)
 * a(n,1) - b(n,1) = c(n,1)
 *
 * Safe for c == a and c == b.
 */
extern void
VVsub(
	const double *		a,
	const double *		b,
	double *		c,
	const int		n
);


/*
 * This will take the dot product of two vectors a and b, and return
 * the result.
 *       T
 * a(n,1) b(n,1) = c(1,1)
 */
extern double
dot(
	const double *		a,
	const double *		b,
	const int		n
);


/*
 * This will take the cross product of two vectors a,b and return
 * in vector c
 * a(3,1) X b(3,1) = c(3,1)
 */
extern void
cross(
	const double *		a,
	const double *		b,
	double *		c
);


/*
 * This will compute the norm of a vector v(n,1)
 * norm(v(n,1)) = r
 */
extern double
norm(
	const double *		v,
	const int		n
);

}

#endif
