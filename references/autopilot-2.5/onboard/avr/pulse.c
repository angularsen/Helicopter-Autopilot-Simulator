/* -*- indent-tabs-mode:T; c-basic-offset:8; tab-width:8; -*- vi: set ts=8:
 * $Id: pulse.c,v 2.0 2002/09/22 02:10:16 tramm Exp $
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
#include "pulse.h"
#include "average.h"


/*************************************************************************
 *
 * Pulse counting inputs for N1 and N2 tach
 *
 * We only count pulses during the 2048 usecond windows, so we have
 * 8192 useconds of counting per 20ms duty cycle.  This value should
 * be indicative of the actual pulse value, so the value output can
 * be multiplied by 256 to produce RPM.
 *
 * I hope.
 */
static volatile uint8_t	pulse_0;
static volatile uint8_t	pulse_1;

uint16_t		pulse_sum[2];
static uint16_t		pulse_samples[2][ PULSE_SAMPLES ];
static uint8_t		pulse_head;


void
init_tach( void )
{
	/* Input pins, open-collector (internal pullup) */
	sbi( PORTD, 2 );
	sbi( PORTD, 3 );

	cbi( DDRD, 2 );
	cbi( DDRD, 3 );

	/* Falling edge triggered interrupts */
	sbi( MCUCR, ISC01 );
	sbi( MCUCR, ISC00 );

	sbi( MCUCR, ISC11 );
	sbi( MCUCR, ISC10 );

	/* Enable the interrupts */
	sbi( GIMSK, INT0 );
	sbi( GIMSK, INT1 );
}


INTERRUPT( SIG_INTERRUPT0 )
{
	pulse_0++;
}


INTERRUPT( SIG_INTERRUPT1 )
{
	pulse_1++;
}


void
pulse_avg( void )
{
	average(
		pulse_0,
		&pulse_sum[0],
		pulse_samples[0],
		pulse_head,
		PULSE_SAMPLES
	);
	pulse_0 = 0;

	average(
		pulse_1,
		&pulse_sum[1],
		pulse_samples[1],
		pulse_head,
		PULSE_SAMPLES
	);
	pulse_1 = 0;

	pulse_head++;
}
