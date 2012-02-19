/* -*- indent-tabs-mode:T; c-basic-offset:8; tab-width:8; -*- vi: set ts=8:
 *  $Id: ahrs.cpp,v 2.2 2003/03/08 05:21:00 tramm Exp $
 *
 * (c) Trammell Hudson
 *
 * Simple program to test the IMU and AHRS codes
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

#include <imu-filter/AHRS.h>
#include <imu-filter/IMU.h>

#include <iostream>
#include <fstream>
#include <vector>
#include <string>
#include <mat/Conversions.h>
#include <state/state.h>
#include <state/Server.h>
#include <state/commands.h>
#include "macros.h"
#include "read_line.h"
#include "timer.h"

#include <sys/types.h>
#include <sys/stat.h>
#include <fcntl.h>


using namespace libmat;
using namespace imufilter;
using namespace libstate;
using namespace std;
using namespace util;


const double		dt = 1024 * 256 / 8000000.0;
int			verbose = 1;
const int		use_serial = 0;
ifstream		serial_port;
Server *		server;



int
main(
	int			argc,
	const char **		argv
)
{
	int			samples = 0;
	double			t = 0;
	state_t			state;

	if( use_serial )
	{
		const char *		serial_dev = "/dev/ttyS1";
		if( argc > 1 )
			serial_dev = argv[1];

		serial_port.open( serial_dev, ios::in );

		if( !serial_port )
		{
			cerr << "Unable to open " << serial_dev << endl;
			return -1;
		}
	} else {
		server = new Server;
		server->connect( "localhost", 2002 );
		server->handle( AHRS_STATE, Server::process_ahrs, (void*) &state );
	}


	IMU			imu;
	AHRS			ahrs( dt );

	while( 1 )
	{
		if( use_serial )
		{
			const string s = read_line( serial_port );

			if( s == "" )
				break;

			const char *line = s.c_str();

			if( strncmp( line, "$GPADC", 6 ) != 0 )
			{
				cerr << "Unknown line: " << line << endl;
				continue;
			}

			imu.update( line );
		} else {
			server->get_packet();
			imu.accel = Vector<3>( state.ax, state.ay, state.az );
			imu.pqr = Vector<3>( state.p, state.q, state.r );
		}

		if( verbose )
		{
			printf( "\nAccel: %+8.4f %+8.4f %+8.4f\n",
				imu.accel[0],
				imu.accel[1],
				imu.accel[2]
			);
			printf( "PQR:   %+8.4f %+8.4f %+8.4f\n",
				imu.pqr[0] * C_RAD2DEG,
				imu.pqr[1] * C_RAD2DEG,
				imu.pqr[2] * C_RAD2DEG
			);
		}

		if( samples++ == 0 )
		{
			ahrs.initialize(
				imu.accel,
				imu.pqr,
				0
			);
		} else {
			ahrs.imu_update(
				imu.accel,
				imu.pqr
			);
		}

		t++;
		if( int(t) == int(t+0.9) )
			ahrs.compass_update( 0 );


		if( verbose )
		{
			printf( "PQR2:  %+8.4f %+8.4f %+8.4f\n",
				ahrs.pqr[0] * C_RAD2DEG,
				ahrs.pqr[1] * C_RAD2DEG,
				ahrs.pqr[2] * C_RAD2DEG
			);

			printf( "Angls: %+8.4f %+8.4f %+8.4f\n",
				ahrs.theta[0] * C_RAD2DEG,
				ahrs.theta[1] * C_RAD2DEG,
				ahrs.theta[2] * C_RAD2DEG
			);
		}
	}

	return 0;
}
