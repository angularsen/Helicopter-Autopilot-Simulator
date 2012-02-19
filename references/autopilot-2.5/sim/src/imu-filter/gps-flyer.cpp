/* -*- indent-tabs-mode:T; c-basic-offset:8; tab-width:8; -*- vi: set ts=8:
 *  $Id: gps-flyer.cpp,v 1.2 2002/10/04 23:04:06 tramm Exp $
 *
 * (c) Trammell Hudson
 * (c) Aaron Kahn
 *
 * AHRS simulator based on Kalman filtering of the gyro and
 * accelerometer data.  Converted from Aaron's matlab code
 * to use the C++ math library.
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

#include <imu-filter/imu-filter.h>
#include <imu-filter/INS.h>

#include <controller/Attitude.h>

#include <mat/Conversions.h>
#include <mat/Quat.h>
#include <mat/Nav.h>

#include <iostream>
#include <fstream>
#include "macros.h"
#include "timer.h"

#include <sys/types.h>
#include <sys/stat.h>
#include <fcntl.h>


using namespace imufilter;
using namespace libcontroller;
using namespace libmat;
using namespace std;
using namespace util;


static bool		real_time	= false;
static int		dt_usec		= 32768;
static double		dt		= double(dt_usec) / 1000000.0;

static int		serial_fd;


/*
 *  Convert a GPS object into an ECEF position
 */
const Vector<3>
gps2xyz(
	const GPS &		/* gps */
)
{
	return Vector<3>( 0, 0, 0 );

/*
	const double		lat_rad = gps.latitude * C_DEG2RAD;
	const double		lon_rad = gps.longitude * C_DEG2RAD;

	const Vector<3>		llh(
		lat_rad,
		lon_rad,
		gps.wgs_alt
	);

	return ECEF2Tangent(
		llh2ECEF( llh ),
		lat_rad,
		lon_rad
	);
*/
}


/*
 *  Convert a GPS object into a body frame velocity.
 * We need an attitude estimate for this to work.
 *
 * XXX: Faked until I talk with Aaron
 */
const Vector<3>
gps2uvw(
	const GPS &		/* gps */,
	const Vector<3> &	/* theta */
)
{
	return Vector<3>( 0, 0, 0 );
}


int
main(
	int			argc,
	const char **		argv
)
{
	const char *		serial_dev = "/dev/ttyS0";

	if( argc > 1 )
		serial_dev = argv[1];

	serial_fd = open( serial_dev, O_RDWR, 0666 );
	if( !serial_fd )
	{
		perror( serial_dev );
		return -1;
	}


	IMU_filter		interface(
		serial_fd,
		real_time,
		dt
	);

	INS 			ins( dt );
	IMU &			imu( interface.imu );
	GPS &			gps( interface.gps );

	Attitude		attitude( dt );

	ofstream		angles	( "/tmp/angles.out" );
	ofstream		ppm	( "/tmp/ppm.out" );
	ofstream		accel	( "/tmp/accel.out" );
	ofstream		rates	( "/tmp/pqr.out" );
	ofstream		servos	( "/tmp/servos.out" );
	ofstream		xyz	( "/tmp/xyz.out" );

	interface.logfile( "/tmp/data.log" );

	int		last_heading_sample	= interface.heading_samples;
	int		last_imu_sample		= interface.imu_samples;
	int		last_gps_sample		= interface.gps_samples;


	/* Throw away a few samples first */
	while( last_heading_sample < 20 && interface.step() )
	{
		if( interface.heading_samples != last_heading_sample )
		{
			last_heading_sample = interface.heading_samples;

			printf( "Init: Heading=%3.2f\n",
				interface.heading
			);
		}

		if( interface.imu_samples != last_imu_sample )
		{
			last_imu_sample = interface.imu_samples;

			printf( "Init: "
				"Accel=%3.2f %3.2f %3.2f "
				"PQR=%3.2f %3.2f %3.2f\n",
				imu.accel[0],
				imu.accel[1],
				imu.accel[2],
				imu.pqr[0],
				imu.pqr[1],
				imu.pqr[2]
			);
		}

		if( interface.gps_samples != last_gps_sample )
		{
			last_gps_sample = interface.gps_samples;
			if( gps.quality == 0 )
			{
				printf( "Init: Bad GPS data\n" );
				continue;
			}

			printf( "Init: "
				"Position=%3.2f %3.2f %3.2f\n",
				gps.latitude,
				gps.longitude,
				gps.wgs_alt
			);
		}
				
	}

	cout << endl << endl << "Initial values:" << endl;
	cout << "xyz    = " << gps2xyz( gps ) << endl;
	cout << "accel  = " << imu.accel << endl;
	cout << "angles = " << accel2euler( imu.accel, interface.heading ) * C_RAD2DEG << endl;
	cout << endl;

	ins.initialize(
		gps2xyz( gps ),
		gps2uvw( gps, Vector<3>(0,0,0) ),
		imu.accel,
		imu.pqr,
		interface.heading
	);


	//printf( "%c[2J", 27 );

	while( interface.step() )
	{
		//printf( "%c[H\n", 27 );
		cout << endl;

		if( interface.imu_samples != last_imu_sample )
		{
			last_imu_sample = interface.imu_samples;

			cout << "IMU update: " << imu.accel << endl;
			ins.imu_update(
				imu.accel,
				imu.pqr
			);
		}

		if( interface.heading_samples != last_heading_sample )
		{
			last_heading_sample = interface.heading_samples;

			cout << "Compass update: " << interface.heading << endl;
			ins.compass_update(
				interface.heading
			);
		}

		if( interface.gps_samples != last_gps_sample )
		{
			last_gps_sample = interface.gps_samples;
			const Vector<3> xyz( gps2xyz( gps ) );
			cout << "GPS: " << xyz << endl;

			ins.gps_update(
				xyz,
				gps2uvw( gps, ins.theta )
			);
		}

		printf( "Time:     %3.2f\n",
			interface.time
		);

		printf( "Trace:    %1.4f = %3.2f %3.2f %3.2f %3.2f %3.2f %3.2f\n",
			ins.trace,
			ins.P[0][0],
			ins.P[1][1],
			ins.P[2][2],
			ins.P[3][3],
			ins.P[4][4],
			ins.P[5][5]
		);

		printf( "Angles:   %2.3f, %2.3f, %2.3f (%2.3f)\n",
			ins.theta[0] * C_RAD2DEG,
			ins.theta[1] * C_RAD2DEG,
			ins.theta[2] * C_RAD2DEG,
			interface.heading * C_RAD2DEG
		);

		angles
			<< interface.time << " "
			<< ins.theta[0] << " "
			<< ins.theta[1] << " "
			<< ins.theta[2] << endl;

		accel
			<< interface.time << " "
			<< imu.accel[0] << " "
			<< imu.accel[1] << " "
			<< imu.accel[2] << endl;

		rates
			<< interface.time << " "
			<< imu.pqr[0] << " "
			<< imu.pqr[1] << " "
			<< imu.pqr[2] << " "
			<< ins.pqr[0] << " "
			<< ins.pqr[1] << " "
			<< ins.pqr[2] << endl;

		xyz
			<< interface.time << " "
			<< ins.xyz[0] << " "
			<< ins.xyz[1] << " "
			<< ins.xyz[2] << " "
			<< ins.uvw[0] << " "
			<< ins.uvw[1] << " "
			<< ins.uvw[2] << endl;
	}

}
