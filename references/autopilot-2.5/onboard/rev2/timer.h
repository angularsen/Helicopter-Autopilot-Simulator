/* -*- indent-tabs-mode:T; c-basic-offset:8; tab-width:8; -*- vi: set ts=8:
 * $Id: timer.h,v 2.2 2003/03/22 18:11:30 tramm Exp $
 *
 * The Mega163 has three timers and we're using all of them.  See
 * timer_init() for a description of what they are doing.
 *
 * (c) 2002 Trammell Hudson <hudson@rotomotion.com>
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


#ifndef _timer_h_
#define _timer_h_

#include <inttypes.h>
#include <avr/signal.h>

/*
 * System clock in MHz.  This is used for UART baud rate generation,
 * servo pulse timing, PPM sync frame detection, etc.
 *
 * You probably want 8 MHz for a 2.2 board.  Custom 2.4 boards
 * have 16 MHz clocks, but they are not stable yet.
 */
#define CLOCK		8ul


/*
 * Enable Timer1 (16-bit) running at Clk/1 for the global system
 * clock.  This will be used for computing the servo pulse widths,
 * PPM decoding, etc.
 *
 * Low frequency periodic tasks will be signaled by timer 0
 * running at Clk/1024.  For 4 Mhz clock, this will be every
 * 65536 microseconds, or 15 Hz.
 */
static inline void
timer_init( void )
{
	/* Timer0 @ Clk/64: Software UART */
	TCCR0		= 0x03;

	/* Timer1 @ Clk/1: System clock, ppm and servos */
	TCCR1A		= 0x00;
	TCCR1B		= 0x01;

	/* Timer2 @ Clk/1024: Periodic clock and LCD clock */
	TCCR2		= 0x07;
}


/*
 * Retrieve the current time from the global clock in Timer1,
 * disabling interrupts to avoid stomping on the TEMP register.
 * If interrupts are already off, the non_atomic form can be used.
 */
static inline uint16_t
timer_now( void )
{
	return TCNT1;
}

static inline uint16_t
timer_now_non_atomic( void )
{
	return TCNT1L;
}


/*
 *  Periodic tasks occur when Timer2 overflows.  Check and unset
 * the overflow bit.  We cycle through four possible periodic states,
 * so each state occurs every 30 Hz.
 */
static inline uint8_t
timer_periodic( void )
{
        if( !bit_is_set( TIFR, TOV2 ) )
                return 0;

	TIFR = 1 << TOV2;
        return 1;
}

#endif
