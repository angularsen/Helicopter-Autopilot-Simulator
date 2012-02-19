/* -*- indent-tabs-mode:T; c-basic-offset:8; tab-width:8; -*- vi: set ts=8:
 * $Id: rpv.cpp,v 2.1 2003/03/08 05:10:23 tramm Exp $
 *
 * (c) Aaron Kahn
 * (c) Trammell Hudson
 *
 * Translate user commands into hovering waypoints.
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

#include <cstdio>
#include <cstdlib>
#include <cstring>
#include <cmath>
#include <unistd.h>

#include <state/state.h>
#include <state/commands.h>
#include "macros.h"
#include "Attitude.h"
#include "joystick.h"

#include <mat/Conversions.h>

using namespace libcontroller;
using namespace libstate;

static state_t			state;

static int
controller_state(
	Attitude *		co
)
{
	int rc = read_state( &state, 0 );
	if( rc < 0 )
		return -1;
	if( rc == 0 )
		return 0;

	const Vector<3>	theta( state.phi, state.theta, state.psi );
	const Vector<3>	pqr( state.p, state.q, state.r );

	Vector<3> servos = co->step( theta, pqr );

	set_parameter( SERVO_ROLL,  servos[0] );
	set_parameter( SERVO_PITCH, servos[1] );
	set_parameter( SERVO_YAW,   servos[2] );

	return 0;
}

static int
handle_button(
	Attitude *		co,
	int			button,
	int			UNUSED( value )
)
{
	switch( button )
	{
	case 5:
		fprintf( stderr, "\nrpv: Resetting\n" );
		send_command( SIM_RESET );
		co->reset();
		break;

	default:
		break;
	}

	return 0;
}


static double
scale(
	double			value,
	double			max,		// Original frame
	double			min,
	double			high,		// New frame
	double			low
)
{
	return (value - min) * (high - low) / (max - min) + low;
}


static int
handle_axes(
	Attitude *		co,
	int			axis,
	int			value
)
{
	switch( axis )
	{
	case 0:
		// Roll
		co->attitude[0] = C_DEG2RAD * scale(
			double(value), -22044, 20767, -10.00, 10.00
		);
		break;

	case 1:
		// Pitch
		co->attitude[1] = C_DEG2RAD * scale(
			double(value), -23066, 21788, -4.0, 5.0
		);
		break;

	case 2:
		// Yaw => Heading
		co->attitude[2] = C_DEG2RAD * scale(
			double(value), -19594, 16890, -180.0, 180.0
		);
		break;

	case 3:
	{
		// collective slider is backwards
		const double		coll	= C_DEG2RAD * scale(
			double(value), -25674.0, 17228.0, 10.5, 8.5
		);

		set_parameter( SERVO_COLL, coll );

		break;
	}

	default:
		// Who knows...
		break;
	}

	return 0;
}


static int
controller_joystick(
	Attitude *		co,
	int			fd
)
{
	struct js_event		e;

	if( joydev_event( fd, &e, 0 ) < 0 )
		return -1;

	if( e.type & JS_EVENT_BUTTON )
		return handle_button( co, e.number, e.value );
	
	if( e.type & JS_EVENT_AXIS )
		return handle_axes( co, e.number, e.value );

	return 0;
}
		

int
main(
	int			UNUSED( argc ),
	char **			argv
)
{
	int			joy_fd = joydev_open( "/dev/js0" );
	if( joy_fd < 0 )
	{
		perror( "/dev/js0" );
		exit( EXIT_FAILURE );
	}


	double			model_dt = state_dt();
	fprintf( stderr,
		"%s: Using %f for dt\n",
		argv[0],
		model_dt
	);


	int			state_fd = connect_state( "localhost", 2002 );

	Attitude		co( model_dt );

	while(1)
	{
		fd_set			fds;
		FD_ZERO( &fds );
		if( state_fd >= 0 )
			FD_SET( state_fd, &fds );
		if( joy_fd >= 0 )
			FD_SET( joy_fd, &fds );

		int rc = select(
			128,
			&fds,
			0,
			0,
			0
		);

		if( rc < 0 )
		{
			perror( "select" );
			break;
		}

		if( rc == 0 )
			continue;

		if( state_fd >= 0
		&&  FD_ISSET( state_fd, &fds )
		&&  controller_state( &co ) < 0 )
			break;

		if( joy_fd >= 0
		&&  FD_ISSET( joy_fd, &fds )
		&&  controller_joystick( &co, joy_fd ) < 0 )
			break;
	
		fprintf( stderr,
			"(%3.4f,%3.4f,%3.4f) -> "
			"(%3.4f,%3.4f,%3.4f)\r",
			state.phi,
			state.theta,
			state.psi,
			co.attitude[0],
			co.attitude[1],
			co.attitude[2]
		);

	}


	// Shutdown the simulator
	send_command( SIM_QUIT );

	return 0;
}
