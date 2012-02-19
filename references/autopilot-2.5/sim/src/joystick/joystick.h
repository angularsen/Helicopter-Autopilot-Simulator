/* -*- indent-tabs-mode:T; c-basic-offset:8; tab-width:8; -*- vi: set ts=8:
 * $Id: joystick.h,v 2.0 2002/09/22 02:07:31 tramm Exp $
 *
 * (c) Trammell Hudson
 *
 * This is a simplistic interface to the Linux joystick devices.
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
#ifndef _JOYSTICK_H_
#define _JOYSTICK_H_

#include <linux/joystick.h>
#include "macros.h"

#define			MAX_AXES		8


typedef struct {
	const char *		name;
	int			min;
	int			max;
	double			last;
} joy_axis_t;


BEGIN_DECLS

extern int
joydev_event(
	int			fd,
	struct js_event *	e,
	int			usec
);


extern int
joydev_open(
	const char *		name
);

END_DECLS

#endif
