/* -*- indent-tabs-mode:T; c-basic-offset:8; tab-width:8; -*- vi: set ts=8:
 * $Id: PID.cpp,v 2.0 2002/09/22 02:07:30 tramm Exp $
 *
 * (c) Aaron Kahn
 * (c) Trammell Hudson
 *
 * PID control logic for a SISO controller.
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

#include <cmath>
#include "macros.h"
#include "PID.h"

namespace libcontroller
{

using namespace util;


/*
 * This is a general PID control law function.  The output is the 
 * result of the controller.
 *
 *  command[0] ---->O--> LIMITER --> Kp ---------------+
 *                - |              |                   |
 *  feedback[0] ----+             INTEGRATE ---> Ki ---+---> OUTPUT
 *                                                     |
 *  command[1] ---->O--> LIMITER --> Kd ---------------+
 *                - |
 *  feedback[1] ----+                              
 */
double
PID::step()
{
	PID *			pid = this;

	double err = limit(
		pid->commend_values[0] - pid->feedback_values[0],
		pid->ProErrorLimits[0],
		pid->ProErrorLimits[1]
	);

	double velerr = limit(
		pid->commend_values[1] - pid->feedback_values[1],
		pid->VelErrorLimits[0],
		pid->VelErrorLimits[1]
	);

	double result =	0.0
		+ pid->Kp*err
		+ pid->Kd*velerr
		+ pid->Ki * pid->int_state;


	pid->int_state = limit(
		pid->int_state + err*pid->int_dt,
		pid->IntStateLimits[0],
		pid->IntStateLimits[1]
	);

	return limit(
		result,
		pid->OutErrorLimits[0],
		pid->OutErrorLimits[1]
	);
}

}

