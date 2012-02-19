/* -*- indent-tabs-mode:T; c-basic-offset:8; tab-width:8; -*- vi: set ts=8:
 * $Id: Matrix.h,v 2.4 2003/02/19 03:22:14 tramm Exp $
 *
 * (c) Aaron Kahn
 * (c) Trammell Hudson
 *
 * Matrix class in C++
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
#ifndef _Matrix_h_
#define _Matrix_h_

#include "Vector.h"
#include "fast_float.h"


namespace libmat
{

template<
	const int		n,		// Rows
	const int		m,		// Cols
	class			T = double	// Type of the components
>
class Matrix
{
protected:
	//Vector<n,Vector<m,T> >		M;
	Vector<m,T>		M[n];

	typedef typename Vector<m,T>::index_t index_t;

public:
	Matrix()
	{
	}

	Matrix(
		const T &		value
	)
	{
		this->fill( value );
	}

	Matrix(
		const Vector<m> &	v0,
		const Vector<m> &	v1,
		const Vector<m> &	v2
	)
	{
		M[0] = v0;
		M[1] = v1;
		M[2] = v2;
	}

	Matrix(
		const Vector<m> &	v0,
		const Vector<m> &	v1,
		const Vector<m> &	v2,
		const Vector<m> &	v3
	)
	{
		M[0] = v0;
		M[1] = v1;
		M[2] = v2;
		M[3] = v3;
	}

	size_t
	rows() const
	{
		return n;
	}


	size_t
	cols() const
	{
		return m;
	}


	void
	fill(
		const T &		value = T()
	)
	{
		for( index_t i=0 ; i<n ; i++ )
			this->row(i).fill(value);
	}


	/**
	 *  Return a row.
	 */
	Vector<m,T> &
	operator[] (
		index_t			row
	)
	{
		return this->row(row);
	}

	const Vector<m,T> &
	operator[](
		index_t			row
	) const
	{
		return this->row(row);
	}

	const Vector<m,T> &
	row(
		index_t			row
	) const
	{
		ASSERT( row < n );
		return this->M[row];
	}

	Vector<m,T> &
	row(
		index_t			row
	)
	{
		ASSERT( row < n );
		return this->M[row];
	}



	/**
	 *  Return a column.
	 */
	const Vector<n,T>
	col(
		index_t			col
	) const
	{
		ASSERT( col < m );
		Vector<n,T>	v;

		for( index_t i=0 ; i < n ; i++ )
			v[i] = (*this)[i][col];
		return v;
	}

	/**
	 *  Fill a column
	 */
	void
	col(
		index_t			col,
		const Vector<n,T> &	v
	)
	{
		ASSERT( col < m );
		for( index_t i=0 ; i<n ; i++ )
			(*this)[i][col] = v[i];
	}

	/**
	 *  Insert a submatrix into the larger matrix
	 */
	template<
		class		T2,
		int		n2,
		int		m2
	>
	void
	insert(
		int		base_n,
		int		base_m,
		const Matrix<n2,m2,T2> &M
	)
	{
		for( index_t i=0 ; i<n2 ; i++ )
			for( index_t j=0 ; j<m2 ; j++ )
				(*this)[i+base_n][j+base_m] = M[i][j];
	}


	/**
	 *  Negate a matrix
	 */
	const Matrix
	operator - () const
	{
		Matrix		M;

		for( index_t i=0 ; i<n ; i++ )
			for( index_t j=0 ; j<m ; j++ )
			{
#ifdef NO_FPU
				decrement( M[i][j], (*this)[i][j] );
#else
				M[i][j] = -(*this)[i][j];
#endif
			}
		return M;
	}


	/**
	 *  Add two vectors, returning the sum of the two
	 */
	const Matrix &
	operator+ (
		const Matrix &	that
	) const
	{
		return Matrix(*this) += that;
	}

	const Matrix &
	operator- (
		const Matrix &	that
	) const
	{
		return Matrix(*this) -= that;
	}

	/**
	 *  Update the matrix in place
	 */
	Matrix &
	operator+= (
		const Matrix &	that
	)
	{
		for( index_t i=0 ; i < n ; i++ )
			(*this)[i] += that[i];

		return (*this);
	}

	Matrix &
	operator-= (
		const Matrix &	that
	)
	{
		for( index_t i=0 ; i < n ; i++ )
			(*this)[i] -= that[i];

		return (*this);
	}



	/**
	 *  Matrix multiplication
	 */
	template<
		int			p
	>
	const Matrix<n,p,T>
	operator * (
		const Matrix<m,p,T> &	B
	) const
	{
		Matrix<n,p,T>		C;
		const Matrix<n,m,T> &	A( *this );

		for( index_t i=0 ; i<n ; i++ )
		{
			const Vector<m,T> &	A_i = A[i];
			Vector<p,T> &		C_i = C[i];

			for( index_t j=0 ; j<p ; j++ )
			{
				T		s = T();

				for( index_t k=0 ; k<m ; k++ )
				{
#ifdef NO_FPU
					const T & B_k_j( B[k][j] );
					const T & A_i_k( A_i[k] );

					if( is_zero( B_k_j )
					||  is_zero( A_i_k )
					)
						continue;

					s += B_k_j * A_i_k;
#else
					s += B[k][j] * A_i[k];
#endif
				}

				C_i[j] = s;
			}
		}

		return C;
	}


	Matrix &
	operator *= (
		const Matrix &		B
	)
	{
		return (*this) = (*this) * B;
	}


	/**
	 *  Scale a matrix
	 */
	const Matrix
	operator * (
		const T &		s
	) const
	{
		return Matrix(*this) *= s;
	}

	Matrix &
	operator *= (
		const T &		s
	)
	{
		for( index_t i=0 ; i<n ; i++ )
			(*this)[i] *= s;

		return (*this);
	}

	/**
	 *  Return the transpose of the matrix
	 */
	const Matrix<m,n,T>
	transpose() const
	{
		const Matrix<n,m,T> &	A( *this );
		Matrix<m,n,T>		B;

		for( index_t i=0 ; i<n ; i++ )
		{
			const Vector<m,T> &	A_i(A[i]);
			for( index_t j=0 ; j<m ; j++ )
				B[j][i] = A_i[j];
		}

		return B;
	}

};


/**
 * These two need to be defined after vector and matrix, so it
 * can be in neither of them.
 */

/**
 *  Vector<n> = Matrix<n,m> * Vector<m>
 */
template<
	const int		n,
	const int		m,
	class			T
>
const Vector<n,T>
operator * (
	const Matrix<n,m,T> &	A,
	const Vector<m,T> &	b
)
{
	typedef typename Vector<n,T>::index_t index_t;
	Vector<n,T>		c;

	for( index_t i=0 ; i<n ; i++ )
		c[i] = A[i] * b;

	return c;
}


/**
 *  Vector<m> = Vector<n> * Matrix<n,m>
 */
template<
	const int		n,
	const int		m,
	class			T
>
const Vector<m,T>
operator* (
	const Vector<n,T> &	a,
	const Matrix<n,m,T> &	B
)
{
	typedef typename Vector<n,T>::index_t index_t;
	Vector<m,T>		c;

	for( index_t i=0 ; i<m ; i++ )
	{
		T			s(0);
		for( index_t j=0 ; j<n ; j++ )
		{
#ifdef NO_FPU
			const T &		a_j( a[j] );
			const T &		B_j_i( B[j][i] );

			if( is_zero( a_j )
			||  is_zero( B_j_i )
			)
				continue;

			s += a_j * B_j_i;
#else
			s += a[j] * B[j][i];
#endif
		}

		c[i] = s;
	}

	return c;
}


/**
 *  Output a matrix to the stream with a little bit of formatting.
 */
template<
	const int		n,
	const int		m,
	class			T
>
std::ostream &
operator<<(
	std::ostream &		out,
	const Matrix<n,m,T> &	M
)
{
	typedef typename Matrix<n,m,T>::index_t index_t;

	out << '[' << std::endl;

	for( index_t i=0 ; i < M.rows() ; i++ )
		out << M[i] << std::endl;

	out << ']';
	return out;
}


/**
 *  Optimization to do three way multiplication in the most
 * optimal form.  We should be able to resolve all this at
 * compile time.
 */
template<
	const int		n,
	const int		m,
	const int		p,
	const int		q,
	class			T
>
const Matrix<n,q,T>
mult3(
	const Matrix<n,m,T> &	a,
	const Matrix<m,p,T> &	b,
	const Matrix<p,q,T> &	c
)
{
	if( n*m*p + n*p*q < m*p*q + n*m*q )
		return (a * b) * c;
	else
		return a * (b * c);
}

};
#endif
