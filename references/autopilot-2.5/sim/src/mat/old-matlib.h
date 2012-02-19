/* -*- indent-tabs-mode:T; c-basic-offset:8; tab-width:8; -*- vi: set ts=8:
 * $Id: old-matlib.h,v 2.0 2002/09/22 02:07:32 tramm Exp $
 *
 * This file contains some useful matrix and general
 * math utilities.  The maximum size of the matrix which
 * can be used is set at 11x11.  
 *
 * Author: Aaron Kahn, Suresh Kannan, Eric Johnson
 * copyright 2001
 *
 * Portions (c) Trammell Hudson
 *
 *************
 *
 *  This file is part of the autopilot simulation package.
 *
 *  For more details:
 *
 *      http://autopilot.sourceforge.net/
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

#ifndef _MATLIB_H_
#define _MATLIB_H_

/******************************************************
*

* @pkg			matlib
* @version		$Revision: 2.0 $
*
*******************************************************/

#define MAXSIZE			16


/********************************************************************/
/************                                   *********************/
/************ MATRIX MATH FUNCTION DECLERATIONS *********************/
/************                                   *********************/
/********************************************************************/

#include "old-vect.h"
#include "old-quat.h"
#include "old-nav_funcs.h"


namespace matlib
{


/* This will zero out the matrix A
zeros(n,m) = A
*/
void Minit(double A[MAXSIZE][MAXSIZE], int n, int m);


/* This will generate an identity matrix I
eye(n) = A(n,n)
*/
void eye(double I[MAXSIZE][MAXSIZE], int n);

/* This will multiply scalar value s will all elements of matrix A(m,n)
* placing result into matrix B(m,n)
* s.*A(m,n) = B(m,n)
*/
void sMmult(double s, double A[MAXSIZE][MAXSIZE], double B[MAXSIZE][MAXSIZE], int m, int n);

/* This will multiply matrix A and matrix B return in matrix C
A(m,n)*B(n,p) = C(m,p)
*/
void MMmult(double A[MAXSIZE][MAXSIZE], double B[MAXSIZE][MAXSIZE], double C[MAXSIZE][MAXSIZE], int m, int n, int p);

/* This will multiply matrix A and vector b return in vector b
A(m,n)*b(n,1) = c(m,1)
*/
extern void
MVmult(
	double			A[MAXSIZE][MAXSIZE],
	const double *		b,
	double *		c,
	int			m,
	int			n
);

extern void
MMcopy(
	double			A[MAXSIZE][MAXSIZE],
	double			B[MAXSIZE][MAXSIZE],
	int			m,
	int			n
);

/* This will multiply vector a and matrix B return in vector c
a(1,m)*B(m,n) = c(1,n)
*/
void VMmult(double a[MAXSIZE], double B[MAXSIZE][MAXSIZE], double c[MAXSIZE], int m, int n);

/* This will traspose a matrix/vector A return in matrix/vector B
A(m,n) = B(n,m)
*/
void transpose(double A[MAXSIZE][MAXSIZE], double B[MAXSIZE][MAXSIZE], int m, int n);

/* This will add matrix A and matrix B return in matrix C
A(n,m) + B(n,m) = C(n,m)
*/
void MMadd(double A[MAXSIZE][MAXSIZE], double B[MAXSIZE][MAXSIZE], double C[MAXSIZE][MAXSIZE], int m, int n);

/* This will subtract matrix A from matrix B placing result in matrix C
* A(n,m) - B(n,m) = C(n,m)
*/
void MMsub(double A[MAXSIZE][MAXSIZE], double B[MAXSIZE][MAXSIZE], double C[MAXSIZE][MAXSIZE], int m, int n);

/* This will perform LU decomp on matrix A return matrix L, matrix U
lu(A(n,n)) => L(n,n) and U(n,n)
*/
void LU(double A[MAXSIZE][MAXSIZE], double L[MAXSIZE][MAXSIZE], double U[MAXSIZE][MAXSIZE], int n);

/* This will perform the inverse on matrix A return in matrix B
inv(A(n,n)) = B(n,n)
*/
void inv(double A[MAXSIZE][MAXSIZE], double B[MAXSIZE][MAXSIZE], int n);

/* This will solve A*x = b, where matrix A is upper triangular
A(n,n)*x(n,1) = b(n,1)
*/
void solveupper(double A[MAXSIZE][MAXSIZE], double b[MAXSIZE], double x[MAXSIZE], int n);

/* This will solve A*x = b, where matrix A is lower triangular
A(n,n)*x(n,1) = b(n,1)
*/
void solvelower(double A[MAXSIZE][MAXSIZE], double b[MAXSIZE], double x[MAXSIZE], int n);

/* This will take column n from matrix A place in vector a
A(:,n) = a(m,1)
m = number of rows
*/
void Mcol(double A[MAXSIZE][MAXSIZE], double a[MAXSIZE], int n, int m);

/* This will take vector a and place into column of matrix A
a(:,1) = A(m,n)
m = number of rows
*/
void Vcol(double a[MAXSIZE], double A[MAXSIZE][MAXSIZE], int n, int m);

/* This will print matrix A to the screen
A(n,m)
*/
void Mprint(double A[MAXSIZE][MAXSIZE], int n, int m);

/* This will take matrix A and insert it as a block into matrix B. A(0,0) will be
placed in B(r,c)
A(n,m) -> B(>n, >m)(r,c)
r=0, c=0 == B(0,0)
*/
void block2M(double A[MAXSIZE][MAXSIZE], double B[MAXSIZE][MAXSIZE], int n, int m, int r, int c);

/* This will perform a Kalman filter Gain, state, and coveriance matrix update.
* What is needed is the linear A matrix, state vector, C matrix, P coveriance matrix,
* measurement vector, and the R matrix.
*
*	A(n,n)		Linear model
*	P(n,n)		Coveriance matrix
*	X(n,1)		State Vector
*	C(m,n)		Measurement matrix; m=# of measurements, n=# of states
*	R(m,n)		Measurement weight matrix 
*	err(m,1)	err = Xmeasurement(m,1) - Xestimate(m,1) vector
*	K(n,m)		Kalman Gain matrix
*/
void kalmanUpdate(double A[MAXSIZE][MAXSIZE], double P[MAXSIZE][MAXSIZE], \
				  double X[MAXSIZE], double C[MAXSIZE][MAXSIZE], double R[MAXSIZE][MAXSIZE], \
				  double err[MAXSIZE], double K[MAXSIZE][MAXSIZE], \
				  int n, int m);

/*	This will compute the dependant value from an independant value based on a 
* linear function.  The linear function is defined by a set of independant and
* dependant points (x and y in cartisian plane).
*
*	RETURNS		dependant value (y on cartisian plane)
*	x			independant value (x on cartisian plane)
*	Y1			dependant component of point 1 (y on cartisian plane)
*	Y2			dependant component of point 2 (y on cartisian plane)
*	X1			independant component of point 1 (x on cartisian plane)
*	X2			independant component of point 2 (x on cartisian plane)
*/
double line(double x, double Y1, double Y2, double X1, double X2);


/*
 * This will compute an orthoginal set of vectors from a given set
 * of vectors.  The seed set of column vectors are arranged as columns
 * of a matrix with full column rank.  The output vectors are arranged
 * as columns of the output matrix with full column rank.
 *
 * A(n,[V1 V2 ... Vm]) --> Q(n,[V1 V2 ... Vm])
 */
extern void
mgs(
	double			A[MAXSIZE][MAXSIZE],
	double			Q[MAXSIZE][MAXSIZE],
	int			n,
	int			m
);


/*	This will produce a hysterious effect on the given value if given the
* previous value and the amount of hysterious play.  The use of this
* function is to simulate slop in linkage and gear chains.
*
*	RETURNS		new value with hysterious
*	old_val		previous value
*	current_val	current value to modify
*	play		the magnitude of play to allow (ie: +/- play)
*/
double hyst(double current_val, double old_val, double play);


/*
 *  Rotate a vector from one coord system to another.
 */
extern void
rotate3(
	double *		v_out,
	const double *		v_in,
	const double *		theta
);


/*
 *  Rotate a two dimensional vector by a heading
 */
extern void
rotate2(
	double *		v_out,
	const double *		v_in,
	const double		theta
);


}

#endif
