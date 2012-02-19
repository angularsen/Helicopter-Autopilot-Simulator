/* -*- indent-tabs-mode:T; c-basic-offset:8; tab-width:8; -*- vi: set ts=8:
 * $Id: instruments.cpp,v 2.0 2002/09/22 02:07:30 tramm Exp $
 *
 * (c) Aaron Kahn
 * (c) Trammell Hudson
 *
 * This is used to draw instruments that would be used in flight operations.
 * These are all based on the GLUT library and the MATLIB library.
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


#include <GL/gl.h>
#include <GL/glu.h>
#include <GL/glut.h>
#include <cstdio>
#include <cmath>

#include "instruments.h"
#include <mat/Conversions.h>



/*
 * draw each character in a stroke font
 */
static void
drawText(
	const char *		message
)
{
	while( *message )
		glutStrokeCharacter(
			GLUT_STROKE_MONO_ROMAN,
			*message++
		);
}


/*
 * generic string drawing routine, 2-D
 */
static void
showMessage(
	GLfloat			x,
	GLfloat			y,
	const char *		message,
	float			scale
)
{
	glPushMatrix();
	glTranslatef( x, y, 0.0 );
	glScalef( 0.0001, 0.0001, 0.0001 );
	glScalef( scale, scale, scale );
	drawText( message );
	glPopMatrix();
}


/*
 * This will draw an artificial horzion for attitude and heading
 * indication for a pilot.  The inputs are pitch, roll, and yaw of the
 * vehicle.  Also, the altitude and speed for the side bars.  In addition
 * to only displaying numbers, a three character message can be displayed
 * as well.
 *
 *  The ranges are data are...
 *	Pitch		-90 to 90 degrees
 *	Roll		-180 to 180 degrees
 *	Yaw		-180 to 180 degrees
 *	Altitude	NO RANGE (ft)
 *	Speed		NO RANGE (knots)
 *	Message	3 characters
 */
void
horizon(
	double			pitch,
	double			roll,
	double			yaw,
	double			altitude,
	double			speed,
	const char *		message
)
{
	int			n;
	char			buffer[20];
	static char		headlabels[37][4] = {
		"S",
		"19", "20", "21", "22", "23", "24", "25", "26",
		"W",
		"28", "29", "30", "31", "32", "33", "34", "35",
		"N",
		"1", "2", "3", "4", "5", "6", "7", "8",
		"E",
		"10", "11", "12", "13", "14", "15", "16", "17",
		"S"
	};
	char *			txt;
	double temp;

	glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT);
	glLoadIdentity();

	/******** ROLL TICK MARKS AND TICK MARKER **********/
	for(n=-30; n<=30; n+=15)
	{
		glPushMatrix();
		glRotatef(n, 0.0f, 0.0f, 1.0f);
		glColor3f(1.0f, 1.0f, 1.0f);
		glBegin( GL_LINES );
			glVertex3f(0.0f, 0.24f, -0.9f);
			glVertex3f(0.0f, 0.23f, -0.9f);
		glEnd();
		glPopMatrix();
	}
	glPushMatrix();
	glRotatef(roll, 0.0f, 0.0f, 1.0f);
	glColor3f(0.85f, 0.5f, 0.1f);
	glBegin( GL_TRIANGLES );
		glVertex3f(0.0f, 0.23f, -0.9f);
		glVertex3f(0.01f, 0.21f, -0.9f);
		glVertex3f(-0.01f, 0.21f, -0.9f);
	glEnd();
	glPopMatrix();


	/******** CENTER MARK ***********************/

	glPushMatrix();
	glColor3f(1.0f, 1.0f, 1.0f);
	glBegin( GL_LINES );
		// right half
		glVertex3f(0.0f, 0.0f, -0.9f);
		glVertex3f(0.015f, -0.02f, -0.9f);

		glVertex3f(0.015f, -0.02f, -0.9f);
		glVertex3f(0.03f, 0.0f, -0.9f);
		
		glVertex3f(0.03f, 0.0f, -0.9f);
		glVertex3f(0.06f, 0.0f, -0.9f);
		// left half
		glVertex3f(0.0f, 0.0f, -0.9f);
		glVertex3f(-0.015f, -0.02f, -0.9f);

		glVertex3f(-0.015f, -0.02f, -0.9f);
		glVertex3f(-0.03f, 0.0f, -0.9f);
		
		glVertex3f(-0.03f, 0.0f, -0.9f);
		glVertex3f(-0.06f, 0.0f, -0.9f);
	glEnd();
	
	glPopMatrix();

	glRotatef(roll, 0.0f, 0.0f, 1.0f);
	glTranslatef(-yaw*C_DEG2RAD, 0.0f, 0.0f);
	/******** HORIZON AND YAW TICK LINE **********/
	glColor3f(1.0f, 1.0f, 1.0f);
	glBegin( GL_LINES );
		glVertex3f(-(180.0+15)*C_DEG2RAD, 0.0f, -0.9f);
		glVertex3f((180.0+15)*C_DEG2RAD, 0.0f, -0.9f);
	glEnd();

	for(n=0; n<37; ++n)
	{
		glBegin( GL_LINES );
			glVertex3f( (double)(n*10 - 180)*C_DEG2RAD, 0.015f, -0.9f);
			glVertex3f( (double)(n*10 - 180)*C_DEG2RAD, 0.0f, -0.9f);
		glEnd();
		glPushMatrix();
		glTranslatef(0.0f, 0.0f, -0.9f);

		txt = &headlabels[n][0];

		showMessage(
			(double)(n*10 - 180)*C_DEG2RAD-0.01f,
			0.02,
			txt,
			1.2
		);
		glPopMatrix();
	}

	// Extra tick mark past S (going W) for overview
	glBegin( GL_LINES );
		glVertex3f( 190.0*C_DEG2RAD, 0.02f, -0.9f);
		glVertex3f( 190.0*C_DEG2RAD, 0.0f, -0.9f);
	glEnd();
	glPushMatrix();
	glTranslatef(0.0f, 0.0f, -0.9f);
	showMessage( 190.0*C_DEG2RAD-0.015, 0.02, "19\0", 1.0);
	glPopMatrix();
	// Extra tick mark past S (going E) for overview
	glBegin( GL_LINES );
		glVertex3f( -190.0*C_DEG2RAD, 0.02f, -0.9f);
		glVertex3f( -190.0*C_DEG2RAD, 0.0f, -0.9f);
	glEnd();
	glPushMatrix();
	glTranslatef(0.0f, 0.0f, -0.9f);
	showMessage( -190.0*C_DEG2RAD-0.015, 0.02, "17\0", 1.0);
	glPopMatrix();


	glPushMatrix();
	glLoadIdentity();
	glRotatef(roll, 0.0f, 0.0f, 1.0f);
	glTranslatef(0.0f, -pitch*C_DEG2RAD, 0.0f);

	/******** COLORED PART OF DISPLAY ************/
	glColor3f(0.0f, 0.0f, 1.0f);
	glBegin( GL_QUADS );
		glVertex3f(-(180.0+15)*C_DEG2RAD, (90.0+15.0)*C_DEG2RAD, -1.0f);
		glVertex3f((180.0+15)*C_DEG2RAD, (90.0+15.0)*C_DEG2RAD, -1.0f);
		glVertex3f((180.0+15)*C_DEG2RAD, 0.0f, -1.0f);
		glVertex3f(-(180.0+15)*C_DEG2RAD, 0.0f, -1.0f);
	glEnd();
	
	// bottom of display
	glColor3f(0.5f, 0.2f, 0.1f);
	glBegin( GL_QUADS );
		glVertex3f(-(180.0+15)*C_DEG2RAD, -(90.0+15.0)*C_DEG2RAD, -1.0f);
		glVertex3f((180.0+15)*C_DEG2RAD, -(90.0+15.0)*C_DEG2RAD, -1.0f);
		glVertex3f((180.0+15)*C_DEG2RAD, 0.0f, -1.0f);
		glVertex3f(-(180.0+15)*C_DEG2RAD, 0.0f, -1.0f);
	glEnd();

	/********* PITCH BARS *****************/
	for(n=0; n<9; ++n)
	{
		temp = (double)(n*10+10)*C_DEG2RAD;
		glColor3f(1.0f, 1.0f, 1.0f);
		// positive pitch lines
		glBegin( GL_LINES );
			glVertex3f(-0.1f, temp-0.01, -1.0f);
			glVertex3f(-0.1f, temp, -1.0f);

			glVertex3f(-0.1f, temp, -1.0f);
			glVertex3f(-0.03f, temp, -1.0f);

			glVertex3f(0.1f, temp-0.01, -1.0f);
			glVertex3f(0.1f, temp, -1.0f);

			glVertex3f(0.1f, temp, -1.0f);
			glVertex3f(0.03f, temp, -1.0f);
			
		glEnd();
		sprintf(buffer, "%d", n*10+10);
		glPushMatrix();
		glTranslatef(0.0f, 0.0f, -1.0f);
		showMessage(0.11f, temp-0.007, buffer, 1.0);
		showMessage(-0.13f, temp-0.007, buffer, 1.0);
		glPopMatrix();

		// negative pitch lines
		glBegin( GL_LINES );
			glVertex3f(-0.1f, -temp+0.01, -1.0f);
			glVertex3f(-0.1f, -temp, -1.0f);

			glVertex3f(-0.1f, -temp, -1.0f);
			glVertex3f(-0.03f, -temp, -1.0f);

			glVertex3f(0.1f, -temp+0.01, -1.0f);
			glVertex3f(0.1f, -temp, -1.0f);

			glVertex3f(0.1f, -temp, -1.0f);
			glVertex3f(0.03f, -temp, -1.0f);
		glEnd();
		sprintf(buffer, "%d", -(n*10+10));
		glPushMatrix();
		glTranslatef(0.0f, 0.0f, -1.0f);
		showMessage(0.11f, -temp, buffer, 1.0);
		showMessage(-0.14f, -temp, buffer, 1.0);
		glPopMatrix();
	}
	// +/- 5 degree tick marks
	glBegin( GL_LINES );
		glVertex3f(-0.05f, 5.0*C_DEG2RAD, -1.0f);
		glVertex3f(0.05f, 5.0*C_DEG2RAD, -1.0f);
	glEnd();
	glBegin( GL_LINES );
		glVertex3f(-0.05f, -5.0*C_DEG2RAD, -1.0f);
		glVertex3f(0.05f, -5.0*C_DEG2RAD, -1.0f);
	glEnd();
	
	glPopMatrix();

	/******** BOUNDARY LINES ON EDGES AND ALITUTDE/SPEED READOUT ******************/
	glPushMatrix();
	glLoadIdentity();

	glColor3f(0.0f, 0.0f, 0.0f);
	glBegin( GL_QUADS );
		glVertex3f(0.18f, 0.16f, -0.8f);
		glVertex3f(0.1f, 0.16f, -0.8f);
		glVertex3f(0.1f, 0.13f, -0.8f);
		glVertex3f(0.18f, 0.13f, -0.8f);
		
		glVertex3f(-0.19f, 0.16f, -0.8f);
		glVertex3f(-0.1f, 0.16f, -0.8f);
		glVertex3f(-0.1f, 0.13f, -0.8f);
		glVertex3f(-0.19f, 0.13f, -0.8f);

		glVertex3f(0.18f, -0.18f, -0.8f);
		glVertex3f(0.14, -0.18f, -0.8f);
		glVertex3f(0.14f, -0.15f, -0.8f);
		glVertex3f(0.18f, -0.15f, -0.8f);
	glEnd();

	// altitude readout
	glColor3f(0.0f, 1.0f, 0.0f);
	sprintf(buffer, "%3.2fft", altitude );
	glPushMatrix();
	glTranslatef(-0.17f, 0.140f, -0.8f);
	showMessage(0.0f, 0.0f, buffer, 1.0);
	glPopMatrix();
	
	// speed readout
	sprintf(buffer, "%3.2fk", speed );
	glPushMatrix();
	glTranslatef(0.12f, 0.140f, -0.8f);
	showMessage(0.0f, 0.0f, buffer, 1.0);
	glPopMatrix();

	// message readout
	glPushMatrix();
	glTranslatef(0.0f, 0.0f, -0.8f);
	showMessage(0.145f, -0.17f, message, 1.0);
	glPopMatrix();
	

	glPopMatrix();
}

