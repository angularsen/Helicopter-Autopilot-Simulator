/* -*- indent-tabs-mode:T; c-basic-offset:8; tab-width:8; -*- vi: set ts=8:
 * $Id: heli-sim.cpp,v 2.1 2003/03/08 05:16:35 tramm Exp $
 *
 * (c) Aaron Kahn
 * (c) Trammell Hudson
 *
 * This code is to be used as a test for the 
 * hils testing in the UAV lab.  It is a simple
 * helicopter picture that inputs data into a 
 * simple structure.  This structure consists of
 * the roll, pitch, yaw, X, Y, Z coordinates
 * of the aircraft.
 *
 **************
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
#include <ctype.h>
#include <unistd.h>
#include <sys/time.h>
#include <sys/types.h>

#include "Heli.h"
#include <state/commands.h>
#include <state/state.h>
#include <state/Server.h>

using namespace std;
using namespace sim;
using namespace libstate;


static double		heli_controls[4];
static const long	out_dt		= 20000;		// usec
static const long	dt		=  2000;		// usec

// Determines if the simulator pauses between time steps
// to allow the system to run in real time.
static const bool	no_wait		= 0;

static Heli		xcell;



#if 0
static int
process_line(
	const std::string &	line
)
{
	char			axis[32];
	double			value;

	if( line == "reset\n" )
	{
		xcell.reset();
		return 0;
	}

	if( line == "quit\n" )
	{
		cerr << "Shutting down" << endl;
		exit( EXIT_SUCCESS );
	}

	if( line == "close\n" )
		return -1;

	const char *		cstr = line.c_str();
	if( sscanf( cstr, "%s %lf", axis, &value ) != 2 )
	{
		cerr << "Unable to parse: " << line << endl;
		return 0;
	}

	if( strcmp( axis, "coll" ) == 0 )
		heli_controls[2] = value;
	else
	if( strcmp( axis, "roll" ) == 0 )
		heli_controls[1] = value;
	else
	if( strcmp( axis, "pitch" ) == 0 )
		heli_controls[0] = value;
	else
	if( strcmp( axis, "yaw" ) == 0 )
		heli_controls[3] = value;
	else
		cerr << "Unknown axis: " << axis << endl;

	return 0;
}
#endif


static void
write_to_clients(
	Server *		server,
	const Forces *		cg
)
{
	state_t			state;

	state.ax	= cg->F[0];
	state.ay	= cg->F[1];
	state.az	= cg->F[2];

	state.p		= cg->pqr[0];
	state.q		= cg->pqr[1];
	state.r		= cg->pqr[2];

	state.x		= cg->NED[0];
	state.y		= cg->NED[1];
	state.z		= cg->NED[2];

	state.phi	= cg->THETA[0];
	state.theta	= cg->THETA[1];
	state.psi	= cg->THETA[2];

	state.vx	= cg->V[0];
	state.vy	= cg->V[1];
	state.vz	= cg->V[2];

	state.mx	= xcell.m.b1;
	state.my	= xcell.m.a1;
	
	server->send_packet(
		AHRS_STATE,
		(void*) &state,
		sizeof(state)
	);
}



/*
 *  Helpers to track time used in different tasks
 */
static struct timeval		start_time;

static void
start( void )
{
	gettimeofday( &start_time, 0 );
}

static unsigned long
stop( void )
{
	struct timeval		stop_time;

	gettimeofday( &stop_time, 0 );

	stop_time.tv_sec -= start_time.tv_sec;

	while( stop_time.tv_sec > 0 )
	{
		stop_time.tv_usec += 1000000;
		stop_time.tv_sec--;
	}

	stop_time.tv_usec -= start_time.tv_usec;

	return stop_time.tv_usec;
}


static void
servo_set(
	void *			priv,
	const host_t *		UNUSED( src ),
	int			type,
	const struct timeval *	when,
	const void *		data,
	size_t			len
)
{
	static struct timeval	last;

	if( len != sizeof( double ) )
	{
		cerr << "Invalid servo packet of type " << type
			<< ".  Expected " << sizeof(double)
			<< " received " << len
			<< " bytes"
			<< endl;
		return;
	}

	if( timercmp( &last, when, > ) )
	{
		cerr << "Old servo packet of type " << type << endl;
		return;
	}

	last = *when;

	double *		servo = (double*) priv;
	double *		value = (double*) data;

	*servo = *value;
}


static void
sim_reset(
	void *			priv,
	const host_t *		UNUSED( src ),
	int			UNUSED( type ),
	const struct timeval *	UNUSED( when ),
	const void *		UNUSED( data ),
	size_t			UNUSED( len )
)
{
	Heli *			heli = (Heli*) priv;
	heli->reset();
}


static void
sim_quit(
	void *			priv,
	const host_t *		UNUSED( src ),
	int			UNUSED( type ),
	const struct timeval *	UNUSED( when ),
	const void *		UNUSED( data ),
	size_t			UNUSED( len )
)
{
	Server *		server = (Server*) priv;

	cout << "Sending close packet" << endl;
	server->send_packet(
		SIM_QUIT,
		0,
		0
	);

	cout << "Shutting down simulator" << endl;
	exit( 0 );
}


static void
sim_close(
	void *			priv,
	const host_t *		src,
	int			UNUSED( type ),
	const struct timeval *	UNUSED( when ),
	const void *		UNUSED( data ),
	size_t			UNUSED( len )
)
{
	Server *		server = (Server*) priv;

	server->del_client( src );
}


int
main(
	int			UNUSED( argc ),
	char **			UNUSED( argv )
)
{
	Server			server( 2002 );

	// Install our handlers for different commands
	server.handle( SERVO_PITCH,	servo_set, (void*) &heli_controls[0] );
	server.handle( SERVO_ROLL,	servo_set, (void*) &heli_controls[1] );
	server.handle( SERVO_COLL,	servo_set, (void*) &heli_controls[2] );
	server.handle( SERVO_YAW,	servo_set, (void*) &heli_controls[3] );
	server.handle( SIM_QUIT,	sim_quit,  (void*) &server );
	server.handle( SIM_RESET,	sim_reset, (void*) &xcell );
	server.handle( COMMAND_CLOSE,	sim_close, (void*) &server );

	/* Set initial conditions for the heli */

	sixdof_fe_inputs_def *	sixdof = &xcell.sixdofIn;

	sixdof->hold_u = 0;		// North
	sixdof->hold_v = 0;		// East
	sixdof->hold_w = 0;		// Down
	sixdof->hold_p = 0;		// Roll
	sixdof->hold_q = 0;		// Pitch
	sixdof->hold_r = 0;		// Yaw


	/* Setup the environment for our clients */
	set_state_dt( double(out_dt) / 1000000 );

	/* Parse the arguments and setup the clients */

	const int steps_per_dt	= out_dt / dt;

	printf( "dt=%lu usec display_dt=%lu usec -> %d\n",
		dt,
		out_dt,
		steps_per_dt
	);

	/* Give the children a short while to catch up */
	usleep( 100000 );

	/* Run the simulation */
	int steps = 0;

	while( 1 ) // steps < 1024 )
	{
		steps++;

		long		time_used;

		
		start();
		for( int step = 0 ; step < steps_per_dt ; step++ )
			xcell.step(
				double(dt) / 1000000,
				heli_controls
			);

		time_used = stop();

		long extra = out_dt - time_used;

		if( extra < 0 )
		{
			fprintf( stderr,
				"Overran quantum by %ld (used %ld usec)\n",
				-extra / steps_per_dt,
				time_used / steps_per_dt
			);

			extra = 0;
		}

		write_to_clients( &server, &xcell.cg  );

		start();
		while( (long) stop() < extra )
			if( server.poll() )
				server.get_packet();

	}

	return 0;
}
