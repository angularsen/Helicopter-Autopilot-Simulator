/* -*- indent-tabs-mode:T; c-basic-offset:8; tab-width:8; -*- vi: set ts=8:
 *  $Id: imu-filter.cpp,v 2.11 2003/03/14 19:43:54 tramm Exp $
 *
 * (c) Trammell Hudson <hudson@rotomotion.com>
 *
 * Rev 2.2 board interface object.
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
#include <vector>
#include <cstring>
#include <sys/types.h>
#include <sys/stat.h>
#include <fcntl.h>

#include "macros.h"
#include "read_line.h"

#include <imu-filter/imu-filter.h>
#include <imu-filter/IMU.h>
#include <imu-filter/GPS.h>
#include <imu-filter/Radio.h>
#include <mat/Conversions.h>

using namespace std;
using namespace util;


namespace imufilter
{



/*
 *  ADC data gets handed off to the IMU class
 */
void
IMU_filter::handle_adc(
	const char *		line
)
{
	this->imu.update( line );

	if( this->real_time )
		this->time = double(stop( &this->start_time )) / 1000000.0;
	else
		this->time += dt;
}


/*
 * Compass data is just a single in.  We assume that it will
 * always be formatted like this.
 *
 * $GPHDM,xxxx
 * 01234567890
 *
 */
void
IMU_filter::handle_compass(
	const char *		line
)
{
	this->heading = double( strtol( line + 7, 0, 10 ) );

	if( this->heading > 180 )
		this->heading -= 360;
	this->heading *= C_DEG2RAD;

	this->heading_samples++;

}


/*
 * AHRS data from an upgraded 2.2 board
 */
void
IMU_filter::handle_angles(
	const char *		line
)
{
	double			theta[2];
	double			trace;

	sscanf( line+7, "%lf,%lf,%lf",
		&theta[0],
		&theta[1],
		&trace
	);

	this->ahrs.trace	= trace;
	this->ahrs.theta[0]	= theta[0];
	this->ahrs.theta[1]	= theta[1];
}


void
IMU_filter::handle_pqr(
	const char *		line
)
{
	double			pqr[2];

	sscanf( line+7, "%lf,%lf",
		&pqr[0],
		&pqr[1]
	);

	this->ahrs.pqr[0]	= pqr[0];
	this->ahrs.pqr[1]	= pqr[1];
}



/**
 *  Braindead code to read line at a time from the serial port.
 */
bool
read_char_line(
	int			fd,
	char *			buf,
	size_t			max_len
)
{
	size_t			i = 0;

	while(1)
	{
		char c;

		if( read( fd, &c, 1 ) <= 0 )
			return false;

		if( c == '\r' )
			continue;

		if( c == '\n' )
			break;

		buf[i++] = c;

		if( i > max_len - 1 )
			break;
	}

	buf[i] = '\0';

	return true;
}



bool
IMU_filter::step( void )
{
	fd_set			fds;
	int			rc;
	int			max_fd = this->serial_fd;

	FD_ZERO( &fds );
	FD_SET( this->serial_fd, &fds );

	FOR_ALL_CONST( handler_map_t, i, this->handlers,
	{
		FD_SET( i->first, &fds );
		if( max_fd < i->first )
			max_fd = i->first;
	} );

	
	rc = select( max_fd+1, &fds, 0, 0, 0 );
	if( rc < 0 )
	{
		perror( "select" );
		return true;
	}

	FOR_ALL_CONST( handler_map_t, i, this->handlers,
	{
		const handler_t &	h = i->second;

		if( FD_ISSET( i->first, &fds ) )
			h.handler( i->first, h.priv );
	} );

	if( !FD_ISSET( this->serial_fd, &fds ) )
		return true;

	char			line[ 256 ];
	int			len;

	if( !read_char_line( this->serial_fd, line, sizeof(line) ) )
	{
		perror( "read_char" );
		return false;
	}

	len = strlen( line );
	line[len++] = '\n';
	line[len] = '\0';

	if( this->log_fd >= 0 )
		write( this->log_fd, line, len );

	if( strncmp( line, "$GPADC", 6 ) == 0 )
	{
		this->handle_adc( line );
		this->imu_samples++;
	} else

	if( strncmp( line, "$GPPPM", 6 ) == 0 )
	{
		this->radio.update( line );
		this->ppm_samples++;
	} else

	if( strncmp( line, "$GPHDM", 6 ) == 0 )
	{
		this->handle_compass( line );
		this->heading_samples++;
	} else

	if( strncmp( line, "$GPANG", 6 ) == 0 )
	{
		this->handle_angles( line );
	} else

	if( strncmp( line, "$GPPQR", 6 ) == 0 )
	{
		this->handle_pqr( line );
		this->ahrs_samples++;
	} else

	if( strncmp( line, "$GPGGA", 6 ) == 0 )
	{
		this->gps.update( line );
		this->gps_samples++;
	} else

	if( strncmp( line, "$GPVGT", 6 ) == 0 )
	{
		this->gps.update( line );
		// Don't increment the sample count
	} else

	{
		// Ignore the line for now
	}

	return true;
}


IMU_filter::IMU_filter(
	int			fd,
	bool			real_time,
	double			dt
) :
	ahrs			( dt ),
	heading			( 0.0 ),

	imu_samples		( 0 ),
	ahrs_samples		( 0 ),
	ppm_samples		( 0 ),
	heading_samples		( 0 ),
	gps_samples		( 0 ),

	serial_fd		( fd ),
	log_fd			( -1 ),

	real_time		( real_time ),
	dt			( dt )
{
	// Start our timer so we know how much time has elapsed
	// between samples
	start( &this->start_time );
	this->time = 0;
}


bool
IMU_filter::logfile(
	const char *		filename
)
{
	if( this->log_fd >= 0 )
		close( this->log_fd );

	this->log_fd = open( filename, O_WRONLY | O_CREAT, 0666 );

	if( this->log_fd < 0 )
	{
		perror( filename );
		return false;
	}

	return true;
}

}
