/* -*- indent-tabs-mode:T; c-basic-offset:8; tab-width:8; -*- vi: set ts=8:
 * $Id: timer.h,v 2.0 2002/09/22 02:10:16 tramm Exp $
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

#ifndef _TIME_H_
#define _TIME_H_

#include <sig-avr.h>
#include <interrupt.h>
#include <io.h>


/*************************************************************************
 *
 *  Global timer code.
 *
 * The global timer uses Timer 1 running at the Clock speed, which
 * should be 4 Mhz.  One clock tick is 0.25 useconds.  This means that
 * we have an overflow every 16.384 miliseconds.
 *
 * If we count those overflows with an 8 bit counter, we can have
 * up to 4 seconds of timing.  However, there are no tasks that require
 * that large of an epoch, so we do not enable the overflow and
 * increment a "high bit" value.
 */

static inline void
init_timer( void )
{
	outp( 0x00, TCCR1A );		/* Ignore OC1X */
	outp( 0x01, TCCR1B );		/* Clk / 1 */
}

static inline uint16_t
time( void )
{
	return __inw_atomic( TCNT1L );
}

static inline uint16_t
time_nonatomic( void )
{
	return __inw( TCNT1L );
}


static inline void
usleep(
	uint16_t		len
)
{
	uint16_t		end = time() + len * 4;

	while( time() < end )
		;
}

static inline void
msleep(
	uint16_t		len
)
{
	while( len-- > 0 )
		usleep( 1024 );
}


#endif
