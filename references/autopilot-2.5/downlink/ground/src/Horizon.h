/* -*- indent-tabs-mode:T; c-basic-offset:8; tab-width:8; -*- vi: set ts=8:
 * $Id: Horizon.h,v 1.2 2002/10/12 18:23:54 tramm Exp $
 *
 * (c) Aaron Kahn
 * (c) Trammell Hudson
 *
 * This is used to draw instruments that would be used in flight operations.
 * These are all based on the GLUT library and the MATLIB library.
 *
 *************
 *
 *  This file is part of the autopilot groundstation package.
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

#ifndef _INSTRUMENTS_H_
#define _INSTRUMENTS_H_

#include <Fl/Fl_Gl_Window.h>

/*
 * This will draw an artificial horzion for attitude and heading
 * indication for a pilot.  The inputs are pitch, roll, and yaw of the
 * vehicle.  Also, the altitude and speed for the side bars.  In addition
 * to only displaying numbers, a three character message can be displayed
 * as well.
 *
 *  The ranges are data are...
 *	Pitch		-90 to 90 degrees
 *	Roll		-180 to 180 degrees
 *	Yaw		-180 to 180 degrees
 *	Altitude	NO RANGE (ft)
 *	Speed		NO RANGE (knots)
 *	Message	3 characters
 */

class Horizon :
	public Fl_Gl_Window
{
public:
	Horizon(
		int			X,
		int			Y,
		int			W,
		int			H,
		const char *		L
	);

	void draw();

public:
	double			pitch;
	double			roll;
	double			yaw;
	double			altitude;
	double			speed;
	const char *		message;

	void
	initialize_gl();
};

#endif

