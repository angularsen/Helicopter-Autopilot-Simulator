/* -*- indent-tabs-mode:T; c-basic-offset:8; tab-width:8; -*- vi: set ts=8:
 * $Id: periodic.c,v 2.0 2002/09/22 02:10:16 tramm Exp $
 *
 * Periodic timer goes off every 61 Hz.
 *
 * Timer 2 runs at Clk/256, which is one tick every 64 usec.
 * It overflows every 64 usec * 256 = 16.384 msec == 61 Hz.
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
#include <sig-avr.h>

volatile uint8_t		periodic;

INTERRUPT( SIG_OVERFLOW2 )
{
	periodic = 1;
}

void
init_periodic( void )
{
	sbi( TIMSK, TOIE2 );
	outp( 0x00, TCNT2 );
	outp( 0x06, TCCR2 );		/* Clk/256 */
	outp( 0x07, TCCR2 );		/* Clk/1024 */
}


