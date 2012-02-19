/* -*- indent-tabs-mode:T; c-basic-offset:8; tab-width:8; -*- vi: set ts=8:
 *  $Id: gpsins-sim.cpp,v 2.0 2002/09/22 02:07:30 tramm Exp $
 *
 * (c) Trammell Hudson
 * (c) Aaron Kahn
 *
 * GPS aided INS simulator.  Test the GPS+INS object and report timing values.
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
#include <time.h>
#include "macros.h"

#include <gpsins/GPSINS.h>
#include <mat/SixDOF.h>
#include <mat/Nav.h>

using namespace gpsins;
using namespace std;
using namespace libmat;

int
main(
	int			UNUSED( argc ),
	char **			UNUSED( argv )
)
{
	srand48( time(0) );

	const double		m = 1.0;		// mass

	GPSINS			gps;
	SixDOF			sixdof(
		m,		// Mass
		1,		// Ixx
		1,		// Iyy
		1,		// Izz
		0		// Ixz
	);

	const int		hz = 30;
	const double		dt = 1.0 / hz;

	for( int i = 0 ; i<hz * 30 ; i++ )
	{
		double t = i * dt;

		/*
		 *  Simulate the truth
		 */
		const Vector<3> 	G( 0.0, 0.0, -9.78 * m );
		Vector<3> 		force(
			eulerDC( sixdof.theta ) * G
		);

		// Add our motion
		force[1] += 0.0;
		force[1] += 0.0;
		force[2] += 0.0;

		const Vector<3> 	lmn(
			0.1 / (t+1),
			0.0,
			0.0
		);

		sixdof.step( dt, 9.78, force, lmn );

		/*
		 *  Generate our noisey inputs into the Kalman filter
		 */

		const Vector<3>		pqr(
			sixdof.pqr + noise<3>( 0.3, -0.3 )
		);

		const Vector<3>		accel(
			force / m + noise<3>( 0.2, -0.2 )
		);

		cout << "accel=" << accel << endl;


		/*
		 *  Feed inputs into the filter
		 */
		gps.imu_update(
			accel,
			pqr,
			dt
		);

		if( i % (hz / 5) == 0 )
			gps.compass_update(
				sixdof.theta[2] + drand48() * 5.0 - 2.5,
				dt
			);

		if( i % (hz / 1) == 0 )
			gps.gps_update(
				sixdof.xyz + noise<3>( 0.1, -0.1 ),
				sixdof.uvw + noise<3>( 0.1, -0.1 ),
				dt
			);

		const Vector<3> theta( gps.theta() );

		if( isnan( theta[0]  ) )
		{
			cerr << "NaN theta" << endl;
			break;
		}

		cout << "theta=" << sixdof.theta << endl;
		cout << "gpstheta=" << theta << endl;
		cout << "gpsxyz=" << gps.xyz() << endl;
		cout << "xyz=" << sixdof.xyz << endl;
		cout << "uvw=" << gps.uvw() << endl;
	}
}
