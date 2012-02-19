/* -*- indent-tabs-mode:T; c-basic-offset:8; tab-width:8; -*- vi: set ts=8:
 * $Id: wind_model.h,v 2.0 2002/09/22 02:07:31 tramm Exp $
 *
 * (c) Aaron Kahn
 * (c) Trammell Hudson
 *
 * This is a basic wind/gust model.
 *
 * It is a dynamic system that will generate random winds, up to a
 * maximum value, in both vertical and horizontal directions.
 *
 * To make this model work, like the servo and 6-DOF models, it is
 * propogated over time for the dynamics.
 *
 * There are two functions.  The first, wind_init is used to initalize
 * the wind model. The second, wind_model, is used to propograte the
 * state of the wind model.
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


#ifndef _WIND_MODEL_H_
#define _WIND_MODEL_H_

#include "macros.h"
#include <mat/Vector.h>
#include <mat/Frames.h>

namespace sim
{

using namespace libmat;

typedef struct
{
	/* maximum wind vector */
	Velocity<Frame::NED>	wind_max;

	/* seed variable for the random number generator */
	int			seed;
} wind_inputs_def;


typedef struct
{
	/* components of wind in earth TP frame [Vn Ve Vd] (ft/s) */
	Velocity<Frame::NED>	Ve;

	/* internal state */
	Vector<6>		X;
} wind_state_def;


extern void
wind_init(
	wind_inputs_def *	pIn,
	wind_state_def *	pX
);


extern void
wind_model(
	wind_inputs_def *	pIn,
	wind_state_def *	pX,
	double			dt
);

}

#endif
