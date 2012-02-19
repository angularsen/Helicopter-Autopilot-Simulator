/* -*- indent-tabs-mode:T; c-basic-offset:8; tab-width:8; -*- vi: set ts=8:
 * $Id: Fin.cpp,v 2.0 2002/09/22 02:07:31 tramm Exp $
 *
 * (c) Aaron Kahn
 * (c) Trammell Hudson
 *
 * Aerodynamic fin simulation
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

#include "Fin.h"
#include <mat/Frames.h>
#include <cmath>

namespace sim {

using namespace libmat;

/*
 * Computes the rotor downwash and other forces on the aerodynamic
 * bodies related to the aircraft.
 */
void
Fin::step(
	Forces *		cg,
	double			downwash
)
{
	double			rho2 = cg->rho() / 2.0;

	const Force<Frame::Body>	F(
		rho2 * this->xuu * cg->uvw[0] * fabs(cg->uvw[0]),
		rho2 * this->yvv * cg->uvw[1] * fabs(cg->uvw[1]),
		rho2 * this->zww * cg->uvw[2] * fabs(cg->uvw[2])
			- rho2 * this->zww * downwash
	);

	const Moment<Frame::Body>	M(
		                F[1]*this->h,
		 F[2]*this->d - F[0]*this->h,
		-F[1]*this->d
	);

	// Sum Up Total Forces and Moments At CG
	cg->F += F;
	cg->M += M;
}

}
