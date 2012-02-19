/* -*- indent-tabs-mode:T; c-basic-offset:8; tab-width:8; -*- vi: set ts=8:
 * $Id: tach.h,v 2.0 2002/09/22 02:10:18 tramm Exp $
 *
 * Engine tachometer counts the pulses on a Hall Effect sensor
 * connected to the INT0 external interrupt pin.  It uses only
 * an 8 bit value, which will overflow after 256 revolutions.
 * For a typical four stroke turning 13000 RPM, that will be
 * roughly every 0.8 seconds.  Be sure to empty the value before
 * that happens.
 *
 * (c) 2002 Trammell Hudson <hudson@rotomotion.com>
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

#ifndef _tach_h_
#define _tach_h_

#include <interrupt.h>
#include <sig-avr.h>
#include "string.h"

#define TACH_N1
#undef TACH_N2

/*
 * The tach is on INT0 external, falling edge triggered interrupts.
 * The pin need to be configured for input with internal pullup.  
 */
static inline void
tach_init( void )
{
	/* Input pins, open-collector (internal pullup) */
	#ifdef TACH_N1
		sbi( PORTD, 2 );
		cbi( DDRD, 2 );
	#endif

	#ifdef TACH_N2
		sbi( PORTD, 3 );
		cbi( DDRD, 3 );
	#endif

	/* Falling edge triggered interrupts */
	#ifdef TACH_N1
		sbi( MCUCR, ISC01 );
		sbi( MCUCR, ISC00 );
	#endif

	#ifdef TACH_N2
		sbi( MCUCR, ISC11 );
		sbi( MCUCR, ISC10 );
	#endif

	/* Enable the interrupts */
	#ifdef TACH_N1
		sbi( GIMSK, INT0 );
	#endif

	#ifdef TACH_N2
		sbi( GIMSK, INT1 );
	#endif
}

#ifdef TACH_N1
static volatile uint8_t		tach_n1;

INTERRUPT( SIG_INTERRUPT0 )
{
	tach_n1++;
}
#endif

#ifdef TACH_N2
static volatile uint8_t		tach_n2;

INTERRUPT( SIG_INTERRUPT1 )
{
	tach_n2++;
}
#endif


static inline void
tach_output( void )
{
	puts( "$GPRPM" );
	#ifdef TACH_N1
		putc( ',' );
		put_uint8_t( tach_n1 );
		tach_n1 = 0;
	#endif

	#ifdef TACH_N2
		putc( ',' );
		put_uint8_t( tach_n2 );
		tach_n2 = 0;
	#endif

	putnl();
}



#endif
