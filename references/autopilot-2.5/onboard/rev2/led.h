/* -*- indent-tabs-mode:T; c-basic-offset:8; tab-width:8; -*- vi: set ts=8:
 * $Id: led.h,v 2.0 2002/09/22 02:10:18 tramm Exp $
 *
 * Controls for the LED's on the rev2 board.
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

#ifndef _led_h_
#define _led_h_

#define LED_RED		3
#define LED_GREEN	4

static inline void
led_set(
	const uint8_t		which,
	const uint8_t		on
)
{
	if( on )
		cbi( PORTB, which );
	else
		sbi( PORTB, which );
}


static inline void
led_init( void )
{
	sbi( DDRB, LED_RED );
	sbi( DDRB, LED_GREEN );

	led_set( LED_RED, 1 );
	led_set( LED_GREEN, 1 );
}

#endif
