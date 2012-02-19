/* -*- indent-tabs-mode:T; c-basic-offset:8; tab-width:8; -*- vi: set ts=8:
 *  $Id: Radio.cpp,v 2.2 2002/10/20 19:33:41 tramm Exp $
 *
 * (c) Trammell Hudson <hudson@rotomotion.com>
 *
 * PPM decoder object.
 *
 * Accepts signals from the radio and translates them into
 * servo commands.
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
#include <imu-filter/Radio.h>
#include "read_line.h"

#include <iostream>
#include <cstdio>

using namespace std;
using namespace util;

namespace imufilter
{

Radio::Radio(
) :
	ppm_valid		(false),
	pulse_width_index	(8),

	/*
	 *  Futaba assignments.
	 *
	 * THESE MUST AGREE WITH THE ONBOARD CODE!
	 *
	 * It is at great risk to yourself and your hardware if they
	 * do not match up with what is installed on the board.
	 *
	 */
	collective_index	(2),
	throttle_index		(5),
	roll_index		(0),
	pitch_index		(1),
	yaw_index		(3),
	extra_index		(7),

	manual_index		(4),
	manual_threshold	(0x3000),

	mode_index		(6),
	mode_threshold_0	(0x2000),
	mode_threshold_1	(0x3800)
{
}


void
Radio::update(
	const char *		line
)
{
	/* Check for well formed line and split into values */
	int			values[9];

	if( nmea_split( line, values, 8 ) != 8 )
	{
		cerr << "Radio: Bad PPM line: " << line << endl;
		return;
	}

	this->collective	= values[ this->collective_index ];
	this->throttle		= values[ this->throttle_index ];
	this->roll		= values[ this->roll_index ];
	this->pitch		= values[ this->pitch_index ];
	this->yaw		= values[ this->yaw_index ];
	this->extra		= values[ extra_index ];

	/* Manual switch requires special handling */
	int manual_width	= values[ this->manual_index ];
	this->manual		= manual_width > this->manual_threshold;

	/* As does the mode switch */
	int mode_width		= values[ this->mode_index ];
	this->mode		= mode_width < this->mode_threshold_0 ? 0 :
				  mode_width < this->mode_threshold_1 ? 1 :
				  2;

	this->ppm_valid		= true;
}

/*
 *  Output a command to the servo controller
 */
int
Radio::output(
	char *			line,
	size_t			len
) const
{
	return snprintf( line, len,
		"S=%04x,%04x,%04x,%04x,%04x\n",
		this->collective,
		this->throttle,
		this->roll,
		this->pitch,
		this->yaw
	);
}



}
