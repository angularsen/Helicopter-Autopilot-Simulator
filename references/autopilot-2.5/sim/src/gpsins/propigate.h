#ifndef _propigate_h_
#define _propigate_h_

#include <mat/Vector.h>

template<class T>
void
propigate_state(
	Vector<11,T> &		X,
	Vector<11,T> &		Xdot,

	Vector<3> &		xyz,
	Vector<3> &		uvw,
	const Vector<3> &	a,
	const Vector<3> &	pqr,
	Vector<4> &		Q,
	double			g,
	double			dt
)
{
	const Matrix<4,4,T> &	Wxq( quatW( pqr ) );
	const Matrix<3,3,T> &	Wx( eulerWx( pqr ) );
	const Matrix<3,3,T> &	dcm( quatDC( Q ) );

	const Vector<3>		NED_dot( dcm.transpose() * uvw );
	const Vector<3>		uvw_dot( a + dcm * Vector<3>(0,0,g) - Wx * uvw );
	const Vector<4>		quat_dot( Wxq * Q );

	insert( Xdot, 0, NED_dot );
	insert( Xdot, 3, uvw_dot );
	insert( Xdot, 6, quat_dot );
	Xdot[10] = 0;

	xyz += NED_dot * dt;
	uvw += uvw_dot * dt;
	Q += quat_dot * dt;
	Q.norm_self();

	insert( X, 0, xyz );
	insert( X, 3, uvw );
	insert( X, 6, Q );
	X[10] = g;
}


#endif
