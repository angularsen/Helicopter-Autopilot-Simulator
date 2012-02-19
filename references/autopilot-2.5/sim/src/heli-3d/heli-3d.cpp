/* -*- indent-tabs-mode:T; c-basic-offset:8; tab-width:8; -*- vi: set ts=8:
 * $Id: heli-3d.cpp,v 2.6 2003/03/25 17:26:34 tramm Exp $
 *
 * (c) Aaron Kahn
 * (c) Trammell Hudson
 * (c) Bram Stolk
 *
 * Helicopter model in OpenGL and wrappers around GLUT.
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

#include <iostream>
#include <cstdio>
#include <cstdlib>
#include <cstring>
#include <cmath>
#include <unistd.h>
#include <ctype.h>
#include <sys/time.h>

#include <GL/gl.h> // system OpenGL includes
#include <GL/glu.h>
#include <GL/glut.h>

#include "simview.h"
#include "graphics.h"
#include <state/state.h>
#include <state/commands.h>
#include <state/Server.h>
#include <getoptions/getoptions.h>
#include "anim.h"

using namespace libstate;
using namespace std;

static UserInterface *	gui		= 0;
viewpoint_t		viewpoint	= view_stationary;

Server *		server		= 0;
static const char *	server_host	= "localhost";
static int		server_port	= 2002;

static state_t		state;
static int		packets		= 0;

static const int	MAX_POINTS	= 1024;
static double		points[ MAX_POINTS ][3];
static int		cur_point	= 0;



void
reconnect_server( void )
{
	server->connect( server_host, server_port );
}



static void
ahrs_state(
	void *			UNUSED( priv ),
	const host_t *		src,
	int			UNUSED( type ),
	const struct timeval *	when,
	const void *		data,
	size_t			len
)
{
	static struct timeval	last;
	static int		draw;

	if( len != sizeof(state) )
	{
		cerr << "Invalid AHRS packet from "
			<< src
			<< ".  Expected "
			<< sizeof(state)
			<< " bytes, but received "
			<< len
			<< endl;
		return;
	}

	if( timercmp( &last, when, > ) )
	{
		cerr << "Old AHRS packet from " << src << endl;
		return;
	}

	state = *(state_t*) data;
	gui->packets->value( packets++ );

	if( draw )
	{
		gui->simview->redraw();
		draw = 0;
	} else
		draw = 1;
}



/**
 *  
 */
static void
resize_window(
	GLsizei			w,
	GLsizei			h
)
{
	if( h == 0 )
		h = 1;
	if( w == 0 )
		w = 1;

	glViewport( 0, 0, w, h );
	glMatrixMode( GL_PROJECTION );
	glLoadIdentity();
	gluPerspective(
		30.0,				// Field of view
		(GLfloat)w/(GLfloat)h,		// Aspect ratio
		1.0,				// Near
		1000.0				// Far
	);

	// select the Modelview matrix
	glMatrixMode( GL_MODELVIEW );
	glLoadIdentity();
}


#if 0
static void
DrawPoints(
	const double		points[][3],
	int			starting,
	int			num_points
)
{
	int			i;

	glColor3f(0.0, 1.0, 0.0);
	glBegin( GL_LINES );

	for( i=0 ; i<num_points-1 ; i++ )
	{
		int j1 = (i + 0 + starting) % num_points;
		int j2 = (i + 1 + starting) % num_points;

		// GL_LINES wants start and end points for each line.
		glVertex3f(
			 points[j1][0],
			-points[j1][2],
			 points[j1][1]
		);

		glVertex3f(
			 points[j2][0],
			-points[j2][2],
			 points[j2][1]
		);
	}

	glEnd();
}


static double
compute_fps()
{
	struct timeval		now;
	static struct timeval	last;

	gettimeofday( &now, 0 );
	timersub( &now, &last, &last );

	double			diff = last.tv_usec;
	last = now;

	return 1000000.0 / diff;
}

static void
draw_text(
	const char *		buf,
	float			x,
	float			y
)
{
	const float		scale = 1.2;

	glPushMatrix();
	glLoadIdentity();

	glTranslatef( x, y, -1 );

	glScalef( 0.0001, 0.0001, 0.0001 );
	glScalef( scale, scale, scale );
	
	while( *buf )
		glutStrokeCharacter(
			GLUT_STROKE_MONO_ROMAN,
			*buf++
		);

	glPopMatrix();
}

static void
draw_fps()
{
	char			buf[12];

	snprintf(
		buf,
		sizeof(buf),
		"FPS=%3.2f",
		compute_fps()
	);

	draw_text( buf, -0.25, -0.230 );
}


static void
draw_pos(
	double			x,
	double			y,
	double			z
)
{
	char			buf[ 32 ];

	snprintf(
		buf,
		sizeof(buf),
		"Pos=(% 3.2f,% 3.2f,% 3.2f)",
		x,
		y,
		z
	);

	draw_text( buf, -0.25, -0.25 );

}
#endif
	


Simview::Simview(
	int			X,
	int			Y,
	int			W,
	int			H,
	const char *		L
) :
	Fl_Gl_Window		(X, Y, W, H, L )
{
	cout << "Simview constructed" << endl;
}


void
Simview::draw()
{
	if( !valid() )
		this->initialize_gl();

	points[cur_point % MAX_POINTS][0] = state.x;
	points[cur_point % MAX_POINTS][1] = state.y;
	points[cur_point % MAX_POINTS][2] = state.z;
	cur_point++;

	DrawScene(
		viewpoint,
		state.x,
		state.y,
		state.z,
		state.phi,
		state.theta,
		state.psi,
		state.mx,
		state.my
	);

/*
	DrawPoints(
		points,
		cur_point,
		MAX_POINTS
	);

	draw_fps();
	draw_pos( state.x, state.y, -state.z );

	glutPostRedisplay();
	glutSwapBuffers();

	save_frame( WIDTH, HEIGHT );
*/
}



/**
 *  init_window() sets up the GL environment and schedules the
 * user callbacks for animation and periodic timing.
 */
void
Simview::initialize_gl()
{
	cout << "Simview initialize_gl" << endl;

	const int		W = this->w();
	const int		H = this->h();

	glShadeModel(GL_FLAT);
	//glClearColor(0.0f, 0.0f, 0.0f, 0.0f);
	glClearColor(0.49, 0.62, 0.75, 0.0);
	glClearDepth(1.0f);

	glEnable( GL_DEPTH_TEST );
	glDepthFunc( GL_LEQUAL );

	glDisable( GL_BLEND );
	glDisable( GL_ALPHA_TEST );

	glMatrixMode( GL_PROJECTION );
	glLoadIdentity();
	gluPerspective(
		30.0,				// Field of view
		(GLfloat) W /(GLfloat) H,		// Aspect ratio
		1.0,				// Near
		1000.0				// Far
	);

	// select the Modelview matrix
	glMatrixMode( GL_MODELVIEW );
	glLoadIdentity();
}


static int
help( void )
{
	cerr <<
"Usage: heli-3d [options]\n"
"\n"
"	-h | --help		This help\n"
"	-s | --server host	Server hostname or IP\n"
"	-p | --port port	Server port\n"
"	-v | --viewpoint v	Viewpoint:\n"
"				0: Stationary\n"
"				1: Walk behind\n"
"				2: Fly behind\n"
"				3: Cockpit\n"
"				4: North up\n"
"				5: Track up\n"
"\n"
	<< endl;

	return -10;
}


int
main(
	int			argc,
	char **			argv
)
{
	int rc = getoptions(
		&argc, &argv,
		"h|?|help=&",		help,
		"s|server=s",		&server_host,
		"p|port=i",		&server_port,
		"v|viewpoint=i",	&viewpoint,
		0
	);

	if( rc == -10 )
		return 0;
	if( rc < 0 )
		return help();


	Fl::gl_visual( FL_RGB );

	gui = new UserInterface();
	gui->make_window()->show();

	// Contact our state server
	server = new Server;
	reconnect_server();
	server->handle( AHRS_STATE, ahrs_state, 0 );

	Fl::add_fd(
		server->sock,
		FL_READ,
		Server::update,
		(void*) server
	);


	return Fl::run();
}
