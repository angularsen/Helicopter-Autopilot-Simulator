/* -*- indent-tabs-mode:T; c-basic-offset:8; tab-width:8; -*- vi: set ts=8:
 * $Id: trainer.c,v 2.0 2002/09/22 02:07:31 tramm Exp $
 *
 * (c) Trammell Hudson
 *
 * This is a simplistic interface to a serial device that translates
 * Futaba trainer port signals into joystick like data of the form:
 *
 *           Axis values .....
 *           vvvv
 *	F181 1168 11A0 1662 1662 08EE 19FE 1A44 1A36
 *      ^^^^ 
 *      Time stamp
 *
 *************
 *
 *  This file is part of the autopilot simulation package.
 *
 *  For more details:
 *
 *	http://autopilot.sourceforge.net/
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

#include <stdio.h>
#include <stdlib.h>
#include <stdint.h>
#include <unistd.h>
#include <fcntl.h>
#include <sys/types.h>
#include <sys/time.h>
#include <sys/stat.h>
#include <sys/ioctl.h>
#include <string.h>


#include "joystick.h"

#define			PPM_STRING	"$GPPPM"

static uint16_t		positions[8];


int
trainer_open(
	const char *		file_name
)
{
	int			fd;

	fd = open( file_name, O_RDONLY );
	if( fd < 0 ) {
		perror( file_name );
		return -1;
	}

	/* Find out the number of axes */
	fprintf( stderr,
		"Joystick has %d axes\n",
		8
	);

	fprintf( stderr,
		"Joystick has %d buttons\n",
		0
	);

	return fd;
}


static int
translate(
	struct js_event *	e,
	int			axis,
	const char *		line
)
{
	static const char *	last;

	if( axis == 0 )
		last = line + sizeof( PPM_STRING );

	e->type		= JS_EVENT_AXIS;
	e->number	= axis;
	e->value	= strtol( last, 0, 16 );

	e->value -= 7700 + 4400;
	e->value *= 6;

	last += 5;

	if( e->value == positions[e->number] )
		return 0;

	positions[e->number] = e->value;
	return 1;
}


static int
read_line(
	int			fd,
	char *			line,
	size_t			max_len
)
{
	size_t			len = 0;

	while( len < max_len )
	{
		char			c;
		int			rc;

		rc = read( fd, &c, 1 );

		if( rc <=0 )
			return rc;

		if( c == '\r' )
			continue;

		line[len++] = c;

		if( c == '\n' )
		{
			line[len] = '\0';
			return len;
		}
	}

	return len;
}

	
int
trainer_event(
	int			fd,
	struct js_event *	e,
	int			usecs
)
{
	static char		line[ 256 ];
	static int		axis = 0;

	int			rc;
	struct timeval		tv;
	fd_set			fds;
	FD_ZERO( &fds );
	FD_SET( fd, &fds );

	tv.tv_sec = 0;
	tv.tv_usec = usecs;

	if( axis )
	{
		int rc = translate( e, axis++, line );

		if( axis == 8 )
			axis = 0;
		return rc;
	}
		
	rc = select(
		fd+1,
		&fds,
		0,
		0,
		usecs >=0 ? &tv : 0
	);

	if( rc <= 0 )
		return rc;

	rc = read_line( fd, line, sizeof(line) );
	if( rc <= 0 )
		return rc;

	if( strncmp( line, PPM_STRING, sizeof(PPM_STRING)-1 ) != 0 )
		return 0;
	if( strncmp( line, "$GPPPM,,", 8 ) == 0 )
		return 0;

	axis = 0;
	return translate( e, axis++, line );
}


