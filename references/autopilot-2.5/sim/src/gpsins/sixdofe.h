#ifndef _FlatEarth_h_
#define _FlatEarth_h_

#include <mat/Vector.h>
#include <mat/Matrix.h>
#include <mat/Matrix_Invert.h>
#include <mat/Quat.h>

using namespace libmat;

template<class T>
const Matrix<3,3,T>
euler_strap(
	const Vector<3,T> &	theta
)
{
	const double		sphi = sin( theta[0] );
	const double		cphi = cos( theta[0] );
	//const double		stheta = sin( theta[1] );
	const double		ctheta = cos( theta[1] );
	const double		ttheta = tan( theta[1] );

	return Matrix<3,3,T>(
		Vector<3>( 1, sphi * ttheta, cphi * ttheta ),
		Vector<3>( 0, cphi,          -sphi ),
		Vector<3>( 0, sphi / ctheta, cphi / ctheta )
	);
}

		
static inline
const Vector<12>
sixdofe(
	const Vector<3> &	XYZ,
	const Vector<3> &	LMN,
	const Vector<4> &	I,
	double			m,
	double			g,
	const Vector<12> &	Xtrue
)
{
	const Vector<3> 	uvw( slice<0,3>( Xtrue ) );
	const Vector<3> 	pqr( slice<3,3>( Xtrue ) );
	const Vector<3> 	theta( slice<6,3>( Xtrue ) );
	//const Vector<3> 	ned( slice<9,3>( Xtrue ) );

	const Matrix<3,3>	dcm( eulerDC( theta ) );
	const Matrix<3,3>	om( eulerWx( pqr ) );

	/* Bug in g++? If these are in the matrix declaration it faults */
	const Vector<3> row1(  I[0],    0, -I[3] );
	const Vector<3> row2(     0, I[1],     0 );
	const Vector<3> row3( -I[3],    0,  I[2] );

	const Matrix<3,3>	J( row1, row2, row3 );
/*
		Vector<3>(  I[0],    0, -I[3] ),
		Vector<3>(     0, I[1],     0 ),
		Vector<3>( -I[3],    0,  I[2] )
*/

	const Matrix<3,3>	invJ( invert( J ) );

	const Matrix<3,3>	E( euler_strap( theta ) );

	const Vector<3>		G( 0, 0, g );

	const Vector<3>		Vb_dot( dcm * G - om * uvw + XYZ / m );
	const Vector<3>		omb_dot( invJ * LMN - invJ * om * J * pqr );
	const Vector<3>		phi_dot( E * pqr );
	const Vector<3>		Ve( dcm.transpose() * uvw );

	Vector<12> Xdot;

	insert( Xdot, 0, Vb_dot );
	insert( Xdot, 3, omb_dot );
	insert( Xdot, 6, phi_dot );
	insert( Xdot, 9, Ve );

	return Xdot;
}

#endif
