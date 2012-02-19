/* -*- indent-tabs-mode:T; c-basic-offset:8; tab-width:8; -*- vi: set ts=8:
 * $Id: gravity_model.cpp,v 2.0 2002/09/22 02:07:31 tramm Exp $
 *
 * (c) Aaron Kahn
 * (c) Trammell Hudson
 *
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
#include "gravity_model.h"

#include <mat/Nav.h>
#include <mat/Conversions.h>
#include <mat/Constants.h>


namespace sim
{

using namespace libmat;

/*
 * This is a gravity and gravitation model.
 * It is based on the WGS-84 elipsoid model of the Earth.  The source
 * for this model is from Dr. Brian Steven's class.
 *
 * See the structures for information.
 */
void
gravity_model(
	grav_inputs_def *	pIn,
	grav_outputs_def *	pOut
)
{
	double			GM;
	double			f;
	double			e;
	double			C2;
	double			P2;
	double			J2;
	double			we;
	double			Rc;
	double			psi;
	double			N;

	Vector<3>		pLLH(
		pIn->latitude,			// rad
		0.0,				// longitude doesn't matter
		pIn->altitude*C_FT2M		// m MSL + up
	);

	// compute the position in ECEF coordinates
	Vector<3>		pECEF( llh2ECEF( pLLH ) );

	// compute the current distance from you to center of the earth
	// (in meters)
	Rc = sqrt( pECEF );

	// compute the geocentric latitude (radians)
	psi = asin( pECEF[2]/Rc );

	// define some constants
	GM = 3.9860015E14;			//m^3/s^2
	f = (C_WGS84_a - C_WGS84_b)/C_WGS84_a;	// flattening of the earth m
	e = sqrt(2.0*f - sqr(f));		// eccentricity of the earth
	C2 = -0.48416685E-3;
	P2 = sqrt(5.0) / 2.0 * ( 3.0*sqr(sin(psi)) - 1.0 );
	J2 = -sqrt(5.0)*C2;
	we = 2.0*C_PI/(24.0*60.0*60.0);		// rad/sec
	N = C_WGS84_a/sqrt(1.0 - sqr(e)*sqr(sin(pIn->latitude)));	

	// compute the Gravitation vector
	pOut->G[0] = ( GM/sqr(Rc) )
		* -sqr(C_WGS84_a/Rc)
		* 3.0
		* J2
		* sin(psi)
		* cos(psi);

	pOut->G[1] = 0.0;
	pOut->G[2] = ( GM/sqr(Rc) )
		* (1.0 - (3.0/2.0)*sqr(C_WGS84_a/Rc)*J2*(3.0*sqr(sin(psi)) - 1.0));

	// compute the gravity vector
	pOut->g[0] -= sqr(we)
		* (N + pIn->altitude*C_FT2M)
		* cos(pIn->latitude)
		* sin(pIn->latitude);

	pOut->g[1] = 0.0;
	pOut->g[2] -= sqr(we)
		* (N + pIn->altitude*C_FT2M)
		* cos(pIn->latitude)
		* cos(pIn->latitude);


	// convert all from m/s/s to ft/s/s
	pOut->G *= C_M2FT;
	pOut->g *= C_M2FT;
}

}
