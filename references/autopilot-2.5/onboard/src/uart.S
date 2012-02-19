/* -*- indent-tabs-mode:T; c-basic-offset:8; tab-width:8; -*- vi: set ts=8:
 * $Id: uart.S,v 2.0 2002/09/22 02:10:18 tramm Exp $
 *
 * Software UART from the AVR app note:
 *
 *	http://www.atmel.com/atmel/acrobat/doc0952.pdf
 *
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

#include <iotn12.h>

/****** Pin definitions */

#define	UART_PORT	PORTB
#define UART_DIR	DDRB
#define	TX_PIN		0


/**
 *  uart_init()
 *
 * Initializes the UART, such as it is.
 */
.global uart_init
uart_init:
	/* Put serial port into output mode */
	cbi	UART_PORT, TX_PIN
	sbi	UART_DIR, TX_PIN			/* DDRD1 = output */
	ret


/***************************************************************************
 *
 * "putchar"
 *
 * This subroutine transmits the byte stored in the "Txbyte" register
 * The number of stop bits used is set with the sb constant
 *
 * Number of words	:14 including return
 * Number of cycles	:Depens on bit rate
 * Low registers used	:None
 * High registers used	:2 (bitcnt,Txbyte)
 * Pointers used	:None
 *
 ***************************************************************************/
#define sb		1		/* Stop bits */
#define	bitcnt		r19
#define	temp		r18
#define	Txbyte		r24

.global putc
putc:
	ldi	bitcnt,9+sb	;1+8+sb (sb is # of stop bits)
	com	Txbyte		;Inverte everything
	sec			;Start bit

putchar0:
	brcc	putchar1		;If carry set		/* 1 | 2 */
	cbi	UART_PORT,TX_PIN	;    send a '0'		/* 1 */
	rjmp	putchar2		;else			/* 2 */

putchar1:
	sbi	UART_PORT,TX_PIN	;    send a '1'		/* 1 */
	nop							/* 1 */

putchar2:
	rcall UART_delay		; One bit delay		/* 128 */
	rcall UART_delay					/* 128 */

	lsr	Txbyte		;Get next bit			/* 1 */
	dec	bitcnt		;If not all bit sent		/* 1 */
	brne	putchar0	;   send next			/* 2 */
				;else
	ret			;   return

/***************************************************************************
 *
 * "UART_delay"
 *
 * This delay subroutine generates the required delay between the bits when
 * transmitting and receiving bytes. The total execution time is set by the
 * constant "b":
 *
 *	3·b + 7 cycles (including rcall and ret)
 *
 * Number of words	:4 including return
 * Low registers used	:None
 * High registers used	:1 (temp)
 * Pointers used	:None
 *
 * Delay = 4 * b / Clk.
 *
 * For Clk = 4 Mhz and b = 31, we delay 31 usec / bit.
 * That means that our 8 + 1 + 1 bits = 310 usec / byte.
 * 
 *
 ***************************************************************************/

/*
 * Some b values: 	(See also table in Appnote documentation)
 * 4 MHz crystal:
 *  19200 bps - b=31	b=30 for Tiny12 / rcall syntax
 *  28800 bps - b=19
 *  57600 bps - b=8
 * 115200 bps - b=2
 */

#define b	30

UART_delay:
	ldi	temp,b			/* 1 */
UART_delay1:
	dec	temp			/* 1 */
	brne	UART_delay1		/* 2 */

	ret

/*
 *  Integer output code
 *
 * (c) Trammell Hudson <hudson@rotomotion.com>
 */

/**
 *  Output the hex digit corresponding to the argument in r24
 *
 * Registers used:
 *	r26/r27 (X)
 *	r24	(arg)
 *
 */
.global make_hexdigit
make_hexdigit:
	andi	r24, 0xF
	cpi	r24, 10
	brlo	hex_digit
dec_digit:
	subi	r24, lo8(-(55))			/* + '0' */
	ret
hex_digit:
	subi	r24, lo8(-(48))			/* + 'A' - 0xA */
	ret


.global put_uint8_t
put_uint8_t:
	mov	r20, r24

	/* High nibble */
	swap	r24
	rcall	make_hexdigit
	rcall	putc

	/* Low nibble */
	mov	r24, r20
	rcall	make_hexdigit
	rcall	putc

	/* Output a trailing space */
	ldi	r24, ' '
	rcall	putc

	ret


.global put_uint12_t
put_uint12_t:
	mov	r20, r24

	/* Put the high nibble */
	mov	r24, r25
	rcall	make_hexdigit
	rcall	putc

	/* Mid nibble */
	mov	r24, r20
	swap	r24
	rcall	make_hexdigit
	rcall	putc

	/* Low nibble */
	mov	r24, r20
	rcall	make_hexdigit
	rcall	putc

	/* Output a trailing space */
	ldi	r24, ' '
	rcall	putc

	ret


.global put_uint16_t
put_uint16_t:
	mov	r20, r24
	mov	r21, r25

	/* Put the high nibble */
	mov	r24, r25
	swap	r24
	rcall	make_hexdigit
	rcall	putc

	/* Put the high nibble */
	mov	r24, r21
	rcall	make_hexdigit
	rcall	putc


	/* Mid nibble */
	mov	r24, r20
	swap	r24
	rcall	make_hexdigit
	rcall	putc

	/* Low nibble */
	mov	r24, r20
	rcall	make_hexdigit
	rcall	putc

	/* Output a trailing space */
	ldi	r24, ' '
	rcall	putc

	ret
