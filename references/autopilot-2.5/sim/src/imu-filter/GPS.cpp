/* -*- indent-tabs-mode:T; c-basic-offset:8; tab-width:8; -*- vi: set ts=8:
 *  $Id: GPS.cpp,v 1.2 2002/09/29 22:13:07 tramm Exp $
 *
 * (c) Trammell Hudson <hudson@rotomotion.com>
 *
 * GPS decoder object
 *
 * Takes NMEA lines and produces position / velocity values.
 *
 * GPGGA sentence details from:
 *	http://home.mira.net/~gnb/gps/nmea.html#gga
 *
 * Sample sentences from Cm3-SV6:
 * $GPGGA,020314.0,3902.848,N,07706.833,W,1,4,002.3,,M,-033,M,,*50
 * $GPVTG,159,T,169,M,04.1,N,07.7,K*48 
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
#include <GPS.h>
#include <sys/types.h>
#include <cstring>
#include <cstdlib>
#include <iostream>

using namespace std;

namespace imufilter
{

GPS::GPS(
) :
	time		(0),
	latitude	(0),
	longitude	(0),
	quality		(0),
	num_sats	(0),
	hdop		(0),
	altitude	(0),
	wgs_alt		(0),
	track		(0),
	ground_speed	(0)
{
}


void
GPS::update(
	const char *		line
)
{
	if( line[3] == 'G' )
		this->gpgga_update( line );
	else
		this->gpvgt_update( line );
}



static int
strntol(
	char *			line,
	size_t			len,
	char **			line_out,
	int			base
)
{
	char			buf[ 8 ];
	if( len > 8 )
		len = 8;

	strncpy( buf, line, len );

	buf[len] = '\0';

	if( line_out )
		*line_out = line + len;

	return strtol( buf, 0, base );
}


/*
 * $GPGGA,020314.0,3902.848,N,07706.833,W,1,4,002.3,,M,-033,M,,*50
 */
void
GPS::gpgga_update(
	const char *		line_in
)
{
	// Gross hack to get around strtol's non-const second argument
	char *			line = (char*) line_in;


	// Skip $GPGGA
	line = index( line, ',' );
	if( !line )
		return;
	line++;

	// Convert the time to seconds past midnight (GMT)
	int			hours	= strntol( line, 2, &line, 10 );
	int			min	= strntol( line, 2, &line, 10 );
	int			sec	= strntol( line, 2, &line, 10 );

	this->time = hours * 3600 + min * 60 + sec;

	// Skip .0,
	line = index( line, ',' );
	if( !line )
		return;
	line++;

	// Convert the lattitude
	double			lat	= strntol( line, 2, &line, 10 );
	double			lat_min	= strtod( line, &line );

	this->latitude = lat + lat_min / 60.0;

	// Find the sign for the lattitude.  South is negative
	if( line[1] == 'S' )
		this->latitude *= -1;
	line += 3; // Skip ,S,

	// Convert the longitude
	double			lng	= strntol( line, 3, &line, 10 );
	double			lng_min	= strtod( line, &line );

	this->longitude = lng + lng_min / 60.0;

	if( line[1] == 'W' )
		this->longitude	*= -1;
	line += 3; // Skip ,W,
	

	// Now for the easier ones...

	this->quality	= strtol( line, &line, 10 );
	line++;		// Skip ,

	this->num_sats	= strtol( line, &line, 10 );
	line++;		// Skip ,

	this->hdop	= strtod( line, &line );
	line++;		// Skip ,

	this->altitude	= strtod( line, &line );
	line += 3;	// Skip ,M,

	this->wgs_alt	= strtod( line, &line );
	line += 3;	// Skip ,M,

	cerr << "GPS:"
		<< " Time=" << this->time
		<< " Lat=" << this->latitude
		<< " Long=" << this->longitude
		<< " fix=" << this->quality
		<< " sats=" << this->num_sats
		<< " hdop=" << this->hdop
		<< " alt=" << this->altitude
		<< " wgs=" << this->wgs_alt
		<< endl;
}


void
GPS::gpvgt_update(
	const char *		line
)
{
	// Don't do anything yet
}

}
