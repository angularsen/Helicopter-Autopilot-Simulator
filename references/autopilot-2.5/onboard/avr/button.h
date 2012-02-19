/* -*- indent-tabs-mode:T; c-basic-offset:8; tab-width:8; -*- vi: set ts=8:
 * $Id: button.h,v 2.0 2002/09/22 02:10:16 tramm Exp $
 *
 * Access to the buttons and LED's on the STK300 board.
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

#ifndef _BUTTON_H_
#define _BUTTON_H_

#include <inttypes.h>

/**
 *  Query a button (and reset its state in the mask)
 */
extern uint8_t
button_get(
	uint8_t		button
);

/**
 *  Reset the state of the button mask
 */
extern void
button_clear( void );


/**
 *  Turn on/off one LED on the board.
 */
extern void
led_set(
	uint8_t		led,
	uint8_t		on
);

/**
 *  Set the LED's to the specified bit pattern
 */
extern void
led_set_mask(
	uint8_t		mask
);



/* Tasks */
extern void button_init( void );

#endif
