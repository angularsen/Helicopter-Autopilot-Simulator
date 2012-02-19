/* -*- indent-tabs-mode:T; c-basic-offset:8; tab-width:8; -*- vi: set ts=8:
 * $Id: Server.cpp,v 2.5 2003/03/25 17:25:23 tramm Exp $
 *
 * (c) Aaron Kahn
 * (c) Trammell Hudson
 *
 * Socket manipulation code for the server side
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

#include <state/Server.h>
#include <state/udp.h>
#include <state/commands.h>

#include <iostream>
#include <cstdio>
#include <unistd.h>
#include <errno.h>
#include <sys/types.h>
#include <sys/socket.h>
#include <netinet/in.h>
#include "macros.h"

namespace libstate
{

using namespace std;
using namespace util;


static void
command_nop(
	void *			UNUSED( priv ),
	const host_t *		UNUSED( src ),
	int			UNUSED( type ),
	const struct timeval *	UNUSED( when ),
	const void *		UNUSED( data ),
	size_t			UNUSED( len )
)
{
}


static void
command_open(
	void *			priv,
	const host_t *		src,
	int			UNUSED( type ),
	const struct timeval *	UNUSED( when ),
	const void *		UNUSED( data ),
	size_t			UNUSED( len )
)
{
	Server *		self = (Server*) priv;

	self->add_client( src );
}



Server::Server(
	int			port
) :
	sock(-1)
{
	this->serve( port );
}


Server::Server(
	const char *		server_host,
	int			server_port,
	int			port
) :
	sock(-1)
{
	this->serve( port );
	this->connect( server_host, server_port );
}


void
Server::serve(
	int			port
)
{
	if( this->sock >= 0 )
		close( this->sock );

	// Create the socket
	this->sock = udp_serve( port );

	if( this->sock < 0 )
	{
		perror( "socket" );
		return;
	}

	int			yes = 1;

	if( setsockopt(
		this->sock,
		SOL_SOCKET,
		SO_REUSEADDR,
		&yes,
		sizeof(int)
	) < 0 )
	{
        	perror("setsockopt");
		return;
	}

	host_t			self;
	udp_self( this->sock, &self );

	cerr << "Server: socket on fd="
		<< this->sock
		<< " on port "
		<< ntohs( self.sin_port )
		<< endl;

	// Zero our handler table and install the defaults
	for( int i=0 ; i < COMMAND_MAX ; i++ )
		this->handle( i, 0, 0 );

	this->handle( COMMAND_NOP, command_nop, 0 );
	this->handle( COMMAND_ACK, command_nop, 0 );
	this->handle( COMMAND_OPEN, command_open, (void*) this );
}


void
Server::connect(
	const char *		hostname,
	int			port
)
{
	if( host_lookup( &this->server, hostname, port ) < 0 )
	{
		perror( hostname );
		return;
	}

	/* Try to connect */
	udp_send(
		this->sock,
		&this->server,
		COMMAND_OPEN,
		0,
		0
	);
}


void
Server::add_client(
	const host_t *		src
)
{
	cout << "Connection from " << *src << endl;
	udp_send( this->sock, src, COMMAND_ACK, 0, 0 );

	/* Don't add a client more than once */
	FOR_ALL_CONST( clientmap_t, client, this->clients,
		if( client->sin_addr.s_addr == src->sin_addr.s_addr
		&&  client->sin_port == src->sin_port
		)
			return;
	);

	this->clients.push_back( *src );
}


void
Server::del_client(
	const host_t *		src
)
{
	cout << "Deleting client " << *src << endl;

	FOR_ALL( clientmap_t, client, this->clients,
		if( client->sin_addr.s_addr == src->sin_addr.s_addr
		&&  client->sin_port == src->sin_port
		)
			this->clients.erase( client );
	);

	udp_send( this->sock, src, COMMAND_ACK, 0, 0 );
}


void
Server::send_packet(
	int			type,
	const void *		buf,
	size_t			len
)
{
	struct timeval		now;
	gettimeofday( &now, 0 );

	FOR_ALL_CONST( clientmap_t, i, this->clients,
		const host_t &client( *i );

		udp_send_raw(
			this->sock,
			&client,
			type,
			&now,
			buf,
			len
		);
	);
}


void
Server::send_command(
	int			type
)
{
	udp_send(
		this->sock,
		&this->server,
		type,
		0,
		0
	);
}


void
Server::send_parameter(
	int			type,
	double			value
)
{
	udp_send(
		this->sock,
		&this->server,
		type,
		&value,
		sizeof(value)
	);
}


int
Server::poll(
	int			usec
)
{
	return udp_poll( this->sock, usec );
}


int
Server::get_packet()
{
	int			len;
	host_t			src;
	char			buf[ 1024 ];
	char *			data;
	struct timeval *	when;
	uint32_t		type;

	len = udp_read( this->sock, &src, buf, sizeof(buf) );
	len -= sizeof( struct timeval ) + sizeof( uint32_t );
	data = udp_parse( buf, &when, &type );

	if( type > COMMAND_MAX )
	{
		cerr << "Invalid packet of type "
			<< type
			<< " from "
			<< src
			<< endl;
		return -1;
	}


	if( !this->handlers[type].func )
	{
		cerr << "Unhandled packet of type "
			<< type
			<< " from "
			<< src
			<< endl;
	} else {
		this->handlers[type].func(
			this->handlers[type].data,
			&src,
			type,
			when,
			data,
			len
		);
	}

	return type;
}


Server::~Server()
{
	close( this->sock );
}


void
Server::update(
	int			UNUSED( fd ),
	void *			voidp
)
{
	Server *		server = (Server*) voidp;
	server->get_packet();
}


void
Server::process_ahrs(
	void *			priv,
	const host_t *		src,
	int			UNUSED( type ),
	const struct timeval *	when,
	const void*		data,
	size_t			len
)
{
	static struct timeval 	last_packet;
	state_t *		state = (state_t*) priv;

	if( len != sizeof( state_t ) )
	{
		cerr
			<< when
			<< ": Invalid AHRS packet from "
			<< src
			<< ".  Expected "
			<< sizeof( state_t )
			<< " bytes, received "
			<< len
			<< endl;
		return;
	}

	if( timercmp( &last_packet, when, > ) )
	{
		cerr
			<< when
			<< ": Old AHRS packet from "
			<< src
			<< ".  Older than "
			<< last_packet
			<< endl;
		return;
	}

	last_packet	= *when;
	memcpy( (void*) state, (void*) data, sizeof(state_t) );
}




}
