/* -*- indent-tabs-mode:T; c-basic-offset:8; tab-width:8; -*- vi: set ts=8:
 * $Id: gravity_model.h,v 2.0 2002/09/22 02:07:31 tramm Exp $
 *
 * (c) Aaron Kahn
 * (c) Trammell Hudson
 *
 * This is a gravity and gravitation model.
 * It is based on the WGS-84 elipsoid model of the Earth.
 * The source for this model is from Dr. Brian Steven's class.
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

#ifndef _GRAVITY_MODEL_H_
#define _GRAVITY_MODEL_H_

#include <mat/Vector.h>

namespace sim
{

using libmat::Vector;


typedef struct
{
	// current geodetic latitude (rad)
	double			latitude;

	// current altitude (ft MSL + up)
	double			altitude;
} grav_inputs_def;


typedef struct
{
	// local gravity vector in earth TP frame [gN gE gD] (ft/s/s)
	Vector<3>		g;

	// local gravitation vector in earth TP frame [GN GE GD] (ft/s/s)
	Vector<3>		G;
} grav_outputs_def;


extern void
gravity_model(
	grav_inputs_def *		pIn,
	grav_outputs_def *		pOut
);


}

#endif
