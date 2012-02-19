/* -*- indent-tabs-mode:T; c-basic-offset:8; tab-width:8; -*- vi: set ts=8:
 * $Id: flybywire.c,v 2.0 2002/09/22 02:10:18 tramm Exp $
 *
 * (c) Trammell Hudson
 *
 * Samples the gyros and accelerometers.  Produces output on the
 * serial port every 1/10th of a second or so.
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
#include "adc.h"
#include "average.h"
#include "uart.h"
#include "timer.h"
#include "accel.h"
#include "string.h"
#include "servo.h"
#include "periodic.h"


static uint8_t
hexdigit(
	uint8_t			c
)
{
	if( '0' <= c && c <= '9' )
		return c - '0';
	if( 'A' <= c && c <= 'F' )
		return c - 'A' + 0xA;
	if( 'a' <= c && c <= 'f' )
		return c - 'a' + 0xA;
	return 0;
}


static void
process_char(
	uint8_t			c
)
{
	static uint8_t		pos;
	static uint8_t		line[ 8 ];
	const uint8_t		max_bytes	= sizeof(line);
	uint16_t		servo_pos	= 0;
	uint8_t			servo;

	line[++pos % max_bytes] = c;

	if( c != '\n' )
		return;

	servo		= hexdigit( line[(pos-5) % max_bytes] );
	servo_pos	|= hexdigit( line[(pos-4) % max_bytes] );
	servo_pos	<<= 4;
	servo_pos	|= hexdigit( line[(pos-3) % max_bytes] );
	servo_pos	<<= 4;
	servo_pos	|= hexdigit( line[(pos-2) % max_bytes] );
	servo_pos	<<= 4;
	servo_pos	|= hexdigit( line[(pos-1) % max_bytes] );

	if( servo > MAX_SERVOS )
		return;

	servo_out[servo] = servo_pos;
}



static int16_t		n1;


static void
pulse_task( void )
{
	static uint8_t		last;
	uint8_t			cur = inp( PINB );

	/* We only care about the bottom bit */
	cur &= 4;
	if( cur == last )
		return;

	last = cur;
	n1++;
}

static void
init_pulse( void )
{
	outp( 0x00, DDRB );
	outp( 0xFF, PORTB );
}


int main( void )
{
	init_timer();
	init_uart();
	init_accel();
	init_adc();
	init_servo();
	init_periodic();
	init_pulse();

	sei();

	puts( "$Id: flybywire.c,v 2.0 2002/09/22 02:10:18 tramm Exp $\r\n" );

	while( 1 )
	{
		uint8_t c;

		servo_task();
		adc_task();
		accel_task();
		uart_task();
		pulse_task();

		while( getc( &c ) )
			process_char( c );

		if( !periodic )
			continue;

		periodic = 0;

		putc( 'X' ); put_uint16_t( accel_sum[0] );
		putc( 'Y' ); put_uint16_t( accel_sum[1] );
		putc( 'P' ); put_uint16_t( volts_sum[0] );
		putc( 'Q' ); put_uint16_t( volts_sum[1] );
		putc( 'R' ); put_uint16_t( volts_sum[2] );
		putc( 'N' ); put_uint16_t( n1 );

		puts( "\r\n" );
	}

	return 0;
}
