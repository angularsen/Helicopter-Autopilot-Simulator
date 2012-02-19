/* -*- indent-tabs-mode:T; c-basic-offset:8; tab-width:8; -*- vi: set ts=8:
 * $Id: Heli.h,v 2.0 2002/09/22 02:07:31 tramm Exp $
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


#ifndef _MODEL_H_
#define _MODEL_H_

#include "Forces.h"
#include "Blade.h"
#include "Fin.h"
#include "Servo.h"
#include "Gear.h"
#include "wind_model.h"
#include "FlatEarth.h"

#include <vector>
#include <mat/Frames.h>

/******** HELICOPTER MODEL STRUCTURES ***********/

namespace sim {

/*
 * Main Rotor Parameters
 */
typedef struct
{
	/*
	 *  User specified parameters
	 */

	// MR horizontal fuse station point (from MR hub in)
	double	fs;

	// MR vertical fuse station point (from MR hub in)
	double	wl;

	// MR equivelent hinge offset (ft)
	double	e;

	// MR blade inertial about root (slug-ft^2)
	double	i_b;

	// profile drag cooeficent for a MR blade
	double	cd0;

	// pitch shaft tilt (+ aft lean rad)
	double	is;

	// roll shaft tile (+ right lean rad)
	double	ib;

	// MR radius (ft)
	double	r;

	// MR blade twist (- for washout rad)
	double	twst;

	// blade lift curve slope (Clalpha */rad)
	double	a;

	// # of MR blades
	double	b;

	// chord of MR blade (ft)
	double	c;

	// MR direction of rotation viewed from top (1 = ccw; -1 = cw)
	double	dir;

	// Delta-3 hinge effect
	double	k1;

	// MR root cutout
	double	ro;


	/*
	 *  Dynamics and computed values
	 */

	// used in MR TPP dynamics
	double	a_sum;

	// laterial TPP deflection wrt hub (+ right tilt rad)
	double	b1;

	// flapping stiffness
	double	kc;

	// longitudinal TPP deflection wrt hub (+ back tilt rad)
	double	a1;

	// TPP lateral dihedral effect derivative
	double	db1dv;

	// used in MR TPP dynamics
	double	b_sum;

	// TPP longitudinal dihedral effect derivative
	double	da1du;

	// used in MR TPP dynamics (longitudinal dynamics)
	double	a1dot;

	// used in MR TPP dynamics (lateral dynamics)
	double	b1dot;

	// z-axis velocity relative to rotor plane
	double	wr;

	// z-axis velocity relative to blade
	double	wb;

	// MR angular velocity (rad/s)
	double	omega;

	// MR total thrust (lb)
	double	thrust;

	// MR induced velocity (ft/s)
	double	vi;

	// used in MR vi and thrust integration
	double	vhat2;

	// used in MR vi and thrust integration
	double	vi2;

	// MR vertical distance of hub to aircraft CG (ft)
	double	h;

	// MR horizontal distance of hub to aircraft CG (ft)
	double	d;

	// tip speed (ft/s)
	double	v_tip;

	// total MR power (lb-ft/s)
	double	power;

	// total MR torque (lb-ft)
	double	torque;

	// MR forces on body in body frame (lb)
	Force<Frame::Body>	F;

	// MR rolling moment on body in body frame (lb-ft)
	Moment<Frame::Body>	M;

	// Rolling moment due to b1 TPP tilt derivative (lb-ft/rad)
	double	dl_db1;

	// MR thrust cooeficent
	double	ct;

	// MR cutout ratio
	double	cutout;

	// MR lock number
	double lock;

	// natural frequency due to hinge offset
	double omega_f;

	// cross coupling cooefficent
	double k2;

	// inplane change in flap
	double w_in;

	// off-plane change in flap
	double w_off;

	// TPP time constant (sec)
	double tau;

	// pitching moment due to a1 TPP tile derivative (lb-ft/rad)
	double dm_da1;

	// solidity of rotor disk
	double sigma;
} mainrotor_def;


/*
 * Tail Rotor Parameters
 */
typedef struct
{
	/*
	 *  User defined parameters
	 */
	// TR horizontal fuse station point (from MR hub in)
	double	fs;

	// TR vertical waterline point (from MR hub in)
	double	wl;

	// TR blade lift curve slope (Clalpha */rad)
	double	a;

	// # of TR blades
	double	b;

	// TR blade chord (ft)
	double	c;

	// TR radius (ft)
	double	r;

	// TR cutout radius (ft)
	double	r0;

	// TR blade twist (- washout rad)
	double	twst;

	// TR blade profile drag cooeficent
	double	cd0;

	// factor to correct for duct augmetation
	double	duct;


	/*
	 *  Dynamics and computed values
	 */

	// velocity perpendicular to rotor disk
	double	vr;

	// horizontal distance of hub to aircraft CG (ft)
	double	d;

	// vertical distance of hub to aircraft CG (ft)
	double	h;

	// velocity perpendicular to rotor blade
	double	vb;

	// angular velocity of rotor (rad/s)
	double	omega;

	// TR thrust (lb)
	double	thrust;

	// TR induced velocity
	double	vi;

	// used in thrust integration
	double	vhat2;

	// used in thrust integration
	double	vi2;

	// TR forces on body in body frame (lb)
	Force<Frame::Body>	F;

	// TR rolling moment on body in body frame (lb-ft)
	Moment<Frame::Body>	M;

	// TR power (lb-ft/s)
	double	power;

	// TR revolutions per minute
	double	rpm;

	// normalized TR drag
	double	fr;

} tailrotor_def;


/*
 *  Fly Bar (control rotor) Parameters
 * Based on Tischler and Mettler)
 */
typedef struct
{
	/*
	 *  User defined parameters
	 */
	// flybar time constant (sec)
	double tau;

	// lateral flybar->main rotor gearing ratio
	double Kd;

	// longitudinal flybar->main rotor gearing ratio
	double Kc;


	/*
	 *  Dynamics and computed parameters
	 */
	// lateral flybar TPP tilt (rad)
	double d;

	// lateral flybar TPP tilt rate (rad/s)
	double d_dot;

	// longitudinal flybar TPP tilt (rad)
	double c;

	// longitudinal flybar TPP tilt rate (rad/s)
	double c_dot;

} flybar_def;


/*
 * Control Surfaces for Model
 */
typedef struct
{
	// main rotor collective (+ right yaw rad)
	double mr_col;

	// tail rotor collective (+ climb rad)
	double tr_col;

	// A1 swashplate angle = roll cyclic pitch (rad + right roll)
	double A1;

	// B1 swashplate angle = pitch cyclic pitch (rad + nose down)
	double B1;

	// tail rotor RPM (rpm)
	double tr_rev;

	// main rotor RPM (rpm)
	double mr_rev;

	// tail rotor hobby gyro gain
	double gyro_gain;

	// weight-on-wheel switch (1=on ground, 0=in air)
	int wow;

} control_def;


/********** THE MAIN HELICOPTER STRUCTURE FOR EVERYTHING **********/
// Main Helicopter Parameters
class Heli
{
public:
	Heli()
	{
		this->reset();
	}

	~Heli() {}

	/*
	 * This will perform the initialization of the entire model.
	 * It is also the function to call to reset the model.
	 * For the case of resetting, more than the minimum calculations
	 * are done, but this miminimizes the number of functions to deal with.
	 */
	void
	reset();

	/*
	 * This will perform the entire calculations of everything that needs
	 * to happen to make the math model of the vehicle work.  This is the
	 * function to call to propogate the helicopter model, 6-DOF,
	 * landing gear, servos.
	 *
	 *	U[4] = [mr_coll, A1, B1, tr_coll]
	 */
	void
	step(
		double			model_dt,
		const double		U[4]
	);



	mainrotor_def		m;
	flybar_def		fb;
	tailrotor_def		t;
	Forces			cg;
	control_def		c;

	Blade			main_rotor_blade;
	Blade			tail_rotor_blade;
	

	/*
	 *  Landing gear/contact points modeling parameters
	 */
	std::vector<Gear>	gear;


	/*
	 *  Servos model parameteres
	 * 0=B1 (pitch), 1=A1 (roll), 2=MR Coll, 3=TR Coll.
	 */
	std::vector<Servo>	servos;

	/*
	 *  Aerodynamic contributions from static members
	 * (fuselage, horizontal fin, vertical fin, skids, etc)
	 */
	std::vector<Fin>		fins;


	/*
	 *  IMU model parameters
	 * use flat-earth sixdof model
	 */
	sixdof_fe_inputs_def	sixdofIn;
	sixdof_fe_state_def	sixdofX;

	/*
	 *  Wind modelling parameters
	 */
	wind_inputs_def		wind_params;
	wind_state_def		wind_state;
private:

	/**
	 *  Routines to setup the helicopter's parameters
	 */
	void setup_cg();
	void setup_controls();
	void setup_main_rotor();
	void setup_tail_rotor();
	void setup_flybar();
	void setup_fins();
	void setup_wind();
	void setup_gear();
	void setup_servos();

	void compute_cg();

	void setup_sixdof();


	/**
	 *  Step routines for each of the parameters
	 */
	void do_wind( double dt );
	void do_servos( double dt, const double U[4] );
	void do_gear( double dt );
	void do_forces( double dt, const Force<Frame::Body> &local_gravity );
};


}

#endif

