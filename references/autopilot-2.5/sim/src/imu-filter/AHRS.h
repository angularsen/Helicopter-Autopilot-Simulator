/* -*- indent-tabs-mode:T; c-basic-offset:8; tab-width:8; -*- vi: set ts=8:
 *  $Id: AHRS.h,v 2.9 2003/03/14 19:44:14 tramm Exp $
 *
 * (c) Trammell Hudson
 * (c) Aaron Kahn
 *
 * AHRS simulator based on Kalman filtering of the gyro and
 * accelerometer data.  Converted from Aaron's matlab code
 * to use the C++ math library.
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
#ifndef _AHRS_H_
#define _AHRS_H_

#include <mat/Vector.h>
#include <mat/Matrix.h>

namespace imufilter
{

using namespace libmat;

class AHRS
{
public:
	AHRS(
		double			dt = 32768.0 / 1000000.0
	);

	void
	reset();

	void
	initialize(
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


	Vector<3>		accel;
	Vector<3>		theta;
	Vector<3>		pqr;
	Vector<3>		bias;

	const double		dt;
	double			trace;

	static const int	N = 7;

	// Covariance matrix
	Matrix<N,N>		P;

private:

	void make_a_matrix(
		Matrix<N,N> &		A,
		const Vector<3> &	pqr
	) const;

	void propagate_state(
		const Vector<3> &	pqr
	);

	void propagate_covariance(
		const Matrix<N,N> &	A
	);

	void kalman_attitude_update(
		const Vector<3> &	accel,
		const Matrix<3,3> &	DCM,
		const Vector<3> &	THETAe
	);

	void kalman_compass_update(
		double			heading,
		const Matrix<3,3> &	DCM,
		const Vector<3> &	THETAe
	);


	/*
	 *  Our Kalman filter uses its own state object that we
	 * hide here.
	 */
	class state_t
	{
	public:
		// Quaternion state estimate
		Vector<4>		q;

		// Gyro bias estimate
		Vector<3>		bias;
	};

	state_t 		state;

	// Noise estimate
	Matrix<N,N>		Q;

	// State estimate for attitude
	Matrix<2,2>		R_attitude;

	// State estimate for heading
	Matrix<1,1>		R_heading;


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
