/* -*- indent-tabs-mode:T; c-basic-offset:8; tab-width:8; -*- vi: set ts=8:
 * $Id: Gear.h,v 2.0 2002/09/22 02:07:31 tramm Exp $
 *
 * (c) Aaron Kahn
 * (c) Trammell Hudson
 *
 * This is a collision/landing gear model.
 *
 * The basis of this model is the vertial displacement method.
 * The idea is that if a collision is detected between the point,
 * and a surface, then the displacement is computed.  The is then 
 * used to compute a force based on Hook's law.
 *
 * To allow for in-elastic collisions, a damping constant can be given.
 *
 * For now, this function assumes that the contact surface is the ground
 * (for landing gear).  For this assumption, the ground is taken as 0
 * altitude.
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
#ifndef _GEAR_MODEL_H_
#define _GEAR_MODEL_H_

#include "Forces.h"
#include <mat/Matrix.h>
#include <vector>

namespace sim
{
using namespace libmat;

class Gear
{
public:
	static const double		default_mu_x;	//	=   0.8;
	static const double		default_mu_y;	//	=   0.8;
	static const double		default_k;	//	= 120.0;

	Gear(
		const char *		name,
		double			max_force,
		double 			cg_x,
		double 			cg_y,
		double 			cg_z,
		double			k		= default_k,
		double			mu_x		= default_mu_x,
		double			mu_y		= default_mu_y,
		double			rotation	= 0
	) :
		name(name),
		max_force(max_force),
		k(k),
		b(b),
		mu_x(mu_x),
		mu_y(mu_y),
		rotation(rotation)
	{
		this->b			= sqrt( 2.0 * k );
		this->cg2point[0]	= cg_x;
		this->cg2point[1]	= cg_y;
		this->cg2point[2]	= cg_z;
	}

	~Gear() {}

	static const std::vector<Gear>
	skids(
		const Forces *		cg,
		double			skid_strength,
		double			skid_length,
		double			skid_width,
		double			skid_offset,
		double			skid_height,
		double			k		= default_k,
		double			mu_x		= default_mu_x,
		double			mu_y		= default_mu_y
	);

	static const std::vector<Gear>
	rotor(
		const Forces *		cg,
		double			radius,
		double			fs,
		double			wl,
		double			k		= default_k,
		double			mu_x		= default_mu_x,
		double			mu_y		= default_mu_y
	);

	/*
	 *  Compute the forces and moments
	 */
	void step(
		Forces *		cg,
		const Rotate<Frame::Body,Frame::NED> &	cEB,
		const Rotate<Frame::NED,Frame::Body> &	cBE,
		const Matrix<3,3> &	wx
	);


private:
	// the vector from vehicle CG -> contact pt, body frame [X Y Z] (ft)
	Position<Frame::Body>	cg2point;

	const char *		name;
	double			max_force;

	// Hook's law spring constant (lbs/ft)
	double			k;

	// damping constant (lbs/ft/s)
	double			b;

	// cooefficent of friction on long axis
	double			mu_x;

	// cooefficent of friction on cross axis
	double			mu_y;

	// relative rotation about vertical axis (for steering) (rad)
	double			rotation;
};


}
#endif
