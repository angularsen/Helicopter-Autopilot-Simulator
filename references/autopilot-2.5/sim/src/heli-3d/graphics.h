/* -*- indent-tabs-mode:T; c-basic-offset:8; tab-width:8; -*- vi: set ts=8:
 * $Id: graphics.h,v 2.1 2003/03/08 05:12:11 tramm Exp $
 *
 * (c) Aaron Kahn
 * (c) Trammell Hudson
 *
 * This defines some values for the graphics of helicopters and
 * draws the scene with the model, the origin and the world.
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


#ifndef _GRAPHICS_H_
#define _GRAPHICS_H_

#define SF 2.25

#include <Fl/Fl_Gl_Window.h>

/*
 * This will draw the 3D helicopter view
 *
 */

class Simview :
	public Fl_Gl_Window
{
public:
	Simview(
		int			X,
		int			Y,
		int			W,
		int			H,
		const char *		L
	);

	void draw();

public:
	void
	initialize_gl();
};


extern void
reconnect_server();


typedef enum {
	view_stationary,
	view_walk_behind,
	view_fly_behind,
	view_cockpit,
	view_north_up,
	view_track_up
} viewpoint_t;

extern viewpoint_t viewpoint;


extern void
DrawScene(
	viewpoint_t		viewpoint,
	double			north,
	double			east,
	double			down,
	double			phi,
	double			theta,
	double			psi,
	double			roll_moment,
	double			pitch_moment
);


extern void
DrawXcellModel(
	double			phi,
	double			roll_moment,
	double			pitch_moment,
	int			shadow = 0
);


extern void
lookat(
	viewpoint_t		viewpoint,
	double			X,
	double			Y,
	double			Z,
	double			phi,
	double			theta,
	double			psi
);


#endif

