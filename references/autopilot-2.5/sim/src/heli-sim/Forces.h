/* -*- indent-tabs-mode:T; c-basic-offset:8; tab-width:8; -*- vi: set ts=8:
 * $Id: Forces.h,v 2.0 2002/09/22 02:07:31 tramm Exp $
 *
 * (c) Aaron Kahn
 * (c) Trammell Hudson
 *
 * The Forces class wraps up all of the forces, moments and actions
 * that are affecting the center of gravity of the aircraft.
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


#ifndef _FORCES_H_
#define _FORCES_H_

#include <mat/Matrix.h>
#include <mat/Frames.h>
#include <mat/Vector.h>
#include <mat/Vector_Rotate.h>

namespace sim {

/*
 * CG Parameters
 */
using namespace libmat;

class Forces
{

public:
	Forces() {}
	~Forces() {}

	/*
	 *  Random accessors
	 */

	// Returns the air density
	double rho();

	// Computes the local gravity for the forces
	const Force<Frame::Body>
	compute_gravity()
	{
		return rotate(
			Force<Frame::NED>( 0, 0, 32.2 * this->m ),
			this->THETA
		);
	}

	/*
	 *  User defined parameters
	 */
	// CG horizontal station point (from MR hub in)
	double	fs_cg;

	// CG vertical waterline point (from MR hub in)
	double	wl_cg;

	// vehicle weight (lb)
	double	wt;

	// Ixx about CG (slug-ft^2)	
	double	ix;

	// Ixz about CG (slug-ft^2)
	double	ixz;

	// Iyy about CG (slug-ft^2)
	double	iy;

	// Izz about CG (slug-ft^2)
	double	iz;

	// HP lost in transmission (HP)
	double	hp_loss;

	// mass of vehicle (slugs)
	double	m;

	// starting density altitude (ft + up)
	double altitude;


	/*
	 *  Computed dynamics
	 */
	// F[X Y Z] forces acting on CG (lb)
	Force<Frame::Body>	F;

	// M[L M N] moments acting about CG (lb-ft)
	Moment<Frame::Body>	M;

	// THETA[phi theta psi] euler body angles (rad)
	Angle<Frame::Body>	THETA;

	// pqr[p q r] body angular rates (rad/s)
	Rate<Frame::Body>	pqr;

	// uvw[u v w] velocity in body axis (ft/s)
	Velocity<Frame::Body>	uvw;

	// V[velN velE velD] velocity in TP NED axis (ft/s)
	Velocity<Frame::NED>	V;

	// NED[North East Down] position (ft)
	Position<Frame::NED>	NED;

	// net power from engine (lb-ft/s)
	double	power;

	// sim time (sec)
	double time;

};

}
#endif

