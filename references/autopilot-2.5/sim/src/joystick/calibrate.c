/*
 * $Id: calibrate.c,v 2.0 2002/09/22 02:07:31 tramm Exp $
 *
 * Tests the joystick for readability.  Looks like it works so far.
 *
 * (c) 2001 by Trammell Hudson <hudson@swcp.com>
 *
 *************
 *
 *  This file is part of the autopilot ground station code package.
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
#include <string.h>

#include "macros.h"
#include "joystick.h"
#include "trainer.h"


static int		verbose = 0;

static int		(*event)(
	int			fd,
	struct js_event *	e,
	int			usec
);


static int
capture_axis(
	int			fd,
	int *			max_out
)
{
	int			max[ MAX_AXES ];
	int			min[ MAX_AXES ];
	int			max_axis = 0;
	int			min_axis = 0;
	int			i;
	struct js_event		e;

	memset( max, 0, sizeof(max) );
	memset( min, 0, sizeof(max) );

	while( event( fd, &e, -1 ) > 0 )
	{
		if( verbose )
			printf( "%08x: %s: ",
				e.time,
				e.type & JS_EVENT_INIT ? "real" : "init"
			);

		if( e.type & JS_EVENT_AXIS ) {
			if( verbose )
				printf( "axis %d -> %d\n",
					e.number,
					e.value
				);

			if( max[ e.number ] < e.value )
				max[ e.number ] = e.value;

			if( min[ e.number ] > e.value )
				min[ e.number ] = e.value;

		} else

		if( e.type & JS_EVENT_BUTTON ) {
			if( verbose )
				printf( "button %d %s\n",
					e.number,
					e.value ? "press" : "release"
				);
			if( e.value )
				break;
		} else

		fprintf( stderr, "Unknown type %02x\n", e.type );

		for( i=0 ; i < 4 ; i++ )
			printf( "%d-%d ", min[i], max[i] );
		printf( "\r" );
		fflush( stdout );
	}

	if( !max_out )
		return -1;

	for( i=0 ; i<4 ; i++ )
	{
		if( max[max_axis] < max[i] )
			max_axis = i;
		if( min[min_axis] > min[i] )
			min_axis = i;
	}

	if( max[max_axis] > -min[min_axis] )
	{
		*max_out = max[max_axis];
		return max_axis;
	}

	*max_out = min[min_axis];
	return min_axis;
}



int
main(
	int			UNUSED( argc ),
	char **			UNUSED( argv )
)
{
	int			fd;
	int			i;
	joy_axis_t		limits[ MAX_AXES ];

	char * 			axis[][3] = {
		{ "coll",	"max",	"min" },
		{ "roll",	"left",	"right"	},
		{ "pitch",	"fore",	"aft"	},
		{ "yaw",	"left",	"right"	},
		{ 0, 0, 0, },
	};

	if( 0 )
	{
		fd = joydev_open( "/dev/js0" );
		event = joydev_event;
	} else {
		fd = trainer_open( "/dev/ttyS0" );
		event = trainer_event;
	}

	if( fd < 0 )
		return -1;

	/* Throw away a bunch of events */
	printf(
		"\n\nMove the joystick to make sure it is working.\n"
		"Then center the sticks and hit any button to move on\n"
	);

	capture_axis( fd, 0 );
	
	for( i=0 ; i < MAX_AXES ; i++ )
		limits[i].name = 0;

	for( i = 0 ; axis[i][0] ; i++ )
	{
		char **			cur_axis = axis[i];
		int			max;
		int			min;
		int			axis;
		int			axis2;

		printf(
			"\nMove the %s axis to the %s\n",
			cur_axis[0],
			cur_axis[1]
		);
		axis = capture_axis( fd, &min );

		printf(
			"\nMove the %s axis to the %s\n",
			cur_axis[0],
			cur_axis[2]
		);
		axis2 = capture_axis( fd, &max );

		if( axis != axis2 )
		{
			fprintf( stderr,
				"\nPlease move the same axis: %d != %d\n",
				axis,
				axis2
			);
			cur_axis -= 3;
			continue;
		}

		if( (max > 0 && min > 0)
		||  (max < 0 && min < 0)
		) {
			fprintf( stderr,
				"\nPlease center all controls and try again\n"
			);
			cur_axis -= 3;
			continue;
		}

		printf( "%s: Axis %d range %d -> %d\n",
			cur_axis[0],
			axis,
			min,
			max
		);

		limits[axis].name = cur_axis[0];
		limits[axis].min  = min;
		limits[axis].max  = max;
	}

	printf( "static joy_axis_t limits[] =\n{\n" );

	for( i=0 ; i < MAX_AXES ; i++ )
	{
		int used = limits[i].name != 0;

		printf( "\t{ \"%s\", %d, %d },\n",
			used ? limits[i].name : "",
			used ? limits[i].min  : 0,
			used ? limits[i].max  : 0
		);
	}

	printf( "};\n" );

	return 0;
}
