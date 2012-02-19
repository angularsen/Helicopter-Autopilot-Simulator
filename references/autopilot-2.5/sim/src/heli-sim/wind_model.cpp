/* -*- indent-tabs-mode:T; c-basic-offset:8; tab-width:8; -*- vi: set ts=8:
 * $Id: wind_model.cpp,v 2.0 2002/09/22 02:07:31 tramm Exp $
 *
 * (c) Aaron Kahn
 * (c) Trammell Hudson
 *
 * This is a basic wind/gust model.  It is a dynamic system that will
 * generate random winds, up to a maximum value, in both vertical and
 * horizontal directions.
 *
 * To make this model work, like the servo and 6-DOF models, it is
 * propogated over time for the dynamics.
 *
 * See the structures for information
 *
 * There are two functions.  The first, wind_init is used to initalize
 * the wind model.  The second, wind, is used to propograte the state
 * of the wind model.
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

#include <cstdio>
#include <cstdlib>
#include <cstring>
#include <cmath>

#include "wind_model.h"
#include "macros.h"
#include <mat/rk4.h>

namespace sim
{

using namespace util;


void
wind_init(
	wind_inputs_def *	pIn,
	wind_state_def *	pX
)
{
	int			i;

	for( i=0 ; i < 3 ; i++ )
		pX->Ve[i] = 0;
	for( i=0 ; i < 6 ; i++ )
		pX->X[i] = 0;

	srand( pIn->seed );		/* seed the random number generator */
}


static void
wind_derivs(
	Vector<6> &		Xdot,
	const Vector<6> &	X,
	const double 		UNUSED( t ),
	const Vector<3> &	u,
	const Vector<2> &	args
)
{
	double			wn	= args[0];
	double			zeta	= args[1];

	/* Vn component */
	Xdot[0] = X[1];
	Xdot[1] = -2.0*zeta*wn*X[1] - wn*wn*X[0] + u[0];

	/* Ve component */
	Xdot[2] = X[3];
	Xdot[3] = -2.0*zeta*wn*X[3] - wn*wn*X[2] + u[1];

	/* Vd component */
	Xdot[4] = X[5];
	Xdot[5] = -2.0*zeta*wn*X[5] - wn*wn*X[4] + u[2];
}


void
wind_model(
	wind_inputs_def *	pIn,
	wind_state_def *	pX,
	double			dt
)
{
	Vector<2>		args;
	args[0]		= 1.5;
	args[1]		= 0.00005;

	const Vector<3>		U(
 		10.0 * ((double)(rand())/( (double)(RAND_MAX)/2.0) - 1.0),
 		10.0 * ((double)(rand())/( (double)(RAND_MAX)/2.0) - 1.0),
 		10.0 * ((double)(rand())/( (double)(RAND_MAX)/2.0) - 1.0)
	);

	Vector<6>		Xdot;

	RK4( pX->X, Xdot, 0, U, args, dt, &wind_derivs );

	pX->Ve[0] = pX->X[0] * sqr(args[0]) * pIn->wind_max[0];
	pX->Ve[1] = pX->X[2] * sqr(args[0]) * pIn->wind_max[1];
	pX->Ve[2] = pX->X[4] * sqr(args[0]) * pIn->wind_max[2];
}

}
