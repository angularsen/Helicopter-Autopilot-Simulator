/* -*- indent-tabs-mode:T; c-basic-offset:8; tab-width:8; -*- vi: set ts=8:
 * $Id: heli-stick.c,v 2.0 2002/09/22 02:07:31 tramm Exp $
 *
 * (c) Trammell Hudson
 *
 * This code integrates with the heli-sim program to translate
 * joystick events into helicopter swashplate commands.
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

#include <stdio.h>
#include <stdlib.h>
#include <unistd.h>
#include "joystick.h"
#include "trainer.h"

#include <math.h>

#include "macros.h"
#include <state/state.h>

static int		(*event)(
	int			fd,
	struct js_event *	e,
	int			usec
);


/**
 *  Define TRAINER if you want to use the serial interface for the
 * trainer device.
 */
#define TRAINER			1

static const int	trainer		= TRAINER;
static const char *	prog		= "heli-stick";
static const char *	trainer_dev	= "/dev/ttyS0";
static const char *	joy_dev		= "/dev/js0";
static const double	pi		= 3.141519;

#define			MAX_AXES	8
#define			MAX_BUTTONS	16

typedef struct {
	double			min_deflection;
	double			max_deflection;
	double			trim;
} trim_t;


/**
 *  Fill this array in with the final output of calibrate
 */
static joy_axis_t limits[8] =
{
#if TRAINER
	{ "roll",	-32768, 32767, 0 },
        { "pitch",	-32768, 32767, 0 },
        { "", 0, 0, 0 },
        { "yaw",	-32768, 32767, 0 },
        { "", 0, 0, 0 },
        { "coll",	-32768, 32767, 0 },
#else
	{ "roll", -22044, 20767, 0 },
	{ "pitch", -23066, 21788, 0 },
	{ "yaw", -19594, 16890, 0 },
	{ "coll", -25674, 17228, 0 },
#endif
};

/**
 *  These will have to be re-ordered if the joystick changes.
 * Bummer.
 */
static trim_t trims[ MAX_AXES ] =
{
#if TRAINER
	/*   Min     Max    Trim */
	{   4.00,  -4.00,  0.00 },		/* roll */
	{   4.00,  -4.00,  0.00 },		/* pitch */
	{ 0, 0, 0 },
	{  30.00, -30.00,  9.00 },		/* yaw */
	{ 0, 0, 0 },
	{  25.00,   0.00,  0.00 },		/* coll */
#else
	{  -4.00,   4.00,  0.58 },              /* roll */
	{   4.00,  -4.00, -0.18 },              /* pitch */
	{ -10.00,  10.00,  6.00 },              /* yaw */
	{  10.80,   6.50,  0.00 },              /* coll */
#endif

	/* Rest are all zero */
};


/**
 *  Scales a joystick reading into degrees of swashplate or
 * tail rotor travel.
 */
static void
handle_axis(
	int			axis,
	int			value
)
{
	joy_axis_t *		values;
	trim_t *		trim;
	double			scaled = value;

	if( axis < 0 || MAX_AXES < axis )
	{
		fprintf( stderr,
			"%s: Invalid axis: %d\n",
			prog,
			axis
		);
		return;
	}

	values	= &limits[axis];
	trim	= &trims[axis];

	if( !values->name || !values->name[0] )
	{
		if( 0 )
			fprintf( stderr,
				"%s: Unhandled axis: %d\n",
				prog,
				axis
			);
		return;
	}

	/* Convert the reading into a value from -1.0 to 1.0 */
	scaled -= values->min;
	scaled /= (values->max - values->min);

	/* Convert the unit value to degrees */
	scaled *= trim->max_deflection - trim->min_deflection;
	scaled += trim->min_deflection;
	scaled += trim->trim;

	/* Convert degrees to radians */
	scaled *= pi / 180.0;

	values->last = scaled;
}


/**
 *  Button handling currently only resets the simulator when the
 * trigger is pulled.
 */
static void
handle_button(
	int			button,
	int			value
)
{
	if( button == 0 )
	{
		if( value == 1 )
			printf( "reset\n" );
		return;
	}

	fprintf( stderr,
		"%s: Unhandled button: %d\n",
		prog,
		button
	);
}
	
static void
handle_joystick(
	int			fd
)
{
	struct js_event		e;
	int			rc;

	rc = event( fd, &e, -1 );
	if( rc < 0 )
	{
		fprintf( stderr, "joystick::event failed\n" );
		exit(-1);
	}

	if( rc == 0 )
		return;

	if( e.type & JS_EVENT_AXIS )
		handle_axis( e.number, e.value );
	else

	if( e.type & JS_EVENT_BUTTON )
		handle_button( e.number, e.value );
	else

	fprintf( stderr,
		"%s: Unknown event type %02x\n",
		prog,
		e.type
	);
}


/*
 *  Read the state from the user
 */
static void
handle_state(
	int			UNUSED( fd )
)
{
	state_t			state;
	int			axis;
	int			rc;

	rc = read_state( &state, 0 );
	if( rc < 0 )
	{
		fprintf( stderr, "state::read_state failed\n" );
		exit(-1);
	}

	if( rc == 0 )
		return;

	/* Send our commands */
	for( axis=0 ; axis<8 ; axis++ )
	{
		joy_axis_t *values	= &limits[axis];

		if( !values->name || !values->name[0] )
			continue;

		set_parameter( values->name, values->last );
	}
}


/**
 *  Open the device and loop forever reading events from it.
 */
int
main(
	int			UNUSED( argc ),
	char **			argv
)
{
	int			joy_fd;
	int			server_fd;
	const char *		file;

	if( trainer )
	{
		file = argv[1] ? argv[1] : trainer_dev;

		joy_fd = trainer_open( file );
		event = trainer_event;
	} else {
		file = argv[1] ? argv[1] : joy_dev;

		joy_fd = joydev_open( file );
		event = joydev_event;
	}

	if( joy_fd < 0 )
	{
		fprintf( stderr,
			"%s: Unable to open joystick: %s\n",
			prog,
			file
		);

		return EXIT_FAILURE;
	}
			
	server_fd = connect_state( 0, 0 );
	if( server_fd < 0 )
		exit(-1);

	while(1)
	{
		int			rc;
		fd_set			fds;
		int			max =
			joy_fd > server_fd ? joy_fd : server_fd;

		FD_ZERO( &fds );
		FD_SET( joy_fd, &fds );
		FD_SET( server_fd, &fds );

		rc = select( max+1, &fds, 0, 0, 0 );
		if( rc < 0 )
		{
			perror( "select" );
			exit(-1);
		}

		if( rc == 0 )
			continue;

		if( FD_ISSET( joy_fd, &fds ) )
			handle_joystick( joy_fd );
		if( FD_ISSET( server_fd, &fds ) )
			handle_state( server_fd );
	}

	return 0;
}

