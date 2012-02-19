/* -*- indent-tabs-mode:T; c-basic-offset:8; tab-width:8; -*- vi: set ts=8:
 * 
 * (c) Aaron Kahn
 * (c) Trammell Hudson
 *
 * The PID class implements a SISO loop that uses PID control.
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

#ifndef _PID_H_
#define _PID_H_

namespace libcontroller
{


class PID
{
public:
	PID(
		double			Kp,
		double			Kd,
		double			Ki,
		double			dt	= 0.0
	) :
		Kp(Kp),
		Kd(Kd),
		Ki(Ki),
		int_dt(dt),
		int_state(0.0)
	{
		this->reset();
	}

	PID()
	{
		this->reset();
	}


	~PID() {}

	// Set the desired point
	void commend(
		double			position,
		double			velocity
	)
	{
		this->commend_values[0] = position;
		this->commend_values[1] = velocity;
	}


	// Update the feedback values
	void feedback(
		double			position,
		double			velocity
	)
	{
		this->feedback_values[0] = position;
		this->feedback_values[1] = velocity;
	}


	// Run the filter
	double step();
	void reset()
	{
		this->int_state		= 0.0;
		this->limit_int( -1000, 1000 );
		this->limit_pro( -1000, 1000 );
		this->limit_vel( -1000, 1000 );
		this->limit_out( -1000, 1000 );
	};


	// Set limits on the different terms
	void limit_int(
		double			min,
		double			max
	)
	{
		this->IntStateLimits[0]	= min;
		this->IntStateLimits[1] = max;
	}


	void limit_vel(
		double			min,
		double			max
	)
	{
		this->VelErrorLimits[0]	= min;
		this->VelErrorLimits[1] = max;
	}


	void limit_pro(
		double			min,
		double			max
	)
	{
		this->ProErrorLimits[0]	= min;
		this->ProErrorLimits[1] = max;
	}


	void limit_out(
		double			min,
		double			max
	)
	{
		this->OutErrorLimits[0] = min;
		this->OutErrorLimits[1] = max;
	}


private:
	// Gains for proportion, derivative and integral terms
	double			Kp;
	double			Kd;
	double			Ki;

	// integrator state and time step
	// dt == 0 implies no integration
	double			int_dt;
	double			int_state;

	// Limits on the error terms.  [MIN MAX]
	double			ProErrorLimits[2];
	double			VelErrorLimits[2];
	double			IntStateLimits[2];
	double			OutErrorLimits[2];

	// [position velocity] commands
	double			commend_values[2];

	// [position velocity] state feedback values
	double			feedback_values[2];
};

}

#endif
