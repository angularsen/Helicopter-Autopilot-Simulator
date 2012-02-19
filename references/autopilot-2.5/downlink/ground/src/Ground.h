/* -*- indent-tabs-mode:T; c-basic-offset:8; tab-width:8; -*- vi: set ts=8:
 * $Id: Ground.h,v 1.2 2002/10/13 20:05:26 tramm Exp $
 *
 * (c) Trammell Hudson
 *
 * Global header for everything in the ground station
 *
 *************
 *
 *  This file is part of the autopilot groundstation package.
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
#ifndef _Ground_h_
#define _Ground_h_

#include "gui.h"

class UserInterface;

extern UserInterface *gui;

extern void
reconnect_server( void );

extern void
reconnect_udp( void );



#endif
