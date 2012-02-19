 /* -*- indent-tabs-mode:T; c-basic-offset:8; tab-width:8; -*- vi: set ts=8:
 * $Id: FlatEarth.cpp,v 2.0 2002/09/22 02:07:31 tramm Exp $
 *
 * (c) Aaron Kahn
 * (c) Trammell Hudson
 *
 * This is a six degree of freedom rigid body dynamic simulator.
 * It is similar the above sixdof() function, but instead of using
 * WGS-84 round earth simulation, this will use the flat-earth
 * approximations.  The inputs are similar in both cases, except that
 * for this simulation, no information is needed on the home latitude
 * and longitude.  All that is needed is the starting NED (north east down)
 * position.  The main difference is that altitude is now + down not up.  
 *
 * Propogation of the dynamics is done with RK4.
 *
 * NOTE: ALL VALUES ARE AT THE CG OF THE VEHCILE!
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

#include <stdio.h>
#include <cmath>
#include <stdlib.h>
#include <string.h>

#include "FlatEarth.h"
#include "macros.h"

#include <mat/rk4.h>
#include <mat/Matrix.h>
#include <mat/Vector.h>
#include <mat/Quat.h>

namespace sim
{

using namespace libmat;
using namespace util;


 /*
 * The below are hidden structures and functions for the propogation
 * of the 6-DOF simulation model.
 */
typedef struct
{
	Velocity<Frame::NED>	Vned;		// [Vn Ve Vd] (ft/s)
	Position<Frame::NED>	NED;		// [N E D] (ft)
	Rate<Frame::Body>	pqr;		// [P Q R] (rad/s)
	Quat			Q;		// [q0 q1 q2 q3]
} sixdofX_fe_def;


typedef struct
{
	Accel<Frame::NED>	Vned_dot;	// [Vndot Vedot Vddot] (ft/s/s)
	Velocity<Frame::NED>	NED_dot;	// [Ndot Edot Ddot] (ft/s)
	RateDot<Frame::Body>	pqr_dot;	// [Pdot Qdot Rdot] (rad/s/s)
	Quat			Q_dot;		// [q0dot q1dot q2dot q3dot]
} sixdofXdot_fe_def;



void
sixdof_fe_init(
	sixdof_fe_init_inputs_def *pInit, 
	sixdof_fe_inputs_def *	pIn,
	sixdof_fe_state_def *	pX)
{
	// compute J
	pIn->J[0][0] = pInit->Ixx;			// slug-ft^2
	pIn->J[0][1] = 0.0;
	pIn->J[0][2] = -pInit->Ixz;			// slug-ft^2
	pIn->J[1][0] = 0.0;
	pIn->J[1][1] = pInit->Iyy;			// slug-ft^2
	pIn->J[1][2] = 0.0;		
	pIn->J[2][0] = -pInit->Ixz;			// slug-ft^2
	pIn->J[2][1] = 0.0;
	pIn->J[2][2] = pInit->Izz;			// slug-ft^2

	// compute Jinv
	double gamma = pInit->Ixx*pInit->Izz - sqr(pInit->Ixz);
	pIn->Jinv[0][0] = pInit->Izz/gamma;
	pIn->Jinv[0][1] = 0.0;
	pIn->Jinv[0][2] = pInit->Ixz/gamma;
	pIn->Jinv[1][0] = 0.0;
	pIn->Jinv[1][1] = 1.0/pInit->Iyy;
	pIn->Jinv[1][2] = 0.0;
	pIn->Jinv[2][0] = pInit->Ixz/gamma;
	pIn->Jinv[2][1] = 0.0;
	pIn->Jinv[2][2] = pInit->Ixx/gamma;

	// initialize the quaternion
	pX->Q = euler2quat( pInit->THETA.v );

	pX->accel.fill();
	pX->alpha.fill();
	pIn->F.fill();
	pIn->M.fill();
	pX->rate	= pInit->pqr;
	pX->Vb		= pInit->uvw;
	pX->THETA	= pInit->THETA;
	pX->NED		= pInit->NED;

	// initialize the velocity in earth frame (ft/s)
	pX->Ve		= quatDC( pX->Q ).transpose() * pX->Vb.v;
	//pX->Ve		= rotate( pX->Vb, pInit->THETA );
	pIn->m		= pInit->m;				// slug
}


static void
sixdof_fe_derivs(
	sixdofXdot_fe_def *	pXdot, 
	sixdofX_fe_def *	pX, 
	sixdof_fe_inputs_def *	pU
)
{
	// make the wx matrix (omega-cross matrix)
	const Matrix<3,3> 	wx( eulerWx( pX->pqr.v ) );

	//const Matrix<3,3> 	cBE( quatDC( pX->Q ) );
	//const Matrix<3,3> 	cEB( cBE.transpose() );
	const Rotate<Frame::Body,Frame::NED>	cBE( quatDC( pX->Q ) );
	const Rotate<Frame::NED,Frame::Body>	cEB( cBE.m.transpose() );

	// Compute the body accelerations
	// rotate the ned velocity to body frame
	const Velocity<Frame::Body> 	Vb_b( cBE * pX->Vned );

	// a = F/m - w X v
	Accel<Frame::Body>	a( pU->F.v / pU->m - wx * Vb_b.v );
	
	// check holds
	if( pU->hold_u )
		a[0] = 0.0;
	if( pU->hold_v )
		a[1] = 0.0;
	if( pU->hold_w )
		a[2] = 0.0;

	// rotate the body acceleration into NED frame
	pXdot->Vned_dot = cEB * a;

	// compute NED rate of change
	pXdot->NED_dot = pX->Vned;

	// Compute body angular accelerations
	// wdot_body = -Jinv*Omega*J*w_body + Jinv*M_body
	pXdot->pqr_dot	= pU->Jinv * (pU->M.v - wx * pU->J * pX->pqr.v );

	// check holds on angular rates
	if( pU->hold_p )
		pXdot->pqr_dot[0] = 0.0;
	if( pU->hold_q )
		pXdot->pqr_dot[1] = 0.0;
	if( pU->hold_r )
		pXdot->pqr_dot[2] = 0.0;

	// Compute the quaternion derivatives
	// make the E matrix == strapdown matrix
	pXdot->Q_dot = quatW( pX->pqr.v ) * pX->Q;
}


static void
integrate_sixdof_fe(
	sixdofXdot_fe_def *	pXdot, 
	sixdofX_fe_def *	pX, 
	sixdof_fe_inputs_def *	pU, 
	double			dt
)
{
	// to run the derivative function
	sixdofX_fe_def		X;

	// to backup the input state
	sixdofX_fe_def		X0;

	// to run the derivative function
	sixdofXdot_fe_def	Xdot;

	// backing up the current input state
	X0.NED		= pX->NED;
	X0.pqr		= pX->pqr;
	X0.Vned		= pX->Vned;
	X0.Q		= pX->Q;

	// Well, lets get started with the first step
	// first, make the X state value
	X.NED		= X0.NED;
	X.pqr		= X0.pqr;
	X.Vned		= X0.Vned;
	X.Q		= X0.Q;
	X.Q.norm_self();

	// run the function and get the Xdot values
	sixdof_fe_derivs(&Xdot, &X, pU);

	// save the k step
	Velocity<Frame::NED>	k1_NED(	 Xdot.NED_dot );
	RateDot<Frame::Body>	k1_pqr(	 Xdot.pqr_dot );
	Accel<Frame::NED>	k1_Vned( Xdot.Vned_dot );
	Quat			k1_Q(    Xdot.Q_dot );

	k1_NED		*= dt;
	k1_pqr		*= dt;
	k1_Vned		*= dt;
	k1_Q		*= dt;

	// make the state value for the next step
	// This could be more clearly written as:
	//	X.foo = k1_foo / 2.0 + X0.foo
	// but g++ can't see that and creates lots of temporaries
	// where we don't need them.
	//
	// The explict .v is to get around the type system.  If we had a
	// dt type, we could show the conversion between position and
	// velocity (etc).
	((X.NED		= k1_NED.v)  /= 2.0) += X0.NED;
	((X.pqr		= k1_pqr.v)  /= 2.0) += X0.pqr;
	((X.Vned	= k1_Vned.v) /= 2.0) += X0.Vned;
	((X.Q		= k1_Q)      /= 2.0) += X0.Q;
	X.Q.norm_self();

	// save the Xdot values for output
	pXdot->NED_dot	= Xdot.NED_dot;
	pXdot->pqr_dot	= Xdot.pqr_dot;
	pXdot->Vned_dot	= Xdot.Vned_dot;
	pXdot->Q_dot	= Xdot.Q_dot;
	

	// run the function and get the Xdot values
	sixdof_fe_derivs(&Xdot, &X, pU);

	// save the k step
	Velocity<Frame::NED>	k2_NED(	 Xdot.NED_dot );
	RateDot<Frame::Body>	k2_pqr(	 Xdot.pqr_dot );
	Accel<Frame::NED>	k2_Vned( Xdot.Vned_dot );
	Quat			k2_Q(    Xdot.Q_dot );

	k2_NED		*= dt;
	k2_pqr		*= dt;
	k2_Vned		*= dt;
	k2_Q		*= dt;

	// make the state value for the next step
	((X.NED		= k2_NED.v)  /= 2.0) += X0.NED;
	((X.pqr		= k2_pqr.v)  /= 2.0) += X0.pqr;
	((X.Vned	= k2_Vned.v) /= 2.0) += X0.Vned;
	((X.Q		= k2_Q)      /= 2.0) += X0.Q;
	X.Q.norm_self();

	// run the function and get the Xdot values
	sixdof_fe_derivs(&Xdot, &X, pU);

	// save the k step
	Velocity<Frame::NED>	k3_NED(	 Xdot.NED_dot );
	RateDot<Frame::Body>	k3_pqr(	 Xdot.pqr_dot );
	Accel<Frame::NED>	k3_Vned( Xdot.Vned_dot );
	Quat			k3_Q(    Xdot.Q_dot );

	k3_NED		*= dt;
	k3_pqr		*= dt;
	k3_Vned		*= dt;
	k3_Q		*= dt;

	// make the state value for the next step
	(X.NED		= k3_NED.v)  += X0.NED;
	(X.pqr		= k3_pqr.v)  += X0.pqr;
	(X.Vned		= k3_Vned.v) += X0.Vned;
	(X.Q		= k3_Q)      += X0.Q;
	X.Q.norm_self();

	// run the function and get the Xdot values
	sixdof_fe_derivs(&Xdot, &X, pU);
	Velocity<Frame::NED>	k4_NED(	 Xdot.NED_dot );
	RateDot<Frame::Body>	k4_pqr(	 Xdot.pqr_dot );
	Accel<Frame::NED>	k4_Vned( Xdot.Vned_dot );
	Quat			k4_Q(    Xdot.Q_dot );

	k4_NED		*= dt;
	k4_pqr		*= dt;
	k4_Vned		*= dt;
	k4_Q		*= dt;

	// Scale the kX values.  I don't know why.
	k1_NED	/= 6.0;
	k1_pqr	/= 6.0;
	k1_Vned	/= 6.0;
	k1_Q	/= 6.0;

	k2_NED	/= 3.0;
	k2_pqr	/= 3.0;
	k2_Vned	/= 3.0;
	k2_Q	/= 3.0;

	k3_NED	/= 3.0;
	k3_pqr	/= 3.0;
	k3_Vned	/= 3.0;
	k3_Q	/= 3.0;

	k4_NED	/= 6.0;
	k4_pqr	/= 6.0;
	k4_Vned	/= 6.0;
	k4_Q	/= 6.0;

	// assemble the final result for the state propogation
	// This is more clearly written as:
	//	pX->foo = X0.foo + k1_foo + k2_foo + k3_foo + k4_foo
	// but g++ doesn't see the optimization.
	// Saves us 5 usec per invocation.

	((((pX->NED  = X0.NED)  += k1_NED.v)  += k2_NED.v)  += k3_NED.v)  += k4_NED.v;
	((((pX->pqr  = X0.pqr)  += k1_pqr.v)  += k2_pqr.v)  += k3_pqr.v)  += k4_pqr.v;
	((((pX->Vned = X0.Vned) += k1_Vned.v) += k2_Vned.v) += k3_Vned.v) += k4_Vned.v;
	((((pX->Q    = X0.Q)    += k1_Q)    += k2_Q)    += k3_Q)    += k4_Q;

	pX->Q.norm_self();
}


void
sixdof_fe(
	sixdof_fe_state_def *	pX,
	sixdof_fe_inputs_def *	pU,
	double			dt
)
{
	sixdofX_fe_def		X;
	sixdofXdot_fe_def	Xdot;

	const Rotate<Frame::Body,Frame::NED> 	cBE( quatDC( pX->Q ) );

	// equate the state vectors and such
	X.pqr		= pX->rate;
	X.Vned		= pX->Ve;
	X.NED		= pX->NED;
	X.Q		= pX->Q.norm();

	// call integrate_sixdof
	integrate_sixdof_fe( &Xdot, &X, pU, dt );

	// solve for the unknown state values for the next time step
	pX->alpha	= Xdot.pqr_dot;		// body axis angular accels
	pX->rate	= X.pqr;		// body axis angular rates
	pX->accel	= pU->F.v / pU->m;	// body axis acceleration
	pX->Ve		= X.Vned;		// body axis velocity in NED
	pX->NED		= X.NED;		// lat, lon, alt position
	pX->Q		= X.Q;
	pX->THETA	= quat2euler( X.Q );	// Convert attitude to euler
	pX->Vb		= cBE * X.Vned;		// body velocity in body frame
}

}
