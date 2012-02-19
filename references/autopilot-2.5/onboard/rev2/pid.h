/* -*- indent-tabs-mode:T; c-basic-offset:8; tab-width:8; -*- vi: set ts=8:
 * $Id: pid.h,v 1.2 2002/10/22 18:14:09 tramm Exp $
 *
 * PID (Proportional/Integral/Derivative) controller routines.
 * Optimized for smaller microcontrollers with limited memory
 * and no floating point units.  Currently no I terms are supported.
 *
 * The gain constants are stored in flash memory so that they don't occupy
 * any of our valuable SRAM.
 *
 * (c) 2002 Trammell Hudson <hudson@rotomotion.com>
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

#ifndef _avr_pid_h_
#define _avr_pid_h_


#include <inttypes.h>
#include "memory.h"


/*
 * Gains and limits are stored in flash rather than SRAM.
 * Keep this in mind when you access the values.
 */
typedef struct
{
	int16_t		err_limit;
	int16_t		vel_limit;
	int16_t		out_limit;
	int16_t		P_gain;
	int16_t		D_gain;
} _gain_t;

typedef _gain_t 	gain_t PROGMEM;


extern int16_t
pid_step(
	const gain_t *		controller,
	int16_t			error,
	int16_t			vel
);

extern void
pid_run( void );


/*
 * These are set when the autopilot is engaged.  An outside
 * routine can modify them if necessary.
 */
extern int16_t		desired_pitch;
extern int16_t		desired_roll;


#endif
