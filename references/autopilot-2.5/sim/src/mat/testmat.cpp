#include <iostream>

#include <mat/Quat.h>
#include <mat/Vector.h>
#include <mat/Matrix.h>
#include <mat/Matrix_Invert.h>
#include <mat/Kalman.h>
#include "old-matlib.h"
#include <timer.h>
#include <cstring>

using namespace std;
using namespace libmat;
using namespace matlib;


#define fill( m1, m2, a00, a01, a02, a10, a11, a12, a20, a21, a22 )	\
	do {								\
		m2[0][0] = m1[0][0]	= a00;				\
		m2[0][1] = m1[0][1]	= a01;				\
		m2[0][2] = m1[0][2]	= a02;				\
									\
		m2[1][0] = m1[1][0]	= a10;				\
		m2[1][1] = m1[1][1]	= a11;				\
		m2[1][2] = m1[1][2]	= a12;				\
									\
		m2[2][0] = m1[2][0]	= a20;				\
		m2[2][1] = m1[2][1]	= a21;				\
		m2[2][2] = m1[2][2]	= a22;				\
	} while(0)							\


#define compare( tag, old_m3, new_m3, n, m )				\
    do {								\
	int failed = 0;							\
	cout << "Comparing " << tag << ": ";				\
	for( int i=0; i<n ; i++ )					\
	{								\
		for( int j=0; j<m ; j++ )				\
		{							\
			if( old_m3[i][j] == new_m3[i][j] )		\
				continue;				\
			cerr						\
				<< "ERROR: "				\
				<< "[" << i << "," << j << "] "		\
				<< old_m3[i][j]				\
				<< " != "				\
				<< new_m3[i][j]				\
				<< endl;				\
			failed = 1;					\
		}							\
	}								\
									\
	cout << (failed ? "FAILED" : "PASSED" ) << endl;		\
	fail_count += failed;						\
	test_count++;							\
    } while(0)								\


int main()
{
	const int	iters = 1<<16;
	int		test_count = 0;
	int		fail_count = 0;

	double		old_m1[MAXSIZE][MAXSIZE];
	double		old_m2[MAXSIZE][MAXSIZE];
	double		old_m3[MAXSIZE][MAXSIZE];

	Matrix<3,3>	new_m1;
	Matrix<3,3>	new_m2;
	Matrix<3,3>	new_m3;

	fill( new_m1, old_m1,
		9, 8, 2,
		2, 3, 9,
		1, 2, 4
	);

	fill( new_m2, old_m2,
		1, 2, 2,
		2, 1, 3,
		4, 2, 4
	);

	cout
		<< "Sizes: "
		<< " new_m1=" << sizeof(new_m1)
		<< " old_m1=" << sizeof(old_m1)
		<< endl;

	/* Identity matrix creation */
	const Matrix<3,3,double> (*new_eye)() = eye<3,double>;
	time_these( iters,
		"new eye<3>", new_m3 = new_eye(),
		"old eye(3)", eye( old_m3, 3 )
	);

	compare( "eye<3>", new_m3, old_m3, 3, 3 );


	/* Matrix addition */
	time_these( iters,
		"new +", new_m3 = new_m1 + new_m2,
		"old +", MMadd( old_m1, old_m2, old_m3, 3, 3 )
	);

	compare( "+", new_m3, old_m3, 3, 3 );

	time_these( iters,
		"new +=", new_m3 += new_m1,
		"old +=", MMadd( old_m3, old_m1, old_m3, 3, 3 )
	);

	compare( "+=", new_m3, old_m3, 3, 3 );

	/* Do dot product */
	Vector<3>	new_v1;
	Vector<3>	new_v2;
	double		old_v1[MAXSIZE];
	double		old_v2[MAXSIZE];

	time_these( iters,
		"new dot", double dot = new_v1 * new_v2,
		"old dot", double d = dot( old_v1, old_v2, 3 )
	);

	/* Do matrix decomposition */
	time_these( iters,
		"new LU", LU( new_m1, new_m2, new_m3 ),
		"old LU", LU( old_m1, old_m2, old_m3, 3 )
	);

	compare( "LU->L", old_m2, new_m2, 3, 3 );
	compare( "LU->U", old_m3, new_m3, 3, 3 );


	/* Do quaternion production */
	typedef Matrix<3,3> m33_t;
	Vector<3> new_angles( 0.17, -0.1, 3.1 );
	new_m3 = eulerWx( new_angles );

	time_these( iters,
		"new wx", const m33_t new_m3( eulerWx( new_angles ) ),
		"old wx", eulerWx( old_m3, 0.17, -0.1, 3.1 )
	);

	compare( "eulerWx", old_m3, new_m3, 3, 3 );


	time_these( iters,
		"new 3x3 mult", new_m3 = new_m1 * new_m2,
		"old 3x3 mult", MMmult( old_m1, old_m2, old_m3, 3, 3, 3 )
	);

	compare( "MMmult", old_m3, new_m3, 3, 3 );


	time_these( iters,
		"new 3x3 invert", new_m2 = invert( new_m1 ),
		"old 3x3 invert", inv( old_m1, old_m2, 3 )
	);

	compare( "invert", new_m2, old_m2, 3, 3 );


	time_these( iters,
		"new transpose", new_m2 = new_m1.transpose(),
		"old transpose", transpose( old_m1, old_m2, 3, 3 )
	);

	compare( "transpose", new_m2, old_m2, 3, 3 );


	/**
	 *  Matrix * Vector ops
	 */
	Vector<3>		v_in( new_m2[0] );
	Vector<3>		v_out;

	time_these( iters,
		"m*v", v_out = new_m1 * v_in,
		"MVmult", MVmult( old_m1, old_m2[0], old_m3[0], 3, 3 )
	);

	new_m3[0] = v_out;
	compare( "m*v", new_m3, old_m3, 3, 3 );


	/**
	 *  Chained operations
	 */
	time_these( iters,
		"new chain", do {
			new_m3 = invert((new_m1 -= new_m2) *= new_m2 - new_m2);
		} while(0),
		"old chain", do {
			double temp[MAXSIZE][MAXSIZE];
			MMsub( old_m1, old_m2, old_m1, 3, 3 );
			MMmult( old_m1, old_m2, temp, 3, 3, 3 );
			MMcopy( temp, old_m1, 3, 3 );
			MMsub( temp, old_m2, temp, 3, 3 );
			inv( temp, old_m3, 3 );
		} while(0)
	);

	//compare( "chain", new_m3, old_m3, 3, 3 );



	/**
	 *  Non-square matrix manipulation
	 */
	Matrix<3,4>		m34;
	Matrix<4,5>		m45;
	Matrix<3,5>		m35;

	memset( old_m1, 0, sizeof(old_m1) );
	memset( old_m2, 0, sizeof(old_m2) );

	fill( m34, old_m1,
		9, 8, 2,
		2, 3, 9,
		1, 2, 4
	);

	fill( m45, old_m2,
		1, 2, 2,
		2, 1, 3,
		4, 2, 4
	);

	time_these( iters,
		"new 3x4x5 mult", m35 = m34 * m45,
		"old 3x4x5 mult", MMmult( old_m1, old_m2, old_m3, 3, 4, 5 )
	);

	compare( "mult 3x4x5", old_m3, m35, 3, 5 );


	/*
	 *  Large matrix inversion
	 */
	Matrix<8,8>		m88;
	Matrix<8,8>		m88_inv;
	fill( m88, old_m1,
		1, 2, 2,
		2, 1, 3,
		4, 2, 4
	);

	time_these( 1024,
		"m88 invert", m88_inv = invert( m88 ),
		"old invert", inv( old_m1, old_m2, 8 )
	);

	//compare( "8x8 invert", m88_inv, old_m2, 8, 8 );


	/**
	 *  Wrap up and output stats
	 */
	cout
		<< endl
		<< test_count << " tests, "
		<< fail_count << " failed." 
		<< endl;

	return 0;
}
