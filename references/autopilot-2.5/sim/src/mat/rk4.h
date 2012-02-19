/* -*- indent-tabs-mode:T; c-basic-offset:8; tab-width:8; -*- vi: set ts=8:
 * $Id: rk4.h,v 2.1 2003/03/03 16:10:22 tramm Exp $
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

namespace libmat
{

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
template<
	class			state_t,		// State object
	class			force_t,		// Forcing values
	class			args_t			// Misc params
>
void
RK4(
	state_t &		state,
	state_t &		state_dot,
	double			t,
	const force_t &		force,
	const args_t &		args,
	double			dt, 
	void			(*func)(
		state_t &		Xdot_out,	// Derivative output
		const state_t &		X_in,		// Current state
		const double 		t_in,		// Current time
		const force_t &		U_in,		// Forcing values
		const args_t &		args		// Misc params
	)
)
{
	/* backup the original state vector */
	const state_t		X0( state );

	/* the first step */
	func( state_dot, X0, t, force, args );

	state_t			X;
	state_t			Xdot( state_dot );
	state_t			k1( Xdot );
	k1 *= dt;

#ifdef NO_OPTIMIZATION
	X = X0 + k1 / 2.0;
#else
	((X = k1 ) /= 2.0 ) += X0;
#endif

	/* the second step */
	func( Xdot, X, t + dt / 2.0, force, args );

	state_t			k2( Xdot );
	k2 *= dt;

#ifdef NO_OPTIMIZATION
	X = X0 + k2 / 2.0;
#else
	((X = k2 ) /= 2.0 ) += X0;
#endif

	/* the third step */
	func( Xdot, X, t + dt / 2.0, force, args );
	state_t			k3( Xdot );

	k3 *= dt;

#ifdef NO_OPTIMIZATION
	X = X0 + k3;
#else
	(X = k3) += X0;
#endif

	/* the forth step */
	func( Xdot, X, t + dt, force, args );
	state_t			k4( Xdot );

	k4 *= dt;

	/* make the final result */
#ifdef NO_OPTIMIZATION
	state = X0
		+ k1 / 6.0
		+ k2 / 3.0
		+ k3 / 3.0
		+ k4 / 6.0;
#else
	state = X0;
	k1 /= 6.0;
	k2 /= 3.0;
	k3 /= 3.0;
	k4 /= 6.0;
	(((state += k1) += k2) += k3) += k4;
#endif
}

}
#endif
