/* -*- indent-tabs-mode:T; c-basic-offset:8; tab-width:8; -*- vi: set ts=8:
 * $Id: main.cpp,v 1.21 2003/03/12 18:15:56 tramm Exp $
 *
 * (c) Trammell Hudson
 *
 * Main routines for the fltk ground station code.  Contact the
 * state server and display the results in the gui.
 *
 *************
 *
 *  This file is part of the autopilot groundstation package.
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


#include <iostream>
#include <cstdio>
#include <cstdlib>
#include <cmath>
#include <unistd.h>
#include "macros.h"

#include "Ground.h"
#include "Joystick.h"

#include <getoptions/getoptions.h>
#include "state/commands.h"
#include "state/Server.h"
#include "mat/Conversions.h"
#include "mat/Constants.h"


using namespace std;
using namespace libstate;
using namespace util;


UserInterface *			gui = 0;
const char *			server_hostname = "127.0.0.1";
int				server_port	= 2002;
int				udp_fd		= -1;
int				verbose		= 0;
static Server *			server;
static int			packets		= 0;



static void
update_angles(
	const double		angles[]
)
{
	static int		draw;

	double			roll	= angles[0] * C_RAD2DEG;
	double			pitch	= angles[1] * C_RAD2DEG;
	double			yaw	= angles[2] * C_RAD2DEG;

	// Update the state display
	gui->angle_phi->value( roll );
	gui->angle_theta->value( pitch );
	gui->angle_psi->value( yaw );

	// Update the artificial horizon object
	gui->horizon->roll	= roll;
	gui->horizon->pitch	= pitch;
	gui->horizon->yaw	= yaw;

	if( draw )
	{
		gui->horizon->redraw();
		draw = 0;
	} else
		draw = 1;
}


#if 0
static void
ahrs_message(
	const char *		data
)
{
	/* AHRS data */
	double		angles[3];
	double		trace;

	sscanf( data, "%lf,%lf,%lf,%lf",
		&angles[0],
		&angles[1],
		&angles[2],
		&trace
	);


	gui->trace->value( trace );
	gui->trace_dial->value( trace );
	gui->packets->value( packets++ );

	update_angles( angles );
}


static void
send_joystick()
{
	int16_t		values[4];

	values[0] = ntohs( joy_roll );
	values[1] = ntohs( joy_pitch );
	values[2] = ntohs( joy_yaw );
	values[3] = ntohs( joy_throttle );

	server->send_packet(
		JOYSTICK,
		(void*) values,
		sizeof( values )
	);
}


static void
ppm_message(
	const char *		data
)
{
	static char		message[3];

	/* PPM data */
#ifdef WIN32
	const unsigned short int * pulses = (const unsigned short int *) data;
#endif
#ifndef WIN32
	const uint16_t * pulses = (const uint16_t *) data;
#endif

	int			mode	= ntohs( pulses[6] );
	int			manual	= ntohs( pulses[5] );

	gui->ppm_roll->value(		ntohs( pulses[0] ) );
	gui->ppm_pitch->value(		ntohs( pulses[1] ) );
	gui->ppm_yaw->value(		ntohs( pulses[2] ) );
	gui->ppm_coll->value(		ntohs( pulses[3] ) );
	gui->ppm_throttle->value(	ntohs( pulses[4] ) );
	gui->ppm_manual->value(		manual );
	gui->ppm_mode->value(		mode );
	gui->ppm_extra->value(		ntohs( pulses[7] ) );

	message[0] = manual ? 'M' : 'A';
	message[1] = mode == 0 ? '0'
		: mode == 1 ? '1'
		: mode == 2 ? '2'
		: '?';
	message[3] = ' ';

	gui->horizon->message	= message;

	/* Should we send our joystick commands now? */
	send_joystick();
}
#endif


/*
 * Handler called when there is AHRS state data available
 */
static void
ahrs_state(
	void *			UNUSED( priv ),
	const host_t *		src,
	int			UNUSED( type ),
	const struct timeval *	UNUSED( when ),
	const void *		data,
	size_t			len
)
{
	const state_t *		state	= (state_t*) data;
	
	if( len != sizeof( *state ) )
	{
		cerr << "Invalid AHRS packet from "
			<< src
			<< ".  Expected "
			<< sizeof(*state)
			<< " bytes, but received "
			<< len
			<< endl;
		return;
	}

	double 			a	= 6378137.0;
	double			lat;
	double			lon;
	double			alt;
	
	// starting position (474 Wando Park Blvd)
	double			lat0	= 32.8315392;
	double			lon0	= -79.8542328;
	double			alt0	= 3;
	
	// Increment our packet count
	gui->packets->value( packets++ );

	// Copy the values out of the state object
	gui->accel_x->value( state->ax );
	gui->accel_y->value( state->ay );
	gui->accel_z->value( state->az );

	gui->pos_x->value( state->x );
	gui->pos_y->value( state->y );
	gui->pos_z->value( state->z );

	gui->vel_x->value( state->vx );
	gui->vel_y->value( state->vy );
	gui->vel_z->value( state->vz );

	gui->rates_p->value( state->p );
	gui->rates_q->value( state->q );
	gui->rates_r->value( state->r );

	// Aaron's method from email
	// TODO: move to Nav.cpp as Vector<3> NED2lla(Vector<3>)
	//
	// X=N points towards the north pole
	// Y=E towards the east
	// Z=D towards the earth center

	// convert starting position to radians
	lat0	= lat0 * C_DEG2RAD;
	lon0	= lon0 * C_DEG2RAD;

	lat	= state->x / (a - state->z) + lat0;
	lon	= state->y / ((a - state->z) * cos(lat)) + lon0;
	alt	= alt0 - state->z;
	
	gui->latitude->value( lat * C_RAD2DEG );
	gui->longitude->value( lon * C_RAD2DEG );
	gui->altitude_llh->value( alt );

	// Setup the artificial horizon object
	Horizon *		ai = gui->horizon;

	ai->altitude	= -state->z;
	ai->speed	= sqrt( sqr(state->vx) + sqr(state->vy) + sqr(state->vz) );
	ai->message	= "MAN";

	double angles[3] = { state->phi, state->theta, state->psi };


	update_angles( angles );
}


/**
 *  Called by main at startup and by the reconnect button
 */
void
reconnect_server()
{
	cout << "Contacting "
		<< server_hostname
		<< ":"
		<< server_port
		<< endl;

	server->connect( server_hostname, server_port );
}


static void
set_connected_led(
	void *			UNUSED( priv ),
	const host_t *		UNUSED( src ),
	int			UNUSED( type ),
	const struct timeval *	UNUSED( when ),
	const void *		UNUSED( data ),
	size_t			UNUSED( len )
)
{
	gui->connected->value( 1 );
}



void
init_gui(
	void
)
{
	gui->guiJoyID->value(0);
	gui->rollAxes->value(0);
	gui->pitchAxes->value(1);
	gui->collAxes->value(2);
	gui->yawAxes->value(3);
}


static int
help( void )
{
	cerr <<
"Usage: ground [options]\n"
"\n"
"	-h | --help		This help\n"
"	-s | --server host	Hostname for the server\n"
"	-p | --port port	Server port\n"
"	-v | --verbose		Increase verbosity\n"
"\n";

	return -10;
}


static int
version( void )
{
	cout << "$Id: main.cpp,v 1.21 2003/03/12 18:15:56 tramm Exp $" << endl;
	return -10;
}


int
main(
	int			argc,
	char **			argv
)
{
	int rc = getoptions( &argc, &argv,
		"h|?|help&",		help,
		"s|server=s",		&server_hostname,
		"p|port=i",		&server_port,
		"v|verbose+",		&verbose,
		"V|version&",		version,
		0
	);

	if( rc == -10 )
		return 0;
	if( rc < 0 )
		return help();

	server = new Server;

	server->handle( AHRS_STATE, ahrs_state, (void*) server );
	server->handle( COMMAND_ACK, set_connected_led, 0 );

	Fl::add_fd(
		server->sock,
		FL_READ,
		Server::update,
		(void*) server
	);


	Fl::gl_visual( FL_RGB );

	gui = new UserInterface();
	gui->make_window()->show();

	init_gui();
	reconnect_server();
	reconnect_joy();

	return Fl::run();
}
