/* -*- indent-tabs-mode:T; c-basic-offset:8; tab-width:8; -*- vi: set ts=8:
 * $Id: Blade.cpp,v 2.0 2002/09/22 02:07:31 tramm Exp $
 *
 * (c) Aaron Kahn
 * (c) Trammell Hudson
 *
 * Blade element calculations
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

#include <stdlib.h>
#include <cmath>

#include "macros.h"
#include "Blade.h"

#include <mat/Conversions.h>
#include <mat/rk4.h>

namespace sim {

using namespace util;

// number of stations to use in blade element (total is always 100)
static const int	num_stations	= 93;



/*
 * This will do a combined blade element momentum theory thrust, power,
 * torque computation on a rotor.  The inputs to the function are all
 * of the input elements of blade_element_def.  Only T, P, Q, and
 * average v are outputs.
 *
 * The directions are as follows...
 *
 *	Vperp = perpendicular velocity to rotor disk
 *		(+ opposite direction of thrust)
 *
 *	avg_v1 = average induced velocity
 *		(+ opposite direction of thrust)
 *
 * Source:
 *	Keys, Pgs 96-98
 *	Proudy, Pg 96
 *
 * See the structure for more information on the input and output parameters.
 */
void
Blade::step()
{
	// Reset the outputs for this computation.
	this->T		= 0.0;
	this->Q		= 0.0;
	this->P		= 0.0;
	this->avg_v1	= 0.0;

	if( isnan(this->Vperp) )
		abort();

	// abcOmega/2 + 4piVperp
	const double		vv	=
		this->a * this->b * this->c * this->omega / 2.0
		+ 4.0 * C_PI * this->Vperp;

	// thickness of the blade element
	const double		dR	=
		(this->R - this->R0) / 100.0;

	// root collective angle
	const double		theta0	=
		fabs(this->collective)
		- this->twst * (0.75 - (this->R0/this->R));

	// Constants hoisted out of the loop
	const double		omega2	= sqr( this->omega );
	const double		vv2	= sqr( vv );
	const double		proudy	= 
		8.0 * C_PI * omega2 * this->a * this->b * this->c;
	const double		keys1	=
		this->Cd0 * this->rho * this->c * dR / 2.0;
	const double		keys2	=
		4.0 * C_PI * this->rho * dR;

	for( int i=0 ;  i<num_stations ;  ++i )
	{
		// ratio of local radius to total radius
		const double	r_R	= (this->R0 + double(i+1)*dR) / this->R;

		// local radius
		const double	r	= r_R * this->R;

		// local collective angle
		const double	theta_r	= theta0 + this->twst*r_R;

		// local velocity
		const double	omega_r	= this->omega*r;

		// local angle of attack
		const double	alpha	= theta_r - this->Vperp/omega_r;

		// Proudy pg 96
		const double	temp	= vv2 + proudy * r * alpha;

		// local induced velocity
		const double	v1	= 
			( sqrt(fabs(temp)) - vv ) / ( 8.0 * C_PI );

		const double	temp2	= this->Vperp + v1;

		// Keys eq 3.11
		// incriment of thrust
		const double	dT	= keys2 * temp2 * v1 * r;

		// Keys eq 3.5
		// incriment of profile drag
		const double	dD	= keys1 * sqr(omega_r);

		// Keys eq 3.9a
		// incriment of torque
		const double	dQ	= r * ( dT*temp2/omega_r + dD );

		// Keys eq 3.9b
		// incriment of power
		const double	dP	= dQ * this->omega;

		// Add this blade element to the entire blade
		this->T		+= dT;
		this->Q		+= dQ;
		this->P		+= dP;
		this->avg_v1	+= v1;
	}

	this->avg_v1	= this->avg_v1 / num_stations;
	
	if( this->collective < 0.0 )
	{
		this->T		*= -1.0;
		this->avg_v1	*= -1.0;
	}
}

}
