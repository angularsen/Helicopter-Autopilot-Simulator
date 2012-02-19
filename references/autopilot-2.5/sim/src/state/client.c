/* -*- indent-tabs-mode:T; c-basic-offset:8; tab-width:8; -*- vi: set ts=8:
 * $Id: client.c,v 2.4 2003/03/13 22:51:48 tramm Exp $
 *
 * (c) Aaron Kahn
 * (c) Trammell Hudson
 *
 * Socket manipulation code for the client side
 *
 *************
 *
 *  This file is part of the autopilot simulation package.
 *
 *  For more details:
 *
 *	http://autopilot.sourceforge.net/
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

#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <unistd.h>
#include <errno.h>
#include <sys/time.h>
#include <sys/types.h>
#include <sys/socket.h>
#include <netdb.h>
#include <netinet/in.h>

#include <state/state.h>
#include <state/udp.h>
#include <state/commands.h>


/**
 *  Default communication is on the stdin / stdout file
 * descriptors.  If connect_state is called, the socket will
 * be used instead.
 */
static int		sock;
static host_t		server;


int
host_lookup(
	struct sockaddr_in *	server,
	const char *		hostname,
	int			port
)
{
	struct hostent *	hp;

	
	if( !(hp = gethostbyname(hostname)) )
		return -1;

	server->sin_family	= AF_INET;
	server->sin_port	= htons( port );
	memcpy( &server->sin_addr, hp->h_addr, hp->h_length );

	return 0;
}


int
connect_state(
	const char *		hostname,
	int			port
)
{
	printf( "Deprecated connect_state called.  Please fix!\n" );

	if( !hostname )
		hostname = getenv( "SIMSERVER" );
	if( !hostname )
		hostname = "localhost";

	if( !port )
	{
		const char *port_env = getenv( "SIMPORT" );
		if( port_env )
			port = atoi( port_env );
	}

	if( !port )
		port = 2002;
	
	if( host_lookup( &server, hostname, port ) < 0 )
	{
		perror( hostname );
		return -1;
	}

	if( (sock = socket(
		PF_INET,
		SOCK_DGRAM,
		0
	)) < 0 )
	{
		perror( "socket" );
		return -1;
	}


	/* Try to connect */
	udp_send(
		sock,
		&server,
		1,
		0,
		0
	);

	return sock;
}



int
set_parameter(
	int			type,
	double			value
)
{
	return udp_send(
		sock,
		&server,
		type,
		&value,
		sizeof(value)
	);
}


int
send_command(
	int			type
)
{
	return udp_send(
		sock,
		&server,
		type,
		0,
		0
	);
}


int
read_state(
	state_t *		state,
	int			forever
)
{
	int			frames = 5;
	int			frames_read = 0;
	static struct timeval	last_packet;

	while( udp_poll( sock, forever ? -1 : 1 ) > 0 && --frames > 0 )
	{
		int			len;
		char			buf[ 1024 ];
		struct timeval *	when;
		uint32_t		type;
		const char *		data;
		host_t			src;

		len = udp_read( sock, &src, buf, sizeof(buf) );
		data = udp_parse( buf, &when, &type );

/*
		printf( "%d:%06d: Got type %d\n",
			when->tv_sec,
			when->tv_usec,
			type
		);
*/

		if( timercmp( &last_packet, when, > ) )
		{
			printf( "\tOld packet\n" );
			continue;
		}

		if( type == SIM_QUIT )
		{
			return -1;
		}

		if( type == AHRS_STATE )
		{
			frames_read++;
			last_packet = *when;

			memcpy( state, data, sizeof(*state) );
		}
	}

	return frames_read;
}


