/* -*- indent-tabs-mode:T; c-basic-offset:8; tab-width:8; -*- vi: set ts=8:
 * $Id: heli-panel.cpp,v 2.0 2002/09/22 02:07:30 tramm Exp $
 *
 * (c) Aaron Kahn
 * (c) Trammell Hudson
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

#include <cstdio>
#include <cstdlib>
#include <cstring>
#include <cmath>
#include <unistd.h>

#include <GL/gl.h> // system OpenGL includes
#include <GL/glu.h>
#include <GL/glut.h>

#include "macros.h"
#include <state/state.h>
#include "instruments.h"

#include <mat/Constants.h>
#include <mat/Conversions.h>


using namespace util;
using namespace libstate;

#define			WIDTH		240
#define			HEIGHT		240
static int		main_window;

static state_t		state;



static void
animate( void )
{
	static double		last_x;
	static double		last_y;
	static double		last_z;

	double			speed = sqrt( 0
		+ sqr( state.vx )
		+ sqr( state.vy )
		+ sqr( state.vz )
	);

	horizon(
		state.theta * C_RAD2DEG,
		state.phi * C_RAD2DEG,
		state.psi * C_RAD2DEG,
		-state.z,
		speed,
		""
	);

	last_x	= state.x;
	last_y	= state.y;
	last_z	= state.z;

	glutPostRedisplay();
	glutSwapBuffers();
}


/**
 *  Called automatically by GLUT whenever the window is resized.
 * We just scale the helicopter and world to fill the window.
 */
static void
resize_window(
	GLsizei			w,
	GLsizei			h
)
{
	if( !h )
		h = 1;
	if( !w )
		w = 1;

	glViewport( 0, 0, w, h );
	glMatrixMode( GL_PROJECTION );
	glLoadIdentity();
	gluPerspective(
		30.0,
		(GLfloat)w / (GLfloat)h,
		0.1f,
		2500.0f
	);

	// select the Modelview matrix
	glMatrixMode( GL_MODELVIEW );
	glLoadIdentity();
	//glRedraw();
}


static void
process_state( void )
{
	if( read_state( &state, 0 ) < 0 )
		exit( EXIT_SUCCESS );
}



int
main(
	int			argc,
	char **			argv
)
{
	// Call these first so that glutInit can override if necessary
	glutInitWindowSize( WIDTH, HEIGHT ); 
	glutInitWindowPosition( 0, 0 );
	glutInitDisplayMode( 0
		| GLUT_DOUBLE
		| GLUT_RGB
		| GLUT_DEPTH
	);

	glutInit( &argc, argv );
	main_window = glutCreateWindow( argv[0] );

	glShadeModel(GL_FLAT);
	glClearColor(0.0f, 0.0f, 0.0f, 0.0f);
	glClearDepth(1.0f);
	glDepthFunc(GL_LEQUAL);
	glEnable(GL_DEPTH_TEST);

	glutDisplayFunc( animate );
	glutReshapeFunc( resize_window ); 
	//glutKeyboardFunc( keyboard );
	glutIdleFunc( process_state );

	connect_state( "localhost", 2002 );

	glutMainLoop();
}

