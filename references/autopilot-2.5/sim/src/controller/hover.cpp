/* -*- indent-tabs-mode:T; c-basic-offset:8; tab-width:8; -*- vi: set ts=8:
 * $Id: hover.cpp,v 2.3 2003/03/25 17:25:58 tramm Exp $
 *
 * (c) Aaron Kahn
 * (c) Trammell Hudson
 *
 * This uses the controller.o module to control the helicopter based on
 * points read from a file.
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

#include "macros.h"
#include "timer.h"
#include <getoptions/getoptions.h>
#include <state/Server.h>
#include <controller/Guidance.h>
#include <mat/Conversions.h>

using namespace libcontroller;
using namespace libstate;
using namespace libmat;
using namespace std;

static double const	close_enough = 0.5;
static int		verbose;

static int
get_position(
	FILE *			infile,
	double *		pos
)
{
	int			rc;
	char			buf[128];
	double			old_pos[5];

	for( int i=0 ; i < 4 ; i++ )
		old_pos[i] = pos[i];

	// Get one line.  Buffer overflow is possible!
	memset( buf, 0, sizeof(buf) );

	rc = fscanf( infile,
		"%[^\n]\n",
		buf
	);

	if( rc <= 0 )
		return -1;

	rc = sscanf( buf,
		"%lf %lf %lf %lf %lf",
		&pos[0],		// North
		&pos[1],		// East
		&pos[2],		// Down
		&pos[3],		// Heading (in degrees)
		&pos[4]			// Steps to use
	);

	if( rc <= 0 )
		return -1;

	// Auto heading
	if( rc <= 3 )
	{
		double		dx = pos[0] - old_pos[0];
		double		dy = pos[1] - old_pos[1];

		pos[3] = atan2( dy, dx );

		fprintf( stderr,
			"Autoheading: dx=%f dy=%f -> %f\n",
			dx,
			dy,
			pos[3]
		);
	} else

	// Convert user specified heading from degrees to radians
	{
		pos[3] *= C_DEG2RAD;
	}

	// Unlimited steps in unspecified or negative
	if( rc <= 4 )
		pos[4] = -1;

	fprintf( stderr,
		"\nNavigating to %+5.2f %+5.2f %+5.2f heading %+5.2f ",
		pos[0],
		pos[1],
		pos[2],
		pos[3]
	);

	if( pos[4] < 0 )
		fprintf( stderr, "until close enough..\n" );
	else
		fprintf( stderr, "using %d steps\n", int(pos[4]) );


	return 0;
}



static FILE *
read_flight_path(
	const char *		file_name,
	double *		position
)
{
	if( !file_name )
	{
		fprintf( stderr,
			"No file specified: Using default hovering position\n"
		);

		return NULL;
	}

	FILE *			flight_path;

	flight_path = fopen( file_name, "r" );
	if( !flight_path )
	{
		perror( file_name );
		return NULL;
	}

	if( get_position( flight_path, position ) < 0 )
	{
		fprintf( stderr,
			"No points in '%s': Using default\n",
			file_name
		);

		return NULL;
	}

	return flight_path;
}


static int
help( void )
{
	cerr <<
"Usage: hover [options] [waypoints]\n"
"\n"
"	-h | --help			This help\n"
"	-v | --verbose			Increase verbosity\n"
"	-V | --version			Version\n"
"	-s | --server hostname		State server\n"
"	-p | --port port		State port\n"
	<< endl;

	return -10;
}

static int
version( void )
{
	cerr << "$Id: hover.cpp,v 2.3 2003/03/25 17:25:58 tramm Exp $" << endl;
	return -10;
}


int
main(
	int			argc,
	char **			argv
)
{
	const char *		program = argv[0];
	const char *		server_host = "127.0.0.1";
	int			server_port = 2002;
	int			handle_yaw	= 1;
	int			handle_roll	= 1;
	int			handle_pitch	= 1;
	int			handle_coll	= 1;

	double			model_dt = 0.02;


	int rc = getoptions( &argc, &argv,
		"h|?|help&",		help,
		"V|version",		version,
		"v|verbose+",		&verbose,
		"s|server=s",		&server_host,
		"p|port=i",		&server_port,
		"t|dt=f",		&model_dt,
		"y|yaw!",		&handle_yaw,
		"r|roll!",		&handle_roll,
		"P|pitch!",		&handle_pitch,
		"c|coll!",		&handle_coll,
		0
	);

	if( rc == -10 )
		return EXIT_FAILURE;
	if( rc < 0 )
		return help();

	state_t			state;
	Server			server( server_host, server_port );

	server.handle( AHRS_STATE, Server::process_ahrs, (void*) &state );


	double			position[5] = {
		0, 0, -5, 0, -1
	};

	fprintf( stderr,
		"%s: Using %f for dt\n",
		program,
		model_dt
	);


	Guidance		co( model_dt );


	/*
	 *  Read in our flight path from the argument, if specified.
	 * Go ahead and tell the controller where we want to go first.
	 */
	FILE *flight_path = read_flight_path( *argv, position );
	co.flyto( position );
	co.heading = position[3];

	/*
	 *  Read state, run the controller, and get a new waypoint
	 * once we reach the current one.
	 */
	int			steps = 0;

	while( 1 )
	{

		int type = server.get_packet();
		if( type < 0 )
		{
			cout << "Received shutdown command" << endl;
			return -1;
		}

		if( type != AHRS_STATE )
			continue;

		stopwatch_t		compute_time;
		start( &compute_time );
		
		// Throw away the first twenty states, to let things
		// settle down in the simulator
		if( steps++ < 20 && 0 )
			continue;

		const Vector<3>	pos_NED( state.x, state.y, state.z );
		const Vector<3>	vel_NED( state.vx, state.vy, state.vz );
		const Vector<3>	theta( state.phi, state.theta, state.psi );
		const Vector<3>	pqr( state.p, state.q, state.r );

		const Vector<4> outputs( co.step(
			pos_NED,
			vel_NED,
			theta,
			pqr
		) );

		if( handle_coll )
			server.send_parameter( SERVO_COLL, outputs[0] );
		if( handle_roll )
			server.send_parameter( SERVO_ROLL, outputs[1] );
		if( handle_pitch )
			server.send_parameter( SERVO_PITCH, outputs[2] );
		if( handle_yaw )
			server.send_parameter( SERVO_YAW, outputs[3] );

		double dist = co.dist( pos_NED, theta );


		fprintf( stderr,
			"%4ld "
			"Dist: %3.2f "
			"(% 3.2f,% 3.2f,% 3.2f,% 3.2f) -> "
			"(% 3.2f,% 3.2f,% 3.2f,% 3.2f)\r",
			stop( &compute_time ),
			dist,
			state.x,
			state.y,
			state.z,
			state.psi,
			position[0],
			position[1],
			position[2],
			position[3]
		);

		fflush( stdout );

		if( !flight_path )
			continue;

		if( position[4] > 0 )
		{
			if( steps < position[4] )
				continue;
		} else {
			if( dist > close_enough )
				continue;
		}

		fprintf( stderr,
			"\nUsed %d steps\n",
			steps
		);

		steps = 0;

		if( get_position( flight_path, position ) < 0 )
		{
			fprintf( stderr,
				"%s: No more points\n",
				*argv
			);

			break;
		}

		co.flyto( position );
		co.heading = position[3];
	}


	// Shutdown the simulator
	printf( "quit\n" );

	return 0;
}
