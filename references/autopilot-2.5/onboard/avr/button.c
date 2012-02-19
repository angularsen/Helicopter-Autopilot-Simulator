/* -*- indent-tabs-mode:T; c-basic-offset:8; tab-width:8; -*- vi: set ts=8:
 * $Id: button.c,v 2.0 2002/09/22 02:10:16 tramm Exp $
 *
 * Accessors to the STK300's buttons and LEDs.  This need not be built
 * into the Mega16 custom PCB release.
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

#include <io.h>
#include "button.h"

#define LED_PORT	PORTB
#define LED_PORT_DIR	DDRB

#define BUTTON_PORT	PIND
#define BUTTON_PORT_DIR	DDRD
#define BUTTON_PORT_CFG	PORTD


/**
 *  Initialize the ports
 */
void
button_init( void )
{
	outp( 0xFF, LED_PORT_DIR );
	outp( 0x00, BUTTON_PORT_DIR );
	outp( 0x00, BUTTON_PORT_CFG );

	led_set_mask( 0x00 );
}

/**
 *  Button state.  The STK300 buttons are active low.
 */
uint8_t
button_get(
	uint8_t			button
)
{
	uint8_t	cur_mask = inp( BUTTON_PORT );
	return (cur_mask & (1 << button)) ? 0 : 1;
}


/**
 *  LED stuff.  We store the inverted mask for the LED's, just like
 * the actual port.  This saves a few bytes of inverting the mask before
 * putting it on the wire.
 */
static uint8_t led_mask;

void
led_set(
	uint8_t			led,
	uint8_t			on
)
{
	uint8_t			bits = 1<<led;

	if( on )
		led_mask &= ~bits;
	else
		led_mask |= bits;

	outp( led_mask, LED_PORT );
}


void
led_set_mask(
	uint8_t			mask
)
{
	led_mask = ~mask;
	outp( led_mask, LED_PORT );
}
