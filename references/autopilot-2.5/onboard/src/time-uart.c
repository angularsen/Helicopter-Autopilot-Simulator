/* -*- indent-tabs-mode:T; c-basic-offset:8; tab-width:8; -*- vi: set ts=8:
 * $Id: time-uart.c,v 2.0 2002/09/22 02:10:18 tramm Exp $
 *
 * Times the UART in polled mode
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

#include <io.h>
#include <interrupt.h>
#include <sig-avr.h>
#include "string.h"
#include "timer.h"
#include "uart.h"


/**
 *  Do the timing tests
 */
int main( void )
{
	uint32_t last;

	time_init();
	uart_init();
	
	while( 1 )
	{
		uint32_t diff;
		uint32_t now = high_bits;

		now <<= 16;
		now |= time();

		diff = now - last;

		puthexl( now );
		putc( ' ' );
		puthexl( diff );
		puts( " = " );
		puthexl( diff / 82 );

		/* Pad out to 80 characters.  82 counts the newline */
		puts( " 01234567890123456789012345678901234567890123456789-\r\n" );

		last = now;
	}
}
