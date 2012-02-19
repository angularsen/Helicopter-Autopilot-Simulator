/* -*- indent-tabs-mode:T; c-basic-offset:8; tab-width:8; -*- vi: set ts=8:
 *  $Id: timethis.c,v 2.0 2002/09/22 02:10:18 tramm Exp $
 *
 * Computes the attitude and heading from IMU-type readings.
 *
 * (c) 2002 Trammell Hudson <hudson@swcp.com>
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

#include <math.h>
#include "../avr/vector.h"
#include "../avr/ahrs.h"


static inline void
process( void )
{
	static m_t		ma = {
		{ 3, 4, 9, 12 },
		{ 9, 3, -1, 2 },
		{ 4, 5, 1, 1 },
		{ 9, -1, 2, 1 },
	};
	static m_t		mb;
	static m_t		mc;
	static double		a = 9.832;
	static double		b = 3.14159;
	static double		c;
	iter_t			i;
	iter_t			j;

	for( i=0 ; i < 128 ; i++ )
	{
		quat2euler( ma[0], ma[1] );
	}
}

/***************************/

#include "uart.h"
#include "timer.h"
#include "string.h"

static volatile uint16_t high_bits;

SIGNAL( SIG_OVERFLOW1 )
{
	high_bits++;
}

static inline uint32_t
high_time( void )
{
	uint32_t now = (uint32_t) high_bits;
	now <<= 16;
	now |= time();
	return now;
}


int
main( void )
{
	uint32_t		start;
	uint32_t		stop;

	init_uart();
	init_timer();

	sbi( TIMSK, TOIE1 );
	sei();

	puts( "time()\r\n" );

	while(1)
	{
		puts( "\r\nstep: " );
		start = high_time();
		process();
		stop = high_time();

/*
		put_uint16_t( (stop >> 16) & 0xFFFF );
		put_uint16_t( (stop >>  0) & 0xFFFF );
		puts( " - " );
		put_uint16_t( (start >> 16) & 0xFFFF );
		put_uint16_t( (start >>  0) & 0xFFFF );
		puts( " = " );
*/

		stop -= start;
		put_uint16_t( (stop >> 16) & 0xFFFF );
		put_uint16_t( (stop >>  0) & 0xFFFF );
	}
}
