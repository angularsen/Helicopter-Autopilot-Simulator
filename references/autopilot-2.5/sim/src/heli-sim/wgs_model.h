/* -*- indent-tabs-mode:T; c-basic-offset:8; tab-width:8; -*- vi: set ts=8:
 * $Id: wgs_model.h,v 2.0 2002/09/22 02:07:31 tramm Exp $
 *
 * (c) Aaron Kahn
 * (c) Trammell Hudson
 *
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
#ifndef _WGS_MODEL_H_
#define _WGS_MODEL_H_

#include <mat/Vector.h>
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

	// starting geodetic latitude (rad)
	double			latitude;

	// starting geodetic longitude (rad)
	double			longitude;

	// starting altitude (ft MSL + up)
	double			altitude;

	// starting body translational velocity [u v w] (ft/s)
	Vector<3> uvw;

	// starting body angular rates [P Q R] (rad/s)
	Vector<3> pqr;

	// starting body attitude [phi theta psi] (rad) (level to TP)
	Vector<3> THETA;
} sixdof_init_inputs_def;


typedef struct
{
	// inertia tensor matrix (slug-ft^2)
	Matrix<3,3>		J;

	// inverse of inertia tensor matrix (slug-ft^2)
	Matrix<3,3>		Jinv;

	// vehicle mass (slug)
	double			m;

	// total force on vehicle CG (lbs)
	Vector<3>		F;

	// total moment on vehicle CG (lbs-ft)
	Vector<3>		M;

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
} sixdof_inputs_def;


typedef struct
{
	// acceleration body frame at cg [X Y Z] (ft/s/s)
	Vector<3>		accel;

	// velocity body frame at cg [u v w] (ft/s)
	Vector<3>		Vb;

	// velocity TP frame at cg [Vnorth Veast Vdown] (ft/s)
	Vector<3>		Ve;

	// position ECEF frame at cg [X Y Z] (ft)
	Vector<3>		Pecef;

	// position LLH frame at cg [lat lon alt] (rad)(rad)(ft MSL + up)
	Vector<3>		Pllh;

	// angular acceleration body frame [Pdot Qdot Rdot] (rad/s/s)
	Vector<3>		alpha;

	// angular rates body frame [P Q R] (rad/s)
	Vector<3>		rate;

	// quaternion [q0 q1 q2 q3]
	Quat			Q;

	// attitude euler angles [phi theta psi] (rad) (level to TP)
	Vector<3>		THETA;
} sixdof_state_def;


extern void
sixdof_init(
	sixdof_init_inputs_def *pInit, 
	sixdof_inputs_def *	pIn,
	sixdof_state_def *	pX
);


extern void
sixdof(
	sixdof_state_def *	pX,
	sixdof_inputs_def *	pU,
	double			dt
);

}
#endif
