/* -*- indent-tabs-mode:T; c-basic-offset:8; tab-width:8; -*- vi: set ts=8:
 * $Id: joystick.c,v 2.0 2002/09/22 02:10:18 tramm Exp $
 *
 * (c) Trammell Hudson
 *
 * Sample a Futaba trainer interface on D2, the IRQ0 pin.  The falling
 * edge of the input is used for clocking the input signal.  A pulse
 * width of 8000 usec or greater is required to signal a sync pulse.
 * The values output are simply the number of clock ticks between
 * falling edges.  Divide by the clock rate (in MHz) to get the pulse
 * width is microseconds.
 *
 * Details on the PPM waveform are here:
 *
 *	http://www.mh.ttu.ee/risto/rc/electronics/radio/signal.htm
 *
 * Futaba --> TTL interface details are here:
 *
 *	http://www.heliguy.com/nexus/fmsinterface.html
 *
 *************
 *
 *  This file is part of the autopilot simulation package.
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

#include "avr.h"
#include "uart.h"
#include "timer.h"
#include "string.h"

#define			SYNC_WIDTH		0x4000

static volatile uint16_t last_int;


SIGNAL( SIG_INTERRUPT0 )
{
	last_int = time_nonatomic();
}



int main( void )
{
	init_timer();
	init_uart();

	/*
	 * Configure the port for incoming pulses with falling edge
	 * triggered IRQ's
	 */
	sbi( PORTD, 2 );
	cbi( DDRD, 2 );
	sbi( MCUCR, ISC01 );
	cbi( MCUCR, ISC00 );
	sbi( GIMSK, INT0 );

	sei();
	puts( "$Id: joystick.c,v 2.0 2002/09/22 02:10:18 tramm Exp $\r\n" );

	while( 1 )
	{
		uint16_t start;
		uint16_t width;


		/*
		 *  The UART is running with interrupts disabled, so we
		 * have to manually flush it until there are no bytes
		 * left in the queue.  This means that we're also missing
		 * frames from the transmitter, but it will resend the
		 * values anyway.
		 *
		 * In practice, we miss every other frame.  The wait
		 * for sync pulse loop will see 8 pulses go by before
		 * the next sync.  This corresponds to one PPM frame.
		 */
		while( !uart_send_empty() )
			uart_task();

		/*
		 * Wait for a sync pulse.
		 */
		start = last_int;

		while(1)
		{
			if( start == last_int )
				continue;

	 		width = last_int - start;
			start = last_int;
			if( width > SYNC_WIDTH )
				break;
		}


		/*
		 * Put out a time stamp for the user to know how long
		 * has passed since the last update.
		 */
		put_uint16_t( start );
		putc( ' ' );


		/*
		 *  Print out pulse widths until we receive another
		 * sync pulse.
		 */
		while( 1 )
		{
			if( start == last_int )
				continue;

	 		width = last_int - start;
			start = last_int;

			if( width > SYNC_WIDTH )
				break;

			put_uint16_t( width );
			putc( ' ' );
		}
		
		puts( "\r\n" );
	}

	return 0;
}
