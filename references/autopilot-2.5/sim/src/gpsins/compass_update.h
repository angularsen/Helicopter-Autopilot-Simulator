#ifndef _compass_update_h_
#define _compass_update_h_

/*
 *  Acceptable standard deviations
 */
double			CompSD	= 0.2;


void
compass_update(
	// Output
	Vector<11> &		X,

	// In/Out
	Matrix<11,11> &		P,

	// Input
	double			heading_measured,
	const Vector<3> &	xyz_est,
	const Vector<3> &	uvw_est,
	const Vector<4> &	Q_est,
	double			G_est
)
{
	insert( X, 0, xyz_est );
	insert( X, 3, uvw_est );
	insert( X, 6, Q_est );
	X[10] = G_est;

	const Matrix<1,1>	R( CompSD * CompSD );

	// Convert heading to quaternion
	const Vector<3>		eul( quat2euler( Q_est ) );

	// make the C matrix
	const Vector<4>		dpsi( dpsi_dq( Q_est ) );
	Matrix<1,11>		C( 0.0 );
	C[0][6] = dpsi[0];
	C[0][7] = dpsi[1];
	C[0][8] = dpsi[2];
	C[0][9] = dpsi[3];

	const Matrix<11,1>	C_transpose( C.transpose() );

	// Compute the E matrix
	const Matrix<1,1>	E( C * P * C_transpose + R );

	// Compute the Kalman gain
	const Matrix<11,1>	K( P * C_transpose * invert( E ) );

	// Update the state vector
	Vector<1>		error;

	error[0] = heading_measured - eul[2];
	X += K * error;

	// Update the covariance matrix
	P -= K * C * P;
}

#endif
