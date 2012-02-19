/* -*- indent-tabs-mode:T; c-basic-offset:8; tab-width:8; -*- vi: set ts=8:
 * $Id: state.h,v 2.3 2003/03/08 05:27:26 tramm Exp $
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

#ifndef _STATE_H_
#define _STATE_H_

#include "macros.h"
#ifndef WIN32
#include <stdint.h>
#endif
#include <sys/time.h>
#include <sys/types.h>

#define STATE_PROTOCOL		2

struct sockaddr_in;

#ifdef __cplusplus
namespace libstate
{
#endif


typedef struct {
	/* Body frame linear accelerations */
	double		ax;
	double		ay;
	double		az;

	/* Body frame rotational rates */
	double		p;
	double		r;
	double		q;

	/* Position relative to the ground */
	double		x;
	double		y;
	double		z;

	/* Euler angles relative to the ground */
	double		phi;
	double		theta;
	double		psi;

	/* Velocity over the ground */
	double		vx;
	double		vy;
	double		vz;

	/* Moments on the rotor mass */
	double		mx;
	double		my;

	/* End of line character to force flush with cat */
	char		end_of_line;
} state_t;

#ifdef __cplusplus
extern "C" {
#endif




/*
 *  If hostname == 0, the environment variable "SIMSERVER" will
 * be used, or "localhost" if no variable is set.
 *
 * If port == 0, the variable "SIMPORT" will be used, or 2002 if
 * no variable is set.
 */
extern int
connect_state(
	const char *		hostname,
	int			port
);


extern int
host_lookup(
	struct sockaddr_in *	addr,
	const char *		hostname,
	int			port
);


extern int
set_parameter(
	int			type,
	double			value
);

extern int
send_command(
	int			type
);


extern int
read_state(
	state_t *		state,
	int			forever
);

extern double
state_dt( void );
	
extern void
set_state_dt(
	double			out_dt
);


#ifdef __cplusplus
}
}
#endif

#endif
