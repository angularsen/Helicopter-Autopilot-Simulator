/* -*- indent-tabs-mode:T; c-basic-offset:8; tab-width:8; -*- vi: set ts=8:
 * $Id: Fin.h,v 2.0 2002/09/22 02:07:31 tramm Exp $
 *
 * (c) Aaron Kahn
 * (c) Trammell Hudson
 *
 * Aerodynamic qualities for fins, fuselages and other static objects
 * on the aircraft.  Skids and sensors could use this, too.
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


#ifndef _FIN_H_
#define _FIN_H_

#include "Forces.h"


namespace sim {

class Fin
{
public:
	Fin(
		const Forces *		cg,
		double			fs,
		double			wl,
		double			xuu,
		double			yvv,
		double			zww
	) :
		fs(fs),
		wl(wl),
		xuu(xuu),
		yvv(yvv),
		zww(zww)
	{
		this->h = (cg->wl_cg - this->wl) / 12.0;
		this->d = (cg->fs_cg - this->fs) / 12.0;
	}

	~Fin() {}

	void
	step(
		Forces *		cg,
		double			downwash
	);

private:
	// fin horizontal fuse station point (from MR hub in)
	double	fs;

	// fin vertical waterling point (from MR hub in)
	double	wl;

	// horizontal flat plate drag area (ft^2)
	double  xuu;

	// side flat plate drag area (ft^2)
	double  yvv;

	// vertical flat plate drag area (ft^2)
	double  zww;


	// vertical distance of fuse center of pressure to CG (ft)
	double  h;

	// horizontal distance of fin to aircraft CG (ft)
	double	d;
};

}

#endif

