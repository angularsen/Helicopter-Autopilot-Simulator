/* -*- indent-tabs-mode:T; c-basic-offset:8; tab-width:8; -*- vi: set ts=8:
 * $Id: Gear.cpp,v 2.0 2002/09/22 02:07:31 tramm Exp $
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

#include <cstdio>
#include <cmath>
#include <cstdlib>
#include <cstring>

#include "Gear.h"
#include <mat/Matrix.h>
#include <mat/Vector.h>
#include <mat/Vector_Rotate.h>
#include <mat/Quat.h>
#include <mat/Frames.h>

namespace sim
{

using namespace libmat;
using namespace std;

/*
 *  Default friction parameters.
 *
 * ANSI C++98 blows -- WTF can't these be in the class definition?
 */
const double	Gear::default_mu_x	=   0.8;
const double	Gear::default_mu_y	=   0.8;
const double	Gear::default_k		= 120.0;
/*
 *
 * For more information, see the structure definitions below.
 */
void
Gear::step(
	Forces *		cg,
	const Rotate<Frame::Body,Frame::NED> &	cBE,
	const Rotate<Frame::NED,Frame::Body> &	cEB,
	const Matrix<3,3> &	wx
)
{
	// compute the position of wheel in earth frame
	Position<Frame::NED>	Pw_e( cEB * this->cg2point );
	Pw_e[2] += cg->NED[2];

	// compute the displacement of the contact force under ground
	double delta = Pw_e[2];

	// underground?  We don't change the values at all
	if( delta < 0.0 )
		return;

	// make the true position of the wheel (earth frame)
	Pw_e[2] = 0.0;

	// position of wheel in body frame
	// "                                " (body frame)
	const Position<Frame::Body>	Pw_b( cBE * Pw_e );

	// compute the velocity of the wheel (body frame)
	Velocity<Frame::Body>		Vw_b(
		Velocity<Frame::Body>( wx * Pw_b.v ) + cg->uvw
	);

	// "                              " (earth frame)
	Velocity<Frame::NED>		Vw_e( cEB * Vw_b );

	// if the wheel has steering ability
#if 0
	if( this->rotation != 0.0 )
	{
		// rotate the body velocity by the rotation of the wheel
		// (+ rotation about Z body axis)
		 tempV1( rotate2( cg->uvw, this->rotation ) );

		// recompute the velocity of wheel in body frame
		Vw_b = wx * Pw_b + tempV1;

		// "                               " earth frame
		Vw_e = cEB * Vw_b;
	}
#endif

	// compute the magnitude of the vertical force on the wheels
	// (earth frame)
	Force<Frame::NED> Fw_e;

	Fw_e[2] = this->k*delta + this->b*Vw_e[2];

	// compute the lateral forces on the wheel in earth frame
	// X
	if( Vw_e[0] != 0.0 )
		Fw_e[0] = -this->mu_x*Fw_e[2]*Vw_e[0]/fabs(Vw_e[0]);
	else
		Fw_e[0] = 0.0;

	// Y
	if( Vw_e[1] != 0.0 )
		Fw_e[1] = -this->mu_y*Fw_e[2]*Vw_e[1]/fabs(Vw_e[1]);
	else
		Fw_e[1] = 0.0;

	// take care of the sign of the vertical force on the wheel in earth frame
	Fw_e[2] *= -1.0;

	// compute the wheel force in body frame for output
	const Force<Frame::Body> F( cBE * Fw_e );

	// compute the moment in earth frame
	// compute the moment in body frame for output
	const Moment<Frame::NED> r( cross( Fw_e.v, Pw_e.v * -1.0 ) );
	const Moment<Frame::Body> M( cBE * r );

	// Add our force and moment to the CG
	cg->F += F;
	cg->M += M;

	if( this->max_force < 0 )
		return;

	// Check for maximum force exceeded
	const double magnitude = F.mag2();
	if( magnitude > this->max_force )
		cerr
			<< this->name << ": "
			<< "Force=" << magnitude
			<< " exceeds max " <<  this->max_force
			<< endl;
}


/**
 * Helper function to produce four points around the CG
 * that act as "skids" or landing gear.  Returns a
 * vector of gear that can be inserted into the model's
 * list of contact points.
 */
const std::vector<Gear>
Gear::skids(
	const Forces *		cg,
	double			skid_strength,
	double			length,
	double			width,
	double			offset,
	double			height,
	double			k,
	double			mu_x,
	double			mu_y
)
{
	std::vector<Gear>	skids;

	const double		front		= offset + length / 2.0;
	const double		back		= offset - length / 2.0;
	const double		side		= width / 2.0;

	const double		skid_front	= (front  - cg->fs_cg) / 12.0;
	const double		skid_back	= (back   - cg->fs_cg) / 12.0;
	const double		skid_side	= (side              ) / 12.0;
	const double		skid_height	= (height            ) / 12.0;

	skids.push_back( Gear(
		"right front",
		skid_strength,
		skid_front,
		skid_side,
		skid_height,
		k,
		mu_x,
		mu_y
	));

	skids.push_back( Gear(
		"right back",
		skid_strength,
		skid_back,
		skid_side,
		skid_height,
		k,
		mu_x,
		mu_y
	));

	skids.push_back( Gear(
		"left front",
		skid_strength,
		skid_front,
		-skid_side,
		skid_height,
		k,
		mu_x,
		mu_y
	));

	skids.push_back( Gear(
		"left back",
		skid_strength,
		skid_back,
		-skid_side,
		skid_height,
		k,
		mu_x,
		mu_y
	));

	return skids;
}


/*
 *  Rotor disc is very fragile.  Touching it at all causes
 * it to "break".
 */
const std::vector<Gear>
Gear::rotor(
	const Forces *		cg,
	double			radius,
	double			fs,
	double			wl,
	double			k,
	double			mu_x,
	double			mu_y
)
{
	std::vector<Gear>	rotor;

	rotor.push_back( Gear(
		"rotor front",
		0.1,
		fs - cg->fs_cg + radius,
		0.0,
		wl - cg->wl_cg,
		k,
		mu_x,
		mu_y
	));

	rotor.push_back( Gear(
		"rotor left",
		0.1,
		fs - cg->fs_cg,
		-radius,
		wl - cg->wl_cg,
		k,
		mu_x,
		mu_y
	));

	rotor.push_back( Gear(
		"rotor right",
		0.1,
		fs - cg->fs_cg,
		radius,
		wl - cg->wl_cg,
		k,
		mu_x,
		mu_y
	));

	return rotor;
}

}
