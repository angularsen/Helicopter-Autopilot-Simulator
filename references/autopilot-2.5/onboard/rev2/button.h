/* -*- indent-tabs-mode:T; c-basic-offset:8; tab-width:8; -*- vi: set ts=8:
 * $Id: button.h,v 2.1 2003/02/27 05:06:41 tramm Exp $
 *
 * Methods to read the two buttons on the rev2.2 board.
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

#ifndef _button_h_
#define _button_h_

#include <inttypes.h>

/**
 *  Initialize the buttons pins to input, pull-up
 */
static inline void
button_init( void )
{
	cbi( DDRC, 6 );
	sbi( PORTC, 6 );

	cbi( DDRC, 7 );
	sbi( PORTC, 7 );
}


static inline uint8_t
button_state(
	const uint8_t		which
)
{
	return bit_is_set( PINC, which == 0 ? 6 : 7 );
}

#endif
