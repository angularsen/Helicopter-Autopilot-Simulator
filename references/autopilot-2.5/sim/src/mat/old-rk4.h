/* -*- indent-tabs-mode:T; c-basic-offset:8; tab-width:8; -*- vi: set ts=8:
 * $Id: old-rk4.h,v 2.0 2002/09/22 02:07:32 tramm Exp $
 *
 * (c) Aaron Kahn
 * (c) Trammell Hudson
 *
 * Fourth order Runge Kutta code
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
#ifndef _RK4_H_
#define _RK4_H_


/*
 *  User function to be integrated
 */
typedef void (*rk4_func)(
	double *		Xdot_out,	/* returned state derivative */
	const double *		X_in,		/* state vector */
	const double *		t_in,		/* time */
	const double *		Uin,		/* forcing values */
	double *		args_in		/* arguments for derivative */
);



/*
 * This is a 4th order Runga-Kutta integration routine.  It is designed to
 * work with a vector of inputs, and produce a vector of outputs at the
 * next time step.
 *
 * A time step input is needed for the dt value, and the number of states
 * to integrate. There is the requirement that the function to be integrated
 * is in a specific format...
 *
 * The state out of this function is the state at t+dt.
 */
extern void
RK4(
	double *		state_in_out,	/* state vector */
	double *		state_dot_out,	/* derivative of state vec */
	double			t_in,		/* current time */
	double *		force_in,	/* forcing values */
	double *		args_in,	/* arguments to derivative */
	int			n,		/* number of steps */
	double			dt, 		/* time step */
	rk4_func		pfunc		/* user function */
);


#endif
