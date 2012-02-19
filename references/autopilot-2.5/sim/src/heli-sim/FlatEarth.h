/* -*- indent-tabs-mode:T; c-basic-offset:8; tab-width:8; -*- vi: set ts=8:
 * $Id: FlatEarth.h,v 2.0 2002/09/22 02:07:31 tramm Exp $
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

#ifndef _FE_MODEL_H_
#define _FE_MODEL_H_

#include <mat/Frames.h>
#include <mat/Matrix.h>

namespace sim
{

using namespace libmat;

typedef struct
{
	// moment of inertia about body X axis (slug-ft^2)
	double			Ixx;

	// moment of inertia about body Y axis (slug-ft^2)
	double			Iyy;

	// moment of inertia about body Z axis (slug-ft^2)
	double			Izz;

	// cross moment of inertia about body XZ axis (slug-ft^2)
	double			Ixz;

	// mass of vehicle (slug)
	double			m;

	// starting body NED [north east down] position (ft)
	Position<Frame::NED>	NED;

	// starting body translational velocity [u v w] (ft/s)
	Velocity<Frame::Body>	uvw;

	// starting body angular rates [P Q R] (rad/s)
	Rate<Frame::Body>	pqr;

	// starting body attitude [phi theta psi] (rad) (level to TP)
	Angle<Frame::Body>	THETA;

} sixdof_fe_init_inputs_def;


typedef struct
{
	// inertia tensor matrix (slug-ft^2)
	Matrix<3,3> J;

	// inverse of inertia tensor matrix (slug-ft^2)
	Matrix<3,3> Jinv;

	// vehicle mass (slug)
	double			m;

	// total force on vehicle CG (lbs)
	Force<Frame::Body>	F;

	// total moment on vehicle CG (lbs-ft)
	Moment<Frame::Body>	M;

	// hold the X body velocity constant (1=hold, 0=free)
	double			hold_u;

	// hold the Y body velocity constant (1=hold, 0=free)
	double			hold_v;

	// hold the Z body velocity constant (1=hold, 0=free)
	double			hold_w;

	// hold the X body angular rate constant (1=hold, 0=free)
	double			hold_p;

	// hold the Y body angular rate constant (1=hold, 0=free)
	double			hold_q;

	// hold the Z body angular rate constant (1=hold, 0=free)
	double			hold_r;
} sixdof_fe_inputs_def;


typedef struct
{
	// acceleration body frame at cg [X Y Z] (ft/s/s)
	Accel<Frame::Body>	accel;

	// velocity body frame at cg [u v w] (ft/s)
	Velocity<Frame::Body>	Vb;

	// velocity TP frame at cg [Vnorth Veast Vdown] (ft/s)
	Velocity<Frame::NED>	Ve;

	// position NED [north east down] (ft)
	Position<Frame::NED>	NED;

	// angular acceleration body frame [Pdot Qdot Rdot] (rad/s/s)
	RateDot<Frame::Body>	alpha;

	// angular rates body frame [P Q R] (rad/s)
	Rate<Frame::Body>	rate;

	// quaternion [q0 q1 q2 q3]
	Quat			Q;

	// attitude euler angles [phi theta psi] (rad) (level to TP)
	Angle<Frame::Body>	THETA;
} sixdof_fe_state_def;



extern void
sixdof_fe_init(
	sixdof_fe_init_inputs_def *pInit, 
	sixdof_fe_inputs_def *	pIn,
	sixdof_fe_state_def *	pX
);


extern void
sixdof_fe(
	sixdof_fe_state_def *	pX,
	sixdof_fe_inputs_def *	pU,
	double			dt
);


}
#endif
