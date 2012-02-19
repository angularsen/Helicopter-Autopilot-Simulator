/* -*- indent-tabs-mode:T; c-basic-offset:8; tab-width:8; -*- vi: set ts=8:
 * $Id: servo.c,v 1.3 2003/03/22 17:28:19 tramm Exp $
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

#include <avr/io.h>
#include <avr/signal.h>
#include "servo.h"
#include "timer.h"

uint16_t		servo_widths[ SERVO_MAX ];
uint16_t		servo_hs;


/*
 * For the 2.4 boards, Servo bank A is the only 4017 bank and
 * it is driven by OCR1A with reset on PORTD7.
 */
#define SERVO_PORT		PORTD
#define SERVO_DDR		DDRD

#define SERVO_A_OCR		OCR1A
#define SERVO_A_ENABLE		OCIE1A
#define SERVO_A_FLAG		OCF1A
#define SERVO_A_FORCE		FOC1A
#define SERVO_A_RESET		7
#define SERVO_A_CLOCK		5
#define SERVO_A_COM0		COM1A0
#define SERVO_A_COM1		COM1A1

#define SERVO_HS_OCR		OCR1B
#define SERVO_HS_ENABLE		OCIE1B
#define SERVO_HS_FLAG		OCF1B
#define SERVO_HS_CLOCK		4
#define SERVO_HS_COM0		COM1B0
#define SERVO_HS_COM1		COM1B1


/*
 * For the High speed servo output, you must select either CONFIG_FUTABA
 * or CONFIG_JR to drive the digital servos.  From experimental tests,
 * the rates are:
 *
 *  Futaba    275 Hz == 3.65 ms frame rate
 *  JR        166 Hz == 6.00 ms frame rate
 */
#define CONFIG_JR

#if defined( CONFIG_FUTABA )
# define HS_SPEED		3650 * CLOCK
#elif defined( CONFIG_JR )
# define HS_SPEED		6000 * CLOCK
#else
# error "servo.c: Neither FUTABA nor JR is defined for high speed output"
#endif



/*
 *  We use the output compare registers to generate our servo pulses.
 * These should be connected to a decade counter that routes the
 * pulses to the appropriate servo.
 *
 * Initialization involves:
 *
 * - Reseting the decade counters
 * - Writing the first pulse width to the counters
 * - Setting output compare to set the clock line by calling servo_enable()
 * - Bringing down the reset lines
 *
 * Ideally, you can use two decade counters to drive 20 servos.
 */
void
servo_init( void )
{
	uint8_t			i;
	uint16_t		mid_point = servo_make_pulse_width( 32768 );

	/* Configure the reset and clock lines */
	SERVO_DDR |= 0
		| ( 1 << SERVO_A_RESET )
		| ( 1 << SERVO_A_CLOCK )
		| ( 1 << SERVO_HS_CLOCK );

	/* Reset the decade counter */
	sbi( SERVO_PORT, SERVO_A_RESET );

	/* Lower the HS servo line */
	cbi( SERVO_PORT, SERVO_HS_CLOCK );

	/* Lower the regular servo line */
	cbi( SERVO_PORT, SERVO_A_CLOCK );

	/* Set all servos (including HS) at their midpoints */
	servo_hs = mid_point;

	for( i=0 ; i < SERVO_MAX ; i++ )
		servo_widths[i] = mid_point;

	/* Set the regular servos to go off some long time from now */
	SERVO_A_OCR	= 32768ul;
	SERVO_HS_OCR	= 65535ul;

	/*
	 * Configure output compare to toggle the output bits.
	 * The high speed servo alternates between the on and
	 * off state, while the regular servo will force a
	 * change.
	 */
	TCCR1A |= 0
		| ( 1 << SERVO_HS_COM0 )
		| ( 0 << SERVO_HS_COM1 )
		| ( 1 << SERVO_A_COM0 )
		| ( 0 << SERVO_A_COM1 );

	/* Clear the interrupt flags in case they are set */
	TIFR = 0
		| ( 1 << SERVO_HS_FLAG )
		| ( 1 << SERVO_A_FLAG );

	/* Unassert the decade counter reset to start it running */
	cbi( SERVO_PORT, SERVO_A_RESET );

	/* Enable our output compare interrupts */
	TIMSK |= 0
		| ( 1 << SERVO_A_ENABLE )
		| ( 1 << SERVO_HS_ENABLE );
}


/*
 * For each servo output, we check to see if the output compare flag
 * has been set.  If it has been set, we compute the next pulse width
 * and reset the output value.
 */
SIGNAL( SIG_OUTPUT_COMPARE1A )
{
	static uint8_t		servo		= 1;
	uint16_t		width;
	uint8_t			tmp = servo;

	if( tmp < SERVO_MAX - 2 )
		goto no_reset;
	sbi( SERVO_PORT, SERVO_A_RESET );
	tmp = -1;

	no_reset:
	servo = ++tmp;
	width = servo_widths[ tmp ];

	if( width < 1000ul * CLOCK )
		width = 1000ul * CLOCK;

	if( tmp == 0 )
		cbi( SERVO_PORT, SERVO_A_RESET );

	SERVO_A_OCR += width;

	/* Force a comparison to toggle the clock pin */
	TCCR1A |= ( 1 << SERVO_A_FORCE );
}


/*
 * High speed servo routine.
 *
 * We want to toggle between on for servo_hs time and off for
 * HS_SPEED (either 275 Hz == 3.65 ms for Futaba or 166 Hz == 6 ms
 * for JR).
 */
SIGNAL( SIG_OUTPUT_COMPARE1B )
{
	static uint8_t state;

	if( state )
	{
		state = 0;
		SERVO_HS_OCR += HS_SPEED - servo_hs;
	} else {
		state = 1;
		SERVO_HS_OCR += servo_hs;
	}
}
