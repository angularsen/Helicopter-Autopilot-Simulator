/* -*- indent-tabs-mode:T; c-basic-offset:8; tab-width:8; -*- vi: set ts=8:
 * $Id: accel.c,v 2.0 2002/09/22 02:10:16 tramm Exp $
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
#include "timer.h"
#include "accel.h"
#include "average.h"


/*************************************************************************
 *
 *  Accelerometer code.
 *
 * The accelerometer is -the- realtime task that we have in the mainloop.
 * Please see the comments in onboard/doc/accelerometer for the
 * algorithm used here.  Link there:
 *
 *     http://autopilot.sf.net/cgi-bin/source?onboard/doc/accelerometer
 *
 * The globals a_x and a_y report in mili-g.  The ADXL202 has a range
 * of +/- 2 g and the ADXL210 has a range of +/-10g, both of which
 * fit into a 16 bit value for mili-g.
 *
 * WARNING:  The values reported have nothing to do with actual units
 * anymore.  The tuning factors are still being work on.  Please bear
 * with me...
 */

uint8_t				accel_state;
uint16_t			accel_sum[3];
static uint16_t			accel_samples[3][ ACCEL_SAMPLES ];
static uint8_t			accel_head;

#define disable( i )		cbi( GIMSK, INT##i )
#define enable( i )		sbi( GIMSK, INT##i )

#define enable_rising( i )						\
	do {								\
		valid = 0;						\
		sbi( MCUCR, ISC##i##0 );				\
		enable( i );						\
	} while(0)

#define enable_falling( i )						\
	do {								\
		valid = 0;						\
		cbi( MCUCR, ISC##i##0 );				\
		enable( i );						\
	} while(0)


static uint16_t		last_int;
static uint8_t		valid;

INTERRUPT( SIG_INTERRUPT0 )
{
	last_int = time_nonatomic();
	valid	 = 1;
}

INTERRUPT( SIG_INTERRUPT1 )
{
	last_int = time_nonatomic();
	valid	 = 1;
}


void
init_accel( void )
{
	/* Input pins, open-collector (internal pullup) */
	sbi( PORTD, 2 );
	sbi( PORTD, 3 );

	cbi( DDRD, 2 );
	cbi( DDRD, 3 );

	/* First rising edge of 0 will trigger our interrupt */
	sbi( MCUCR, ISC11 );
	sbi( MCUCR, ISC01 );

	accel_state = 0;
	disable( 1 );
	enable_rising( 0 );
}




/**
 */
uint8_t
accel_task( void )
{
	static uint16_t		t_a;
	static uint16_t		t_b;
	static uint16_t		t_c;
	static uint16_t		t_d;
	static uint16_t		t1_x;
	static uint16_t		t1_y;
	static uint16_t		t2;
	static uint16_t		a_x;
	static uint16_t		a_y;

	switch( accel_state )
	{
	case 0:
		/* Rising edge of 0 */
		if( !valid )
			return 0;

		disable( 0 );

		accel_state	= 1;
		t_a		= last_int;

		enable_falling( 0 );
		return 0;

	case 1:
		/* Fallng edge of 0 */
		if( !valid )
			return 0;

		disable( 0 );

		accel_state	= 2;
		t_b		= last_int;
		t1_x		= 2 * (t_b - t_a);

		enable_rising( 1 );
		return 0;

	case 2 :
		/* Rising edge of 1 */
		if( !valid )
			return 0;

		disable( 1 );

		accel_state	= 3;
		t_c		= last_int;
		t2		= t_c - t_a;

		enable_falling( 1 );
		return 0;

	case 3 :
		/* Falling edge of 1 */
		if( !valid )
			return 0;

		disable( 1 );

		accel_state	= 4;
		t_d		= last_int;
		t1_y		= 2 * (t_d - t_c);
		t2		+= t_d - t_b;

		return 0;

	case 4 :
		/*
		 * Cleanup computation:  Compute the x and y accelerations.
		 * The one-pass algorithm was cribbed from the Analog
		 * Devices data sheet.  We do not scale to mili-G or
		 * anything else.
		 */
		t2	/= 512;
		a_x	= t1_x / t2;
		a_y	= t1_y / t2;

		accel_state	= 5;

		return 0;

	case 5:
		/**
		 *  Store the last ACCEL_SAMPLES of each and compute
		 * the running average.  This gives a more analog feel
		 * to the readings.
		 */
		average(
			a_x,
			&accel_sum[0],
			accel_samples[0],
			accel_head,
			ACCEL_SAMPLES
		);
	
		average(
			a_y,
			&accel_sum[1],
			accel_samples[1],
			accel_head,
			ACCEL_SAMPLES
		);

		accel_head++;

		accel_state	 = 0;
		enable_rising( 0 );
		return 1;
	}

	/* Not reached! */
	return 0;
}

