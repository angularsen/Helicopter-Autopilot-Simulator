/* -*- indent-tabs-mode:T; c-basic-offset:8; tab-width:8; -*- vi: set ts=8:
 * $Id: old-matlib.cpp,v 2.0 2002/09/22 02:07:32 tramm Exp $
 *
 *	This is the source file for the matrix and math utiltiy
 *	library.
 *
 *	Author: Aaron Kahn, Suresh Kannan, Eric Johnson
 *	copyright 2001
 *	Portions (c) Trammell Hudson
 */

#include <stdio.h>
#include <cmath>
#include <string.h>

#include "old-matlib.h"
#include "old-vect.h"
#include <mat/Conversions.h>
#include "macros.h"

namespace matlib
{

/*
 * This will zero out the matrix A
 * zeros(n,m) = A
 */
void
Minit(
	double			A[MAXSIZE][MAXSIZE],
	int			n,
	int			m
)
{
	for( int i=0 ; i<n ; ++i )
		for( int j=0 ; j<m ; ++j )
			A[i][j] = 0.0;
}


/*
 * This will generate an identity matrix I
 * eye(n) = A(n,n)
 */
void
eye(
	double			I[MAXSIZE][MAXSIZE],
	int			n
)
{
	Minit(I,n,n);
	for( int i=0 ; i<n ; ++i )
		I[i][i] = 1.0;
}


/*
 * This will multiply scalar value s will all elements of matrix A(m,n)
 * placing result into matrix B(m,n)
 * s.*A(m,n) = B(m,n)
 *
 * Ok for A == B.
 */
void
sMmult(
	double			s,
	double			A[MAXSIZE][MAXSIZE],
	double			B[MAXSIZE][MAXSIZE],
	int			m,
	int			n
)
{
	for( int i=0 ; i<m ;  ++i )
		for( int j=0 ; j<n ; ++j )
			B[i][j] = s*A[i][j];
}


/*
 * This will multiply matrix A and matrix B return in matrix C
 * A(m,n)*B(n,p) = C(m,p)
 */
void
MMmult(
	double			A[MAXSIZE][MAXSIZE],
	double			B[MAXSIZE][MAXSIZE],
	double			C[MAXSIZE][MAXSIZE],
	int			m,
	int			n,
	int			p
)
{
	for( int i=0 ; i<m ; ++i )
	{
		for( int j=0 ; j<p ; ++j )
		{
			double			s = 0.0;

			for( int k=0 ; k<n ; ++k )
				s += A[i][k] * B[k][j];

			C[i][j] = s;
		}
	}
}


/*
 * This will multiply matrix A and vector b return in vector b
 * A(m,n)*b(n,1) = c(m,1)
 */
void
MVmult(
	double			A[MAXSIZE][MAXSIZE],
	const double *		b,
	double *		c,
	int			m,
	int			n
)
{
	for( int j=0 ; j<m ; ++j )
	{
		double			s = 0.0;

		for( int i=0 ; i<n ; ++i )
			s += A[j][i] * b[i];

		c[j] = s;
	}
}


/*
 * This will multiply vector a and matrix B return in vector c
 * c(1,n) = a(1,m)*B(m,n)
 */
void
VMmult(
	double			a[MAXSIZE],
	double			B[MAXSIZE][MAXSIZE],
	double			c[MAXSIZE],
	int			m,
	int			n
)
{
	for( int i=0 ;  i<n ;  ++n)
	{
		double			s = 0.0;

		for( int j=0 ; j<m ; ++m )
			s += a[j]*B[j][i];

		c[i] = s;
	}
}


/*
 * This will traspose a matrix/vector A return in matrix/vector B
 * A(m,n) = B(n,m)
 */
void
transpose(
	double			A[MAXSIZE][MAXSIZE],
	double			B[MAXSIZE][MAXSIZE],
	int			m,
	int			n
)
{
	for( int i=0 ; i<m ; ++i )
		for( int j=0 ; j<n ; ++j )
			B[j][i] = A[i][j];
}


void
MMcopy(
	double			A[MAXSIZE][MAXSIZE],
	double			B[MAXSIZE][MAXSIZE],
	int			m,
	int			n
)
{
	for( int i=0 ; i<m ; i++ )
		for( int j=0 ; j<n ; j++ )
			B[i][j] = A[i][j];
}


/*
 * This will add matrix A and matrix B return in matrix C
 * C(n,m) = A(n,m) + B(n,m)
 *
 * Ok for C == A and C == B
 */
void
MMadd(
	double			A[MAXSIZE][MAXSIZE],
	double			B[MAXSIZE][MAXSIZE],
	double			C[MAXSIZE][MAXSIZE],
	int			m,
	int			n
)
{
	for( int i=0 ; i<n ; ++i )
		for( int j=0 ; j<m ; ++j )
			C[i][j] = A[i][j] + B[i][j];
}


/*
 * This will subtract matrix A from matrix B placing result in matrix C
 * C(n,m) = A(n,m) - B(n,m)
 *
 * Ok for C == A and C == B.
 */
void
MMsub(
	double			A[MAXSIZE][MAXSIZE],
	double			B[MAXSIZE][MAXSIZE],
	double			C[MAXSIZE][MAXSIZE],
	int			m,
	int			n
)
{
	for( int i=0 ;  i<n ;  ++i )
		for( int j=0 ; j<m ; ++j )
			C[i][j] = A[i][j] - B[i][j];
}


/*
 * This will perform LU decomp on matrix A return matrix L, matrix U
 * lu(A(n,n)) => L(n,n) and U(n,n)
 *
 * the LU decomp algorithm for no pivoting
 * for k=1:n-1
 *   for i=k+1:n
 *      A(i,k)=A(i,k)/A(k,k);
 *      for j=k+1:n
 *         A(i,j)=A(i,j)-A(i,k)*A(k,j);
 *      end
 *   end
 * end
 */
void
LU(
	double			A[MAXSIZE][MAXSIZE],
	double			L[MAXSIZE][MAXSIZE],
	double			U[MAXSIZE][MAXSIZE],
	int			n
)
{
	double			Acopy[MAXSIZE][MAXSIZE];

	/* copy A matrix */
	for( int i=0 ;  i<n ; ++i )
		for( int j=0 ; j<n ;  ++j )
			Acopy[i][j] = A[i][j];


	for( int k=0 ; k<n-1 ; ++k )
	{
		for( int i=k+1 ; i<n ; ++i )
		{
			Acopy[i][k] = Acopy[i][k] / Acopy[k][k];

			for( int j=k+1 ; j<n ; ++j )
			{
				Acopy[i][j] -= Acopy[i][k] * Acopy[k][j];
			}
		}
	}

	
	/* make an identity matrix */
	eye( L, n );
	
	/* separate the L matrix */
	for( int j=0 ; j<n-1 ; ++j )
		for( int i=j+1 ; i<n ; ++i )
			L[i][j] = Acopy[i][j];

	/* separate out the U matrix */
	Minit( U, n, n );
	for( int i=0 ; i<n ; ++i )
		for( int j=i ; j<n ; ++j )
			U[i][j] = Acopy[i][j];
}


/*
 * This will perform the inverse on matrix A return in matrix B
 * inv(A(n,n)) = B(n,n)
 */
void
inv(
	double			A[MAXSIZE][MAXSIZE],
	double			B[MAXSIZE][MAXSIZE],
	int			n
)
{
	if( n == 1)
	{
		B[0][0] = 1.0/A[0][0];
		return;
	}

	if( n == 2 )
	{
		double detA = A[0][0] * A[1][1] - A[0][1] * A[1][0];
		B[0][0] =  A[1][1] / detA;
		B[0][1] = -A[0][1] / detA;
		B[1][0] = -A[1][0] / detA;
		B[1][1] =  A[0][0] / detA;

		return;
	}

	// Should we special case n==3?

	/*
	 * General case.  Do an LU thingy
	 */
	double			identCol[MAXSIZE];
	double			ident[MAXSIZE][MAXSIZE];
	double			L[MAXSIZE][MAXSIZE];
	double			U[MAXSIZE][MAXSIZE];
	double			invUcol[MAXSIZE];
	double			invLcol[MAXSIZE];
	double			invU[MAXSIZE][MAXSIZE];
	double			invL[MAXSIZE][MAXSIZE];


	/* construct an identity matrix */
	eye( ident, n );
	
	/* perform LU decomp on A */
	LU( A, L, U, n );

	for( int i=0 ; i<n ; ++i )
	{
		/* separates the ith column */
		Mcol( ident, identCol, i, n );

		solveupper( U, identCol, invUcol, n );
		solvelower( L, identCol, invLcol, n );

		/* places invUcol in ith column of invU */
		Vcol( invUcol, invU, i, n );

		/* places invLcol in ith column of invL */
		Vcol( invLcol, invL, i, n );
	}

	/* inv(A) = inv(U)*inv(L) */
	MMmult( invU, invL, B, n, n, n );
}

	

/*
 * This will solve A*x = b, where matrix A is upper triangular
 * A(n,n)*x(n,1) = b(n,1)
 */
void
solveupper(
	double			A[MAXSIZE][MAXSIZE],
	double			b[MAXSIZE],
	double			x[MAXSIZE],
	const int		n
)
{
	const int		p = n + 1;

	for( int i=1 ; i<=n ; ++i )
	{
		x[p-i-1] = b[p-i-1];

		for( int j=(p+1-i) ; j<=n ; ++j )
			x[p-i-1] -= A[p-i-1][j-1]*x[j-1];

		x[p-i-1] = x[p-i-1] / A[p-i-1][p-i-1];
	}
}


/*
 * This will solve A*x = b, where matrix A is lower triangular
 * A(n,n)*x(n,1) = b(n,1)
 */
void
solvelower(
	double			A[MAXSIZE][MAXSIZE],
	double			b[MAXSIZE],
	double			x[MAXSIZE],
	int			n
)
{
	int i,j;

	for(i=1; i<=n; ++i)
	{
		x[i-1] = b[i-1];
		for(j=1; j<=i-1; ++j)
		{
			x[i-1] = x[i-1] - A[i-1][j-1]*x[j-1];
		}
		x[i-1] = x[i-1]/A[i-1][i-1];
	}
	
}

/* This will take column n from matrix A place in vector a
A(:,n) = a(m,1)
m = number of rows
*/
void Mcol(double A[MAXSIZE][MAXSIZE], double a[MAXSIZE], int n, int m)
{
	int i;
	for(i=0; i<m; ++i)
	{
		a[i] = A[i][n];
	}
}


/* This will take vector a and place into column of matrix A
a(:,1) = A(m,n)
m = number of rows
*/
void Vcol(double a[MAXSIZE], double A[MAXSIZE][MAXSIZE], int n, int m)
{
	int i;
	for(i=0; i<m; ++i)
	{
		A[i][n] = a[i];
	}
}

/* This will print matrix A to the screen
A(n,m)
*/
void Mprint(double A[MAXSIZE][MAXSIZE], int n, int m)
{
	int i,j;
	for(i=0; i<n; ++i)
	{
		for(j=0; j<m; ++j)
		{
			printf("%7.5lf ",A[i][j]);
		}
		printf("\n");
	}
	printf("\n");
}


/* This will take matrix A and insert it as a block into matrix B. A(0,0) will be
placed in B(r,c)
A(n,m) -> B(>n, >m)(r,c)
r=0, c=0 == B(0,0)
*/
void
block2M(
	double			A[MAXSIZE][MAXSIZE],
	double			B[MAXSIZE][MAXSIZE],
	int			n,
	int			UNUSED( m ),
	int			r,
	int			c
)
{
	for( int i=0 ; i<n ; ++i)
		for( int j=0 ; j<n ; ++j)
			B[i+r][j+c] = A[i][j];
}


/*
 * This will perform a Kalman filter Gain, state, and coveriance
 * matrix update.
 *
 * What is needed is the linear A matrix, state vector, C matrix,
 * P coveriance matrix, measurement vector, and the R matrix.
 *
 *	A(n,n)		Linear model
 *	P(n,n)		Coveriance matrix
 *	X(n,1)		State Vector
 *	C(m,n)		Measurement matrix; m=# of measurements, n=# of states
 *	R(m,n)		Measurement weight matrix 
 *	err(m,1)	err = Xmeasurement(m,1) - Xestimate(m,1) vector
 *	K(n,m)		Kalman Gain matrix
 */
void
kalmanUpdate(
	double			/* A */[MAXSIZE][MAXSIZE],
	double			P[MAXSIZE][MAXSIZE],
	double			X[MAXSIZE],
	double			C[MAXSIZE][MAXSIZE],
	double			R[MAXSIZE][MAXSIZE],
	double			err[MAXSIZE],
	double			K[MAXSIZE][MAXSIZE],
	int			n,
	int			m
)
{
	double			E[MAXSIZE][MAXSIZE];
	double			T1[MAXSIZE][MAXSIZE];	// temp matrix 1
	double			T2[MAXSIZE][MAXSIZE];	// temp matrix 2
	double			TV1[MAXSIZE];		// temp vector 1
	
	// perform E = C*P*C' + R 
	MMmult(C,P,T1,m,n,n);
	transpose(C,T2,m,n);
	MMmult(T1,T2,E,m,n,m);
	MMadd(E,R,E,m,m);
	
	// perform K = P*C'*inv(E) 
	transpose(C,T2,m,n);
	MMmult(P,T2,T1,n,n,m);
	inv(E,T2,m);
	MMmult(T1,T2,K,n,m,m);
	
	// perform x = x + K*( ys - yp )
	MVmult(K,err,TV1,n,m);
	VVadd(X,TV1,X,n);
	
	
	// perform P = P - K*C*P 
	MMmult(K,C,T1,n,m,n);
	MMmult(T1,P,T2,n,n,n);
	MMsub( P, P, T2, n, n );
}


/*
 * This will compute an orthoginal set of vectors from a given set
 * of vectors.  The seed set of column vectors are arranged as columns
 * of a matrix with full column rank.  The output vectors are arranged
 * as columns of the output matrix with full column rank.
 *
 * A(n,[V1 V2 ... Vm]) --> Q(n,[V1 V2 ... Vm])
 */
void
mgs(
	double			A[MAXSIZE][MAXSIZE],
	double			Q[MAXSIZE][MAXSIZE],
	int			n,
	int			m
)
{
	double			Tv1[MAXSIZE];
	double			Tv2[MAXSIZE];

	/* copy A matrix */
	for( int i=0 ; i<n ; ++i )
		for( int j=0 ; j<m ; ++j )
			Q[i][j] = A[i][j];

	for( int i=0 ; i<m ; ++i )
	{
		Mcol(Q, Tv1, i, n);

		double			r = norm( Tv1, n );

		for( int k=0 ; k<n ; ++k )
			Q[k][i] = Q[k][i]/r;

		for( int j=i+1 ; j<n ; ++j )
		{
			Mcol(Q, Tv1, i, n);
			Mcol(Q, Tv2, j, n);

			double			r = dot( Tv1, Tv2, n );

			for( int k=0 ; k<n ; ++k )
				Q[k][j] = Q[k][j] - r*Q[k][i];
		}
	}
}



/*
 * This will compute the dependant value from an independant value
 * based on a linear function.  The linear function is defined by a
 * set of independant and dependant points (x and y in cartisian plane).
 *
 *	RETURNS	dependant value (y on cartisian plane)
 *	x	independant value (x on cartisian plane)
 *	Y1	dependant component of point 1 (y on cartisian plane)
 *	Y2	dependant component of point 2 (y on cartisian plane)
 *	X1	independant component of point 1 (x on cartisian plane)
 *	X2	independant component of point 2 (x on cartisian plane)
 */
double
line(
	double			x,
	double			Y1,
	double			Y2,
	double			X1,
	double			X2
)
{
	double			m = (Y1 - Y2) / (X1 - X2);

	return m * (x - X2) + Y2;
}
		

/*
 * This will produce a hysterious effect on the given value if given the
 * previous value and the amount of hysterious play.  The use of this
 * function is to simulate slop in linkage and gear chains.
 *
 *	RETURNS		new value with hysterious
 *	old_val		previous value
 *	current_val	current value to modify
 *	play		the magnitude of play to allow (ie: +/- play)
 */
double
hyst(
	double			current_val,
	double			old_val,
	double			play
)
{
	if( current_val >= old_val+play )
		return current_val - play;

	if( current_val <= old_val-play )
		return current_val + play;

	return old_val;
}


/**
 *  Rotate a vector from one coord system to another.
 */
void
rotate3(
	double *		v_out,
	const double *		v_in,
	const double *		theta
)
{
	double			cBE[MAXSIZE][MAXSIZE];

	eulerDC(
		cBE,
		theta[0],
		theta[1],
		theta[2]
	);

	MVmult(
		cBE,
		(double*) v_in,
		v_out,
		3, 3
	);
}

/*
 *  Rotate a vector by one angle.  This is not thread safe, but
 * fast for repeatedly rotating about the same angle.
 */
void
rotate2(
	double *		v_out,
	const double *		v_in,
	const double 		theta
)
{
	static double		cos_theta = 1.0;
	static double		sin_theta = 0.0;
	static double		last_theta = 0.0;
	
	if( last_theta != theta )
	{
		last_theta	= theta;
		cos_theta	= cos(theta);
		sin_theta	= sin(theta);
	}

	v_out[0] = v_in[0] * cos_theta + v_in[1] * sin_theta;
	v_out[1] = v_in[1] * cos_theta - v_in[0] * sin_theta;
}

}
