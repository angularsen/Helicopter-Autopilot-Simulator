/* -*- indent-tabs-mode:T; c-basic-offset:8; tab-width:8; -*- vi: set ts=8:
 * $Id: adc.h,v 2.1 2002/10/20 13:51:05 tramm Exp $
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

#ifndef _ADC_H_
#define _ADC_H_

#include "uart.h"
#include "string.h"

extern uint16_t		adc_samples[ 8 ];

extern void
adc_init( void );


static inline void
adc_output( void )
{
	uint8_t i;

	puts( "$GPADC" );

	for( i=0 ; i < 8 ; i++ )
	{
		putc( ',' );
		put_uint16_t( adc_samples[i] );
	}

	putnl();
}


#endif
