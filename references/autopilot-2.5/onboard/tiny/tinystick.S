/* -*- indent-tabs-mode:T; c-basic-offset:8; tab-width:8; -*- vi: set ts=8:
 * $Id: tinystick.S,v 1.1 2002/09/27 02:14:07 tramm Exp $
 *
 * (c) 2002 by Trammell Hudson <hudson@rotomotion.com>
 *
 * Optimized pulse width measuring routine.  Should require only a few
 * microseconds per call.  One pulse = 1.25 microseconds.
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

#define INPUT_PULLUP		PORTB
#define INPUT_DIR		DDRB
#define INPUT_PORT		PINB
#define INPUT_PIN		1


.text

/**
 *  Our main loop is very simple -- it uses the register file to store
 * eight 16-bit pulse widths and then outputs them in turn.
 *
 * Registers used:
 *
 *	r2..r17
 *
 * Functions called:
 *
 *	wait_for_sync
 *	pulse_width
 *	putc
 *	put_uint12_t
 *
 */
main:
	rjmp	measure_pulses

.global measure_pulses
measure_pulses:

	/* Bring up the software UART */
	rcall	uart_init

	/* Configure the port for incoming pulses */
	sbi	INPUT_PULLUP, INPUT_PIN		/* PORTD2 = 1 */
	cbi	INPUT_DIR, INPUT_PIN		/* DDRD2 = 0 */

mainloop:
	rcall	wait_for_sync

pulse_loop:
	rcall	pulse_width
	mov	r2, r24
	mov	r3, r25
	rcall	pulse_width
	mov	r4, r24
	mov	r5, r25
	rcall	pulse_width
	mov	r6, r24
	mov	r7, r25
	rcall	pulse_width
	mov	r8, r24
	mov	r9, r25
	rcall	pulse_width
	mov	r10, r24
	mov	r11, r25
	rcall	pulse_width
	mov	r12, r24
	mov	r13, r25
	rcall	pulse_width
	mov	r14, r24
	mov	r15, r25
	rcall	pulse_width
	mov	r16, r24
	mov	r17, r25

output_loop:
	mov	r24, r2
	mov	r25, r3
	rcall	put_uint12_t
	mov	r24, r4
	mov	r25, r5
	rcall	put_uint12_t
	mov	r24, r6
	mov	r25, r7
	rcall	put_uint12_t
	mov	r24, r8
	mov	r25, r9
	rcall	put_uint12_t
	mov	r24, r10
	mov	r25, r11
	rcall	put_uint12_t
	mov	r24, r12
	mov	r25, r13
	rcall	put_uint12_t
	mov	r24, r14
	mov	r25, r15
	rcall	put_uint12_t
	mov	r24, r16
	mov	r25, r17
	rcall	put_uint12_t

	ldi	r24, '\r'
	rcall	putc
	ldi	r24, '\n'
	rcall	putc

	/* Start it all over again... */
	rjmp	mainloop


/**
 *  The width of the sync pulses, in ticks.  The pulses are around
 * 10 ms in length, while the data pulses are less than 2 ms.  So
 * any suitably large number should work.
 *
 * We are clocking from falling edge to rising edge on the pulses,
 * so there is a constant factor that we are missing.  We could
 * preload r24/r25 with it if we wanted to do so.
 *
 * Registers used:
 *
 *	r24, r25, r19
 *
 * Functions called:
 *
 *	None
 *
 */
#define SYNC_WIDTH		0x1000

.global pulse_width
pulse_width:
	ldi	r24, lo8(0)
	ldi	r25, hi8(0)

wait_for_lo:
	in	r19, INPUT_PORT		/* 1 */
	sbrc	r19, INPUT_PIN		/* 1 */
	rjmp	wait_for_hi
	inc	r24
	brne	wait_for_lo
	inc	r25
	rjmp	wait_for_lo


wait_for_hi:
	in	r19, INPUT_PORT		/* 1 */
	sbrs	r19, INPUT_PIN		/* 1 */
	ret
	inc	r24
	brne	wait_for_hi
	inc	r25
	rjmp	wait_for_hi

	ret


/*
 * Registers used:
 *
 *	r19
 *
 * Functions called
 *
 *	pulse_width
 */
.global wait_for_sync
wait_for_sync:
	in	r19, INPUT_PORT	
	sbrc	r19, INPUT_PIN
	rjmp	wait_for_sync

	rcall	pulse_width
	subi	r24, lo8( SYNC_WIDTH )
	sbci	r25, hi8( SYNC_WIDTH )
	brlo	wait_for_sync

	ret

