/* -*- indent-tabs-mode:T; c-basic-offset:8; tab-width:8; -*- vi: set ts=8:
 * $Id: pid.c,v 1.3 2002/10/22 18:14:23 tramm Exp $
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

#include "pid.h"
#include "ahrs.h"
#include "string.h"

int16_t		desired_pitch;
int16_t		desired_roll;

static inline int16_t
limit(
	int16_t		val,
	int16_t		low,
	int16_t		high
)
{
	if( val < low )
		return low;
	if( val > high )
		return high;
	return val;
}


/*
 * Are we MSB or LSB in memory?  I don't know...
 */
static inline int16_t
load_word(
	const uint16_t *	addr
)
{
	int16_t			x;
	const char *		p = (const char*) addr;

	x = PRG_RDB( p + 1 );
	x <<= 8;
	x |= PRG_RDB( p );

	return x;
}


int16_t
pid_step(
	const gain_t *		c,
	int16_t			error,
	int16_t			vel
)
{
	int16_t		L;

	L = load_word( &c->err_limit );
	int16_t		err = limit( error, -L, L );

	L = load_word( &c->vel_limit );
	int16_t		vel_err = limit( vel, -L, L );

	L = load_word( &c->P_gain );
	int16_t		result = L * err;

	L = load_word( &c->D_gain );
	result += L * vel_err;

	L = load_word( &c->out_limit );
	return limit( result, -L, L );
}


/*
 * We scale the floating point degrees into ints to speed up the
 * computation.  We can use the gains to produce a reasonable value
 * for the servo output.
 *
 * The DEG2PID constant gives us +/- 90 degrees of range for a 16 bit
 * value.  This is roughly xxx deg/bit.
 */
#define DEG2PID			((int16_t)(3.1415 * 42000.0 / 180.0))

static gain_t			roll_pid = {
	 8 * DEG2PID,		// Proportional max
	10 * DEG2PID,		// Velocity max
	 8 * DEG2PID,		// Output max
	  4,			// P gain
	  2			// D gain
};

static gain_t			pitch_pid = {
	 5 * DEG2PID,		// Proportional max
	10 * DEG2PID,		// Velocity max
	 5 * DEG2PID,		// Output max
	  1,			// P gain
	  0			// D gain
};

#define SERVO_ROLL		1
#define SERVO_PITCH		2

void
pid_run( void )
{
	int16_t			result;
	static uint16_t		command_pitch;
	static uint16_t		command_roll;
	extern uint16_t		servo_widths[];
	extern uint16_t		ppm_pulses[];
	extern int8_t		ppm_valid;

	if( !ppm_valid )
		return;

	if( ppm_pulses[4] >= 0x3000 )
	{
		desired_pitch = 0xFFFF;
		return;
	}

	if( desired_pitch == (int16_t) 0xFFFF )
	{
		desired_roll	= ahrs_theta[0] * DEG2PID;
		desired_pitch	= ahrs_theta[1] * DEG2PID;

		command_roll	= ppm_pulses[0];
		command_pitch	= ppm_pulses[1];
		return;
	}

	puts( "$GPAUT," );

	int16_t		actual_pitch = ahrs_theta[1] * DEG2PID;
	result = pid_step(
		&pitch_pid,
		desired_pitch - actual_pitch,
		-ahrs_pqr[1] * DEG2PID
	);

	put_int16_t( result );
	putc( ',' );
/*
	put_int16_t( desired_pitch );
	putc( ',' );
	put_int16_t( actual_pitch );
*/

	servo_widths[ SERVO_PITCH ] = command_pitch + result;

	result = pid_step(
		&roll_pid,
		desired_roll - (ahrs_theta[0] * DEG2PID),
		-ahrs_pqr[0] * DEG2PID
	);

	put_int16_t( result );
	putnl();

	servo_widths[ SERVO_ROLL ] = command_roll + result;
}

