/* -*- indent-tabs-mode:T; c-basic-offset:8; tab-width:8; -*- vi: set ts=8:
 * $Id: lcd.h,v 2.1 2002/10/22 01:42:06 tramm Exp $
 *
 * This implements a Hitachi 44780 LCD controller interface on Port C.
 * It is largely based on code from Peter Fluery's LCD library.
 *
 * (c) 2002 Trammell Hudson <hudson@rotomotion.com>
 *
 * Portions:
 * (c) 2000 Peter Fleury <pfleury@gmx.ch>  http://jump.to/fleury
 *
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
#ifndef _lcd_h_
#define _lcd_h_

extern uint8_t		lcd_buf[];

extern void
lcd_init( void );

#endif

