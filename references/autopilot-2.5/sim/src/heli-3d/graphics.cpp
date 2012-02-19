/* -*- indent-tabs-mode:T; c-basic-offset:8; tab-width:8; -*- vi: set ts=8:
 * $Id: graphics.cpp,v 2.1 2003/03/08 05:11:39 tramm Exp $
 *
 * (c) Aaron Kahn
 * (c) Trammell Hudson
 * (c) Tim Myrtle
 *
 * This defines some values for the graphics of helicopters and
 * draws the scene with the model, the origin and the world.
 *
 * It is wrapped in an Fltk widget to do the drawing.
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
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with Autopilot; if not, write to the Free Software
 *  Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
 *
 */

#include <cstdio>
#include <cmath>
#include <GL/gl.h>
#include <GL/glu.h>
#include <GL/glut.h>

#include "graphics.h"
#include <mat/Quat.h>
#include <mat/Vector_Rotate.h>
#include <mat/Conversions.h>

using namespace libmat;

static GLfloat		red[4]		= { 0.40, 0.00, 0.00, 1.00 };
static GLfloat		yellow[4]	= { 0.90, 0.90, 0.00, 1.00 };
static GLfloat		dull_yellow[4]	= { 0.40, 0.40, 0.00, 1.00 };
static GLfloat		grey[4]		= { 0.40, 0.40, 0.40, 1.00 };
static GLfloat		black[4]	= { 0.00, 0.00, 0.00, 1.00 };
static GLfloat		shadowed[4]	= { 0.00, 0.00, 0.00, 0.20 };
	
static GLfloat		dull[1]		= {  10.00 };
static GLfloat		shiny[1]	= { 100.00 };
static int		shadow		= 0;


void
DrawScene(
	viewpoint_t		viewpoint,
	double			north,
	double			east,
	double			down,
	double			phi,
	double			theta,
	double			psi,
	double			roll_moment,
	double			pitch_moment
)
{
	int			e;
	
	float			X = north; // 0.6858 + 0.00;
	float			Y = -down; // 0.6858 + 0.40;
	float			Z =  east; // 0.6858 - 4.00;
	

	// all of this stuff is just for the lighting of the scene.
	GLfloat groundAmbient[4]	= { 0.02, 0.30, 0.10, 1.00 };
	GLfloat local_ambient[4]	= { 0.70, 0.70, 0.70, 1.00 };
	
	GLfloat ambient0[4]		= { 0.00, 0.00, 0.00, 1.00 };
	GLfloat diffuse0[4]		= { 1.00, 1.00, 1.00, 1.00 };
	GLfloat specular0[4]		= { 1.00, 0.00, 0.00, 1.00 };
	GLfloat position0[4]		= { 2.00, 100.50, 1.50, 1.00 };
	
	GLfloat ambient1[4]		= { 0.00, 0.00, 0.00, 1.00 };
	GLfloat diffuse1[4]		= { 1.00, 1.00, 1.00, 1.00 };
	GLfloat specular1[4]		= { 1.00, 0.00, 0.00, 1.00 };
	GLfloat position1[4]		= { -2.00, 100.50, 1.00, 0.00 };
	
	glLightModelfv(GL_LIGHT_MODEL_AMBIENT, local_ambient);
	glLightModeli(GL_LIGHT_MODEL_LOCAL_VIEWER, 1);
	glLightModeli(GL_LIGHT_MODEL_TWO_SIDE, 0);
	
	glEnable(GL_LIGHT0);
	glLightfv(GL_LIGHT0, GL_AMBIENT, ambient0);
	glLightfv(GL_LIGHT0, GL_POSITION, position0);
	glLightfv(GL_LIGHT0, GL_DIFFUSE, diffuse0);
	glLightfv(GL_LIGHT0, GL_SPECULAR, specular0);
	
	glEnable(GL_LIGHT1);
	glLightfv(GL_LIGHT1, GL_AMBIENT, ambient1);
	glLightfv(GL_LIGHT1, GL_POSITION, position1);
	glLightfv(GL_LIGHT1, GL_DIFFUSE, diffuse1);
	glLightfv(GL_LIGHT1, GL_SPECULAR, specular1);
	// end of lighting
	

	glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT);
	glLoadIdentity();
	

	lookat( viewpoint, X, Y, Z, phi, theta, psi );
	
	glEnable(GL_NORMALIZE);
	glDisable(GL_LIGHTING);
	glColor3f( 0.00, 0.00, 1.00);
	glBegin(GL_LINES);
	glVertex3f( 0.00, 0.00, 0.00);
	glVertex3f( 0.00, 5.00, 0.00);
	glEnd();

	//this is the ground
#ifdef TEXTURE_GROUND
	extern void apply_texture( void );
	apply_texture();
#else
	glEnable(GL_LIGHTING);
	glMaterialfv(GL_FRONT, GL_AMBIENT_AND_DIFFUSE, groundAmbient);
	
	glBegin(GL_POLYGON);
	glNormal3f( 0.00, 1.00, 0.00 );
	const double ground_height = -0.1;
	glVertex3f(  31200.00, ground_height, -31200.00);
	glVertex3f( -31200.00, ground_height, -31200.00);
	glVertex3f( -31200.00, ground_height,  31200.00);
	glVertex3f(  31200.00, ground_height,  31200.00);
	glEnd();
	
	glDisable(GL_LIGHTING);
	for(e=-1000; e<=1000; e+=50)
	{
		glColor3f( 0.00, 0.00, 0.00);
		glBegin(GL_LINES);
		glVertex3f( e, 0.00, -1000.00);
		glVertex3f( e, 0.00,  1000.00);
		
		glVertex3f( -1000.00, 0.00, e);
		glVertex3f( 1000.00, 0.00, e);
		glEnd();
	}
	glEnable(GL_LIGHTING);
#endif
	
	glPushMatrix();
	glTranslatef(X, Y, Z);
	glRotatef( psi   * C_RAD2DEG, 0.00, -1.00, 0.00);
	glRotatef( theta * C_RAD2DEG, 0.00,  0.00, 1.00);
	glRotatef( phi   * C_RAD2DEG, 1.00,  0.00, 0.00);

	DrawXcellModel(
		phi,
		roll_moment,
		pitch_moment
	);

	glPopMatrix();
	glPushMatrix();
	
	// Draw the shadows
	// make the neccessary squashing matrix to flatten everything
	// onto the ground
	const float mat[16] = {
		1.00, 0.00, 0.00, 0.00,
		0.00, 0.00, 0.00, 0.00,
		0.00, 0.00, 1.00, 0.00,
		0.00, 0.00, 0.00, 1.00,
	};
	
	
	glTranslatef(X, 0.00, Z);
	glMultMatrixf(mat);
	
	// Enable alpha channel for the shadows
	glEnable( GL_BLEND );
	glBlendFunc( GL_SRC_ALPHA, GL_ONE_MINUS_SRC_ALPHA );

	glRotatef( psi * C_RAD2DEG,   0.00, -1.00, 0.00);
	glRotatef( theta * C_RAD2DEG, 0.00,  0.00, 1.00);
	glRotatef( phi * C_RAD2DEG,   1.00,  0.00, 0.00);

	DrawXcellModel(
		phi,
		roll_moment,
		pitch_moment,
		1	// Shadow
	);

	glDisable( GL_BLEND );

	glPopMatrix();

}


static void
draw_rotor(
	double			outer_radius,
	double			inner_radius = 0
)
{
	double			x1		= 0;
	double			x2		= 0;

	double			z1		= inner_radius;
	double			z2		= outer_radius;

	/*
	 * Draw the inner and outer rotor discs that represent
	 * the tip path plane.
	 */
	for( double k = 0 ; k < 2*C_PI ; k += C_PI/12 )
	{
		double		newx1		= outer_radius * sin(k);
		double		newz1		= outer_radius * cos(k);

		double		newx2		= inner_radius * sin(k);
		double		newz2		= inner_radius * cos(k);

		glBegin( GL_POLYGON );
		glVertex3f( x2, 0.00, z2 );
		glVertex3f( x1, 0.00, z1 );
		glVertex3f( newx1, 0.00, newz1 );
		glVertex3f( newx2, 0.00, newz2 );
		glEnd();

		x1 = newx1;
		x2 = newx2;

		z1 = newz1;
		z2 = newz2;
	}


	/*
	 * Draw the rotating blades.  Assume two, although we should
	 * have a way to query the simulator to find out more.  We
	 * assume the blade advances a set amount per call, which may
	 * break on faster or slower hardware.
	 */
	static double		angle;
	x1	= outer_radius * sin( angle );
	x2	= outer_radius * sin( angle + 0.10 );
	z1	= outer_radius * cos( angle );
	z2	= outer_radius * cos( angle + 0.10 );
	angle += 0.10;

	// Blade 1
	glBegin( GL_TRIANGLES );
	glVertex3f( x1, 0.00, z1 );
	glVertex3f( x2, 0.00, z2 );
	glVertex3f( 0,  0.00, 0  );
	glEnd();

	// Blade 2
	glBegin( GL_TRIANGLES );
	glVertex3f( -x1, 0.00, -z1 );
	glVertex3f( -x2, 0.00, -z2 );
	glVertex3f( 0,  0.00, 0  );
	glEnd();

}


static inline void
do_color(
	GLfloat			r,
	GLfloat			g,
	GLfloat			b,
	GLfloat			a,
	GLfloat			ambient,
	GLfloat			diffuse,
	GLfloat			specular,
	GLfloat			shininess,
	GLfloat			emmision
)
{
	GLfloat			color[] = { r, g, b, a };

	glMaterialfv(
		GL_FRONT,
		GL_AMBIENT_AND_DIFFUSE,
		shadow ? shadowed : color
	);
}

#ifndef MODEL
#define	MODEL	0
#endif

#if MODEL==1
#include "hh60.cpp"
#endif

#if MODEL==2
#include "hh65.cpp"
#endif


void
DrawXcellModel(
	double			phi,
	double			roll_moment,
	double			pitch_moment,
	int			shadow_arg
)
{
	shadow = shadow_arg;

#if MODEL != 0

	glMaterialfv(
		GL_FRONT,
		GL_AMBIENT_AND_DIFFUSE,
		shadow ? shadowed : red
	);

	glEnable( GL_NORMALIZE );

	glPushMatrix();
	glRotatef(  90.00, 1.00,  0.00, 0.00 );
	glRotatef( 180.00, 0.00,  1.00, 0.00 );
	glRotatef(  90.00, 0.00,  0.00, 1.00 );

	do_standard_0();
	glPopMatrix();

	// now to draw the main rotor
	glMaterialfv(
		GL_FRONT,
		GL_AMBIENT_AND_DIFFUSE,
		shadow ? shadowed : grey
	);
	glPushMatrix();

	static double angle = 0;

	glTranslatef( 1.60, 0.80, 0.00 );
	glRotatef( 90.0,			-1.00,  0.00, 0.00 );
	glRotatef( angle += 10.0,	 	 0.00,  0.00, 1.00 );
	glRotatef( phi * C_RAD2DEG,		 0.00, -1.00, 0.00 );
	glRotatef( pitch_moment * C_RAD2DEG,	 0.00,  0.00, 1.00 );
	glRotatef( roll_moment * C_RAD2DEG,	 1.00,  0.00, 0.00 );	
	glNormal3f( 0.00, 1.00, 0.00 );

	//draw_rotor( 6.00, 5.80 );
	do_rotor_0();
	glPopMatrix();
#else

	glScalef( SF, SF, SF );

	// this is the red cone that makes the "Pod"
	//glEnable( GL_LIGHTING );
	glEnable( GL_NORMALIZE );
	glMaterialfv(
		GL_FRONT,
		GL_AMBIENT_AND_DIFFUSE,
		shadow ? shadowed : red
	);

	glBegin( GL_TRIANGLES );
	glNormal3f(  0.00,  1.00,  1.00 );
	glVertex3f(  0.00,  0.20,  0.10 );
	glVertex3f(  0.00,  0.10,  0.20 );
	glVertex3f(  0.60, -0.20,  0.00 );
	
	glNormal3f(  0.00,  0.00,  1.00 );	
	glVertex3f(  0.00,  0.10,  0.20 );
	glVertex3f(  0.00, -0.10,  0.20 );
	glVertex3f(  0.60, -0.20,  0.00 );
	
	glNormal3f(  0.00, -0.50,  0.50 );
	glVertex3f(  0.00, -0.10,  0.20 );
	glVertex3f(  0.00, -0.20,  0.10 );
	glVertex3f(  0.60, -0.20,  0.00 );
	
	glNormal3f(  0.00, -1.00,  0.00 );
	glVertex3f(  0.00, -0.20,  0.10 );
	glVertex3f(  0.00, -0.20, -0.10 );
	glVertex3f(  0.60, -0.20,  0.00 );
	
	glNormal3f(  0.00, -0.50, -0.50 );
	glVertex3f(  0.00, -0.20, -0.10 );
	glVertex3f(  0.00, -0.10, -0.20 );
	glVertex3f(  0.60, -0.20,  0.00 );
	
	glNormal3f(  0.00,  0.00, -1.00 );
	glVertex3f(  0.00, -0.10, -0.20 );
	glVertex3f(  0.00,  0.10, -0.20 );
	glVertex3f(  0.60, -0.20,  0.00 );
	
	glNormal3f(  0.00,  0.50, -0.50 );
	glVertex3f(  0.00,  0.10, -0.20 );
	glVertex3f(  0.00,  0.20, -0.10 );
	glVertex3f(  0.60, -0.20,  0.00 );
	
	glNormal3f(  0.00,  1.00,  0.00 );
	glVertex3f(  0.00,  0.20, -0.10 );
	glVertex3f(  0.00,  0.20,  0.10 );
	glVertex3f(  0.60, -0.20,  0.00 );
	glEnd();

	
	// The back of the cone is black and yellow
	glMaterialfv(
		GL_FRONT,
		GL_AMBIENT_AND_DIFFUSE,
		black
	);

	glMaterialfv( GL_FRONT, GL_SHININESS, shiny );
	glBegin( GL_POLYGON );
	glNormal3f( -1.00,  0.00,  0.00 );
	glVertex3f(  0.00,  0.00,  0.00 );
	glVertex3f(  0.00,  0.20,  0.00 );
	glVertex3f(  0.00,  0.20,  0.10 );
	glVertex3f(  0.00,  0.10,  0.20 );
	glVertex3f(  0.00,  0.00,  0.20 );
	glVertex3f(  0.00,  0.00,  0.20 );
	glEnd();

	glBegin( GL_POLYGON );
	glNormal3f( -1.00,  0.00,  0.00 );
	glVertex3f(  0.00,  0.00,  0.00 );
	glVertex3f(  0.00, -0.20,  0.00 );
	glVertex3f(  0.00, -0.20, -0.10 );
	glVertex3f(  0.00, -0.10, -0.20 );
	glVertex3f(  0.00,  0.00, -0.20 );
	glEnd();
	
	// The high-contrast yellow
	glMaterialfv(
		GL_FRONT,
		GL_AMBIENT_AND_DIFFUSE,
		shadow ? shadowed : yellow
	);

	glBegin( GL_POLYGON );
	glNormal3f( -1.00,  0.00,  0.00 );
	glVertex3f(  0.00,  0.00,  0.20 );
	glVertex3f(  0.00, -0.10,  0.20 );
	glVertex3f(  0.00, -0.20,  0.10 );
	glVertex3f(  0.00, -0.20,  0.00 );
	glVertex3f(  0.00,  0.00,  0.00 );
	glEnd();

	glBegin( GL_POLYGON );
	glNormal3f( -1.00,  0.00,  0.00 );
	glVertex3f(  0.00,  0.00, -0.20 );
	glVertex3f(  0.00,  0.10, -0.20 );
	glVertex3f(  0.00,  0.20, -0.10 );
	glVertex3f(  0.00,  0.20,  0.00 );
	glVertex3f(  0.00,  0.00,  0.00 );
	glEnd();


	// Draw the tail boom
	glMaterialfv( GL_FRONT, GL_SHININESS, dull );
	glMaterialfv(
		GL_FRONT,
		GL_AMBIENT_AND_DIFFUSE,
		shadow ? shadowed : dull_yellow
	);

	glBegin(GL_POLYGON);
	glNormal3f(  0.00,  0.00,  1.00 );
	glVertex3f(  0.00,  0.02,  0.02 );
	glVertex3f( -1.40,  0.02,  0.02 );
	glVertex3f( -1.40, -0.02,  0.02 );
	glVertex3f(  0.00, -0.02,  0.02 );
	
	glEnd();
	glBegin(GL_POLYGON);
	glNormal3f(  0.00,  0.00, -1.00 );
	glVertex3f(  0.00,  0.02, -0.02 );
	glVertex3f( -1.40,  0.02, -0.02 );
	glVertex3f( -1.40, -0.02, -0.02 );
	glVertex3f(  0.00, -0.02, -0.02 );
	
	glEnd();
	glBegin(GL_POLYGON);
	glNormal3f(  0.00,  1.00,  0.00 );
	glVertex3f(  0.00,  0.02,  0.02 );
	glVertex3f( -1.40,  0.02,  0.02 );
	glVertex3f( -1.40,  0.02, -0.02 );
	glVertex3f(  0.00,  0.02, -0.02 );
	
	glEnd();
	glBegin(GL_POLYGON);
	glNormal3f(  0.00, -1.00,  0.00 );
	glVertex3f(  0.00, -0.02,  0.02 );
	glVertex3f( -1.40, -0.02,  0.02 );
	glVertex3f( -1.40, -0.02, -0.02 );
	glVertex3f(  0.00, -0.02, -0.02 );
	
	glEnd();
	
	// now to draw the tail rotor
	glPushMatrix();
	glTranslatef( 0.12, 0.00, 0.02 );

	glMaterialfv(
		GL_FRONT,
		GL_AMBIENT_AND_DIFFUSE,
		shadow ? shadowed : grey
	);
	glTranslatef( -1.40, 0.00, 0.00 );
	glRotatef( 90.00, 1.00,  0.00, 0.00 );
	glNormal3f( 0.00, 0.00, 1.00 );

	draw_rotor( 0.12, 0.10 );
	glPopMatrix();
		

	// now to draw the main rotor
	glPushMatrix();

	glTranslatef( 0.00, 0.34, 0.00 );
	glRotatef( phi * C_RAD2DEG,		0.00, -1.00, 0.00 );
	glRotatef( pitch_moment * C_RAD2DEG,	0.00,  0.00, 1.00 );
	glRotatef( roll_moment * C_RAD2DEG,	1.00,  0.00, 0.00 );	
	glNormal3f( 0.00, 1.00, 0.00 );

	draw_rotor( 1.00, 0.90 );
	glPopMatrix();


	//now for the main mast
	glColor3f(  0.40, 0.70, 0.20 );
	glBegin( GL_LINES );
	glVertex3f( 0.00, 0.00, 0.00 );
	glVertex3f( 0.00, 0.34, 0.00 );
	glEnd();
	

	// now for the skids
	// We have to adapt it to the waterline of the aircraft,
	// since the numbers that we receive from the simulator are
	// for the CG, not for the bottom of the skids.
	const bool		training_gear	= 0;
	const double		cg_wl		= 10.91;
	double                  length          =  8.0;
       	double                  width           =  5.6;
       	double                  offset          =  2.0;
       	double                  height          = 15.0;

       	if( training_gear )
	{
       		length          = 30.0;
		width           = 30.0;
		offset          =  0.0;
		height          = 20.0;
	}


	const double	skid_height	= -(height - cg_wl) / 12.0;
	const double	skid_front	= (offset + length / 2.0) / 12.0;
	const double	skid_back	= (offset - length / 2.0) / 12.0;
	const double	skid_side	= (width / 2.0) / 12.0;
	
	const double	skid_strut1	= (offset + length / 3.0) / 12.0;
	const double	skid_strut2	= (offset - length / 3.0) / 12.0;

	glColor3f( 0.00, 0.00, 1.00 );
	glBegin( GL_LINES );
	// Struts
	glVertex3f(  0.20, -0.20,  0.10 );
	glVertex3f(  skid_strut1, skid_height, skid_side );
	
	glVertex3f(  0.20, -0.20, -0.10 );
	glVertex3f(  skid_strut1, skid_height, -skid_side );
	
	glVertex3f(  0.00, -0.20,  0.10 );
	glVertex3f(  skid_strut2, skid_height, skid_side );
	
	glVertex3f(  0.00, -0.20, -0.10 );
	glVertex3f(  skid_strut2, skid_height, -skid_side );
	
	// Skid lines
	glVertex3f( skid_back, skid_height, skid_side );
	glVertex3f( skid_front, skid_height, skid_side );

	glVertex3f( skid_back, skid_height, -skid_side );
	glVertex3f( skid_front, skid_height, -skid_side );
	glEnd();

	glEnable(GL_LIGHTING);
#endif
}
