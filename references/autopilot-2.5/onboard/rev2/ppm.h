/* -*- indent-tabs-mode:T; c-basic-offset:8; tab-width:8; -*- vi: set ts=8:
 * $Id: ppm.h,v 2.4 2003/03/22 18:10:49 tramm Exp $
 *
 * Decoder for the trainer ports or hacked receivers for both
 * Futaba and JR formats.  The ppm_valid flag is set whenever
 * a valid frame is received.
 *
 * Pulse widths are stored as unscaled 16-bit values in ppm_pulses[].
 * If you require actual microsecond values, divide by CLOCK.
 * For an 8 Mhz clock and typical servo values, these will range
 * from 0x1F00 to 0x4000.
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

#ifndef _ppm_h_
#define _ppm_h_

/**
 *  Receiver types are:
 *    1  Futaba
 *    2  JR
 */
#define PPM_RX_TYPE	2


#include <string.h>		/* For memset */
#include "string.h"		/* For puts, etc */
#include "uart.h"


/*
 * PPM pulses are falling edge clocked on the ICP, which records
 * the state of the global clock.  We do not use any noise
 * canceling features.
 *
 * JR might be rising edge clocked; set that as an option
 */
static inline void
ppm_init( void )
{
#if   PPM_RX_TYPE == 1
	cbi( TCCR1B, ICES1 );
#elif PPM_RX_TYPE == 2
	sbi( TCCR1B, ICES1 );
#else
#	error "ppm.h: Unknown receiver type in PPM_RX_TYPE"
#endif

	/* No noise cancelation */
	cbi( TCCR1B, ICNC1 );

	/* Set ICP to input, internal pull up */
	sbi( PORTD, 6 );
	cbi( DDRD, 6 );

	/* Enable interrupt on input capture */
	sbi( TIMSK, TICIE1 );
}


/*
 *  The pulse widths are recorded into this array.  If the values
 * are not zeroed by the users, they will never be reset.
 */
#define			PPM_MAX_PULSES	8
uint16_t		ppm_pulses[ PPM_MAX_PULSES ];
uint8_t			ppm_valid;


/*
 * Pulse width is computed as the difference between now and the
 * previous pulse.  If no pulse has been received between then and
 * now, the time of the last pulse will be equal to the last pulse
 * we measured.  Unfortunately, the Input Capture Flag (ICF1) will
 * not be set since the interrupt routine disables it.
 * 
 * Sync pulses are timed with Timer2, which runs at Clk/1024.  This
 * is slow enough at both 4 and 8 Mhz to measure the lengthy (10ms
 * or longer) pulse.
 *
 * Otherwise, compute the pulse width with the 16-bit timer1,
 * push the pulse width onto the stack and increment the
 * pulse counter until we have received eight pulses.
 */
/*
static inline void
ppm_task( void )
*/
SIGNAL( SIG_INPUT_CAPTURE1 )
{
	static uint16_t		last;
	uint16_t		this;
	uint16_t		width;
	static uint8_t		state;
	static uint8_t		sync_start; // Also sync width

	this		= ICR1;
	width		= this - last;
	last		= this;

	if( state == 0 )
	{
		uint8_t			end	= inp( TCNT2 );
		uint8_t			diff	= end - sync_start;
		sync_start = end;

		/*
		 * Waiting for a sync pulse.  10000 us * CLOCK / 1024.
		 * If we use Clk/256 then it overflows too quickly.
		 */
		if( diff > 0x20 )
		{
			state = 1;
			sync_start = diff;
		}

		return;
	}

	if( state < 9 )
	{
		/* Read a data pulses */
		ppm_pulses[ state++ - 1 ] = width;
		return;
	}

	/* All done.  Mark it as valid if the sync pulse is valid */
	if( 0x30 < sync_start && sync_start < 0x60 )
	{
		extern uint16_t servo_widths[];
		uint8_t		start	= 0;

		ppm_valid	= 1;

		if( ppm_pulses[4] < 0x3000 )
			start	= 2;

		for( ; start < 8 ; start++ )
			servo_widths[start+1] = ppm_pulses[start];
	}

	state		= 0;
	sync_start	= TCNT2;
}


static inline void
ppm_output( void )
{
	uint8_t			i;

	puts( "$GPPPM" );

	if( !ppm_valid )
	{
		puts( ",,,,,,,,\r\n" );
		return;
	}

	for( i=0 ; i < PPM_MAX_PULSES ; i++ )
	{
		putc( ',' );
		put_uint16_t( ppm_pulses[i] );
	}

	putnl();
}


#endif
