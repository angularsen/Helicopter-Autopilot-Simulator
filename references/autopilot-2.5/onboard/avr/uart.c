/* -*- indent-tabs-mode:T; c-basic-offset:8; tab-width:8; -*- vi: set ts=8:
 * $Id: uart.c,v 2.0 2002/09/22 02:10:16 tramm Exp $
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

#include <sig-avr.h>
#include <interrupt.h>
#include <io.h>



/*************************************************************************
 *
 *  UART code.
 *
 * We can poll or interrupt for both transmit and receive.
 * UART_IRQ_TX and UART_IRQ_RX are the defines.  The default
 * is to poll for transmissions and interrupt for incoming data.
 */
#undef UART_IRQ_RX
#undef UART_IRQ_TX

#define TX_BUF_SIZE		64
#define RX_BUF_SIZE		64
#define TX_BUF_MASK		( TX_BUF_SIZE - 1 )
#define RX_BUF_MASK		( RX_BUF_SIZE - 1 )

uint8_t				tx_head;
volatile uint8_t		tx_tail;
static uint8_t			tx_buf[ TX_BUF_SIZE ];

static volatile uint8_t		rx_head;
static uint8_t			rx_tail;
static uint8_t			rx_buf[ RX_BUF_SIZE ];


/*
 * UART Baud rate generation settings:
 *
 * With 4.0 Mhz clock, UBRR=12 => 19200 baud
 * With default oscillator and OSCCAL=0xFF, UBRR=48 ==> 2400 baud.
 * With default oscillator and OSCCAL=0xFF, UBRR=23 ==> 4800 baud.
 * With default oscillator and OSCCAL=0xFF, UBRR=11 ==> 9600 baud.
 * With default oscillator and OSCCAL=0xFF, UBRR= 5 ==> 19200 baud.
 */
void
init_uart( void )
{
	/* Baudrate is 19.2 for a 4 Mhz clock */
	outp( 12, UBRR );

	/* Enable the UART for sending and receiving */
	sbi( UCSRB, RXEN );
	sbi( UCSRB, TXEN );

	tx_head = tx_tail = 0;
	rx_head = rx_tail = 0;

#ifdef UART_IRQ_RX
	/*
	 * Enable the interrupts for receiving, if we are using
	 * interrupt driven IO.  Transmit interrupts will be
	 * enabled by putc() as needed.
	 */
	sbi( UCSRB, RXCIE );
#endif
}


static inline void
write_to_uart( void )
{
	uint8_t			tmp_tail;

	if( tx_head == tx_tail )
	{
		cbi( UCSRB, UDRIE );
		return;
	}

	tx_tail = tmp_tail = ( tx_tail + 1 ) & TX_BUF_MASK;
	
	outp( tx_buf[tmp_tail], UDR );
}

static inline void
read_from_uart( void )
{
	char			c = inp( UDR );
	uint8_t			tmp_head;

	tmp_head = (rx_head + 1) & RX_BUF_MASK;

	/* Check for free buffer */
	if( tmp_head == rx_tail )
		return;

	rx_buf[ rx_head = tmp_head ] = c;
}


#ifdef UART_IRQ_TX
SIGNAL( SIG_UART_DATA )
{
	write_to_uart();
}
#endif

#ifdef UART_IRQ_RX
SIGNAL( SIG_UART_RECV )
{
	read_from_uart();
}
#endif



uint8_t
getc(
	uint8_t *		c
)
{
	uint8_t			tmp_tail;
	if( rx_head == rx_tail )
		return 0;

	rx_tail = tmp_tail = ( rx_tail + 1 ) & RX_BUF_MASK;
	
	*c = rx_buf[ tmp_tail ];
	return 1;
}

void
putc(
	uint8_t			c
)
{
	uint8_t			tmp_head;
	tmp_head = ( tx_head + 1 ) & TX_BUF_MASK;
	if( tmp_head == tx_tail )
		return;

	tx_buf[ tx_head = tmp_head ] = c;

#ifdef UART_IRQ_TX
	sbi( UCSRB, UDRIE );
#endif
}


/**
 *  NOP if we have interrupts enabled for the UART.  Otherwise we
 * handshake out the bytes.
 */
void
uart_task( void )
{
#ifndef UART_IRQ_RX
	if( bit_is_set( UCSRA, RXC ) )
		read_from_uart();
#endif

#ifndef UART_IRQ_TX
	if( bit_is_set( UCSRA, UDRE ) )
		write_to_uart();
#endif
}

