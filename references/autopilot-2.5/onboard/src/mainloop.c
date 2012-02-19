/* -*- indent-tabs-mode:T; c-basic-offset:8; tab-width:8; -*- vi: set ts=8:
 *  $Id: mainloop.c,v 2.0 2002/09/22 02:10:18 tramm Exp $
 *
 * Everything-in-one mainloop.  It combines features of all of
 * the avr/foo.c and src/flybywire.c files into one amalgamated
 * mainloop.
 *
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
#include <sig-avr.h>
#include <interrupt.h>
#include <io.h>
#include <math.h>
#include "timer.h"
#include "uart.h"
#include "string.h"
#include "servo.h"
#include "accel.h"
#include "adc.h"
#include "pulse.h"
#include "average.h"


/**
 *  Per-board definitions
 */
#define GYRO_ROLL		0
#define GYRO_PITCH		1
#define GYRO_YAW		2


static uint16_t
gyro_bias[ 3 ];

/*************************************************************************
 *
 * Command processor
 *
 * Right now we just do servos, but this is where much of the
 * development will occur
 *
 */
static void
commands( void )
{
	uint8_t			c;
	uint8_t			count = 0;
	static uint8_t		got_sync = 0;
	static uint8_t		servo = 0xFF;

	while( ++count < 10 && getc( &c) )
	{
		if( !got_sync )
		{
			if( c == 0xFF )
				got_sync = 1;
			continue;
		}
			
		if( servo == 0xFF )
		{
			servo = c;
			continue;
		}

		servo_set( servo, c );
		got_sync = 0;
		servo = 0xFF;
	}
}




/*************************************************************************
 *
 *  The main loop does everything...
 */


#define A_OUTPUT_CYCLE		128
#define V_OUTPUT_CYCLE		 16
#define P_OUTPUT_CYCLE		128

#define WASTE_PHASES		  4u
#define WASTE_SAMPLES		  4u

static uint16_t waste_sum[WASTE_PHASES];
static uint16_t waste_samples[WASTE_PHASES][WASTE_SAMPLES];
static uint8_t	waste_head[WASTE_PHASES];

#if 0
static inline void
a_output( void )
{
	putc( 'A' );
	put_uint16_t( ACCEL_SAMPLES );
	put_uint16_t( accel_sum[0] );
	put_uint16_t( accel_sum[1] );
	put_uint16_t( accel_sum[2] );
	puts( "\r\n" );
}


static inline void
v_output( void )
{
	putc( 'V' );
	put_uint16_t( VOLTS_SAMPLES );
	put_uint16_t( volts_sum[3] );
	put_uint16_t( volts_sum[5] );
	put_uint16_t( volts_sum[6] );
	puts( "\r\n" );
}


static inline void
p_output( void )
{
	putc( 'P' );
	pulse_avg();
	put_uint16_t( pulse_sum[0] );
	put_uint16_t( pulse_sum[1] );
	put_uint16_t( volts_sum[4] );	/* Gyro temp */
	put_uint16_t( volts_sum[0] );	/* CHT */
	puts( "\r\n" );
}
#endif

static inline void
do_output( void )
{
	put_uint16_t( accel_sum[0] );
	putc( ' ' );
	put_uint16_t( accel_sum[1] );
	putc( ' ' );
	put_uint16_t( accel_sum[2] );
	putc( ' ' );
	put_uint16_t( volts_sum[ GYRO_ROLL ]  - gyro_bias[ 0 ] );
	putc( ' ' );
	put_uint16_t( volts_sum[ GYRO_PITCH ] - gyro_bias[ 1 ] );
	putc( ' ' );
	put_uint16_t( volts_sum[ GYRO_YAW ]   - gyro_bias[ 2 ] );
	puts( "\r\n" );
}


static void
do_stuff( void )
{
	static uint8_t		cycle_count;
	static uint16_t		last;
	uint16_t		diff;
	uint8_t			phase;
	uint16_t		delay;

	diff = time() - last;

	servo_output();

	phase = cycle_count % WASTE_PHASES;
	average(
		diff,
		&waste_sum[phase],
		waste_samples[phase],
		waste_head[phase]++,
		WASTE_SAMPLES
	);

	commands();

	if( cycle_count % 32 == 0 )
		do_output();

#if 0
	if( cycle_count % A_OUTPUT_CYCLE == 8  )
		a_output();

	if( cycle_count % V_OUTPUT_CYCLE == 0 )
		v_output();

	if( cycle_count % P_OUTPUT_CYCLE == 4 )
		p_output();
#endif

	/* Track where we are */
	cycle_count++;

	/* Make full use of our timecycle */
	idle();
	last = time();

	/* Estimate how long we have left... */
	delay = waste_sum[(phase + 1) % WASTE_PHASES] / 64;

	/* Sleep for lots of clock cycles... */
	while( delay-- > 0 )
		;

}

static inline void
init_gyros( void )
{
	int			i;

	sei();
	puts( "# Sampling gyro bias: " );
	for( i = 0 ; i < 32 ; i++ )
	{
		msleep( 1024 );
		putc( '.' );
	}

	gyro_bias[ 0 ] = volts_sum[ GYRO_ROLL ];
	gyro_bias[ 1 ] = volts_sum[ GYRO_PITCH ];
	gyro_bias[ 2 ] = volts_sum[ GYRO_YAW ];

	puts( "Done\r\n" );

	/* Let the UART output the data before we move on */
	msleep( 1024 );
	cli();
}


int main( void )
{
	init_timer();
	init_uart();
	init_accel();
	init_adc();
	init_servo();
	init_gyros();
	/* We're stealing these pins for the accelerometer now */
	if( 0 ) init_tach();

	while(1)
		accel_mainloop( do_stuff );
}

