/* -*- indent-tabs-mode:T; c-basic-offset:8; tab-width:8; -*- vi: set ts=8:
 * $Id: mainloop.c,v 2.14 2003/03/25 17:44:05 tramm Exp $
 *
 * Mainloop for the rev2.x boards.  Integrates the ADC, UART, NMEA and
 * tachometer, as well as lots of other stuff.
 *
 * (c) 2002 Trammell Hudson <hudson@swcp.com>
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
#include <avr/pgmspace.h>

#include "timer.h"
#include "uart.h"
#include "string.h"

#include "servo.h"
#include "ppm.h"
#include "adc.h"
#include "led.h"

/**
 *  These are optional features that control the software UART (for
 * a GPS), the engine tach pulse counter and the LCD output.  We're not
 * using them right now since they are detracting from the main goal
 * of servo driving and flight control
 */
#if 0
#include "soft_uart.h"
#include "tach.h"
#include "lcd.h"
#endif


static uint8_t		manual;
static uint8_t		mode;
static uint16_t		command[8];


/*
 *  Command processing and servo manipulation happens in here
 */
static inline void
user_task( void )
{
	/*
	 * Toggle the manual control flag when ever the
	 * H thingy is flipped.  This is a vital thing
	 * to happen!
	 */
	if( ppm_valid )
	{
		uint16_t width = ppm_pulses[6];

		mode =
			width < 0x2000 ? 0 :
			width < 0x3800 ? 1 :
			2;

		manual = ppm_pulses[4] > 0x3000;
	}


	/* Manual control of throttle with good link! */
	memcpy(
		&servo_widths[1],
		&ppm_pulses[0],
		8 * sizeof(servo_widths[0] )
	);

	servo_hs = ppm_pulses[3];

	/* Manual means all servos go to the receiver */
	if( manual )
		return;

	/* Mode 0 is just roll */
	if( mode == 0 )
	{
		servo_widths[1] = command[0];
	} else

	/* Mode 1 is just pitch */
	if( mode == 1 )
	{
		servo_widths[2] = command[1];
	} else

	/* Mode 2 is both */
	if( mode == 2 )
	{
		servo_widths[1] = command[0];
		servo_widths[2] = command[1];
	}
}


static inline void
input_init( void )
{
	uint8_t			i;

	for( i=0 ; i<8 ; i++ )
		command[i] = servo_make_pulse_width( 32768ul );
}


static inline void
input_task( void )
{
	static uint8_t		phase;
	static uint8_t		servo;
	static uint8_t		high_bits;
	static uint8_t		low_bits;

	uint8_t			i = 4;
	uint8_t			c = '-';

	while( i-- && getc( &c ) )
	{
		switch( phase )
		{
		case 0:
			/* Wait for sync pulse */
			if( c == 0xFF )
				phase = 1;
			break;

		case 1:
			servo = c;
			phase = 2;
			break;

		case 2:
			high_bits = c;
			phase = 3;
			break;

		case 3:
			low_bits = c;

			if( servo > SERVO_MAX )
				continue;

			command[ servo ] = 0
				| ((int)low_bits)  << 0
				| ((int)high_bits) << 8;

			puts( "$SRV" );
			put_uint8_t( servo );
			putc( ',' );
			put_uint16_t( command[ servo ] );
			putnl();

			/* Fall through */

		default:
			phase = 0;
		}
	}
}


int main( void )
{
	led_init();
	timer_init();
	uart_init();
	servo_init();
	ppm_init();
	adc_init();
	input_init();

	sei();

	puts( "$Id: mainloop.c,v 2.14 2003/03/25 17:44:05 tramm Exp $\r\n" );

	while( 1 )
	{
		input_task();
		user_task();

		if( ppm_valid )
		{
			ppm_output();
			ppm_valid = 0;
		}

		/* Every 32768 microseconds */
		if( timer_periodic() == 0 )
			continue;

		adc_output();

	}
}
