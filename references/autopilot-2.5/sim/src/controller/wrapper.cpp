/**
 *  $Id: wrapper.cpp,v 2.2 2003/03/08 05:10:36 tramm Exp $
 *
 * C wrapper for the Controller object.
 *
 * You only get one and it is very simple...
 */

#include <controller/Guidance.h>
#include <state/state.h>
#include <state/commands.h>
#include <mat/Nav.h>
#include <cstdlib>
#include <iostream>

using namespace libcontroller;
using namespace libmat;
using namespace libstate;
using namespace std;

static Guidance *	controller;
static int		connected;
static bool		add_noise;


BEGIN_DECLS


int
controller_connected( void )
{
	return connected;
}


int
controller_reset(
	const char *		hostname,
	int			port
)
{
	if( !controller )
		controller = new Guidance( state_dt() );
	else
		controller->reset();

	int sock = connect_state( hostname, port );
	if( sock < 0 )
		connected = 0;
	else
		connected = 1;

	return connected;
}


void
controller_set(
	double			north,
	double			east,
	double			down,
	double			heading
)
{
	if( !controller )
		controller_reset( "localhost", 2002 );

	controller->position[0] = north;
	controller->position[1] = east;
	controller->position[2] = down;
	controller->position[3] = heading;
}


static double
noise(
	double			range
)
{
	return drand48() * range / 2.0 - range / 2.0;
}


int
controller_step(
	double *		north,
	double *		east,
	double *		down,
	double *		roll,
	double *		pitch,
	double *		heading
)
{
	if( !controller )
		controller_reset( "localhost", 2002 );

	state_t			state;

	int			rc = read_state( &state, 0 );
	if( rc < 0 )
		connected	= 0;
	if( rc <= 0 )
		return rc;

	Vector<3>	pos_NED( state.x, state.y, state.z );
	Vector<3>	vel_NED( state.vx, state.vy, state.vz );
	Vector<3>	theta( state.phi, state.theta, state.psi );
	Vector<3>	pqr( state.p, state.q, state.r );

	if( add_noise )
	{
		pos_NED	+= noise<3>( 0.1 );
		vel_NED += noise<3>( 1.3 );
		theta	+= noise<3>( 0.3 );
		pqr	+= noise<3>( 0.1 );
	}

	const Vector<4> v( controller->step(
		pos_NED,
		vel_NED,
		theta,
		pqr
	) );

	set_parameter( SERVO_COLL,	v[0] );
	set_parameter( SERVO_ROLL,	v[1] );
	set_parameter( SERVO_PITCH,	v[2] );
	set_parameter( SERVO_YAW,	v[3] );

	*north		= state.x;
	*east		= state.y;
	*down		= state.z;
	*roll		= state.phi;
	*pitch		= state.theta;
	*heading	= state.psi;

	return 1;
}



END_DECLS
