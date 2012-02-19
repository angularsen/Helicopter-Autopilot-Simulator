/* -*- indent-tabs-mode:T; c-basic-offset:8; tab-width:8; -*- vi: set ts=8:
 * $Id: voltmeter.c,v 2.0 2002/09/22 02:10:18 tramm Exp $
 *
 * Samples all the voltage inputs, printing them every 1/8th of a second.
 *
 ****************
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
#include "timer.h"
#include "uart.h"
#include "string.h"
#include "adc.h"
#include "average.h"


int main( void )
{
	init_timer();
	init_uart();
	init_adc();

	/* Turn on interrupts */
	sei();

	while(1)
	{
		int i;
		put_uint16_t( time() );

		for( i=0 ; i < 8 ; i++ )
		{
			putc( ' ' );
			put_uint16_t( volts_sum[i] );
		}

		puts( "\r\n" );

		msleep( 1024 );
	}

	return 0;
}

