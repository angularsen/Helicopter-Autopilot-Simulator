/* -*- indent-tabs-mode:T; c-basic-offset:8; tab-width:8; -*- vi: set ts=8:
 * $Id: pwm.S,v 2.0 2002/09/22 02:10:18 tramm Exp $
 *
 * (c) 2002 by Trammell Hudson <hudson@rotomotion.com>
 *
 * Drive a PWM motor
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

#define OUTPUT_PORT		PORTB
#define OUTPUT_PIN1		1
#define OUTPUT_PIN2		2

#define OFF							\
	cbi	OUTPUT_PORT, OUTPUT_PIN1;			\
	cbi	OUTPUT_PORT, OUTPUT_PIN2;			\

#define FORWARD							\
	sbi	OUTPUT_PORT, OUTPUT_PIN1;			\
	cbi	OUTPUT_PORT, OUTPUT_PIN2;			\

#define REVERSE							\
	cbi	OUTPUT_PORT, OUTPUT_PIN1;			\
	sbi	OUTPUT_PORT, OUTPUT_PIN2;			\
	

#define DELAY16( ticks )					\
	ldi	r24, lo8(ticks)  				\
	ldi	r25, hi8(ticks);				\
	subi	r24, lo8(1);					\
	sbci	r25, hi8(1);					\
	brne	-3;						\


.text

/**
 *  All we do is generate on pulses for the output pin every second.
 * Nothing fancy yet.
 */
main:
	OFF
	rcall uart_init
	sbi	DDRB, OUTPUT_PIN1
	sbi	DDRB, OUTPUT_PIN2

do_it:
	FORWARD

	ldi	r24, lo8(1)  
	ldi	r25, hi8(1)
on_delay:
	subi	r24, lo8(1)
	sbci	r25, hi8(1)
	brne	on_delay

	OFF

	ldi	r24, lo8(65535)  
	ldi	r25, hi8(65535)
off_delay:
	subi	r24, lo8(1)
	sbci	r25, hi8(1)
	brne	off_delay

	rjmp do_it


delay16:
	subi	r24, lo8(1)
	sbci	r25, hi8(1)
	brne	delay16
	ret


#if 0
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

#endif
