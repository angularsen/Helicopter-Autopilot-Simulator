/* -*- indent-tabs-mode:T; c-basic-offset:8; tab-width:8; -*- vi: set ts=8:
 * $Id: soft_uart.c,v 2.1 2002/10/20 22:43:47 tramm Exp $
 *
 * (c) 2002 Trammell Hudson <hudson@swcp.com>
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

#include <io.h>
#include <sig-avr.h>
#include "string.h"
#include "soft_uart.h"

static uint8_t		bit_count;
volatile uint8_t	soft_uart_dir; /* 0 == in, 1 == out */
static uint8_t		soft_uart_char;
uint8_t			soft_uart_line[ 65 ];

/*
 *  Values from the app note for the baud rate generator
 */
//#define			N	26ul
#define			N	13ul
#define			C	64ul

/*
 * Incoming data is required to be on the external interrupt.
 * Outgoing data is selectable for what ever pin you desire.
 */
#define			OUT_PORT	PORTC
#define			OUT_DDR		DDRC
#define			OUT_PIN		7



/*
 * 4800 baud software UART using the INT2 external interrupt.
 * Used for decoding NMEA sentances from the GPS.
 */
void
soft_uart_init( void )
{
	/* Input pins, open-collector (internal pullup) */
	cbi( DDRD, 3 );
	cbi( PORTD, 3 );
	cbi( PUD, 3 );

	/* Output pin */
	sbi( OUT_DDR, OUT_PIN );
	sbi( OUT_PORT, OUT_PIN );

	/* Enable falling edge triggered interrupts */
	sbi( MCUCR, ISC11 );
	cbi( MCUCR, ISC10 );

	/* Enable the pulse interrupt, disable timer overflow */
	sbi( GIMSK, INT1 );
	cbi( TIMSK, TOIE0 );

}


SIGNAL( SIG_INTERRUPT1 )
{
	/* Enable our timer to expire in 1.5 bit widths */
	outp( 256ul - N + N/2, TCNT0 );
	outp( 1 << TOV0, TIFR );
	sbi( TIMSK, TOIE0 );

	bit_count	= 0;

	/* Disable the falling edge interrupt */
	cbi( GIMSK, INT1 );
}


SIGNAL( SIG_OVERFLOW0 )
{
	static uint8_t		byte;

	/* Reload our timer for 1 bit width */
	outp( 256ul - N, TCNT0 );

	if( soft_uart_dir == 0 )
	{
		/* Shift in the next bit */
		byte >>= 1;
		if( bit_is_set( PIND, 3 ) )
			byte |= 0x80;
	} else {
		/* Output the next bit */
		if( soft_uart_char & 0x01 )
			sbi( OUT_PORT, OUT_PIN );
		else
			cbi( OUT_PORT, OUT_PIN );
		soft_uart_char >>= 1;
	}

	if( ++bit_count < 9 )
		return;

	/* Reset the interrupts for the next start bit */
	outp( 1 << INTF1, GIFR );
	sbi( GIMSK, INT1 );
	cbi( TIMSK, TOIE0 );

	if( soft_uart_dir == 0 )
	{
		/* Save the character for soft_uart_task() */
		soft_uart_char	= byte;
		byte		= 0;
		bit_count	= 0;
	} else {
		/* Stop bit */
		soft_uart_dir	= 0;
		soft_uart_char	= 0;
		bit_count	= 0;
		sbi( OUT_PORT, OUT_PIN );
	}
}


void
soft_uart_task( void )
{
	static uint8_t		head;
	uint8_t			c;
	const char *		s;

	/* Don't do anything if we are printing a character */
	if( soft_uart_dir )
		return;

	/* Check to see if we just received anything */
	if( !soft_uart_char )
		return;

	/* Store the latest character */
	soft_uart_line[ head++ ] = c = soft_uart_char;
	soft_uart_char = 0;

	putc( c );

#if 0
	/* Check for overflow or full line and print it out */
	if( c != '\n'
	&&  head < sizeof(soft_uart_line) - 1
	)
		return;

	soft_uart_line[head] = '\0';

	/* Can't call puts() since it expects a flash constant */
	s = &soft_uart_line[0];
	while( (c = *s++) )
		putc( c );

	head = 0;
#endif
}


void
soft_uart_putc(
	uint8_t			c
)
{
	/* Disable the falling edge interrupt */
	cbi( GIMSK, INT1 );

	soft_uart_dir	= 1;
	soft_uart_char	= c;
	bit_count	= 0;
	

	/* Set the start bit to expire in one bit width */
	cbi( PORTC, 7 );
	outp( 256u - N, TCNT0 );

	/* Disable the overflow if it has happened */
	outp( 1 << TOV0, TIFR );

	/* Enable the overflow interrupt */
	sbi( TIMSK, TOIE0 );
}

