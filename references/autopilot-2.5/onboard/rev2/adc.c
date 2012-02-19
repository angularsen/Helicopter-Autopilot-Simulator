/* -*- indent-tabs-mode:T; c-basic-offset:8; tab-width:8; -*- vi: set ts=8:
 * $Id: adc.c,v 2.2 2003/03/22 18:10:10 tramm Exp $
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


#include <avr/signal.h>
#include <avr/interrupt.h>
#include <avr/io.h>
#include "adc.h"


/*************************************************************************
 *
 *  Analog to digital conversion code.
 *
 * We allow interrupts during the 2048 usec windows.  If we run the
 * ADC clock faster than Clk/64 we have too much overhead servicing
 * the interrupts from it and end up with servo jitter.
 *
 * For now we've slowed the clock to Clk/128 because it lets us
 * be lazy in the interrupt routine.
 */
#define VOLTAGE_TIME	0x07
#define ANALOG_PORT	PORTA
#define ANALOG_PORT_DIR	DDRA

uint16_t		adc_samples[8];


void
adc_init( void )
{
	/* Ensure that our port is for input with no pull-ups */
	ANALOG_PORT 	= 0x00;
	ANALOG_PORT_DIR	= 0x00;

	/* Select our external voltage ref, which is tied to Vcc */
	ADMUX		= 0x00;

	/* Turn off the analog comparator */
	sbi( ACSR, ACD );

	/* Select out clock, turn on the ADC interrupt and start conversion */
	ADCSR		= 0
		| VOLTAGE_TIME
		| ( 1 << ADEN )
		| ( 1 << ADIE )
		| ( 1 << ADSC );
}


/**
 * Called when the voltage conversion is finished
 */
SIGNAL( SIG_ADC )
{
	uint8_t			adc_input	= ADMUX & 0x7;

	adc_samples[ adc_input ] = ADCW;

	/* Find the next input and select it */
	if( ++adc_input == 8 )
		adc_input = 0;

	ADMUX = adc_input;

	/* Restart the conversion */
	sbi( ADCSR, ADSC );
}
