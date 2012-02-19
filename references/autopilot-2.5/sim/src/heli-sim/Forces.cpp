/* -*- indent-tabs-mode:T; c-basic-offset:8; tab-width:8; -*- vi: set ts=8:
 * $Id: Forces.cpp,v 2.0 2002/09/22 02:07:31 tramm Exp $
 *
 * (c) Aaron Kahn
 * (c) Trammell Hudson
 *
 * Random force manipulation functions
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

#include "Forces.h"
#include <mat/Vector_Rotate.h>
#include <mat/Quat.h>
#include <mat/Nav.h>

namespace sim
{

using namespace libmat;


double
Forces::rho()
{
	// compute the current atmospheric conditions
	// density of air (slug/ft^3)
	double			rho;

	// air pressure (lb/ft^2)
	double			pressure;

	// air temperature (deg R)
	double			temperature;

	// local speed of sound (ft/s)
	double			sp_sound;

	// current density altitude (ft MSL + up)
	double			densAlt = this->altitude - this->NED[2];

	atmosphere(
		densAlt,
		&rho,
		&pressure,
		&temperature,
		&sp_sound
	);

	return rho;
}


}
