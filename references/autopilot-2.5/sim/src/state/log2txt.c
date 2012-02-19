/* -*- indent-tabs-mode:T; c-basic-offset:8; tab-width:8; -*- vi: set ts=8:
 * $Id: log2txt.c,v 2.0 2002/09/22 02:07:32 tramm Exp $
 *
 * (c) Trammell Hudson
 *
 * Convert v2 sensor data (packed binary) to v1 text.
 *
 *************
 *
 *  This file is part of the autopilot simulation package.
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
#include <math.h>
#include <string.h>
#include "macros.h"
#include "state.h"


int
main(
	int			UNUSED( argc ),
	char **			UNUSED( argv )
)
{
	while(1)
	{
		state_t			state;

		int rc = read_state( &state, 1 );
		if( rc < 0 )
			break;
		if( rc == 0 )
			continue;

		printf(
			"%f %f %f "		/* body accelerations */
			"%f %f %f "		/* body rotational rates */
			"%f %f %f "		/* NED positions */
			"%f %f %f "		/* NED euler angles */
			"%f %f %f "		/* NED velocities */
			"%f %f "		/* Rotor mass moments */
			"\n",
	
			state.ax,
			state.ay,
			state.az,
	
			state.p,
			state.q,
			state.r,
	
			state.x,
			state.y,
			state.z,
	
			state.phi,
			state.theta,
			state.psi,
	
			state.vx,
			state.vy,
			state.vz,
	
			state.mx,
			state.my
		);


	}

	return 0;
}
