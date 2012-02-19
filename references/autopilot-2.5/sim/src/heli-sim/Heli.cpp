/* -*- indent-tabs-mode:T; c-basic-offset:8; tab-width:8; -*- vi: set ts=8:
 * $Id: Heli.cpp,v 2.1 2003/03/08 05:15:30 tramm Exp $
 *
 * (c) Aaron Kahn
 * (c) Trammell Hudson
 *
 * This is the math model for the XCell model helicopter.  The
 * forces and momement are computed in this model.  The 6-DOF rigid body
 * motion, wind, servos, landing gear, and IMU are all take care of inside 
 * the simulation library.
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
#include <ctime>

#include "macros.h"
#include "Heli.h"
#include "Servo.h"
#include "wind_model.h"
#include "Gear.h"
#include "Fin.h"

#include <mat/rk4.h>
#include <mat/Conversions.h>
#include <mat/Matrix.h>
#include <mat/Vector.h>
#include <mat/Vector_Rotate.h>
#include <mat/Quat.h>
#include <mat/Nav.h>

namespace sim {

using std::vector;
using namespace libmat;
using namespace util;

/*
 * this is for the rotor flapping
 *              .  .  . .
 *	Xdot = [a1 b1 d c]
 *	X = [a1 b1 d c]
 *	A1B1 = [A1 B1]
 *	args = [u v p q db1dv da1du w_in w_off kc dir tau Kc Kd]
 */
static void
RotorFlapDynamics(
	Vector<4> &		Xdot,
	const Vector<4> &	X,
	const double 		UNUSED( t ),
	const Vector<2> &	A1B1,
	const Vector<13> &	args
)
{
	double	u	= args[0];
	double	v	= args[1];
	double	p	= args[2];
	double	q	= args[3];
	double	db1dv	= args[4];
	double	da1du	= args[5];
	double	w_in	= args[6];
	double	w_off	= args[7];
	double	kc	= args[8];
	double	dir	= args[9];
	double	tau	= args[10];
	double	Kc	= args[11];
	double	Kd	= args[12];

	double	a1	= X[0];
	double	b1	= X[1];
	double	d	= X[2];
	double	c	= X[3];

	// flybar mixing (lateral and longitudinal)
	double	A1	= A1B1[0] + Kd*d;
	double	B1	= A1B1[1] + Kc*c;

	double	a_sum	= b1 - A1 + dir*kc*a1 - db1dv*v*0.3;
	double	b_sum	= a1 + B1 - dir*kc*b1 - da1du*u*0.3;
	double	a1dot	= -w_in*b_sum - w_off*a_sum - q;
	double	b1dot	= -w_in*a_sum + w_off*b_sum - p;

	double	d_dot = -d/tau - p + 0.2731*A1B1[0]/tau;
	double	c_dot = -c/tau - q - 0.2587*A1B1[1]/tau;

	Xdot[0] = a1dot;
	Xdot[1] = b1dot;
	Xdot[2] = d_dot;
	Xdot[3] = c_dot;

	// If this is unchecked, the program dies eventually
	if( isnan( Xdot[0] ) )
		abort();

#ifdef CHECK_NAN
	if( Xdot.isnan() )
		abort();
#endif
}


/*
 *  Initializes and runs the wind simulation
 */
void
Heli::setup_wind()
{
	wind_inputs_def *	params	= &this->wind_params;
	wind_state_def *	state	= &this->wind_state;

	params->seed		= time(0);
	params->wind_max	= Velocity<Frame::NED>(
		0,			// N
		0,			// E
		0			// D
	);

	wind_init(
		params,
		state
	);
}




void
Heli::do_wind(
	double			dt
)
{
	wind_inputs_def *	params	= &this->wind_params;
	wind_state_def *	state	= &this->wind_state;

	/* Update the wind state */
	wind_model(
		params,
		state,
		dt
	);

	/* Transform the NED wind velocities into body velocities */
	this->cg.uvw += rotate<Frame::Body>(
		state->Ve,
		this->cg.THETA
	);
}




/*
 * Servo Information (all servos are generic)
 *
 */
void
Heli::setup_servos()
{
	this->servos.clear();

	Servo		pitch( 	 -8.0*C_DEG2RAD,  8.0*C_DEG2RAD );
	Servo		roll(	 -8.0*C_DEG2RAD,  8.0*C_DEG2RAD );
	Servo		coll(	-12.5*C_DEG2RAD, 18.0*C_DEG2RAD );
	Servo		tr(	-20.0*C_DEG2RAD, 20.0*C_DEG2RAD );

	this->servos.push_back( pitch );
	this->servos.push_back( roll );
	this->servos.push_back( coll );
	this->servos.push_back( tr );
}


/*
 * This will perform the time stepping of the servo model.
 * The values of U[7] are as follows...
 *	U[0] = B1 swashplate tilt (+ nose down) (rad)
 *	U[1] = A1 swashplate tilt (+ right roll) (rad)
 *	U[2] = main rotor collective (rad)
 *	U[3] = tail rotor collective (rad)
 *
 *  The svIn[3] and svX[3] are as follows...
 *	sv[0] = B1 (pitch servo)
 *	sv[1] = A1 (roll servo)
 *	sv[2] = main rotor collective
 *	sv[3] = tail rotor collective
 */
void
Heli::do_servos(
	double			dt,
	const double		U[4]
)
{
	control_def *		c = &this->c;

	double *		controls[] = {
		&c->B1,
		&c->A1,
		&c->mr_col,
		&c->tr_col,
	};

	for( int i=0 ; i < 4 ; i++ )
		*controls[i] = this->servos[i].step( dt, U[i] );
}


/*
 *  Configure the landing gear with four points plus the
 * tail skid.
 */
void
Heli::setup_gear()
{
	Forces *		cg		= &this->cg;

	// Remove any that we had last time
	this->gear.clear();


	/*
	 *  Landing skids
	 */
	const double		skid_strength	= 5000; // lb/ft?
	const bool		training_gear	= 0;
	double			length		= 12.0;
	double			width		=  5.6;
	double			offset		=  2.0;
	double			height		= 15.0;

	if( training_gear )
	{
		length		= 30.0;
		width		= 30.0;
	 	offset		=  0.0;
		height		= 20.0;
	}

	this->gear = Gear::skids(
		cg,
		skid_strength,
		length,
		width,
		offset,
		height
	);

	/*
	 *  Skid on the tail is not as strong as the landing skids.
	 * It also has more friction.
	 */
	this->gear.push_back( Gear(
		"tail skid",
		skid_strength / 3.0,
		(-41.5 - cg->fs_cg) / 12.0,
		(  0.0                 ) / 12.0,
		( 15.0 - cg->wl_cg) / 12.0,
		140.0
	));


	/**
	 *  Add contact points for the main rotor.
	 * HTF do you append to a std::vector?
	 */
	const std::vector<Gear>	rotors = Gear::rotor(
		cg,
		this->m.r,
		this->m.fs,
		this->m.wl,
		140.0
	);

	FOR_ALL_CONST( std::vector<Gear>, gear, rotors,
		this->gear.push_back( *gear );
	);
}


/*
 * This will perform the landing gear calculations with the landing gear
 * model provided in the simulation library.
 */
void
Heli::do_gear(
	double			UNUSED( dt )
)
{
	Forces *		cg = &this->cg;

	// body -> earth transformation matrix
	const Matrix<3,3> 	cBE( eulerDC( cg->THETA.v ) );

	// make the cEB matrix
	// earth -> body transformation matrix
	const Matrix<3,3> 	cEB( cBE.transpose() );

	// make the wx matrix (omega-cross matrix)
	const Matrix<3,3>	wx( eulerWx( cg->pqr.v ) );


	FOR_ALL( vector<Gear>, g, this->gear,
		g->step( cg, cBE, cEB, wx );
	);
}


/*
 * This will perform the forces and moments calculations on the aircraft
 * before the calculations of the 6-DOF.  The landing gear calculations are done
 * after this function.  The servo and wind models are run after this as well.
 */
void
Heli::do_forces(
	double			dt,
	const Force<Frame::Body> &	g
)
{
	mainrotor_def *		m	= &this->m;
	flybar_def *		fb	= &this->fb;
	tailrotor_def *		t	= &this->t;
	Forces *		cg	= &this->cg;
	Blade *			mb	= &this->main_rotor_blade;
	Blade *			tb	= &this->tail_rotor_blade;
	control_def *		c	= &this->c;

	// Zero our moments, and compute our local gravitational vector.
	cg->M.fill();
	cg->F = g;

	// compute the current atmospheric conditions
	// density of air (slug/ft^3)
	double			rho	= cg->rho();


	// Main Rotor Calculations
	mb->a		= m->a;
	mb->b		= m->b;
	mb->c		= m->c;
	mb->Cd0		= m->cd0;
	mb->collective	= c->mr_col;
	mb->e		= 0.7;			// oswalds efficency factor
	mb->omega	= c->mr_rev*C_TWOPI/60.0;
	mb->R		= m->r;
	mb->R0		= m->ro;
	mb->rho		= rho;
	mb->twst	= m->twst;
	mb->Vperp	= cg->uvw[0] * (m->is + m->a1)
			- cg->uvw[1] * (m->ib + m->b1)
			- cg->uvw[2];

	mb->step();

	m->thrust	= mb->T;
	m->power	= mb->P;
	m->torque	= mb->Q;
	m->vi		= mb->avg_v1;

	m->F		= Force<Frame::Body>(
		-m->thrust*(m->is + m->a1),
		 m->thrust*(m->ib + m->b1),
		-m->thrust
	);

	m->M		= Moment<Frame::Body>(
		m->F[1] * m->h + m->dl_db1 * m->b1,
		m->F[2] * m->d + m->dm_da1 * m->a1 - m->F[0] * m->h,
		m->torque * m->dir
	);

	// Tail Rotor Calculations

	tb->a		= t->a;
	tb->b		= t->b;
	tb->c		= t->c;
	tb->Cd0		= t->cd0;
	tb->collective	= c->tr_col
			- c->gyro_gain * cg->pqr[2];
	tb->e		= 0.7;			// oswalds efficency factor
	tb->omega	= c->tr_rev*C_TWOPI/60.0;
	tb->R		= t->r;
	tb->R0		= t->r0;
	tb->rho		= rho;
	tb->twst	= t->twst;
	tb->Vperp	= m->dir * (cg->uvw[1] - t->d*cg->pqr[2]);

	tb->step();

	t->thrust	= tb->T + tb->T*t->duct;
	t->power	= tb->P - tb->P*t->duct;

	t->F		= Force<Frame::Body>(
		0.0,
		t->thrust * m->dir,
		0.0
	);

	t->M		= Moment<Frame::Body>(
		 t->F[1] * t->h,
		0.0,
		-t->F[1] * t->d
	);


	/*
	 *  Compute the aerodynamic forces from each of the
	 * fins / fuselage / skids / etc.  This uses the induced
	 * velocity from the main rotor, computed above.
	 */
	FOR_ALL( std::vector<Fin>, fin, this->fins,
		fin->step( cg, m->vi );
	);


	// Main Rotor TPP Dynamics
	// Everything gets wrapped into args for
	// the Runge Kutta routine.
	// for RK4 routine (rotor dynamics)
	Vector<4>		Xdot;
	Vector<4>		X(
		m->a1,
		m->b1,
		fb->d,
		fb->c
	);

	static double old_ma1;
	old_ma1	= m->a1;
	if( isnan( m->a1 ) )
		abort();

	Vector<2>		U;
	U[0]			= c->A1;
	U[1]			= c->B1;

	Vector<13>		args;
	args[ 0]		= cg->uvw[0];
	args[ 1]		= cg->uvw[1];
	args[ 2]		= cg->pqr[0];
	args[ 3]		= cg->pqr[1];
	args[ 4]		= m->db1dv;
	args[ 5]		= m->da1du;
	args[ 6]		= m->w_in;
	args[ 7]		= m->w_off;
	args[ 8]		= m->kc;
	args[ 9]		= m->dir;
	args[10]		= fb->tau;
	args[11]		= fb->Kc;
	args[12]		= fb->Kd;
	
	RK4( X, Xdot, cg->time, U, args, dt, &RotorFlapDynamics );

	// Extract out our new state
	m->a1		= X[0];
	m->b1		= X[1];
	fb->d		= X[2];
	fb->c		= X[3];
	m->a1dot	= Xdot[0];
	m->b1dot	= Xdot[1];
	fb->d_dot	= Xdot[2];
	fb->c_dot	= Xdot[3];

	if( isnan( m->a1 ) )
		abort();

	// Sum Up Total Forces and Moments At CG of main and tail rotors
	cg->F += m->F;
	cg->F += t->F;

	cg->M += m->M;
	cg->M += t->M;
}


/*
 *  Static CG information for the XCell
 */
void
Heli::setup_cg()
{
	Forces *		cg = &this->cg;

	cg->fs_cg	=  0.0;			// in
	cg->wl_cg	= 10.91;		// in
	cg->wt		= 19.5;			// lbs
	cg->ix		=  0.2184;		// slug-ft^2
	cg->iy		=  0.3214;		// slug-ft^2
	cg->iz		=  0.4608;		// slug-ft^2
	cg->ixz		=  0.0337;		// slug-ft^2
	cg->hp_loss	=  0.1;			// HP
	cg->m		= cg->wt / 32.2;	// slugs
	cg->altitude	=  0.0;			// initial DA ft
}
	

/*
 *  Setup the initial control inputs
 */
void
Heli::setup_controls()
{
	control_def *		c = &this->c;

	c->A1		= 0.0;			// roll (rad + right wing down)
	c->B1		= 0.0;			// pitch (rad + nose down)
	c->mr_col	= 2.5*C_DEG2RAD;	// mr col (rad)
	c->tr_col	= 4.5*C_DEG2RAD;	// tr col (rad)
	c->mr_rev	= 1500.0;		// mr RPM
	c->tr_rev	= 4.6 * c->mr_rev;	// tr RPM
	c->gyro_gain	= 0.08;			// Basic rate gyro is 0.08
}


/*
 *  Setup the main rotor parameters
 */
void
Heli::setup_main_rotor()
{
	mainrotor_def *		m	= &this->m;
	Forces *		cg	= &this->cg;

	/* Parameters */
	m->fs		=  0.0;			// in
	m->wl		=  0.0;			// in
	m->is		=  0.0;			// longitudinal shaft tilt (rad)
	m->e		=  0.0225;		// ft
	m->i_b		=  0.0847;		// slug-ft^2
	m->r		=  2.25;		// ft
	m->ro		=  0.6;			// ft
	m->a		=  6.0;			// */rad
	m->cd0		=  0.01;		// nondimentional
	m->b		=  2;			// # of blades
	m->c		=  0.1979;		// ft
	m->twst		=  0.0;			// rad
	m->k1		=  0;			// delta-3 hinge	
	m->dir		= -1.0;			// MR direction of rotation viewed from top (1 = ccw; -1 = cw)
	m->ib		=  0.0;			// laterial shaft tilt (rad)


	/* Dynamics */
	double			rho;
	double			temp;
	double			pres;
	double			sp_sound;


	atmosphere(
		cg->altitude - cg->NED[2],
		&rho,
		&pres,
		&temp,
		&sp_sound
	);

	m->omega	= this->c.mr_rev * C_TWOPI / 60.0;	// rad/s
	m->v_tip	= m->r * this->m.omega;	// ft/s

	// mr lock number
	m->lock		= rho*m->a*m->c*pow(m->r, 4.0) / m->i_b;

	// natural freq shift
	m->omega_f	= ( m->lock*m->omega/16.0 )
		* (1.0 + (8.0/3.0)*(m->e/m->r));

	// cross-couple coef.
	m->k2		= 0.75 * (m->e/m->r) * (m->omega/m->omega_f);

	// total cross-couple coef.
	m->kc		= m->k1 + m->k2;

	// time constant of rotor
	m->tau		= 16.0 / (m->omega * m->lock);

	// off-axis flap
	m->w_off	= m->omega / (1.0 + sqr(m->omega / m->omega_f));

	// on-axis flap
	m->w_in		= m->omega / m->omega_f * m->w_off;

	// moment coef
	m->dl_db1	= 0.75 * m->b * m->c * sqr(m->r) * rho
		* sqr(m->v_tip) * m->a * m->e / ( m->lock*m->r );
	m->dm_da1	= m->dl_db1;

	// thrust coef
	m->ct		= cg->wt / ( rho * C_PI * sqr(m->r) * sqr(m->v_tip) );

	// solidity
	m->sigma	= m->b * m->c / (C_PI * m->r);

	// flap back coef
	m->db1dv	= -( 2.0 / m->v_tip )
		* (8.0 * m->ct / (m->a * m->sigma) + sqrt(m->ct/2.0) );

	// flab back coef
	m->da1du	= -m->db1dv;

	m->vi		= 15.0;
	m->a1		= 0.0;
	m->b1		= 0.0;
	m->a1dot	= 0.0;
	m->b1dot	= 0.0;
	m->thrust	= 0.0;
	m->F.fill();
	m->M.fill();
}


/*
 *  Flybar Information
 *
 * (Tischler and Mettler, System Identification Modeling
 * Of a Model-Scale Helicopter)
 */
void
Heli::setup_flybar()
{
	flybar_def *		fb = &this->fb;

	/* Parameters */
	fb->tau		= 0.36;			// sec
	fb->Kd		= 0.3;
	fb->Kc		= 0.3;

	/* Dyanmics */
	fb->c		= 0.0;
	fb->c_dot	= 0.0;
	fb->d		= 0.0;
	fb->d_dot	= 0.0;
}


void
Heli::setup_fins()
{
	Forces *		cg	= &this->cg;
	std::vector<Fin> &	fins	= this->fins;

	fins.clear();

	// Fuselage
	fins.push_back( Fin( cg,
		  3.0000,		// fs
		 12.0000,		// waterline
		 -0.4240,		// xuu
		 -1.2518,		// yvv
		 -0.8861		// zww
	));

	// Horizontal fin
	fins.push_back( Fin( cg,
		  0.0000,		// fs
		  0.0000,		// waterline
		 -1.0000,		// xuu
		  0.0000,		// yvv
		  0.0000		// zww
	));

	// Vertical fin
	fins.push_back( Fin( cg,
		-41.5000,		// fs
		  7.2500,		// wl
		  0.0000,		// xuu
		 -1.4339,		// yvv
		  0.0000		// zww
	));
}


/*
 * Tailrotor parameters
 */
void
Heli::setup_tail_rotor()
{
	tailrotor_def *		t	= &this->t;

	/* Parameters */
	t->fs		= -41.5;		// in
	t->wl		=   7.25;		// in
	t->r		=   0.5417;		// ft
	t->r0		=   0.083;		// ft
	t->a		=   3.0;		// */rad
	t->b		=   2;			// # of TR blades
	t->c		=   0.099;		// ft
	t->twst		=   0.0;		// rad
	t->cd0		=   0.01;		// nondimentional
	t->duct		=   0.0;		// duct augmetation (duct*thrust; power/duct)


	/* Dyanmics */
	t->omega	= this->c.tr_rev * C_TWOPI / 60.0;
	t->fr		= t->cd0 * t->r * t->b * t->c;
	t->vi		= 10.0;
	t->thrust	= 0.0;
	t->F.fill();
	t->M.fill();
}


/*
 * Compute CG relative positions for the different components
 */
void
Heli::compute_cg()
{
	Forces *		cg = &this->cg;

	// Waterline of the center of gravity
	double			wl = cg->wl_cg;

	// Fuselage station of the center of gravity
	double			fs = cg->fs_cg;


	mainrotor_def *		m = &this->m;
	m->h		= (wl - m->wl) / 12.0;
	m->d		= (fs - m->fs) / 12.0;

	tailrotor_def *		t = &this->t;
	t->h		= (wl - t->wl) / 12.0;
	t->d		= (fs - t->fs) / 12.0;
}


void
Heli::setup_sixdof()
{
	sixdof_fe_init_inputs_def sixinit;
	Forces *		cg = &this->cg;

	sixinit.Ixx	= cg->ix;
	sixinit.Iyy	= cg->iy;
	sixinit.Izz	= cg->iz;
	sixinit.Ixz	= cg->ixz;
	sixinit.m	= cg->m;

	sixinit.THETA.fill();
	sixinit.pqr.fill();
	sixinit.uvw.fill();
	sixinit.NED.fill();

	/* Start in low hover */
	sixinit.NED[2]	= -1.00;

	sixdof_fe_init(
		&sixinit,
		&this->sixdofIn,
		&this->sixdofX
	);
}



/*
 * This will perform the entire calculations of everything that needs
 * to happen to make the math model of the vehicle work.  This is the
 * function to call to propogate the helicopter model, 6-DOF,
 * landing gear, servos, and wind.
 */
void
Heli::step(
	double			model_dt,
	const double		U[4]
)
{
	Forces *		cg = &this->cg;
	const Force<Frame::Body>	g( cg->compute_gravity() );

	if( isnan( cg->F[0] ) )
		abort();

	this->do_forces( model_dt, g );
	if( isnan( cg->F[0] ) )
		abort();

	this->do_gear( model_dt );
	if( isnan( cg->F[0] ) )
		abort();

	this->do_servos( model_dt, U );
	if( isnan( cg->F[0] ) )
		abort();


	this->sixdofIn.F = cg->F;
	this->sixdofIn.M = cg->M;

	sixdof_fe(
		&this->sixdofX,
		&this->sixdofIn,
		model_dt
	);

	cg->NED		= this->sixdofX.NED;
	cg->uvw		= this->sixdofX.Vb;
	cg->V		= this->sixdofX.Ve;
	cg->THETA	= this->sixdofX.THETA;
	cg->pqr		= this->sixdofX.rate;

	/*
	 * Aaron says to do this after sixdof_fe() and memcpy().
	 * It is turned off right now because it costs 10 usec
	 * and we have zero wind all the time now.
	 */
	//this->do_wind( model_dt );

	/* Advance the time clock */
	cg->time += model_dt;

	// Remove the force of gravity to show what an accelerometer
	// would read at the cg.
	cg->F -= g;

	if( isnan( cg->NED[0] ) )
		abort();
}


/*
 * This will perform the initialization of the entire model.
 * It is also the function to call to reset the model.  For the case
 * of resetting, more than the minimum calculations are done, but this
 * miminimizes the number of functions to deal with.
 */
void
Heli::reset()
{
	/*
	 *  CG and controls must be setup before anything else.
	 */
	this->setup_cg();
	this->setup_controls();
	this->setup_main_rotor();
	this->setup_tail_rotor();
	this->setup_flybar();
	this->setup_fins();
	this->setup_wind();
	this->setup_gear();
	this->setup_servos();

	/*
	 * Now that everything is setup, compute the CG for the system
	 */
	this->compute_cg();

	/*
 	 * 6-DOF Initializations must happen after
	 * CG is computed.
	 */
	this->setup_sixdof();

	// CG Initialization
	this->cg.time		= 0.0;
	this->cg.altitude	= 0.0;

	this->cg.NED =   this->sixdofX.NED;
	this->cg.uvw =   this->sixdofX.Vb;
	this->cg.V =     this->sixdofX.Ve;
	this->cg.THETA = this->sixdofX.THETA;
	this->cg.pqr =   this->sixdofX.rate;

	this->cg.F.fill();
	this->cg.M.fill();

	// set/clear the holds
	this->sixdofIn.hold_p	= 0;
	this->sixdofIn.hold_q	= 0;
	this->sixdofIn.hold_r	= 0;
	this->sixdofIn.hold_u	= 0;
	this->sixdofIn.hold_v	= 0;
	this->sixdofIn.hold_w	= 0;
}

}
