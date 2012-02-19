/* -*- indent-tabs-mode:T; c-basic-offset:8; tab-width:8; -*- vi: set ts=8:
 * $Id: Servo.cpp,v 2.0 2002/09/22 02:07:31 tramm Exp $
 *
 * (c) Aaron Kahn
 * (c) Trammell Hudson
 *
 * This file contains the code for the SimLib function.
 *
 *************
 *
 *  This file is part of the autopilot simulation package.
 *
 *  For more details:
 *
 *	http://autopilot.sourceforge.net/
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

#include <cstdio>
#include <cmath>
#include <cstdlib>
#include <cstring>

#include "macros.h"
#include "Servo.h"
#include <mat/rk4.h>
#include <mat/Vector.h>

namespace sim
{

using namespace libmat;
using namespace util;

/**
 *  Maximum number of steps.  This is limits the resolution
 * of the servo to match that of the real world.  Things start
 * to fail if we drop below 8 bits (256) of resolution.
 */
const double Servo::max_steps	= 1024.0;


/*
 * This function will provide a servo-actuator model.  It contains two main
 * components.  The first is a second-order dynamic model which is tuneable 
 * via wn and zeta values.  The second, is a hysterisous model.  This feature
 * is designed around basic deadband slop that may be found in system linkages.
 *
 * See the structure for details.  Also, this system contains a state
 * structure that needs to be maintained for state propogation.
 */
static void
servo_derivs(
	Vector<2> &		Xdot,
	const Vector<2> &	X,
	const double 		UNUSED( t ),
	const double & 		u,
	const Vector<2> &	args
)
{
	double			wn	= args[0];
	double			zeta	= args[1];

	Xdot[0] = X[1];
	Xdot[1] = -2.0*zeta*wn*X[1] - wn*wn*X[0] + u;
}


double
Servo::step(
	double			dt,
	double			command
)
{
	Vector<2>		Xdot;

	// the model of the actuator is assumed to be of the following TF
	//		        s + wn^2
	//     -------------------------
	//     s^2 + 2*zeta*wn*s + wn^2

	Vector<2>		args;
	args[0]		= this->wn;
	args[1]		= this->zeta;

	// Limit the command to the (swashplate's) travel
	command = limit( command, this->min, this->max );

	// Round the command to an even step
	command = floor(command * this->max_steps) / this->max_steps;

	// integrate the state
	RK4( this->X, Xdot, 0.0, command, args, dt, &servo_derivs );

	// Return the output position
	return this->X[1] + this->wn * this->wn * this->X[0];
}

}
