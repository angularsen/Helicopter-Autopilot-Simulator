/* -*- indent-tabs-mode:T; c-basic-offset:8; tab-width:8; -*- vi: set ts=8:
 * $Id: Attitude.cpp,v 2.5 2003/03/25 17:39:35 tramm Exp $
 *
 * This is a basic flight control law for the X-Cell helicopter model.
 * It will control the helicopter.  This is the iinner attitude control
 * loop.
 *
 * (c) Aaron Kahn
 * (c) Trammell Hudson
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

#include <cstdio>
#include <cstdlib>
#include <cstring>
#include <cmath>
#include <unistd.h>

#include "state.h"
#include "macros.h"
#include "Attitude.h"

#include <mat/Vector.h>
#include <mat/Vector_Rotate.h>
#include <mat/Quat.h>
#include <mat/Nav.h>
#include <mat/Conversions.h>

namespace libcontroller
{

using namespace libmat;


/*
 * For the Concept 60, these pitch and roll parameters were found
 * to be the best so far.
 * {	 0.1900,  0.0000,  0.0000 },		// Pitch
 * {	 0.1500,  0.0000,  0.0000 },		// Yaw
 */
static double const	gains[][3] = {
	//	 P        D        I
	{	 0.4000,  0.0200,  0.0000 },		// Roll
	{	 0.5000,  0.0200,  0.0000 },		// Pitch
	{	 0.5500,  0.2000,  0.1000 },		// Yaw

};



/*
 *  Our attitude controll just resets it self on startup
 */
Attitude::Attitude(
	const double		dt
) :
	dt( dt )
{
	this->reset();
}


/*
 * This is the attitude controller.  The inputs are the helicopter state,
 * controller information, and the attitude command.  The output is the actuator
 * commands for the roll, pitch, and heading.
 *
 *	U[3] --> [A1 (roll), B1 (pitch), TR_coll] (rad)
 *
 * I do not know why the controller requires the offset on the gyro.
 * It seems to need about 0.18 radians or 10.3 degrees steady state
 * bias for some reason.
 */
const Vector<3>
Attitude::step(
	const Vector<3> &	theta,
	const Vector<3> &	pqr
)
{
	roll.commend(  this->attitude[0], 0.0 );
	pitch.commend( this->attitude[1], 0.0 );
	yaw.commend(   this->attitude[2], 0.0 );

	roll.feedback(  theta[0], pqr[0] );
	pitch.feedback( theta[1], pqr[1] );

	yaw.feedback(
		smallest_angle( theta[2], this->attitude[2] ),
		pqr[2]
	);

	// run the controllers
	return Vector<3>(
	 	 roll.step(),
		-pitch.step(),	// note: negative to account for +B1 = -theta
		 yaw.step()
	);
}



/*
 * This will initialize the controller for the helicopter model.
 * It is assumed that the helicopter is on the ground on startup.
 */
void
Attitude::reset()
{
	// Zero our desired position
	this->attitude = Vector<3>( 0, 0, 0 );

	/******** ATTITUDE CONTROLLER GAINS AND LIMITS **********/
	// roll
	PID roll(
		gains[0][0],
		gains[0][1],
		gains[0][2]
	);

	roll.limit_int(  -0.6*C_DEG2RAD,  0.6*C_DEG2RAD );
	roll.limit_pro(  -8.0*C_DEG2RAD,  8.0*C_DEG2RAD );
	roll.limit_vel( -10.0*C_DEG2RAD, 10.0*C_DEG2RAD );
	roll.limit_out(  -8.0*C_DEG2RAD,  8.0*C_DEG2RAD );


	// pitch
	PID pitch(
		gains[1][0],
		gains[1][1],
		gains[1][2]
	);

	pitch.limit_int(  -0.6*C_DEG2RAD,  0.6*C_DEG2RAD );
	pitch.limit_pro(  -5.0*C_DEG2RAD,  5.0*C_DEG2RAD );
	pitch.limit_vel( -10.0*C_DEG2RAD, 10.0*C_DEG2RAD );
	pitch.limit_out(  -5.0*C_DEG2RAD,  5.0*C_DEG2RAD );

	// yaw
	// will be controlling at display rate
	PID yaw(
		gains[2][0],
		gains[2][1],
		gains[2][2]
	);

	yaw.limit_int(  -90.0*C_DEG2RAD,   90.0*C_DEG2RAD );
	yaw.limit_pro(  -20.0*C_DEG2RAD,   20.0*C_DEG2RAD );
	yaw.limit_vel( -100.0*C_DEG2RAD, 1000.0*C_DEG2RAD );
	yaw.limit_out(  -10.0*C_DEG2RAD,   20.0*C_DEG2RAD );


	// Store these in our object
	this->roll	= roll;
	this->pitch	= pitch;
	this->yaw	= yaw;
}



}

