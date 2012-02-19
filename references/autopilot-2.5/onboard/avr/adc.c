/* -*- indent-tabs-mode:T; c-basic-offset:8; tab-width:8; -*- vi: set ts=8:
 * $Id: adc.c,v 2.0 2002/09/22 02:10:16 tramm Exp $
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
#include "adc.h"
#include "average.h"


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
#define VOLTAGE_REF	0xC0
#define VOLTAGE_TIME	0x07
#define ANALOG_PORT	PORTA
#define ANALOG_PORT_DIR	DDRA

uint16_t		volts_sum[ 8 ];
static uint16_t		volts_samples[8][ VOLTS_SAMPLES ];


void
init_adc( void )
{
	/* Ensure that our port is for input */
	outp( 0x00, ANALOG_PORT );	
	outp( 0x00, ANALOG_PORT_DIR );

	/* Select out internal voltage ref */
	outp( VOLTAGE_REF, ADMUX );
	outp( VOLTAGE_TIME, ADCSR );
	sbi( ADCSR, ADEN );

	/* Turn off the analog comparator */
	sbi( ACSR, ACD );

#ifdef ADC_IRQ
	/* Enable our interrupt, if we have interrupts turned on */
	sbi( ADCSR, ADIE );
#endif

	/* Start the first translation */
	sbi( ADCSR, ADSC );
}


/**
 * Called when the voltage conversion is finished, or periodically
 * by the user's mainloop.
 */
static inline void
read_adc( void )
{
	static uint8_t		volts_head;

	uint8_t			adc_input	= inp( ADMUX ) % 8u;
	uint16_t		value		= __inw( ADCL );

	average(
		value,
		&volts_sum[adc_input],
		volts_samples[adc_input],
		volts_head,
		VOLTS_SAMPLES
	);


	/* Find the next input and select it */
	if( ++adc_input == 8 )
	{
		adc_input = 0;
		volts_head++;
	}

	outp( adc_input | VOLTAGE_REF, ADMUX );

	/* Restart the conversion */
	sbi( ADCSR, ADSC );
}


/**
 *  If ADC interrupts are not enabled, we don't bother compiling
 * this routine.
 */
#ifdef ADC_IRQ
SIGNAL( SIG_ADC )
{
	read_adc();
}
#endif


/*
 * If interrupts are enabled, then this is a NOP.  Otherwise
 * we check for conversion finished and read the sample.
 */
void
adc_task( void )
{
#ifndef ADC_IRQ
	if( bit_is_set( ADCSR, ADIF ) )
		read_adc();
#endif
}


