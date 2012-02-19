/* -*- indent-tabs-mode:T; c-basic-offset:8; tab-width:8; -*- vi: set ts=8:
 * $Id: servo.h,v 2.0 2002/09/22 02:10:16 tramm Exp $
 *
 * (c) 2002 Trammell Hudson <hudson@swcp.com>
 *************
 *
 *  This file is part of the autopilot onboard code package.
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

#ifndef _SERVO_H_
#define _SERVO_H_

#include <io.h>

#define MAX_SERVOS	8
extern uint16_t		servo_out[ MAX_SERVOS ];


extern void
init_servo( void );

extern void
servo_task( void );

#if 0
static inline void
servo_set(
	uint8_t		servo,
	uint8_t		pos
)
{
	servo_out[ servo % MAX_SERVOS ] = 128 + pos / 2;
}

static inline void
idle( void )
{
	while( !wait_for_pulse )
		;
	wait_for_pulse = 0;
}
#endif

#endif
