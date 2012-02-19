/* -*- indent-tabs-mode:T; c-basic-offset:8; tab-width:8; -*- vi: set ts=8:
 * $Id: Guidance.cpp,v 2.0 2002/09/22 02:07:29 tramm Exp $
 *
 * This is a basic flight control law for the X-Cell helicopter model.
 * It will basically navigate and control the helicopter in a local
 * NED frame.  The controller is based on a 4 channel SISO PID control
 * law with an inner attitude control loop and outer guidance control 
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

#include "macros.h"
#include "Guidance.h"

#include <mat/Vector.h>
#include <mat/Vector_Rotate.h>
#include <mat/Quat.h>
#include <mat/Nav.h>
#include <mat/Conversions.h>

namespace libcontroller
{

using namespace libmat;


static double const	gains[][3] = {
	//	 P        D        I
	//{	-0.0080, -0.0100, -0.0005 },		// X
	{	-0.0150, -0.0300, -0.0009 },		// X (new)
	//{	 0.0080,  0.0200,  0.0005 },		// Y
	{	 0.0200,  0.0350,  0.0009 },		// Y (new)
	{	-0.1500, -0.0700, -0.0900 },		// Down
};



/*
 * This is the guidance controller. It is based on a receding horizon 
 * control law.  The reason for the receding horizon controller is to 
 * reduce the top speed of the guidance, and thus the dynamics of the 
 * controller.  The outputs are the actuator commands.
 *
 *	Command[4] -->	[N E D H] commands (ft,ft,ft,rad)
 *
 *	OUTPUTS:
 *	U[4] -->	[roll attitude, pitch attitude, MR_coll] (rad)
 */
const Vector<4>
Guidance::step(
	const Vector<3> &	pos_NED,
	const Vector<3> &	vel_NED,
	const Vector<3> &	theta,
	const Vector<3> &	pqr
)
{
	// Transform the [N E] into body frame
	const Vector<3>		com_XYZ( rotate2( this->position, pqr[2] ) );
	const Vector<3>		pos_XYZ( rotate2( pos_NED, pqr[2] ) );
	const Vector<3>		vel_XYZ( rotate2( vel_NED, pqr[2] ) );

	X.commend( com_XYZ[0], 0.0 );
	Y.commend( com_XYZ[1], 0.0 );
	D.commend( position[2], 0.0 );

	X.feedback( pos_XYZ[0], vel_XYZ[0] );
	Y.feedback( pos_XYZ[1], vel_XYZ[1] );
	D.feedback( pos_NED[2], vel_NED[2] );

	// run the controllers and output roll/pitch/yaw values

	this->attitude.attitude[0] = Y.step();
	this->attitude.attitude[1] = X.step();
	this->attitude.attitude[2] = this->heading;

	Vector<3>		servos( this->attitude.step( theta, pqr ) );

	return Vector<4>(
		D.step(),	// Collective
		servos[0],	// Roll
		servos[1],	// Pitch
		servos[2]	// Yaw
	);
}



/*
 *  Constructor just sets the flags and calls reset
 */
Guidance::Guidance(
	double			dt
) :
	attitude(dt),
	dt(dt)
{
	this->reset();
}



/*
 * This will initialize the controller for the helicopter model.
 * It is assumed that the helicopter is on the ground on startup.
 */
void
Guidance::reset()
{
	// Zero our desired position
	this->position	= Vector<3>( 0, 0, -5 );
	this->heading	= 0;

	/******* GUIDANCE CONTROLLER GAINS AND LIMITS *********/
	// X
	// will be controlling at display rate
	PID X(
		gains[0][0],
		gains[0][1],
		gains[0][2],
		this->dt
	);

	X.limit_int( -20.0,  20.0 );
	X.limit_pro( -10.0,  10.0 );
	X.limit_vel( -10.0,  10.0 );

	// Y
	// will be controlling at display rate
	PID Y(
		gains[1][0],
		gains[1][1],
		gains[1][2],
		this->dt
	);

	Y.limit_int( -100.0,  100.0 );
	Y.limit_pro(  -10.0,   10.0 );
	Y.limit_vel(  -10.0,   10.0 );


	// Down
	// will be controlling at display rate
	PID D(
		gains[2][0],
		gains[2][1],
		gains[2][2],
		this->dt
	);

	D.limit_int(  -2.0,   2.0 );
	D.limit_pro( -10.0,  10.0 );
	D.limit_vel( -10.0,  10.0 );
	D.limit_out( -18.0*C_DEG2RAD, 10.0*C_DEG2RAD );

	this->X = X;
	this->Y = Y;
	this->D = D;
}




/*
 * Return the distance from the state to the desired position.
 * We should be able to do this without the state variable, but
 * just punt for now.
 */
double
Guidance::dist(
	const Vector<3> &	pos_NED,
	const Vector<3> &	theta
)
{
	double			dist2 = (pos_NED - this->position).mag2();
	double			dh = this->heading - smallest_angle(
		theta[2], this->heading
	);

	return sqrt( dist2 + dh*dh );
}


}

