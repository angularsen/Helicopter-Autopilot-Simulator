/* -*- indent-tabs-mode:T; c-basic-offset:8; tab-width:8; -*- vi: set ts=8:
 * $Id: Matrix_Invert.h,v 2.1 2002/10/04 14:57:53 tramm Exp $
 *
 * (c) Aaron Kahn
 * (c) Trammell Hudson
 *
 * Square matrix helpers for the C++ Matrix class.
 * Don't include this unless you need it.  It will slow
 * you do immensely.
 *
 *************
 *
 *  This file is part of the autopilot simulation package.
 *
 *  For more details:
 *
 *	http://autopilot.sourceforge.net/
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
#ifndef _Matrix_Invert_h_
#define _Matrix_Invert_h_

#include <mat/Matrix.h>

namespace libmat
{

/**
 *  Make a square identity matrix
 */
template<
	const int		n,
	class			T
>
const Matrix<n,n,T>
eye()
{
	Matrix<n,n,T>		A;

	for( int i=0 ; i<n ; i++ )
		A[i][i] = T(1);
	return A;
}


/**
 *  Compute the LU factorization of a square matrix
 * A is modified, so we pass by value.  Sorry!
 */
template<
	const int		n,
	class			T
>
void LU(
	Matrix<n,n,T> 		A,
	Matrix<n,n,T> &		L,
	Matrix<n,n,T> &		U
)
{
	for( int k=0 ; k<n-1 ; k++ )
	{
		for( int i=k+1 ; i<n ; i++ )
		{
			A[i][k] = A[i][k] / A[k][k];

			for( int j=k+1 ; j<n ; j++ )
			{
#ifdef NO_FPU
				T &		A_i_j( A[i][j] );
				const T &	A_i_k( A[i][k] );
				const T &	A_k_j( A[k][j] );

				if( is_zero( A_i_k )
				||  is_zero( A_k_j )
				)
					continue;

				A_i_j -= A_i_k * A_k_j;
#else
				A[i][j] -= A[i][k] * A[k][j];
#endif
			}
		}
	}

	L = eye<n,T>();

	/* Separate the L matrix */
	for( int j=0 ; j<n-1 ; j++ )
		for( int i=j+1 ; i<n ; i++ )
			L[i][j] = A[i][j];

	/* Separate the M matrix */
	U.fill();
	for( int i=0 ; i<n ; i++ )
		for( int j=i ; j<n ; j++ )
			U[i][j] = A[i][j];
}


/**
 *  Invert a matrix using LU.
 * Special case for a 1x1 and 2x2 matrix first
 */
template<
	class			T
>
const Matrix<1,1,T>
invert(
	const Matrix<1,1,T> &	A
)
{
	Matrix<1,1,T>		B;
	B[0][0] = T(1) / A[0][0];
	return B;
}


template<
	class			T
>
const Matrix<2,2,T>
invert(
	const Matrix<2,2,T> &	A
)
{
	Matrix<2,2,T>		B;
	const T			det(
		A[0][0] * A[1][1] - A[0][1] * A[1][0]
	);

	B[0][0] =  A[1][1] / det;
	B[0][1] = -A[0][1] / det;
	B[1][0] = -A[1][0] / det;
	B[1][1] =  A[0][0] / det;

	return B;
}


template<
	const int		n,
	class			T
>
const Vector<n,T>
solve_upper(
	const Matrix<n,n,T> &	A,
	const Vector<n,T> &	b
)
{
	Vector<n,T>		x;

	for( int i=n-1 ; i>=0 ; i-- )
        {
		T			s( b[i] );
		const Vector<n,T> &	A_i( A[i] );

                for( int j=i+1 ; j<n ; ++j )
		{
#ifdef NO_FPU
			const T &	A_i_j( A_i[j] );
			const T &	x_j( x[j] );

			if( is_zero( A_i_j )
			||  is_zero( x_j )
			)
				continue;

                        s -= A_i_j * x_j;
#else
                        s -= A_i[j]*x[j];
#endif
		}

                x[i] = s / A_i[i];
        }

	return x;
}


template<
	const int		n,
	class			T
>
const Vector<n,T>
solve_lower(
	const Matrix<n,n,T> &	A,
	const Vector<n,T> &	b
)
{
	Vector<n,T>		x;

	for( int i=0; i<n; ++i)
	{
		T			s( b[i] );
		const Vector<n,T> &	A_i( A[i] );

		for( int j=0; j<i; ++j)
		{
#ifdef NO_FPU
			const T &	A_i_j( A_i[j] );
			const T &	x_j( x[j] );

			if( is_zero( A_i_j )
			||  is_zero( x_j )
			)
				continue;

			s -= A_i_j * x_j;
#else
			s -= A_i[j] * x[j];
#endif
		}

		x[i] = s / A_i[i];
	}

	return x;
}



template<
	const int		n,
	class			T
>
const Matrix<n,n,T>
invert(
	const Matrix<n,n,T> &	M
)
{
	typedef Matrix<n,n,T>	Matrix;
	typedef Vector<n,T>	Vector;

	Matrix			L;
	Matrix			U;
	Matrix			invU;
	Matrix			invL;
	Vector			identCol;

	LU( M, L, U );

	for( int i=0 ; i<n ; i++ )
	{
		identCol[i] = T(1);
		invU.col( i, solve_upper( U, identCol ) );
		invL.col( i, solve_lower( L, identCol ) );
		identCol[i] = T(0);
	}

	return invU * invL;
}


}
#endif

