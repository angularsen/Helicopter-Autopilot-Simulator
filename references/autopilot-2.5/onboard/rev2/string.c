/* -*- indent-tabs-mode:T; c-basic-offset:8; tab-width:8; -*- vi: set ts=8:
 * $Id: string.c,v 2.5 2003/03/22 18:11:14 tramm Exp $
 *
 * Basic string and number output functions.  These should be generalized
 * to allow for sharing with the LCD module.
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

#include <avr/io.h>

#include "string.h"
#include "uart.h"


/*************************************************************************
 *
 * String and number output functions
 *
 */
void
_puts(
	PGM_P			s
)
{
	char			c;
	while( (c = PRG_RDB( s++ )) )
		putc( c );
}


void
put_uint8_t(
	uint8_t			i
)
{
	putc( hexdigit( (i >> 4) & 0x0F ) );
	putc( hexdigit( (i >> 0) & 0x0F ) );
}


void
put_uint12_t(
	uint16_t		i
)
{
	putc( hexdigit( (i >> 8) & 0x0F ) );
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


char float_buf[8];

void
render_float(
	const float *		f
)
{
	float			v = *f;

	if( v < 0 )
	{
		v = -v;
		float_buf[0] = '-';
	} else {
		float_buf[0] = '+';
	}

	float_buf[1] = ((uint8_t) v) % 10 + '0';
	float_buf[2] = '.';

	v *= 10;
	float_buf[3] = ((uint8_t) v) % 10 + '0';

	v *= 10;
	float_buf[4] = ((uint8_t) v) % 10 + '0';

	v *= 10;
	float_buf[5] = ((uint8_t) v) % 10 + '0';

	v *= 10;
	float_buf[6] = ((uint8_t) v) % 10 + '0';

	float_buf[7] = 0;
}


void
put_float(
	const float *		f
)
{
	uint8_t			i;

	render_float( f );

	for( i=0 ; i<7 ; i++ )
		putc( float_buf[i] );
}


