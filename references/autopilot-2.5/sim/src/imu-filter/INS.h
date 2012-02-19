/* -*- indent-tabs-mode:T; c-basic-offset:8; tab-width:8; -*- vi: set ts=8:
 *  $Id: INS.h,v 1.6 2002/10/04 18:00:07 tramm Exp $
 *
 * (c) Aaron Kahn
 * (c) Trammell Hudson
 *
 * GPS aided INS object.
 *
 * Converted from Aaron's matlab code to use the C++ math library.
 *
 **************
 *
 *  This file is part of the autopilot simulation package.
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
#ifndef _INS_H_
#define _INS_H_

#include <mat/Vector.h>
#include <mat/Matrix.h>

namespace imufilter
{

using namespace libmat;

class INS
{
public:
	INS(
		double			dt = 32768.0 / 1000000.0
	);

	void
	reset();

	void
	initialize(
		const Vector<3> &	ned,
		const Vector<3> &	uvw,
		const Vector<3> &	accel,
		const Vector<3> &	pqr,
		double			heading
	);


	void
	imu_update(
		const Vector<3> &	accel,
		const Vector<3> &	pqr
	);


	void
	compass_update(
		double			heading
	);


	void
	gps_update(
		const Vector<3> &	ned,
		const Vector<3> &	uvw
	);

	// Position and velocity estimate
	Vector<3>		xyz;
	Vector<3>		uvw;

	// Quaternion state estimate (and shadow copy theta)
	Vector<4>		q;
	Vector<3>		theta;

	// Body rate estimate (unbiased)
	Vector<3>		pqr;

	// Gravity estimate
	double			g;

	// Gyro bias estimate
	Vector<3>		bias;

	const double		dt;
	double			trace;

	static const int	N = 3 + 3 + 4 + 1 + 3;

	// Covariance matrix
	Matrix<N,N>		P;

private:

	void make_a_matrix(
		Matrix<N,N> &		A,
		const Vector<3> &	uvw,
		const Vector<3> &	pqr,
		const Matrix<3,3> &	DCM,
		const Matrix<4,4> &	Wxq,
		const Matrix<3,3> &	Wx
	) const;


	void propagate_state(
		const Vector<3> &	uvw,
		const Vector<3> &	accel,
		const Vector<3> &	pqr,
		const Matrix<3,3> &	DCM,
		const Matrix<4,4> &	Wxq,
		const Matrix<3,3> &	Wx
	);


	void propagate_covariance();

	// The system derivative matrix
	Matrix<N,N>			A;
	Matrix<N,N>			Pdot;
	Matrix<N,N>			Ptemp;
	int				stage;


	void
	kalman_attitude_update(
		const Vector<3> &	pqr,
		const Vector<3> &	accel,
		const Matrix<3,3> &	DCM,
		const Vector<3> &	THETAe
	);


	// Noise estimate
	Matrix<N,N>		Q;

	// State estimate for attitude
	Matrix<2,2>		R_attitude;

	// State estimate for heading
	Matrix<1,1>		R_heading;

	// Estimate for the GPS
	Matrix<6,6>		R_position;


	// Wrapper to serialize our state and jump into the kalman filter
	template<
		int			m
	>
	void
	do_kalman(
		const Matrix<m,N> &	C,
		const Matrix<m,m> &	R,
		const Vector<m> &	eTHETA
	);

};

}
#endif
