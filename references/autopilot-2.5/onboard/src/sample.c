/* -*- indent-tabs-mode:T; c-basic-offset:8; tab-width:8; -*- vi: set ts=8:
 *  $Id: sample.c,v 2.0 2002/09/22 02:10:18 tramm Exp $
 *
 * Samples the gyros and accelerometers.  Produces output on the
 * serial port every 1/10th of a second.
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
#include "avr.h"
#include "adc.h"
#include "average.h"
#include "uart.h"
#include "timer.h"
#include "accel.h"
#include "string.h"
#include "periodic.h"

int main( void )
{
	int cycle_count = 0;

	init_timer();
	init_uart();
	init_accel();
	init_adc();
	init_periodic();

	sei();

	puts( "FOO\r\n" );

	while( 1 )
	{
		accel_task();

		if( !periodic )
			continue;
		periodic = 0;

		putc( 'X' ); put_uint16_t( accel_sum[0] );
		putc( 'Y' ); put_uint16_t( accel_sum[1] );
		putc( 'P' ); put_uint16_t( volts_sum[0] );
		putc( 'Q' ); put_uint16_t( volts_sum[1] );
		putc( 'R' ); put_uint16_t( volts_sum[2] );
		puts( "N0000" );

		puts( "\r\n" );
	}

	return 0;
}
