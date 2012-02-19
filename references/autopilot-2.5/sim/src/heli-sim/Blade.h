/* -*- indent-tabs-mode:T; c-basic-offset:8; tab-width:8; -*- vi: set ts=8:
 * $Id: Blade.h,v 2.0 2002/09/22 02:07:31 tramm Exp $
 *
 * (c) Aaron Kahn
 * (c) Trammell Hudson
 *
 * This is the blade elment computations
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


#ifndef _BLADE_MODEL_H_
#define _BLADE_MODEL_H_

namespace sim {

/*
 *  Structure for the blade element calculations
 */
class Blade
{
public:
	Blade() {}
	~Blade() {}

	void step();


	// lift curve slope (*/rad)
	double a;

	// blade radius (ft)
	double R;

	// rotor angular velocity (rad/s)
	double omega;

	// number of rotor blades
	double b;

	// rotor blade chord (ft)
	double c;

	// blade root cutout (ft)
	double R0;

	// blade collective pitch @ 75% R (rad)
	double collective;

	// blade twist (- for washout rad)
	double twst;

	// profile drag cooeficent (nondimentional)
	double Cd0;

	// oswald efficency factor (nondimentional)
	double e;

	// perpendicular velocity to rotor disk (ft/s - in direction of T)
	double Vperp;

	// air density (slug/ft^3)
	double rho;

	// output thrust (lbs)
	double T;

	// output torque (lb-ft)
	double Q;

	// output power (lb-ft/s)
	double P;

	// output average induced velocity (ft/s)
	double avg_v1;

};



}

#endif

