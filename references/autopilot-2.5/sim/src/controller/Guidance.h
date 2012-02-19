/***********************************************************************
 * $Id: Guidance.h,v 2.0 2002/09/22 02:07:30 tramm Exp $
 *
 * This is a very basic flight controller for the X-Cell helicopter.
 * It is a 4 channel SISO controller for altitude, heading, X, and Y position.
 * 
 * (c) Aaron Kahn
 * (c) Trammell Hudson
 *
 *************
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

#ifndef _Guidance_h_
#define _Guidance_h_

#include "PID.h"
#include "Attitude.h"

#include <mat/Vector.h>

namespace libcontroller
{

using namespace libmat;

class Guidance
{
public:
	Guidance(
		double			dt
	);

	~Guidance() {}

	void reset();

	void
	flyto(
		const double		pos[3]
	) {
		this->position[0] = pos[0];
		this->position[1] = pos[1];
		this->position[2] = pos[2];
	}

	Vector<3>		position;
	double			heading;


	// servo outputs: [ coll roll pitch yaw ]
	const Vector<4>
	step(
		const Vector<3> &	pos_NED,
		const Vector<3> &	vel_NED,
		const Vector<3> &	theta,
		const Vector<3> &	pqr
	);

	// Distance from the listed state to the desired point
	double dist(
		const Vector<3> &	pos_NED,
		const Vector<3> &	theta
	);

protected:
	// [X Y Altitude] guidance PID structures
	PID			X;
	PID			Y;
	PID			D;

	// [roll pitch yaw] attitude PID structures
	Attitude		attitude;

	// Our integration time step
	double			dt;
};


}
#endif
