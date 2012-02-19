/* -*- indent-tabs-mode:T; c-basic-offset:8; tab-width:8; -*- vi: set ts=8:
 * $Id: wgs_model.cpp,v 2.0 2002/09/22 02:07:31 tramm Exp $
 *
 * (c) Aaron Kahn
 * (c) Trammell Hudson
 *
 * This is the ECEF (non-flat earth) IMU model
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

#include "macros.h"
#include "wgs_model.h"

#include <mat/Conversions.h>
#include <mat/rk4.h>
#include <mat/Quat.h>
#include <mat/Nav.h>

namespace sim
{

using namespace libmat;
using namespace util;


#define _N	0
#define _E	1
#define _D	2

/*
 * The below are hidden structures and functions for the propogation
 * of the 6-DOF simulation model.
 */
typedef struct
{
	Vector<3>	Vned;		// [Vn Ve Vd] (ft/s)
	Vector<3>	llh;		// [lat lon alt] (rad)(rad)(ft)
	Vector<3>	pqr;		// [P Q R] (rad/s)
	Quat		Q;		// [q0 q1 q2 q3]
} sixdofX_def;


typedef struct
{
	Vector<3>	Vned_dot;	// [Vndot Vedot Vddot] (ft/s/s)
	Vector<3>	llh_dot;	// [latdot londot altdot] (rad/s)(rad/s)(ft/s)
	Vector<3>	pqr_dot;	// [Pdot Qdot Rdot] (rad/s/s)
	Quat		Q_dot;		// [q0dot q1dot q2dot q3dot]
} sixdofXdot_def;



/*
 * This is a 6 degree-of-freedom (6-DOF) rigid body simulation.
 * The values that are needed are listed in the input structure.
 * The other structure, the state structure needs to be initialized
 * with the desired starting values.  
 *
 * An Init function is provided to help with this initialization.
 * It only needs to be called once.  The structure for the Init function
 * is sixdof_init_inputs_def.
 *
 * It is noted that this simulation is based on quaternion parameters.
 * For this reason, euler2quat function needs to be used for the attitude
 * initialization.  Latitude, longitude, and altitude (rad)(rad)(ft MSL + up)
 * need to be used for the initialization.  ECEF is generated from this.
 * Units are listed in the structure comments.  
 *
 * Propogation of the state vector is done with RK4.
 *
 * ALL STATE VALUES ARE ASSUMED TO BE AT THE CG OF THE VEHICLE!!!
 *
 */
void
sixdof_init(
	sixdof_init_inputs_def *pInit, 
	sixdof_inputs_def *	pIn,
	sixdof_state_def *	pX
)
{
	// earth -> body transformation matrix
	const Matrix<3,3>	cBE( eulerDC( pInit->THETA ) );

	// body -> earth transformation matrix
	const Matrix<3,3>	cEB( cBE.transpose() );

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
	pX->Q		= euler2quat( pInit->THETA );

	// initialize the LLH
	pX->Pllh[0]	= pInit->latitude;			// rad
	pX->Pllh[1]	= pInit->longitude;			// rad
	pX->Pllh[2]	= pInit->altitude;			// ft

	// initialize the ECEF.  We need alt in meters for the llh2ecef
	Vector<3>		temp_llh( pX->Pllh );
	temp_llh[2] *= C_FT2M; // 
	pX->Pecef	= llh2ECEF( temp_llh ) * C_M2FT;

	// initialize the all of the other values
	pIn->F.fill();
	pIn->M.fill();
	pX->accel.fill();
	pX->alpha.fill();
	pX->rate	= pInit->pqr;
	pX->Vb		= pInit->uvw;
	pX->THETA	= pInit->THETA;

	// initialize the velocity in earth frame
	pX->Ve		= cEB * pX->Vb;

	// provide a placeholder for the mass value
	pIn->m = 1.0;					// slug
}


static void
sixdof_derivs(
	sixdofXdot_def *	pXdot, 
	sixdofX_def *		pX, 
	sixdof_inputs_def *	pU
)
{
#if 0
	// omega-cross matrix
	double			wx[MAXSIZE][MAXSIZE];

	// quaternion strapdown matrix
	double			E[MAXSIZE][MAXSIZE];

	// earth TP -> body transformation matrix
	double			cBE[MAXSIZE][MAXSIZE];

	// body -> earth TP transformation matrix
	double			cEB[MAXSIZE][MAXSIZE];

	// velocity of the body in NED frame (m)
	double			Vb_n[MAXSIZE];

	// specific body force in NED frame (m/s/s)
	double			Fned[MAXSIZE];

	// copy of body force to play with
	double			Fb[MAXSIZE];

	double			win[MAXSIZE];
	double			tempV1[MAXSIZE];
	double			tempV2[MAXSIZE];
#endif

	double			clat;
	double			f;
	double			e;
	double			Rlat;
	double			Rlon;
	double			h;

	// make the wx matrix (omega cross matrix)
	const Matrix<3,3>	wx( eulerWx( pX->pqr ) );

	// make the cBE matrix
	const Matrix<3,3>	cBE( quatDC( pX->Q ) );

	// make the cEB matrix
	const Matrix<3,3>	cEB( cBE.transpose() );

	/*
	 *  make the radii for the propogation of the latitude,
	 * longitude, altitude.
	 * see pg 191, Farrel for the llh propogation equations
	 */
	f = (C_WGS84_a - C_WGS84_b) / C_WGS84_a;
	e = sqrt(f*(2 - f));
	Rlat = ( C_WGS84_a*(1 - sqr(e)) )/pow( 1-sqr(e)*sqr(sin(pX->llh[0])), 1.5 );	// m
	Rlon = C_WGS84_a/pow( 1-sqr(e)*sqr(sin(pX->llh[0])), 0.5 ); // m
	clat = cos(pX->llh[0]);
	h = pX->llh[2]*C_FT2M;					// ft -> m

	// compute the derivative of the latitude, longitude and altitude
	// first rotate body axis velocity --> TP velocity components
	const Vector<3>		Vb_n( pX->Vned * C_FT2M );

	// compute the rate of change of latitude, longitude, altitude
	pXdot->llh_dot[0] = 1.0/( Rlat + h)*Vb_n[_N];		// rad/s
	pXdot->llh_dot[1] = 1.0/( (Rlon + h)*clat )*Vb_n[_E];	// rad/s
	pXdot->llh_dot[2] = -1.0*Vb_n[_D]*C_M2FT;		// ft/s


	// compute the state time derivative of the angular rate.
	// wdot_body = -Jinv*Omega*J*w_body + Jinv*M_body
	pXdot->pqr_dot = pU->Jinv * ( pU->M - wx * pU->J * pX->pqr );

	// check holds on angular rates
	if( pU->hold_p )
		pXdot->pqr_dot[0] = 0.0;
	if( pU->hold_q )
		pXdot->pqr_dot[1] = 0.0;
	if( pU->hold_r )
		pXdot->pqr_dot[2] = 0.0;

	// check for holds on velocities
	Vector<3>		Fb = pU->F;

	if( pU->hold_u )
		Fb[0] = 0.0;
	if( pU->hold_v )
		Fb[1] = 0.0;
	if( pU->hold_w )
		Fb[2] = 0.0;

	// compute the time derivative of body velocity in Earth TP frame
	// compute the body specific force in NED frame
	const Vector<3>		Fned = cEB * Fb / ( pU->m * C_FT2M );

	pXdot->Vned_dot = Fned;

	pXdot->Vned_dot[_N] +=
		- ( Vb_n[_N]/((Rlon + h)*clat) )*Vb_n[_E]*sin(pX->llh[0])
		+ ( Vb_n[_N]*Vb_n[_D] )/( Rlat + h );

	pXdot->Vned_dot[_E] +=
		+ ( Vb_n[_E]/((Rlon + h)*clat) )*Vb_n[_N]*sin(pX->llh[0])
		+ ( Vb_n[_E]*Vb_n[_D] )/( Rlon + h ); 

	pXdot->Vned_dot[_D] +=
		- sqr(Vb_n[_E])/(Rlon + h)
		- sqr(Vb_n[_N])/(Rlat + h);

	pXdot->Vned_dot *= C_M2FT;

	// compute the derivative of the quaternion, but first compute the
	// corrected angular rate due to motion over the earth.  The attitude
	// is measured from the TP under the aircraft.  This is moving, and thus
	// a new angular rate needs to be computed to account for the curvature
	// of the world.
	const Vector<3>		win(
		 pXdot->llh_dot[1] * cos(pX->llh[0]),
		-pXdot->llh_dot[0],
		-pXdot->llh_dot[1] * sin(pX->llh[0])
	);

	// compute the derivative of the quaternion
	pXdot->Q_dot = quatW( pX->pqr - cBE * win ) * pX->Q;
}		


/* Why doesn't this call RK4? */
static void
integrate_sixdof(
	sixdofXdot_def *	pXdot,
	sixdofX_def *		pX,
	sixdof_inputs_def *	pU,
	double			dt
)
{
	// to run the derivative function
	sixdofX_def		X;

	// to backup the input state
	sixdofX_def		X0;

	// to run the derivative function
	sixdofXdot_def		Xdot;

	int			i;
	double			k1[13];
	double			k2[13];
	double			k3[13];
	double			k4[13];

	// backing up the current input state
	X0.llh		= pX->llh;
	X0.pqr		= pX->pqr;
	X0.Vned		= pX->Vned;
	X0.Q		= pX->Q;


	// Well, lets get started with the first step
	// first, make the X state value
	X.llh		= X0.llh;
	X.pqr		= X0.pqr;
	X.Vned		= X0.Vned;
	X.Q		= X0.Q.norm();

	// run the function and get the Xdot values
	sixdof_derivs(&Xdot, &X, pU);
	for(i=0; i<3; ++i)
	{
		// save the k step
		k1[i] = dt*Xdot.llh_dot[i];		// latitude, longitude, altitude
		k1[i+3] = dt*Xdot.pqr_dot[i];		// rates
		k1[i+6] = dt*Xdot.Vned_dot[i];		// accelerations
		k1[i+9] = dt*Xdot.Q_dot[i];		// quaternion

		// make the state value for the next step
		X.llh[i] = X0.llh[i] + k1[i]/2.0;
		X.pqr[i] = X0.pqr[i] + k1[i+3]/2.0;
		X.Vned[i] = X0.Vned[i] + k1[i+6]/2.0;
		X.Q[i] = X0.Q[i] + k1[i+9]/2.0;
	}

	k1[12] = dt*Xdot.Q_dot[3];
	X.Q[3] = X0.Q[3] + k1[12]/2.0;
	X.Q.norm_self();

	// save the Xdot values for output
	pXdot->llh_dot	= Xdot.llh_dot;
	pXdot->pqr_dot	= Xdot.pqr_dot;
	pXdot->Vned_dot	= Xdot.Vned_dot;
	pXdot->Q_dot	= Xdot.Q_dot;
	

	// run the function and get the Xdot values
	sixdof_derivs(&Xdot, &X, pU);
	for(i=0; i<3; ++i)
	{
		// save the k step
		k2[i] = dt*Xdot.llh_dot[i];		// latitude, longitude, altitude
		k2[i+3] = dt*Xdot.pqr_dot[i];		// rates
		k2[i+6] = dt*Xdot.Vned_dot[i];		// accelerations
		k2[i+9] = dt*Xdot.Q_dot[i];			// quaternion

		// make the state value for the next step
		X.llh[i] = X0.llh[i] + k2[i]/2.0;
		X.pqr[i] = X0.pqr[i] + k2[i+3]/2.0;
		X.Vned[i] = X0.Vned[i] + k2[i+6]/2.0;
		X.Q[i] = X0.Q[i] + k2[i+9]/2.0;
	}

	k2[12] = dt*Xdot.Q_dot[3];
	X.Q[3] = X0.Q[3] + k2[12]/2.0;
	X.Q.norm_self();

	// run the function and get the Xdot values
	sixdof_derivs(&Xdot, &X, pU);
	for(i=0; i<3; ++i)
	{
		// save the k step
		k3[i] = dt*Xdot.llh_dot[i];			// latitude, longitude, altitude
		k3[i+3] = dt*Xdot.pqr_dot[i];		// rates
		k3[i+6] = dt*Xdot.Vned_dot[i];		// accelerations
		k3[i+9] = dt*Xdot.Q_dot[i];			// quaternion

		// make the state value for the next step
		X.llh[i] = X0.llh[i] + k3[i];
		X.pqr[i] = X0.pqr[i] + k3[i+3];
		X.Vned[i] = X0.Vned[i] + k3[i+6];
		X.Q[i] = X0.Q[i] + k3[i+9];
	}
	k3[12] = dt*Xdot.Q_dot[3];
	X.Q[3] = X0.Q[3] + k3[12];
	X.Q.norm_self();

	// run the function and get the Xdot values
	sixdof_derivs(&Xdot, &X, pU);
	for(i=0; i<3; ++i)
	{
		// save the k step
		k4[i] = dt*Xdot.llh_dot[i];			// latitude, longitude, altitude
		k4[i+3] = dt*Xdot.pqr_dot[i];		// rates
		k4[i+6] = dt*Xdot.Vned_dot[i];		// accelerations
		k4[i+9] = dt*Xdot.Q_dot[i];			// quaternion
	}
	k4[12] = dt*Xdot.Q_dot[3];

	
	// assemble the final result for the state propogation
	for(i=0; i<3; ++i)
	{
		pX->llh[i] = X0.llh[i] + k1[i]/6.0 + k2[i]/3.0 + k3[i]/3.0 + k4[i]/6.0;
		pX->pqr[i] = X0.pqr[i] + k1[i+3]/6.0 + k2[i+3]/3.0 + k3[i+3]/3.0 + k4[i+3]/6.0;
		pX->Vned[i] = X0.Vned[i] + k1[i+6]/6.0 + k2[i+6]/3.0 + k3[i+6]/3.0 + k4[i+6]/6.0;
		pX->Q[i] = X0.Q[i] + k1[i+9]/6.0 + k2[i+9]/3.0 + k3[i+9]/3.0 + k4[i+9]/6.0;
	}
	pX->Q[3] = X0.Q[3] + k1[12]/6.0 + k2[12]/3.0 + k3[12]/3.0 + k4[12]/6.0;


}


void
sixdof(
	sixdof_state_def *	pX,
	sixdof_inputs_def *	pU,
	double			dt
)
{
	// earth TP -> body transformation matrix
	const Matrix<3,3>	cBE( quatDC( pX->Q ) );

	sixdofX_def		X;
	sixdofXdot_def		Xdot;


	// equate the state vectors and such
	X.pqr		= pX->rate;
	X.Vned		= pX->Ve;
	X.llh		= pX->Pllh;
	X.Q		= pX->Q.norm();

	// call integrate_sixdof
	integrate_sixdof(&Xdot, &X, pU, dt);

	// solve for the unknown state values for the next time step
	X.Q.norm_self();

	// body axis angular accelerations
	pX->alpha	= Xdot.pqr_dot;

	// body axis angular rates
	pX->rate	= X.pqr;

	// body axis acceleration
	pX->accel	= pU->F / pU->m;

	// body axis velocity in earth frame
	pX->Ve		= X.Vned;

	// lat, lon, alt position
	pX->Pllh	= X.llh;

	pX->Q		= X.Q;

	// compute the ECEF position
	X.llh[2] *= C_FT2M;

	pX->Pecef	= llh2ECEF( X.llh );
	pX->Pecef	*= C_M2FT;


	// euler angle convertions
	pX->THETA	= quat2euler( X.Q );
	
	// body velocity in body frame
	pX->Vb		= cBE * X.Vned;
}

}
