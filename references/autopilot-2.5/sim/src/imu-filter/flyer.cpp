/* -*- indent-tabs-mode:T; c-basic-offset:8; tab-width:8; -*- vi: set ts=8:
 *  $Id: flyer.cpp,v 2.23 2003/03/15 05:54:54 tramm Exp $
 *
 * (c) Trammell Hudson
 * (c) Aaron Kahn
 *
 * AHRS simulator based on Kalman filtering of the gyro and
 * accelerometer data.  Converted from Aaron's matlab code
 * to use the C++ math library.
 *
 * Exports AHRS data and PPM state.
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
#include <imu-filter/AHRS.h>

#include <controller/Attitude.h>

#include <mat/Conversions.h>
#include <mat/Quat.h>

#include <state/Server.h>
#include <getoptions/getoptions.h>

#include <iostream>
#include <fstream>
#include <vector>
#include "macros.h"
#include "timer.h"

#include <sys/types.h>
#include <sys/stat.h>
#include <fcntl.h>


using namespace imufilter;
using namespace libcontroller;
using namespace libstate;
using namespace libmat;
using namespace std;
using namespace util;


static double		dt		= 1024.0 * 256.0 / 8000000.0;
static int		verbose;

static ifstream		serial_port;
static int		serial_fd;


static void
servo_set(
	int		which,
	uint16_t	pos
)
{
	char		buf[4];

	buf[0] = 0xFF;
	buf[1] = (char) which;
	buf[2] = (char) (pos >> 8);
	buf[3] = (char) (pos >> 0);

	if( write( serial_fd, buf, 4 ) < 4 )
	{
		perror( "serial write" );
		return;
	}

	//serial_port.write( buf, 4 );
	//serial_port.flush();
}


#if 0
void
hover(
	ofstream &		servo_file,
	double			time,
	const AHRS &		ahrs,
	const Radio &		radio,
	Attitude &		attitude
)
{
	static bool		last_manual;
	static Vector<3>	desired_attitude;

	goto_xy( 2, 1 );

	if( !radio.ppm_valid )
	{
		printf( "NO PPM DATA                       \n" );
		return;
	}

	printf( "Mode %d: %s\n",
		radio.mode,
		  radio.mode == 0 ? "XY control with PPM "
		: radio.mode == 1 ? "XYZ control with PPM"
		: radio.mode == 2 ? "XYZ control with USB"
		:                   "UNKNOWN CONTROL MODE"
	);

	goto_xy( 9, 1 );
	//       123456789
	printf( "PPM      %05d %05d %05d %05d\n",
		radio.roll,
		radio.pitch,
		radio.yaw,
		radio.collective
	);
	printf( "Joy      % 05d % 05d % 05d % 05d\n",
		joy_delta[0],
		joy_delta[1],
		joy_delta[2],
		joy_coll
	);
		


	if( radio.manual )
	{
		if( !last_manual )
			clear();

		last_manual		= true;
		return;
	}

	if( last_manual )
	{
		// Prevent integrator wind up by reseting
		// the PID loop
		attitude.reset();
		last_manual = false;

		desired_attitude	= ahrs.theta;
		trim = Vector<3,int>( radio.roll, radio.pitch, radio.yaw );
	}

	goto_xy( 8, 1 );
	//       123456789
	printf( "Trim     %05d %05d %05d\n",
		trim[0],
		trim[1],
		trim[2]
	);


	/*
	 * Use the cyclic on the USB joystick for adjusting the trim.
	 * The range is from -20000 to +20000 or so.
	 * We want a +/- 16 deg range of attitude.
	 */
	const double		low	= 0x2600;
	const double		high	= 0x3800;
	const double		range	= 16.0 * C_DEG2RAD;
	const double		scale	= range / ( high - low );

	const Vector<3> 	desired_delta(
/*
		joy_delta[0] * scale,
		joy_delta[1] * scale,
		joy_delta[2] * scale
*/
		-(radio.roll - trim[0]) * scale,
		+(radio.pitch - trim[1]) * scale,
		-(radio.yaw - trim[2]) * scale * 10.0
	);

	attitude.attitude = desired_attitude + desired_delta;
	

	/*
	 *  Automatic flight.  Get commands from our controller
	 */
	const Vector<3>		commands(
		attitude.step(
			ahrs.theta,
			ahrs.pqr
		)
	);

	/*
	 * Pitch and roll are limited to +/- 1.5 degrees by the
	 * flight controller.  The simulator allows +/- 8 degrees.
	 * I'm not sure what we allow on the real helicopter, but
	 * lets assume 8.
	 *
	 * That means that 8000 ticks = 16 degrees, so 500 ticks is
	 * 1 degree.  For +/- 1.5 deg that is +/- 750 ticks.
	 *
	 * The pitch servo is "backwards".
	 */

	const Vector<3>	adjust( commands * (500.0 * C_RAD2DEG) );

	const Vector<3,int> cmd(
		(int)(trim[0] - adjust[0]),
		(int)(trim[1] - adjust[1]),
		(int)(trim[2] - adjust[2])
	);

	for( int i=0 ; i<3 ; i++ )
		servo_set( i, cmd[i] );

	servo_file << time
		<< " " << cmd[0]
		<< " " << cmd[1]
		<< " " << cmd[2]
		<< endl;

	goto_xy( 5, 1 );
	//       123456789
	printf( "Level    % 4.3f % 4.3f % 4.3f\n",
		desired_attitude[0] * C_RAD2DEG,
		desired_attitude[1] * C_RAD2DEG,
		desired_attitude[2] * C_RAD2DEG
	);

	printf( "Delta    % 4.3f % 4.3f % 4.3f\n",
		desired_delta[0] * C_RAD2DEG,
		desired_delta[1] * C_RAD2DEG,
		desired_delta[2] * C_RAD2DEG
	);

	printf( "Cmd      %05d %05d %05d\n",
		cmd[0],
		cmd[1],
		cmd[2]
	);
}



static void
handle_joy(
	const char *		data
)
{
	const int16_t *		cmds = (const int16_t*) data;

	joy_delta[0]	= (int16_t) ntohs( cmds[0] );
	joy_delta[1]	= (int16_t) ntohs( cmds[1] );
	joy_delta[2]	= (int16_t) ntohs( cmds[2] );
	joy_coll	= (int16_t) ntohs( cmds[3] );
}


void
udp_handler(
	int			fd
)
{
	int			len;
	host_t			src;
	char			buf[ 1024 ];
	char *			data;
	struct timeval *	when;
	uint32_t		type;
	static struct timeval 	last;

	len = udp_read( fd, &src, buf, sizeof(buf) );
	data = udp_parse( buf, &when, &type );

	switch( type )
	{
	case 1:
		/* New client */
		udp_clients.push_back( src );
		udp_send( fd, &src, 0x0002, 0, 0 );
		break;

	case 2:
		/* Ack from server */
		// Ignore for now
		break;

	case 10:
		/* Joystick command type */
		if( timercmp( &last, when, > ) )
			break;
		last = *when;

		handle_joy( data );
		break;

	default:
		cerr << "Unknown message type " << type << endl;
		break;
	}
}


void
send_all(
	uint32_t		type,
	const void *		buf,
	size_t			len
)
{
	for( vector<host_t>::const_iterator i = udp_clients.begin() ;
		i != udp_clients.end() ;
		i++
	) {
		const host_t &client( *i );

		udp_send(
			udp_fd,
			&client,
			type,
			buf,
			len
		);
	}
}




void
send_ahrs_data(
	const AHRS &		ahrs
)
{
	char			buf[ 256 ];
	int			len;

	len = snprintf( buf, sizeof(buf),
		"%3.4f,%3.4f,%3.4f,%3.4f",
		ahrs.theta[0],
		ahrs.theta[1],
		ahrs.theta[2],
		ahrs.trace
	);

	send_all( 3, buf, len );
}



void
send_ppm_data(
	const Radio &		radio
)
{
	int16_t			pulses[8];

	pulses[0] = htons( radio.roll );
	pulses[1] = htons( radio.pitch );
	pulses[2] = htons( radio.yaw );
	pulses[3] = htons( radio.collective );
	pulses[4] = htons( radio.throttle );
	pulses[5] = htons( radio.manual );
	pulses[6] = htons( radio.mode );
	pulses[7] = htons( radio.extra );

	send_all( 4, (const void*) pulses, sizeof( pulses ) );
}
#endif


static int
help( void )
{
	cerr <<
"Usage: flyer [options]\n"
"\n"
"	-h | --help			This help\n"
"	-V | --version			Display version\n"
"	-p | --port port		Port to serve data on\n"
"	-d | --device serial_dev	Serial device to use\n"
"	-s | --speed baud_rate		Serial speed\n"
"\n"
	<< endl;

	return -10;
}


static int
version( void )
{
	cerr << "$Id: flyer.cpp,v 2.23 2003/03/15 05:54:54 tramm Exp $" << endl;
	return -10;
}


int
main(
	int			argc,
	char **			argv
)
{
	const char *		serial_dev	= "/dev/ttyS0";
	int			serial_speed	= 38400;
	int			port		= 2002;
	int			real_time	= 0;

	int rc = getoptions( &argc, &argv,
		"h|?|help&",		help,
		"V|version&",		version,
		"v|verbose+",		&verbose,
		"p|port=i",		&port,
		"d|device=s",		&serial_dev,
		"s|speed=i",		&serial_speed,
		"r|realtime!",		&real_time,
		"t|dt=f",		&dt,
		0
	);

	if( rc == -10 )
		return EXIT_FAILURE;
	if( rc < 0 )
		return help();

	if( verbose )
	{
		cout << "serial " << serial_dev << "@" << serial_speed << endl;
		cout << "port " << port << endl;
		cout << "realtime " << real_time << endl;
		cout << "dt " << dt << endl;
	}

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

	AHRS & 			ahrs( interface.ahrs );
	IMU &			imu( interface.imu );
	Radio &			radio( interface.radio );

	Attitude		attitude( dt );

	ofstream		angles	( "/tmp/angles.out" );
	ofstream		ppm	( "/tmp/ppm.out" );
	ofstream		accel	( "/tmp/accel.out" );
	ofstream		rates	( "/tmp/pqr.out" );
	ofstream		servos	( "/tmp/servos.out" );

	interface.logfile( "/tmp/data.log" );

	Server			server( port );

	interface.add_fd( server.sock, Server::update, (void*) &server );

	int		last_heading_sample	= interface.heading_samples;
	int		last_imu_sample		= interface.imu_samples;
	int		last_ahrs_sample	= interface.ahrs_samples;
	int		last_ppm_sample		= interface.ppm_samples;

	/* Throw away a few samples first */
	while( last_imu_sample < 20 && interface.step() )
	{
		if( interface.heading_samples != last_heading_sample )
		{
			last_heading_sample = interface.heading_samples;

			printf( "Init: Heading=%4.3f\n",
				interface.heading
			);
		}

		if( interface.imu_samples != last_imu_sample )
		{
			last_imu_sample = interface.imu_samples;

			printf( "Init: "
				"Accel=%4.3f %4.3f %4.3f "
				"PQR=%4.3f %4.3f %4.3f\n",
				imu.accel[0],
				imu.accel[1],
				imu.accel[2],
				imu.pqr[0],
				imu.pqr[1],
				imu.pqr[2]
			);
		}
	}

	ahrs.initialize(
		imu.accel,
		imu.pqr,
		0
	);


	while( interface.step() )
	{
		/* Fake compass readings every 5 Hz */
		if( int( interface.time * 5 ) == int( interface.time * 5 + 0.9 ) )
		{
			interface.heading_samples++;
			interface.heading = 0;
		}

		if( interface.heading_samples != last_heading_sample )
		{
			static double last_heading;

			last_heading_sample = interface.heading_samples;

			if( interface.heading < 359 )
				last_heading = interface.heading;

			ahrs.compass_update(
				0
			);
		}


		if( interface.imu_samples != last_imu_sample )
		{
			last_imu_sample = interface.imu_samples;

			ahrs.imu_update(
				imu.accel,
				imu.pqr
			);

			if( verbose )
			{
				cout << "imu: accel=" << imu.accel << endl;
				cout << "imu: pqr  =" << imu.pqr << endl;
				cout << "ahrs: pqr =" << ahrs.pqr << endl;
			}

			interface.ahrs_samples++;
		}

		if( interface.ahrs_samples != last_ahrs_sample )
		{
			last_ahrs_sample = interface.ahrs_samples;

			state_t state;
			memset( (void*)&state, 0, sizeof(state) );

			state.ax	= ahrs.accel[0];
			state.ay	= ahrs.accel[1];
			state.az	= ahrs.accel[2];

			state.p		= ahrs.pqr[0];
			state.q		= ahrs.pqr[1];
			state.r		= ahrs.pqr[2];

			state.phi	= ahrs.theta[0];
			state.theta	= ahrs.theta[1];
			state.psi	= ahrs.theta[2];

			state.z		= -5;

			server.send_packet(
				AHRS_STATE,
				(void*) &state,
				sizeof(state)
			);
#if 0
			send_ahrs_data( ahrs );

			goto_xy( 1, 1 );
#endif

			printf( "Time: % 4.3f Trace: % 3.4f\n\n\n",
				interface.time,
				ahrs.trace
			);

			//       123456789
			printf( "Angles:  % 4.3f % 4.3f % 4.3f (% 4.3f)\n",
				ahrs.theta[0] * C_RAD2DEG,
				ahrs.theta[1] * C_RAD2DEG,
				ahrs.theta[2] * C_RAD2DEG,
				interface.heading * C_RAD2DEG
			);

			angles
				<< interface.time << " "
				<< ahrs.theta[0] << " "
				<< ahrs.theta[1] << " "
				<< ahrs.theta[2] << endl;

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
				<< ahrs.pqr[0] << " "
				<< ahrs.pqr[1] << " "
				<< ahrs.pqr[2] << endl;
		}


#if 0
		if( interface.ppm_samples != last_ppm_sample )
		{
			last_ppm_sample = interface.ppm_samples;

			hover(
				servos,
				interface.time,
				ahrs,
				interface.radio,
				attitude
			);

			ppm
				<< interface.time << " "
				<< radio.roll << " "
				<< radio.pitch << " "
				<< radio.yaw << " "
				<< radio.throttle << " "
				<< radio.collective << " "
				<< radio.manual << " "
				<< radio.mode << " "
				<< endl;

			send_ppm_data( radio );
		}
#endif
	}

	return 0;
}
