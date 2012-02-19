/* -*- indent-tabs-mode:T; c-basic-offset:8; tab-width:8; -*- vi: set ts=8:
 * $Id: string.c,v 2.0 2002/09/22 02:10:16 tramm Exp $
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
#include "uart.h"


/*************************************************************************
 *
 * String and number output functions
 *
 */
void
puts(
	const char *		s
)
{
	while( *s )
		putc( *s++ );
}

void
put_hexdigit(
	uint8_t			i
)
{
	if( i < 0x0A )
		i += '0';
	else
		i += 'A' - 0x0A;
	putc( i );
}	


void
put_uint8_t(
	uint8_t			i
)
{
	put_hexdigit( (i >> 4) & 0x0F );
	put_hexdigit( (i >> 0) & 0x0F );
}


void
put_uint12_t(
	uint16_t		i
)
{
	put_hexdigit( (i >> 8) & 0x0F );
	put_uint8_t(  (i >> 0) & 0xFF );
}


void
put_uint16_t(
	uint16_t		i
)
{
	put_uint8_t(  (i >> 8) & 0xFF );
	put_uint8_t(  (i >> 0) & 0xFF );
}
