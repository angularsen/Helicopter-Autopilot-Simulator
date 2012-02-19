/* -*- indent-tabs-mode:T; c-basic-offset:8; tab-width:8; -*- vi: set ts=8:
 * $Id: state.c,v 2.1 2003/02/03 20:34:23 dennisda Exp $
 *
 * (c) Aaron Kahn
 * (c) Trammell Hudson
 *
 * State communication to and from the simulator.  It encapsualtes
 * the data that the simulator core math model outputs so that
 * each modular piece does not have to understand the protocol.
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
#ifndef WIN32
#include <sys/types.h>
#endif
#include <sys/time.h>
#include <string.h>

#include <state/state.h>


int
write_all(
	int			fd,
	const char *		buf,
	size_t			len
)
{
	while( len > 0 )
	{
		ssize_t w = write( fd, buf, len );
		if( w <= 0 )
			return -1;
		len -= w;
		buf += w;
	}

	return 0;
}


int
swrite_state(
	char *			buf,
	int			max_len,
	const state_t *		state
)
{
#if STATE_PROTOCOL == 2
	int			len = sizeof(*state);
	if( max_len < len )
		return -1;

	((state_t*)state)->end_of_line = '\n';
	memcpy( buf, state, len );
#endif

#if STATE_PROTOCOL == 1
	int len = snprintf(
		buf, max_len,
		"%f %f %f "		/* body accelerations */
		"%f %f %f "		/* body rotational rates */
		"%f %f %f "		/* NED positions */
		"%f %f %f "		/* NED euler angles */
		"%f %f %f "		/* NED velocities */
		"%f %f "		/* Rotor mass moments */
		"\n",

		state->ax,
		state->ay,
		state->az,

		state->p,
		state->q,
		state->r,

		state->x,
		state->y,
		state->z,

		state->phi,
		state->theta,
		state->psi,

		state->vx,
		state->vy,
		state->vz,

		state->mx,
		state->my
	);

	if( len == max_len )
		return -1;
#endif

	return len;
}


/**
 *  Set the DT in the environment
 */
void
set_state_dt(
	double			out_dt
)
{
	static char		dt_buf[32];

	sprintf( dt_buf,
		"HELI_DT=%1.5f",
		out_dt
	);

	putenv( dt_buf );
}


/*
 * Extract DT from environment, if it exists
 */
double
state_dt( void )
{
	const char *		dt_env = getenv( "HELI_DT" );

	if( !dt_env )
		return 0.025;
	return atof( dt_env );
}

