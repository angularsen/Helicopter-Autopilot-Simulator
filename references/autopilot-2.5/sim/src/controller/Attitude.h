/***********************************************************************
 *
 * $Id: Attitude.h,v 2.0 2002/09/22 02:07:29 tramm Exp $
 *
 *	This is a very basic flight controller for the X-Cell helicopter.
 * It is a 4 channel SISO controller for maintaining a stable attitude.
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

#ifndef _Attitude_h_
#define _Attitude_h_

#include "PID.h"
#include <mat/Vector.h>

namespace libcontroller
{

using namespace libmat;

class Attitude
{
public:
	Attitude(
		double			dt
	);

	~Attitude() {}

	void reset();


	// servo outputs: [ roll pitch yaw ]
	const Vector<3>
	step(
		const Vector<3> &	theta,
		const Vector<3> &	pqr
	);

	// Our desired position
	Vector<3>		attitude;

protected:
	// [roll pitch yaw] attitude PID structures
	PID			roll;
	PID			pitch;
	PID			yaw;

	// Our integration time step
	double			dt;

};


}
#endif
