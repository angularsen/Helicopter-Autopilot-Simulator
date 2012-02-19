/* -*- indent-tabs-mode:T; c-basic-offset:8; tab-width:8; -*- vi: set ts=8:
 * $Id: imu.c,v 1.1 2002/10/22 01:38:22 tramm Exp $
 *
 * IMU handling code.
 *
 * calib_init() must have been called before this to read the accelerometer
 * parameters from the EEPROM.
 *
 * (c) 2002 Trammell Hudson <hudson@swcp.com>
 *
 *************
 *
 *  This file is part of the autopilot onboard code package.
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
#include "imu.h"
#include "calib.h"
#include "ahrs.h"
#include "adc.h"

static int16_t		zero_pqr[2];


void
imu_update( void )
{
	int16_t			rate;

	rate		= adc_samples[ IMU_INDEX_P ] - zero_pqr[0];
	ahrs_pqr[0]	= rate * -3.14159 / 187.0;

	rate		= adc_samples[ IMU_INDEX_Q ] - zero_pqr[1];
	ahrs_pqr[1]	= rate *  3.14159 / 187.0;

	rate		= adc_samples[ IMU_INDEX_AX ] - bias_ax - ACCEL_OFFSET;
	ahrs_accel[0]	= rate * -accel_scale; // -9.78 / 213.0;

	rate		= adc_samples[ IMU_INDEX_AY ] - bias_ay - ACCEL_OFFSET;
	ahrs_accel[1]	= rate * accel_scale; //  9.78 / 213.0;
}


void
imu_init( void )
{
	zero_pqr[0]	= adc_samples[ IMU_INDEX_P ];
	zero_pqr[1]	= adc_samples[ IMU_INDEX_Q ];

	imu_update();
}
