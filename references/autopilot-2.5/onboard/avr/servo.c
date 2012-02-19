/* -*- indent-tabs-mode:T; c-basic-offset:8; tab-width:8; -*- vi: set ts=8:
 * $Id: servo.c,v 2.0 2002/09/22 02:10:16 tramm Exp $
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

#include <sig-avr.h>
#include <interrupt.h>
#include <io.h>
#include "servo.h"
#include "timer.h"
#include "string.h"

/*************************************************************************
 *
 *  Servo output code.
 *
 * Servos are output one at a time in a one-shot fashion based on
 * the 16 bit timer compare.  We don't do free-running so that the
 * interrupt overhead is reduced.
 *
 */
#define SERVO_PORT	PORTC
#define SERVO_PORT_DIR	DDRC

uint16_t		servo_out[ MAX_SERVOS ];
volatile uint8_t	servo_busy;


void
init_servo( void )
{
	uint8_t			servo;

	outp( 0xFF, SERVO_PORT_DIR );
	outp( 0x00, SERVO_PORT );

	for( servo = 0 ; servo < MAX_SERVOS ; servo++ )
		servo_out[servo] = 32768;
}


void
servo_task( void )
{
	static uint16_t *	servo			= &servo_out[0];
	static uint8_t		servo_bit		= 0;
	uint8_t			temp_bit;
	uint16_t		pulse_width;
	uint16_t		pulse_time;

	if( servo_busy )
		return;

	/*
	 * Shift to the left to select the next output bit.  If it
	 * overflows, shift in a 1 from the right to simulate a rotate.
	 * The temp_bit variable avoids two unecessary stores and
	 * makes use of the skip instruction for extra speed.
	 */
	temp_bit	= servo_bit << 1;

	if( temp_bit == 0 )
	{
		temp_bit	= 1;
		servo		= &servo_out[0];
	}

	/*
	 * Scale the pulse width to be between 0 and 4096 pulses,
	 * or 0 and 1000 useconds.  Then add 1 msec worth of pulses.
	 */
	servo_busy	= 1;
	servo_bit	= temp_bit;
	pulse_width	= *servo++ / 16 + 4000;
	pulse_time	= time() + pulse_width;

	/* Set the output bit and start the timer */
	outp( temp_bit,		SERVO_PORT );
	__outw( pulse_time,	OCR1AL );
	sbi( TIMSK, OCIE1A );
}


/*
 * The interrupt routine is now simple -- bring down the output pin.
 *
 */
INTERRUPT( SIG_OUTPUT_COMPARE1A )
{
	outp( 0x00, SERVO_PORT );
	cbi( TIMSK, OCIE1A );
	servo_busy = 0;
}


