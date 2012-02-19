/* -*- indent-tabs-mode:T; c-basic-offset:8; tab-width:8; -*- vi: set ts=8:
 * $Id: joystick.c,v 2.0 2002/09/22 02:07:31 tramm Exp $
 *
 * (c) Trammell Hudson
 *
 * This is a simplistic interface to the Linux joystick devices.
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
#include <unistd.h>
#include <fcntl.h>
#include <sys/types.h>
#include <sys/time.h>
#include <sys/stat.h>
#include <sys/ioctl.h>


#include "joystick.h"

int
joydev_open(
	const char *		file_name
)
{
	int			fd;
	char			value;

	fd = open( file_name, O_RDONLY );
	if( fd < 0 ) {
		perror( file_name );
		return -1;
	}

	/* Find out the number of axes */
	if( ioctl( fd, JSIOCGAXES, &value ) == 0 )
		fprintf( stderr,
			"Joystick has %d axes\n",
			value
		);

	if( ioctl( fd, JSIOCGBUTTONS, &value ) == 0 )
		fprintf( stderr,
			"Joystick has %d buttons\n",
			value
		);

	return fd;
}


int
joydev_event(
	int			fd,
	struct js_event *	e,
	int			usecs
)
{
	int			rc;
	struct timeval		tv;
	fd_set			fds;
	FD_ZERO( &fds );
	FD_SET( fd, &fds );

	tv.tv_sec = 0;
	tv.tv_usec = usecs;

	rc = select(
		fd+1,
		&fds,
		0,
		0,
		usecs >=0 ? &tv : 0
	);

	if( rc <= 0 )
		return rc;

	return read( fd, e, sizeof(*e) );
}


