/* -*- indent-tabs-mode:T; c-basic-offset:8; tab-width:8; -*- vi: set ts=8:
 * $Id: memory.h,v 1.3 2003/03/22 18:10:26 tramm Exp $
 *
 * Code to manipulate memory spaces
 *
 * (c) 2002 Trammell Hudson <hudson@swcp.com>
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

#ifndef _avr_memory_h_
#define _avr_memory_h_

#include <avr/pgmspace.h>	/* For attributes */
#include <string.h>		/* For memset etc */

/*
 * Copy a string from flash into RAM.  Don't include the terminating
 * zero.
 */
#define pmemcpy( dest, src )						\
	memcpy_P( dest, PSTR( src ), sizeof( src ) - 1 )

#endif
