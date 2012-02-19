/* -*- indent-tabs-mode:T; c-basic-offset:8; tab-width:8; -*- vi: set ts=8:
 * $Id: Servo.h,v 2.0 2002/09/22 02:07:31 tramm Exp $
 *
 * (c) Aaron Kahn
 * (c) Trammell Hudson
 *
 * This function will provide a servo-actuator model.
 *
 * It contains two main components.  The first is a second-order
 * dynamic model which is tuneable via wn and zeta values.  The second,
 * is a hysterisous model.  This feature is designed around basic
 * deadband slop that may be found in system linkages.
 *
 * See the structure for details.
 *
 * Also, this system contains a state structure that needs to be
 * maintained for state propogation.
 *
 * The TF model of the servo used is as follows...
 *
 *          S + wn^2            y
 *  ------------------------ = ---
 *  S^2 + 2*zeta*wn*S + wn^2    u
 *
 * Nominal values of wn and zeta are...  wn = 10.43; zeta = 0.8; 
 * (for KR2000 actuators)
 *
 * Use this equation to find the appropriate value of wn assuming
 * zeta = 0.8...
 *
 *                pi
 *  wn =  ------------------
 *        Tp*sqrt(1 - zeta^2)
 *
 * Tp = peek time (ex: servo takes 0.6 sec to go 60 deg, Tp = 0.6)
 *
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
#ifndef _SERVO_MODEL_H_
#define _SERVO_MODEL_H_

#include <mat/Vector.h>

namespace sim {

using libmat::Vector;

class Servo
{
public:
	Servo(
		double			min,
		double			max,
		double			wn	= 38.2261,
		double			zeta	=  0.5118,
		double			slop	=  0.0000
	) :
		min(min),
		max(max),
		wn(wn),
		zeta(zeta),
		slop(slop)
	{
	}

	virtual ~Servo() {}

	virtual double
	step(
		double			dt,
		double			command
	);

private:
	/* Maximum number of steps between the max and min */
	static const double	max_steps;

	/*
	 * Minimum and maximum rotation (of the swashplate, not
	 * the servo itself)
	 */
	double		min;
	double		max;

	/* natural freq. of the dynamic model */
	double		wn;

	/* damping ratio of the dynamic model */
	double		zeta;

	/*
	 * half of the total slop in the system for hysterisous
	 * (ie: +/- slop)
	 */
	double		slop;

	/*
	 *  Internal state vector
	 */
	Vector<2>	X;
};



/*
 *  Basic 9202 servos are slow
 */
class futaba_9202 :
	public Servo
{
public:
	futaba_9202(
		double			max,
		double			min
	) : Servo(
		max,
		min,
		38.2261,
		 0.5118,
		 0.0000
	)
	{
	}
};


/*
 *  Digital 9250 servos are much faster
 */
class futaba_9253 :
	public Servo
{
public:
	futaba_9253(
		double			max,
		double			min
	) : Servo(
		max,
		min,
		32.2261,
		 0.5118,
		 0.0000
	)
	{
	}
};


}
#endif
