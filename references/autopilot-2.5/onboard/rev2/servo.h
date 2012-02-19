/* -*- indent-tabs-mode:T; c-basic-offset:8; tab-width:8; -*- vi: set ts=8:
 * $Id: servo.h,v 2.5 2003/03/22 18:11:03 tramm Exp $
 *
 * (c) 2002 Trammell Hudson <hudson@rotomotion.com>
 *
 * This is the new decade counter based servo driving code.  It uses
 * one 16-bit output compare registers to determine when the regular
 * servo clock line should be toggled, causing the output to move to the
 * next servo.  The other 16-bit output compare is used to drive a
 * JR or Futaba compatible high-speed digital servo.
 *
 * User visibile routines:
 *
 * - servo_init();
 *
 * Call once at the start of the program to bring the servos online
 * and start the external decade counters.  This will also start the
 * high speed servo.
 *
 * - servo_make_pulse_width( length );
 *
 * Converts a position value between 0 and 65536 to actual pulsewidth.  0 is
 * all the way left (1.0 ms pulse) and 65536 is all the way right (2.0 ms
 * pulse). Use it like this:
 *
 * servo_widths[ i ] = servo_make_pulse_width( val )
 *
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

#ifndef _servo_h_
#define _servo_h_

#include <inttypes.h>
#include "timer.h"

#define			SERVO_MAX	10
extern uint16_t		servo_widths[ SERVO_MAX ];
extern uint16_t		servo_hs;

/*
 * To set a servo position, call this routine.  All values from 0
 * to 65536 are valid.  Actual resolution depends on the system clock
 * rate, but between 12 bits at 4 Mhz and 14 bits at 16 Mhz are
 * possible.
 */
static inline uint16_t
servo_make_pulse_width(
	uint16_t		position
)
{
	return 1000ul * CLOCK + position / (64ul / CLOCK);
}

extern void
servo_init( void );

#endif // _servo_h_
