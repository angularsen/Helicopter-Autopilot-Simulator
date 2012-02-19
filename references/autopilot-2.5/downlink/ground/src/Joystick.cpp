/* -*- indent-tabs-mode:T; c-basic-offset:8; tab-width:8; -*- vi: set ts=8:
 * $Id: Joystick.cpp,v 1.8 2003/02/07 04:34:12 dennisda Exp $
 *
 * (c) Trammell Hudson
 *
 * Joystick handling routines for the groundstation
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
#include "state/state.h"
#include "state/udp.h"
 

#include "Ground.h"
#include "Joystick.h"

#ifndef WIN32
#include "joystick/joystick.h"
#endif
 
#ifdef WIN32
#include <windows.h>
#include <mmsystem.h>
#endif

using namespace std;
using namespace util;
using namespace libstate;

int			joy_pitch;
int			joy_roll;
int			joy_yaw;
int			joy_throttle;
int			joy_button[8];
static const double	pi		= 3.141519;
#ifdef WIN32
static UINT joystickID = 0;
#endif
static joy_axis_t limits[8] =
{

	{ "roll", -32000, 32000, 0 },
	{ "pitch", -32000, 32000, 0 },
	{ "coll", 32000, -32000, 0 },
	{ "yaw", -32000, 32000, 0 },
	
};

static trim_t trims[ 8 ] =
{
	{  -4.00,   4.00,  0.58 },              /* roll */
	{   4.00,  -4.00, -0.18 },              /* pitch */
	{  3.00,   13.00,  0.00 },              /* coll */
	{ -13.00,  13.00,  6.00 },              /* yaw */
	
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
	
		return;
	}

	values	= &limits[axis];
	trim	= &trims[axis];

	if( !values->name || !values->name[0] )
	{
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

#ifndef WIN32
static void
joy_handler(
	int			fd,
	void *			UNUSED( user_arg )
)
{
	struct js_event		e;
	int			rc;

	rc = joydev_event( fd, &e, 0 );

	if( rc < 0 )
	{
		Fl::remove_fd( fd );
		close( fd );
		gui->joy_status->value( 0 );
		return;
	}

	if( rc == 0 )
		return;

	if(0) cout
		<< ( e.type & JS_EVENT_AXIS ? "axis: "
		:  e.type & JS_EVENT_BUTTON ? "button: "
		: "UNKNOWN" )
		<< int(e.number)
		<< "="
		<< e.value
		<< endl;

	const int		num	= e.number;
	const int		value	= e.value;


	if( e.type & JS_EVENT_AXIS )
	{
		if(0) cout
			<< "axis: "
			<< num
			<< "="
			<< value
			<< endl;

		Fl_Group *g = gui->axes;

		if( num >= g->children() )
			return;

		Fl_Valuator *a = (Fl_Valuator*) g->child( num );
		a->value( value );

		/*
		 * This should have a config file of some sort.
		 * Convert to the right hand rule.
		 */
		switch( num )
		{
		case 0:
			joy_roll	= value;
			break;
		case 1:
			joy_pitch	= value;
			break;
		case 2:
			joy_yaw		= value;
			break;
		case 3:
			joy_throttle	= 16890 - value;
			break;
		default:
			/* Do nothing */
			break;
		}

		return;
	}

	if( e.type & JS_EVENT_BUTTON )
	{
		Fl_Group *g = gui->buttons;

		if( e.number >= g->children() )
			return;

		Fl_Button *b = (Fl_Button*) g->child( e.number );
		b->value( e.value );
		joy_button[e.number] = e.value;

		return;
	}
}


void
reconnect_joy(
	const char *		dev_name
)
{
	/* Get the joystick device, if desired */
	static int		joy_fd = -1;

	if( joy_fd >= 0 )
		close( joy_fd );

	joy_fd = joydev_open( dev_name );

	if( joy_fd < 0 )
	{
		gui->joy_status->value( 0 );
		perror( dev_name );
		return;
	}

	Fl::add_fd(
		joy_fd,
		FL_READ,
		joy_handler
	);

	gui->joy_status->value( 1 );
}

#endif

#ifdef WIN32
static void
joy_handler(
	void *			UNUSED( user_arg )
)
{
	MMRESULT result;
	JOYINFOEX joyinfo;
	int i;
	signed int joy[4];
	signed int rollA, pitchA, collA, yawA;
	
	joyinfo.dwSize=sizeof(joyinfo);
	joyinfo.dwFlags=JOY_RETURNALL;
	result=joyGetPosEx(joystickID, &joyinfo);

	if( result != JOYERR_NOERROR )
	{
		gui->joy_status->value( 0 );
		return;
	}

/* need WIN32 equivalent for this log statement */
/*	if(0) cout
		<< ( e.type & JS_EVENT_AXIS ? "axis: "
		:  e.type & JS_EVENT_BUTTON ? "button: "
		: "UNKNOWN" )
		<< int(e.number)
		<< "="
		<< e.value
		<< endl; */


	if(0) {
		cout<< "axis: "<< "X"<< "="<< joyinfo.dwXpos<< endl;
		cout<<"axis: Y="<<joyinfo.dwYpos<<endl;
		cout<<"axis: Z="<<joyinfo.dwZpos<<endl;
		cout<<"axis: R="<<joyinfo.dwRpos<<endl;
	}
		

	joy[0]=(signed int) joyinfo.dwXpos - 32000;
	joy[1]=(signed int) joyinfo.dwYpos - 32000;
	joy[2]=(signed int) joyinfo.dwZpos - 32000;
	joy[3]=(signed int) joyinfo.dwRpos - 32000;
	
	rollA=(int)gui->rollAxes->value();
	pitchA=(int)gui->pitchAxes->value();
	collA=(int)gui->collAxes->value();
	yawA=(int)gui->yawAxes->value();

	joy_roll=joy[rollA];
	joy_pitch=joy[pitchA];
	joy_yaw=joy[yawA];
	joy_throttle=joy[collA];

	handle_axis(0,joy_roll);  // roll
	handle_axis(1,joy_pitch);  // pitch
	handle_axis(2,joy_throttle);  // coll
	handle_axis(3,joy_yaw);  // yaw

	gui->rollSlider->value(joy[0]);
	gui->pitchSlider->value(joy[1]);
	gui->collSlider->value(joy[2]);
	gui->yawSlider->value(joy[3]);

	if( joyinfo.dwFlags & JOY_RETURNBUTTONS )
	{
		for (i=0; i <= 6; ++i) {
			Fl_Group *g = gui->buttons;

			Fl_Button *b = (Fl_Button*) g->child( i );
			if (joyinfo.dwButtons & JOY_BUTTON_FLAG(i) )
				b->value( 1 );
			else b->value( 0 );
			joy_button[i] = joyinfo.dwButtons & JOY_BUTTON_FLAG(i) ;	
		}
	}

	Fl::add_timeout(.01,joy_handler);
	
	return;
}

static void
null_handler(
	void *			UNUSED( user_arg )
)
{
	Fl::add_timeout(
		.01,
		null_handler
	);
	
}

/*
 *  Read the state from the user
 */
static void
send_jcmds(
	void *			UNUSED( user_arg )
)
{
	int			axis;

	/* Send our commands */
	for( axis=0 ; axis<8 ; axis++ )
	{
		joy_axis_t *values	= &limits[axis];

		if( !values->name || !values->name[0] )
			continue;

		set_parameter( values->name, values->last );
	}

	Fl::add_timeout(
		.03,
		send_jcmds
	);
	
}


void
reconnect_joy(
	UINT		uJoyID
)
{
	MMRESULT result;
	JOYINFOEX joyinfo;
	
	joyinfo.dwSize = sizeof(joyinfo);
	joyinfo.dwFlags = JOY_RETURNALL;

	int tempJoyID = (int) gui->guiJoyID->value();
	
	result = joyGetPosEx(tempJoyID, &joyinfo);
	if ( result != JOYERR_NOERROR ) {
		gui->joy_status->value( 0 );
		Fl::add_timeout(
			.05,
			null_handler
		);
			
		return;
	}

	Fl::add_timeout(
		.01,
		joy_handler
	);

	Fl::add_timeout(
		.02,
		send_jcmds
	);

	joystickID=tempJoyID;
	gui->joy_status->value( 1 );
}

#endif

