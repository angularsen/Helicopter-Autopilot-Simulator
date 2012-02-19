/* -*- indent-tabs-mode:T; c-basic-offset:8; tab-width:8; -*- vi: set ts=8:
 * $Id: viewpoint.cpp,v 2.0 2002/09/22 02:07:30 tramm Exp $
 *
 * (c) Aaron Kahn
 * (c) Trammell Hudson
 * (c) Tim Myrtle
 *
 * This defines the viewpoints available to the 3D model.
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
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with Autopilot; if not, write to the Free Software
 *  Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
 *
 */

#include <cstdio>
#include <cmath>
#include <GL/gl.h>
#include <GL/glu.h>
#include <GL/glut.h>

#include "graphics.h"
#include <mat/Quat.h>
#include <mat/Vector_Rotate.h>
#include <mat/Conversions.h>

using namespace libmat;

void
lookat(
	viewpoint_t		viewpoint,
	double			X,
	double			Y,
	double			Z,
	double			phi,
	double			theta,
	double			psi
)
{
	double			camera[3]	= { 1.00, 6.00, 30.00 };
	double			dest[3]		= {    X,    Y,     Z };
	double			up[3]		= { 0.00, 1.00,  0.00 };

	double			dx	= 10 * cos(psi);
	double			dz	= 10 * sin(psi);

	switch( viewpoint )
	{
	default:
	case view_stationary:
		// Nothing to do. 0 Use the defaults
		break;

	case view_fly_behind:
		// Position the camera just behind and above the boom
		camera[0] = X - dx;
		camera[1] = Y + 1;
		camera[2] = Z - dz;
		break;

	case view_walk_behind:
		// Position the camera at eye height just behind the boom
		camera[0] = X - dx;
		camera[2] = Z - dz;
		break;

	case view_cockpit:
	{
		// this is the view from the cockpit (if you were there)
		// put the camera in the nose if "in_front", otherwise
		// put it back behind the tail boom.
		const bool	in_front	= false;
		const double	pos		= in_front ? 5.0 : -10.0;
		
		camera[0] = X + pos*cos(psi)*cos(theta);
		camera[1] = Y + pos*sin(theta);
		camera[2] = Z + pos*sin(psi);

		// recompute the "up" vector
		up[0] = -sin(phi)*sin(psi);
		up[1] =  cos(phi)*cos(theta);
		up[2] =  sin(phi)*cos(psi);

		// pick a destination out ahead of the vehicle to look at
		dest[0] = X + 6.0*cos(theta)*cos(psi);
		dest[1] = Y + 6.0*sin(theta);
		dest[2] = Z + 6.0*sin(psi);
		break;
	}
		
	case view_north_up:
		// Camera is in the sky
		camera[0] = X;
		camera[1] = 100;
		camera[2] = Z;

		// North is "up"
		up[0] = 1;
		break;

	case view_track_up:
		// Camera is in the sky
		camera[0] = X;
		camera[1] = 100;
		camera[2] = Z;

		// Up is along the boom
		up[0] = dx;
		up[2] = dz;
		break;
	}

	gluLookAt(
		camera[0],	camera[1],	camera[2],
		dest[0],	dest[1],	dest[2],
		up[0],		up[1],		up[2]
	);
}


