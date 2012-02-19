/* -*- indent-tabs-mode:T; c-basic-offset:8; tab-width:8; -*- vi: set ts=8:
 *  $Id: IMU.cpp,v 2.8 2003/03/15 05:54:07 tramm Exp $
 *
 * (c) Trammell Hudson
 * (c) Aaron Kahn
 *
 * Simplistic IMU object that reads lines from the Rev 2.2 board
 * and proccesses them into sensor data.
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
#include <IMU.h>
#include <mat/Conversions.h>
#include "read_line.h"

using namespace util;
using namespace std;

namespace imufilter
{


/*
 * 2.4 boards do not do any digital low-pass filtering.  The 10 bit
 * values are output directly.
 *
 * My 2.4 board has the following ranges:
 *
 *		-1 G	0 G	+1 G		Scale
 *  AX   	0x303	0x225	0x15F	=> 	-0x1A4
 *  AY   	0x357	0x270	0x19C	=>	-0x1BB
 *  AZ   	0x137	0x210	0x2F3	=>	 0x1BC
 *
 * The scale factor for the gyros is computed as:
 *
 * 1.1 mV/deg/sec * 4.7 X gain = 5.17 mV/deg/sec
 * 5 V / 1024 bits = 4.88 mV/bit
 * 4.88 mv/bit / 5.17 mV/deg/sec = 0.944 deg/sec / bit
 *
 
 */
IMU::IMU(
) :
	ax_index( 5 ),
	ay_index( 6 ),
	az_index( 4 ),

	p_index( 3 ),
	q_index( 2 ),
	r_index( 7 ),

	accel_zero(
		0x0225,
		0x0270,
		0x0210
	),

	accel_scale(
		-9.81 * 2.0 / 0x1A4,
		-9.81 * 2.0 / 0x1BB,
		 9.81 * 2.0 / 0x1BC
	),

	gyro_zero(
		0x0224,
		0x01C5,
		0x01D7
	),

	gyro_scale(
		0.9444 * C_DEG2RAD,
		0.9444 * C_DEG2RAD,
		0.9444 * C_DEG2RAD
	)
{
}


void
IMU::update(
	const char *		line
)
{
	int			samples[8];
	int			rc;

	if( (rc = nmea_split( line, samples, 8 )) < 8 )
	{
		cerr << "ADC: Bad ADC line -- expected 8, got "
			<< rc << ": '" << line << "'" << endl;
		return;
	}

	this->update(
		Vector<3>(
	 		samples[ this->ax_index ],
	 		samples[ this->ay_index ],
	 		samples[ this->az_index ]
		),
		Vector<3>(
			samples[ this->p_index ],
			samples[ this->q_index ],
			samples[ this->r_index ]
		)
	);
}


void
IMU::update(
	const Vector<3> &	accel,
	const Vector<3> &	pqr
)
{
	this->accel	= accel;
	this->pqr	= pqr;

	this->accel -= this->accel_zero;

	for( int i=0 ; i<3 ; i++ )
		this->accel[i] *= this->accel_scale[i];

	this->pqr -= this->gyro_zero;

	for( int i=0 ; i<3 ; i++ )
		this->pqr[i] *= this->gyro_scale[i];
}

}
