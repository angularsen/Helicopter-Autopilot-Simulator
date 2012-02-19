/**
 *  $Id: simple-hover.c,v 2.0 2002/09/22 02:07:30 tramm Exp $
 *
 * C program that uses the controller library.
 * It doesn't do much, but demonstrates how simple a hovering
 * program can be.
 *
 * (c) Trammell Hudson
 */

#include <stdio.h>
#include <stdlib.h>
#include <unistd.h>
#include <controller/wrapper.h>

int main( void )
{
	controller_reset( "localhost", 2002 );
	controller_set( 0, 0, -5, 0 );

	while( 1 )
	{
		double			north;
		double			east;
		double			down;
		double			roll;
		double			pitch;
		double			yaw;

		int			rc;

		rc = controller_step(
			&north,
			&east,
			&down,
			&roll,
			&pitch,
			&yaw
		);

		if( rc == 0 )
			continue;
		if( rc < 0 )
			break;

		fprintf( stderr,
			"(%2.3f,%2.3f,%2.3f,%2.3f)\r",
			north,
			east,
			down,
			yaw
		);
	}

	fprintf( stderr, "\n" );
	return 0;
}
