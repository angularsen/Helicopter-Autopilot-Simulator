/* -*- indent-tabs-mode:T; c-basic-offset:8; tab-width:8; -*- vi: set ts=8:
 * $Id: sweep.c,v 2.0 2002/09/22 02:10:18 tramm Exp $
 *
 * Sweeps all the servos from 0 to 255 and back.  Outputs the
 * accelerometer data along the way every 1/10th of a second.
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

static void
do_something( void )
{
}


int main( void )
{
	int		cycle_count	= 0;
	uint8_t		servo_pos	= 0;

	avr_init();
	adc_init( 0x70 );

	puts( "$Id: sweep.c,v 2.0 2002/09/22 02:10:18 tramm Exp $" );
	putnl();

	while( 1 )
	{
		accel_task( adc_task );

		if( ++cycle_count % 32 )
			continue;

		puthex( high_bits );
		puthex( time() );
		putc( ':' );

		puts( " Ax=" ); puthex( a_x );
		puts( " Ay=" ); puthex( a_y );
		puts( " pos=" ); puthexs( servo_pos );

		servo_set( 0, servo_pos );
		servo_set( 1, servo_pos );
		servo_set( 2, servo_pos );
		servo_set( 3, servo_pos );
		servo_set( 4, servo_pos );
		servo_set( 5, servo_pos );
		servo_set( 6, servo_pos );
		servo_set( 7, servo_pos );

		servo_pos += 8;

		putnl();
	}

	return 0;
}
