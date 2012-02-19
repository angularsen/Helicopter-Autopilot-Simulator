/* -*- indent-tabs-mode:T; c-basic-offset:8; tab-width:8; -*- vi: set ts=8:
 * $Id: Joystick.h,v 1.5 2003/03/08 05:37:54 tramm Exp $
 *
 * (c) Trammell Hudson
 *
 * Joystick handling routines for the groundstation
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
#ifndef _Joystick_h_
#define _Joystick_h_

#ifdef WIN32
#include <windows.h>
#endif

#define JOY_BUTTON_FLAG(n) (1<<n)

extern int			joy_pitch;
extern int			joy_roll;
extern int			joy_yaw;
extern int			joy_throttle;
extern int			joy_button[8];

#define MAX_AXES 8

/*
typedef struct {
	const char *		name;
	int			min;
	int			max;
	double			last;
} joy_axis_t;
*/

typedef struct {
	double			min_deflection;
	double			max_deflection;
	double			trim;
} trim_t;


extern void
reconnect_joy(
#ifdef WIN32
	UINT		uJoyID = 1
#endif

#ifndef WIN32
	const char *		dev_name = "/dev/js0"
#endif
);


#endif
