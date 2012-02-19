/* -*- indent-tabs-mode:T; c-basic-offset:8; tab-width:8; -*- vi: set ts=8:
 *  $Id: IMU.h,v 2.2 2003/03/08 05:19:16 tramm Exp $
 *
 * (c) Trammell Hudson
 * (c) Aaron Kahn
 *
 * Simplistic IMU object that reads lines from the Rev 2.2 board
 * and proccesses them into sensor data.
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
#ifndef _imu_h_
#define _imu_h_

#include <mat/Vector.h>

namespace imufilter
{

using namespace libmat;

class IMU
{
public:
	IMU();

	void
	update(
		const char *	line
	);

	void
	update(
		const Vector<3> &	accel,
		const Vector<3> &	pqr
	);

	Vector<3>		accel;
	Vector<3>		pqr;

private:
	const int		ax_index;
	const int		ay_index;
	const int		az_index;

	const int		p_index;
	const int		q_index;
	const int		r_index;


	const Vector<3>		accel_zero;
	const Vector<3>		accel_scale;

	const Vector<3>		gyro_zero;
	const Vector<3>		gyro_scale;
};

}

#endif
