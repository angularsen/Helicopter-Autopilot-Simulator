/* -*- indent-tabs-mode:T; c-basic-offset:8; tab-width:8; -*- vi: set ts=8:
 *  $Id: gpsins.cpp,v 1.2 2003/03/15 05:54:35 tramm Exp $
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

#include <imu-filter/INS.h>
#include <imu-filter/IMU.h>

#include <iostream>
#include <fstream>
#include <vector>
#include <string>
#include <mat/Conversions.h>
#include <mat/Vector_Rotate.h>
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



const double		dt = 20000.0 / 1000000.0;
int			verbose = 1;

int
main(
	int			argc,
	const char **		argv
)
{
	int			samples = 0;
	double			t = 0;
	state_t			state;


	Server			server;
	server.connect( "localhost", 2002 );
	server.handle( AHRS_STATE, Server::process_ahrs, (void*) &state );

	/* Throwing away first packet */
	server.get_packet();

	INS			ins( dt );

	while( 1 )
	{
		static double last_x;

		int rc = server.get_packet();
		if( rc < 0 )
			break;
		if( rc != AHRS_STATE )
			continue;

		last_x = state.x;

		const Vector<3> ned( state.x, state.y, state.z );
		const Vector<3> vel( state.vx, state.vy, state.vz );
		const Vector<3> acc( state.ax, state.ay, state.az );
		const Vector<3> pqr( state.p, state.q, state.r );
		const Vector<3> ang( state.phi, state.theta, state.psi );

		if( samples++ == 0 )
		{
			// Should use estimate angle instead
			const Vector<3> uvw( rotate3( vel, ang ) );

			cout << "Initilizing ins: " << ned << vel << uvw << endl;
			ins.initialize( ned, vel, acc, pqr, ang[2] );
			continue;
		}

		t += dt;

		ins.imu_update( acc, pqr );

		if( samples % 10 == 0 )
		{
			cerr << "Compass update" << endl;
			ins.compass_update( ang[2] );
		}

		if( samples % 50 == 0 )
		{
			// Should use estimate angle instead
			cout << "Position update" << endl;
			const Vector<3> uvw( rotate3( vel, ins.theta ) );
			ins.gps_update( ned, uvw );
		}

		cout << "Angle: " << ins.theta << endl;
		cout << "PQR:   " << ins.pqr << endl;
		cout << "NED:   " << ins.xyz << endl;
		cout << "UVW:	" << ins.uvw << endl;
		cout << "XYZ:	" << ned << endl;
		cout << "Acc:	" << acc << endl;
		cout << "PQR2:  " << pqr << endl;
		cout << "G:     " << ins.g << endl;
	}

	return 0;
}
