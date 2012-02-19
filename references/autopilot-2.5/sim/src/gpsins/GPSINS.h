/* -*- indent-tabs-mode:T; c-basic-offset:8; tab-width:8; -*- vi: set ts=8:
 *  $Id: GPSINS.h,v 2.0 2002/09/22 02:07:30 tramm Exp $
 *
 * (c) Trammell Hudson
 * (c) Aaron Kahn
 *
 * GPS aided INS object.
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
#ifndef _GPSINS_H_
#define _GPSINS_H_

#include <mat/Vector.h>
#include <mat/Matrix.h>
#include <mat/Quat.h>

namespace gpsins
{

using namespace libmat;

class GPSINS
{
public:
	GPSINS();

	void
	imu_update(
		const Vector<3> &	accel,
		const Vector<3> &	pqr,
		double			dt
	);

	void
	gps_update(
		const Vector<3> &	xyz,
		const Vector<3> &	uvw,
		double			dt
	);

	void
	compass_update(
		const double		heading,
		double			dt
	);

	// Public accessors for our state vector.
	const Vector<3>
	xyz()
	{
		return Vector<3>(
			this->X[ X_index ],
			this->X[ Y_index ],
			this->X[ Z_index ]
		);
	};

	const Vector<3>
	uvw()
	{
		return Vector<3>(
			this->X[ U_index ],
			this->X[ V_index ],
			this->X[ W_index ]
		);
	};

	const Vector<4>
	q()
	{
		return Vector<4>(
			this->X[ Q0_index ],
			this->X[ Q1_index ],
			this->X[ Q2_index ],
			this->X[ Q3_index ]
		);
	};

	double
	g()
	{
		return this->X[ G_index ];
	}

	const Vector<3>
	theta()
	{
		return quat2euler( this->q() );
	};

private:
	// Private copy of our state vector.
	// Canonical copy is in the data members
	Vector<11>		X;

	static const int	X_index		= 0;
	static const int	Y_index		= 1;
	static const int	Z_index		= 2;

	static const int	U_index		= 3;
	static const int	V_index		= 4;
	static const int	W_index		= 5;

	static const int	Q0_index	= 6;
	static const int	Q1_index	= 7;
	static const int	Q2_index	= 8;
	static const int	Q3_index	= 9;

	static const int	G_index		= 10;


	void update_state();
	
	int			imu_samples;
	int			compass_samples;
	int			gps_samples;

	// Process noise
	Matrix<11,11>		Q;

	// Covariance matrix
	Matrix<11,11>		P;

	// Standard deviation values?
	const double		compass_sd;
	const double		position_sd;
	const double		velocity_sd;

	void
	imu_init(
		const Vector<3> &	accel,
		const Vector<3> &	pqr
	);

	void
	compass_init(
		double			heading
	);

	void
	gps_init(
		const Vector<3> &	xyz,
		const Vector<3> &	uvw
	);
		
	void make_a_matrix(
		Matrix<11,11> &		A,
		const Vector<3> &	pqr
	);

	void propagate_state(
		const Vector<3> &	accel,
		const Vector<3> &	pqr,
		double			dt
	);

	void propagate_covariance(
		const Vector<3> &	pqr,
		double			dt
	);
};

}

#endif
