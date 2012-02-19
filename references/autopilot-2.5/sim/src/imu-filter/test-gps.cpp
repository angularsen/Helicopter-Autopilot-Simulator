/* -*- indent-tabs-mode:T; c-basic-offset:8; tab-width:8; -*- vi: set ts=8:
 *  $Id: test-gps.cpp,v 1.6 2002/10/04 18:53:18 tramm Exp $
 *
 * (c) Trammell Hudson
 *
 * GPS aided INS object test code.
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

#include <iostream>
#include <INS.h>
#include <mat/Vector.h>
#include <mat/SixDOF.h>
#include <mat/Quat.h>
#include <mat/Nav.h>
#include "timer.h"

using namespace imufilter;
using namespace std;
using namespace libmat;

int main( void )
{
	const double		mass	= 1;
	const double		heading	= 0;
	const double		dt	= 32768.0 / 1000000.0;
	unsigned long		total_time = 0;
	int			i;


	SixDOF			sixdof(
		mass,	// mass
		1,	// Ixx
		1,	// Iyy
		1,	// Izz
		0	// Ixz
	);

	const Vector<3>		zero( 0, 0, 0 );

	INS			ins( dt );

	ins.initialize(
		Vector<3>( 0, 0, 0 ),		// xyz
		zero,				// uvw
		Vector<3>( 0, 0, -9.81 ),	// accel
		zero,				// pqr
		heading
	);

	for( i=1 ; i < 300 ; i++ )
	{
		const Vector<3> accel( 0, 0, -9.81 );
		const Vector<3>	force( eulerDC( sixdof.theta ) * accel );

		//cerr << "**** step " << i << endl;

/*
		cout << "Force:  " << force << endl;
		cout << "Angles: " << sixdof.theta << endl;
		cout << "PQR:    " << sixdof.pqr << endl;
		cout << "XYZ:    " << sixdof.xyz << endl;
		cout << "UVW:    " << sixdof.uvw << endl;
		cout << endl;
*/


		sixdof.step(
			dt,
			9.81,			// G
			force,			// force
			Vector<3>( 0, 0, 0.1 )	// lmn
		);

		stopwatch_t		start_time;
		start( &start_time );

		if( i % 1 == 0 )
		{
			//cout << "imu_update=" << sixdof.force / mass << endl;
			ins.imu_update(
				sixdof.force / mass + noise<3>( 0.2, -0.2 ),
				sixdof.pqr + noise<3>( 0.4, -0.3 )
			);
		}

		if( i % 6 == 0 )
		{
			//cout << "compass_update=" << sixdof.theta[2] << endl;
			ins.compass_update(
				sixdof.theta[2] + drand48() * 0.2 - 0.1
			);
		}

		if( i % 30 == 0 )
		{
			//cout << "gps_update=" << sixdof.xyz << endl;
			ins.gps_update(
				sixdof.xyz,
				sixdof.uvw
			);
		}

		total_time += stop( &start_time );

		cout
			<< double(i) * dt
			<< " "
			<< ins.theta[0] << " "
			<< ins.theta[1] << " "
			<< ins.theta[2] << " "
			<< sixdof.theta[0] << " "
			<< sixdof.theta[1] << " "
			<< sixdof.theta[2] << endl;
	}

	cerr << i << " samples in " << total_time << " usec" << endl;
	cerr << double(i) * 1000000.0 / double(total_time) << " Hz" << endl;
	cerr << "Trace=" << ins.trace << endl;

}
