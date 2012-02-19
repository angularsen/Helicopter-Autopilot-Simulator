#ifndef _gena_h_
#define _gena_h_

const Matrix<11,11>
gena(
	const Vector<3> &	uvw,
	const Vector<3> &	pqr,
	const Vector<4> &	Q,
	double			g
)
{
	const double		u = uvw[0];
	const double		v = uvw[1];
	const double		w = uvw[2];

	//const double		p = pqr[0];
	const double		q = pqr[1];
	//const double		r = pqr[2];

	const double		q0 = Q[0];
	const double		q1 = Q[1];
	const double		q2 = Q[2];
	const double		q3 = Q[3];


	Matrix<11,11>		A;

	const Matrix<3,3>	C( quatDC( Q ) );
	const Matrix<4,4>	Wxq( quatW( pqr ) );
	const Matrix<3,3>	Wx( eulerWx( pqr ) );

	const Matrix<3,3>	AA( 0.0 );

	const Matrix<3,3>	AB( C.transpose() );
	const Matrix<3,4>	AC(
		Vector<4>(
			-2 * v * q3 + 2 * w * q2,
			 2 * v * q2 + 2 * w * q3,
			-4 * u * q2 + 2 * v * q1,
			-4 * u * q3 - 2 * v * q0
		),
		Vector<4>(
			 2 * u * q3 - 2 * q * q1,
			 2 * u * q2 - 4 * v * q1 - 2 * w * q0,
			 2 * u * q1 + 2 * w * q3,
			 2 * u * q0 - 4 * v * q3 + 2 * w * q2
		),
		Vector<4>(
			-2 * u * q2 + 2 * v * q1,
			 2 * u * q3 + 2 * v * q0 - 4 * w * q1,
			-2 * u * q0 + 2 * v * q3 - 4 * w * q2,
			 2 * u * q1 + 2 * v * q2
		)
	);

	const Matrix<3,1>	AD( 0.0 );
	const Matrix<3,3>	AE( 0.0 );
	const Matrix<3,3>	AF( -Wx );
	const Matrix<3,4>	AG(
		Vector<4>(
			-2 * g * q2,
			 2 * g * q3,
			-2 * g * q0,
			 2 * g * q1
		),
		Vector<4>(
			 2 * g * q1,
			 2 * g * q0,
			 2 * g * q3,
			 2 * g * q2
		),
		Vector<4>(
			 0
			-4 * g * q1,
			-4 * g * q2,
			0
		)
	);

	Matrix<3,1>		AH;
	AH[0][0] = C[0][2];
	AH[1][0] = C[1][2];
	AH[2][0] = C[2][2];

	const Matrix<4,3>	AI( 0.0 );
	const Matrix<4,3>	AJ( 0.0 );
	const Matrix<4,4>	AK( Wxq );
	const Matrix<4,1>	AL( 0.0 );

	const Matrix<1,11>	AM( 0.0 );

	A.insert( AA, 0, 0 );
	A.insert( AB, 0, 3 );
	A.insert( AC, 0, 6 );
	A.insert( AD, 0, 10 );

	A.insert( AE, 3, 0 );
	A.insert( AF, 3, 3 );
	A.insert( AG, 3, 6 );
	A.insert( AH, 3, 10 );

	A.insert( AI, 6, 0 );
	A.insert( AJ, 6, 3 );
	A.insert( AK, 6, 6 );
	A.insert( AL, 6, 10 );

	A.insert( AM, 10, 0 );

	return A;
}


#endif
