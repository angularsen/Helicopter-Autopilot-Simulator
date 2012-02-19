/* -*- indent-tabs-mode:T; c-basic-offset:8; tab-width:8; -*- vi: set ts=8:
 *  $Id: level.c,v 2.0 2002/09/22 02:10:18 tramm Exp $
 *
 * Watches the accelerometer to find "level", then commands
 * servos to make it happen.
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
#include "pulse.h"

static void
do_something( void )
{
	uart_task();
}

int main( void )
{
	uint16_t x = 0;
	uint16_t y = 0;
	uint8_t i;

	avr_init();
	adc_init( 0xFF );

	/* Retract all the servos */
	servo_set( 0, 128 );

	puts( "Reading level\r\n" );
	msleep( 8129 );

	for( i = 0 ; i < 100 ; i++ )
		accel_task( do_something );

	x = a_y;


	puts( "level x=" );
	puthex( x );
	putnl();

	/* Now work to keep us there */
	setpoint( 0, x );
	tune( 0, 16, -4, 0, 0 );
	i = 0;

	while( 1 )
	{
		int16_t d;
		uint8_t s;

		accel_task( do_something );
		if( ++i % 4 )
			continue;

		sample( 0, a_y );

		d = output( 0 );
		s = out_servos[0];

		if( d == 0 )
		{
			putc( 'L' );
		} else

		if( d < 0 ) {
			putc( '-' );
			if( s > -d )
			{
/*
				out_servos[0] -= -d;
*/
			} else {
				putc( '!' );
/*
				out_servos[0] = 5;
*/
			}
		} else

		{
			putc( '+' );
			if( s + d < 0xFF )
			{
/*
				out_servos[0] += d;
*/
			} else {
				putc( '!' );
/*
				out_servos[0] = 0xFF;
*/
			}
		}

		if( i % 64 )
			continue;


		puts( " l=" ); puthex( x );
		puts( " X=" ); puthex( a_y );
		if( d >= 0 )
		{
			puts( " +" );
			puthex( d );
		} else {
			puts( " -" );
			puthex( -d );
		}

		puts( " s=" ); puthexs( out_servos[0] );
		puts( " v=" ); puthex( volts[0] );
		puts( " v=" ); puthex( volts[1] );
		puts( " v=" ); puthex( volts[2] );
		puts( " v=" ); puthex( volts[3] );
		putnl();
	}
	return 0;
}
