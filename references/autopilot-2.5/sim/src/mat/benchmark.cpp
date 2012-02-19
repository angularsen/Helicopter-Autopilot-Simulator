#include <iostream>
#include <cstdlib>
#include <mat/Vector.h>
#include <mat/Matrix.h>
#include <mat/Matrix_Invert.h>
#include "timer.h"

using namespace std;
using namespace libmat;


template<
	int			n,
	int			m,
	class			T
>
const Matrix<n,m,T>
noise(
	const T &		min,
	const T &		max
)
{
	Matrix<n,m,T>		M;

	for( int i=0 ; i<n ; i++ )
		for( int j=0 ; j<m ; j++ )
			M[i][j] = drand48() * (max - min) + min;

	return M;
}


template<
	int			n,
	int			m,
	int			p,
	int			q,
	class			T
>
void
matrix_mult3(
	int			times
)
{
	Matrix<n,m,T>		A( noise<n,m,T>( -10.0, 10.0 ) );
	Matrix<m,p,T>		B( noise<m,p,T>( -10.0, 10.0 ) );
	Matrix<p,q,T>		C( noise<p,q,T>( -10.0, 10.0 ) );

	for( int i=0 ; i<times ; i++ )
	{
		const Matrix<n,q,T>	D( mult3( A, B, C ) );
	}
}


template<
	int			n,
	class			T
>
void
sparse_matrix_mult2(
	int			times
)
{
	Matrix<n,n,T>		A;
	Matrix<n,n,T>		P( noise<n,n,T>( -1, 1 ) );

	for( int i=0 ; i<n ; i++ )
	{
		for( int j=0 ; j<n ; j++ )
			A[i][j] = i < n/2 ? drand48() : 0;
	}

	for( int i=0 ; i<times ; i++ )
	{
		Matrix<n,n,T>	D( eye<n,T>() );
		D += A * P;
		D += P * A.transpose();
	}
}


template<
	int			n,
	class			T
>
void
dense_matrix_mult2(
	int			times
)
{
	Matrix<n,n,T>		A( noise<n,n,T>( -1, 1 ) );
	Matrix<n,n,T>		P( noise<n,n,T>( -1, 1 ) );

	for( int i=0 ; i<times ; i++ )
	{
		Matrix<n,n,T>	D( A * P );
		(void) D.rows();
	}
}



template<
	class			T
>
void
scalar_mult(
	const int		iters,
	volatile const T &	a,
	volatile const T &	b
)
{

	volatile T	val;

	for( int i=0 ; i<iters ; i++ )
	{
		if( *(int32_t*) &a == 0
		||  *(int32_t*) &b == 0
		)
			val = 0.0;
		else
			val = a * b;
	}
}

#define time_this( iters, code )					\
	do {								\
		int count = (iters);					\
		stopwatch_t timer;					\
		start( &timer );					\
		(void) code;						\
		unsigned long diff = stop( &timer );			\
		cout							\
			<< count					\
			<< " iterations of "				\
			<< #code					\
			<< " = " 					\
			<< diff						\
			<< " usec = "					\
			<< double(diff) / double(iters)			\
			<< " usec/iter"					\
			<< endl;					\
	} while(0)

int main( void )
{
	const int		iters = 1000;

	time_this( iters, ( sparse_matrix_mult2<7,double>( iters ) ) );
	time_this( iters, ( sparse_matrix_mult2<7,float>( iters ) ) );

	time_this( iters, ( dense_matrix_mult2<7,double>( iters ) ) );
	time_this( iters, ( dense_matrix_mult2<7,float>( iters ) ) );

	time_this( iters, ( matrix_mult3<2,7,7,2,double>( iters ) ) );
	time_this( iters, ( matrix_mult3<2,7,7,2,float>( iters ) ) );

	time_this( 1<<20, ( scalar_mult<uint16_t>( iters, 0xDEAD, 0x200 ) ) );
	time_this( 1<<20, ( scalar_mult<uint32_t>( iters, 0xDEADBEEF, 0x200 ) ) );
	time_this( 1<<20, ( scalar_mult<double>( iters, 3.15159, 0.0 ) ) );
}
