/**
 *  $Id: Observer.cpp,v 2.0 2002/09/22 02:07:32 tramm Exp $
 */
#include "Observer.h"
#include <iostream>
#include <cstdio>
#include <cstdlib>
#include <cstring>
#include <unistd.h>
#include <sys/types.h>
#include <sys/time.h>
#include <sys/socket.h>
#include <netinet/in.h>
#include <netdb.h>

using namespace std;


namespace Observer
{

static std::ostream &
operator << (
	std::ostream &		out,
	const client_t &	client
)
{
	uint32_t		bytes = client.sin_addr.s_addr;

	return out
		<< "[" << ((bytes >>  0) & 0xFF)
		<< "." << ((bytes >>  8) & 0xFF)
		<< "." << ((bytes >> 16) & 0xFF)
		<< "." << ((bytes >> 24) & 0xFF)
		<< ":" << ntohs( client.sin_port )
		<< "]";
}

bool
operator != (
	const sockaddr_in &	a,
	const sockaddr_in &	b
)
{
	return a.sin_addr.s_addr != b.sin_addr.s_addr
	||            a.sin_port != b.sin_port;
}


Server::Server(
	int			port
)
{
	int			s = socket(
		PF_INET,
		SOCK_DGRAM,
		0
	);

	if( s < 0 )
	{
		perror( "Server: socket" );
		return;
	}

	struct sockaddr_in	addr;

	addr.sin_family		= AF_INET;
	addr.sin_port		= htons( port );
	addr.sin_addr.s_addr	= INADDR_ANY;

	if( bind(
		s,
		(struct sockaddr*) &addr,
		sizeof( addr )
	) < 0 )
	{
		perror( "Server: bind" );
		return;
	}

	this->sock		= s;
}


Server::~Server()
{
	if( this->sock >= 0 )
		close( this->sock );
	this->sock		= -1;
}

bool
Server::connect(
	const char *		hostname,
	int			port
)
{
	if( hostname == 0 )
		hostname = "localhost";
	if( port == 0 )
		port = 2002;

	struct hostent *	hp;
	struct sockaddr_in	server;

	if( !(hp = gethostbyname( hostname )) )
	{
		perror( hostname );
		return false;
	}

	server.sin_family	= AF_INET;
	server.sin_port		= htons( port );
	memcpy(
		&server.sin_addr,
		hp->h_addr,
		hp->h_length
	);

	this->server		= server;
	return true;
}


void
Server::sendme(
	uint32_t		msgtype
)
{
	msg_hdr_t		hdr;

	hdr.command	= SENDME;
	hdr.type	= msgtype;
	gettimeofday( &hdr.tv, 0 );

	this->write(
		&this->server,
		&hdr,
		sizeof(hdr)
	);
}


void
Server::nosend(
	uint32_t		msgtype
)
{
	msg_hdr_t		hdr;

	hdr.command	= NOSEND;
	hdr.type	= msgtype;
	gettimeofday( &hdr.tv, 0 );

	this->write(
		&this->server,
		&hdr,
		sizeof(hdr)
	);
}

bool
Server::poll(
	int			usec
)
{
	if( this->sock < 0 )
		return false;

	fd_set			fds;

	FD_ZERO( &fds );
	FD_SET( this->sock, &fds );

	struct timeval		tv = { 0, usec };
	int			rc;

	rc = select(
		this->sock+1,
		&fds,
		0,
		0, 
		usec >= 0 ? &tv : 0
	);

	return rc;
}


ssize_t
Server::read(
	void *			buf,
	size_t			bufmax,
	client_t *		client
)
{
	if( this->sock < 0 )
		return -1;

	struct sockaddr_in	addr;
	socklen_t		addr_len = sizeof(addr);
	int			rc;

	rc = recvfrom(
		this->sock,
		buf,
		bufmax,
		0,
		(struct sockaddr *) &addr,
		&addr_len
	);

	if( rc < 0 )
	{
		perror( "recvfrom" );
		return -1;
	}

	*client = *(client_t*) &addr;

	return rc;
}


bool
Server::write(
	client_t *		dest,
	const void *		buf,
	size_t			buflen
)
{
	int			rc;

	rc = sendto(
		this->sock,
		buf,
		buflen,
		0,
		(struct sockaddr *) dest,
		sizeof( *dest )
	);

	if( rc < 0 )
		perror( "write" );

	return rc < 0;
}


void
Server::do_sendme(
	uint32_t		msgtype,
	client_t *		client
)
{
	client_list_t &		cl( this->clients[msgtype] );

	cl.push_back( *client );
}


void
Server::do_nosend(
	uint32_t		msgtype,
	client_t *		client
)
{
	client_list_t &		cl( this->clients[msgtype] );

	for( client_list_t::iterator i = cl.begin() ;
		i != cl.end() ;
		i++
	)
	{
		if( *i != *client )
			continue;
		cl.erase( i );
		break;
	}
}


void
Server::resend(
	uint32_t		msgtype,
	const void *		buf,
	size_t			len
)
{
	client_list_t &		cl( this->clients[msgtype] );

	for( client_list_t::iterator i = cl.begin();
		i != cl.end();
		i++
	)
	{
		this->write( i, buf, len );
	}
}


void
Server::send(
	uint32_t		msgtype,
	const void *		user_buf,
	size_t			len
)
{
	char			buf[ 65536 ];
	msg_hdr_t *		hdr = (msg_hdr_t*) &buf[0];

	hdr->command	= OBJECT;
	hdr->type	= msgtype;
	gettimeofday( &hdr->tv, 0 );

	if( len > sizeof(buf) )
	{
		cerr << "Server::send: len=" << len
			<< " > 65536" << endl;
		return;
	}

	memcpy( buf + sizeof( msg_hdr_t ), user_buf, len );

	this->write(
		&this->server,
		buf,
		sizeof(msg_hdr_t) + len
	);
}

bool
Server::handle()
{
	char			buf[ 4096 ];
	ssize_t			len;
	client_t		client;

	len = this->read( buf, sizeof(buf), &client );
	cout << "Read " << len << " bytes from " << client << endl;

	const msg_hdr_t *	hdr = (msg_hdr_t*) &buf[0];

	cout << "Command: "
		<< hex
		<< hdr->command
		<< ":"
		<< hdr->type
		<< dec
		<< endl;

	switch( hdr->command )
	{
	case NOP:
	case ACK:
	case NACK:
		break;
	case SENDME:
		this->do_sendme( hdr->type, &client );
		break;
	case NOSEND:
		this->do_nosend( hdr->type, &client );
		break;
	case OBJECT:
	{
		this->resend( hdr->type, buf, len );

		handler_map_t::iterator i = this->handlers.find( hdr->type );
		if( i == this->handlers.end() )
			break;
		handler_pair_t &handler( i->second );

		handler.first(
			hdr->type,
			buf + sizeof(*hdr), 
			len - sizeof(*hdr),
			handler.second
		);

		break;
	}
	default:
		cerr << "Unhandled command: "
			<< hex
			<< hdr->command
			<< ":"
			<< hdr->type
			<< dec
			<< endl;
		break;
	}

	return true;
}


/*
 *  Local handlers
 */
void
Server::subscribe(
	uint32_t		msgtype,
	handler_t		handler,
	void *			user_data
)
{
	this->handlers[msgtype] = handler_pair_t( handler, user_data );
}

void
Server::unsubscribe(
	uint32_t		msgtype
)
{
	this->handlers.erase( msgtype );
}

}
