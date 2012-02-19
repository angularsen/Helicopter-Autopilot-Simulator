/* -*- indent-tabs-mode:T; c-basic-offset:8; tab-width:8; -*- vi: set ts=8:
 * $Id: spi-bridge.S,v 1.5 2002/09/27 22:17:22 tramm Exp $
 *
 * (c) 2002 by Trammell Hudson <hudson@rotomotion.com>
 *
 * 16 bit to 8 bit SPI bridge.  The Tiny12 reads in 16 bit data as
 * an SPI slave, then outputs it as two 8 bit packets as an SPI master.
 * The code assumes that the top two bits of the 16 are zero, such
 * as with a Vector 2X dual axis magnetometer (compass).
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

#define V2X_PORT	PINB
#define V2X_DDR		DDRB
#define V2X_PUD		PORTB
#define V2X_ENABLE	1
#define V2X_MOSI	3
#define V2X_SCK		4

#define SPI_PORT	PORTB
#define SPI_DDR		DDRB
#define SPI_MOSI	0
#define SPI_SCK		2


	.text
main:
	/*
	 * Configure our ports for input on the slave SPI port
	 * and output on the master SPI port.
	 */
	sbi	V2X_PUD, V2X_ENABLE
	sbi	V2X_DDR, V2X_ENABLE

	cbi	V2X_DDR, V2X_MOSI
	cbi	V2X_PUD, V2X_MOSI

	cbi	V2X_DDR, V2X_SCK
	cbi	V2X_PUD, V2X_SCK

	sbi	SPI_DDR, SPI_MOSI
	sbi	SPI_DDR, SPI_SCK

	/* Clock is high when idle */
	sbi	SPI_PORT, SPI_SCK

	/* Let the V2x init for 2 seconds */
	ldi	r24, 0xFF
	rcall	delay
	ldi	r24, 0xFF
	rcall	delay
	ldi	r24, 0xFF
	rcall	delay
	ldi	r24, 0xFF
	rcall	delay


	/*
	 * Put a non-sensical value on the SPI port to let them know
	 * that we're alive.  This heading could never be produced by
	 * the V2X, so they can discard it.
	 */
	ldi	r24, 0xFF
	rcall	put_byte

	ldi	r24, 0x7F
	rcall	put_byte


	/*
	 * Main loop -- request a heading, read two bytes, put two bytes
	 * later rinse repeat...
	 */
loop:
	/* Request a heading */
	cbi	V2X_PUD, V2X_ENABLE

	rcall	get_byte
	mov	r17, r24

	sbi	V2X_PUD, V2X_ENABLE

	rcall	get_byte
	mov	r16, r24


	/*
	 * Transform the bytes and put them out again.
	 *
	 * The high byte is shifted left once and the high bit from the
	 * low byte is shifted over.  The MSB is set to 1 to indicate that
	 * this is the high byte and it is sent over the SPI.
	 *
	 * The low byte has its MSB set to zero to indicate that it is
	 * the low byte.  It is then sent over the SPI and another
	 * heading is requested from the Vector 2X.
	 */
	mov	r24, r17
	lsl	r24
	ori	r24, 0x80
	lsl	r16
	brcc	high_bit_is_zero
	ori	r24, 0x01
high_bit_is_zero:
	rcall	put_byte

	lsr	r16
	mov	r24, r16
	rcall	put_byte

	/* Lather, rinse, repeat */
	rjmp	loop;


/**
 *  Delay by some large number of msec
 */
delay:
	ldi	r25, 0xFF
_delay:
	subi	r25, 1
	brne	_delay

	subi	r24, 1
	brne	delay
	ret


/**
 * Read a byte from the SPI master.
 * We should have a time out in case the V2X isn't initialized yet.
 */
get_byte:
	ldi	r25, 8
	ldi	r24, 1

wait_for_low:
	sbic	V2X_PORT, V2X_SCK
	rjmp	wait_for_low

	lsl	r24

wait_for_hi:
	sbis	V2X_PORT, V2X_SCK	
	rjmp	wait_for_hi

	sbic	V2X_PORT, V2X_MOSI
	ori	r24, 1

	subi	r25, 1
	brne	wait_for_low

	ret

/**
 *  Write a byte to the SPI slave
 */
put_byte:
	ldi	r25, 8

out_bit:
	sbi	SPI_PORT, SPI_SCK
	lsl	r24
	brcc	out_low_bit
out_high_bit:
	sbi	SPI_PORT, SPI_MOSI
	rjmp	out_next_bit
out_low_bit:
	cbi	SPI_PORT, SPI_MOSI
out_next_bit:

	/* Pulse the clock */
	cbi	SPI_PORT, SPI_SCK
	nop
	nop
	nop
	nop

	subi	r25, 1
	brne	out_bit

	sbi	SPI_PORT, SPI_SCK
	ret
