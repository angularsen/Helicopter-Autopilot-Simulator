/* -*- indent-tabs-mode:T; c-basic-offset:8; tab-width:8; -*- vi: set ts=8:
 * $Id: read_line.h,v 2.1 2003/03/08 05:24:24 tramm Exp $
 *
 * (c) Trammell Hudson
 *
 * Utility to read a line and split it into pieces
 *
 *************
 *
 *  This file is part of the autopilot ground station package.
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
#include <string>
#include <vector>
#include <stdint.h>
#include <inttypes.h>
#include <sys/types.h>
#include <sys/time.h>
#include <unistd.h>

#ifndef __cplusplus
#error "<read_line.h> is a C++ file"
#endif

namespace util
{


/**
 *  Profiling showed that we spent an inordinate amount of time
 * in ba_string::replace, due to the naive implementation of read_line.
 * We called s+=c every loop, which had to grow the string, etc.
 * It was painful, so a local buffer was used to accumulate the
 * output.
 */
static inline const std::string
read_line(
	std::istream &		is
)
{
	std::string		s;
	char			buf[ 1024 ];
	size_t			i = 0;

	while(1)
	{
		char c;

		if( !is.get( c ) )
			break;

		if( c == '\r' )
			continue;

		if( c == '\n' )
			break;

		buf[i++] = c;

		if( i < sizeof(buf)-1 )
			continue;

		buf[i] = '\0';
		s += buf;
	}

	buf[i] = '\0';
	s += buf;

	return s;
}


static inline std::vector<std::string>
split(
	const std::string &	s,
	const char		c	= ' '
)
{
	std::vector<std::string> l;
	const int		len	= s.length();
	int			start = 0;

	for( int i=0 ; i < len ; i++ )
	{
		if( s[i] != c )
			continue;

		l.push_back( std::string( s, start, i - start ) );
		start = i+1;
	}

	if( start != len )
		l.push_back( std::string( s, start, len - start ) );

	return l;
}

static inline
int
nmea_split(
	const char *		line,
	int *			values,
	int			max_values
)
{
	int			i;

	// Skip NMEA style header
	line = index( line, ',' );
	if( !line )
		return 0;

	for( i=0 ; i<max_values ; i++ )
	{
		char *			end_ptr;

		values[i] = strtol( line+1, &end_ptr, 16 );

		line = end_ptr;

		if( ! *line )
			break;
	}

	return i+1;
}



/**
 *  Returns -1 on error, 0 if not ready and 1 if ready for reading
 */
static inline int
readable(
	int			fd,
	int			usec = 0
)
{
	fd_set			fds;

	FD_ZERO( &fds );
	FD_SET( fd, &fds );

	struct timeval		tv = { 0, usec };

	return select(
		fd+1,
		&fds,
		0,
		0, 
		usec >= 0 ? &tv : 0
	);
}

};
