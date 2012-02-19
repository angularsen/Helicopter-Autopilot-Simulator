#ifndef _gps_update_h_
#define _gps_update_h_

/* Standard deviation */
double			PosSD	= 0.4;
double			VelSD	= 0.1;


void
gps_update(
	// Outputs
	Vector<11> &		X,

	// In/Out
	Matrix<11,11> &		P,

	// Inputs
	const Vector<3> &	xyz_measured,
	const Vector<3> &	uvw_measured,
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

	//const Matrix<3,3>	dcm( quatDC( Q_est ) );

	Matrix<6,6>		R( 0.0 );

	R[0][0] = PosSD * PosSD;
	R[1][1] = PosSD * PosSD;
	R[2][2] = PosSD * PosSD;
	R[3][3] = VelSD * VelSD;
	R[4][4] = VelSD * VelSD;
	R[5][5] = VelSD * VelSD;

	Matrix<6,11>		C( 0.0 );
	for( int i=0 ; i<6 ;i++ )
		C[i][i] = 1.0;

	const Matrix<11,6>	C_transpose( C.transpose() );

	const Matrix<6,6>	E( C * P * C_transpose + R );

	// Compute the Kalman gain
	const Matrix<11,6>	K( P * C_transpose * invert( E ) );

	// Combined position / velocity vector
	Vector<6>		measured;
	insert( measured, 0, xyz_measured );
	insert( measured, 3, uvw_measured );

	// Update the state vector
	X += K * ( measured - C * X );
};

#endif
