/* -*- indent-tabs-mode:T; c-basic-offset:8; tab-width:8; -*- vi: set ts=8:
 * $Id: udp.c,v 1.4 2003/03/08 05:28:23 tramm Exp $
 *
 * (c) Trammell Hudson
 *
 * UDP code for both server and client
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
#include <state/udp.h>

#include <unistd.h>
#include <netinet/in.h>
#include <sys/time.h>
#include <sys/types.h>
#include <sys/socket.h>
#include <sys/time.h>
#include <sys/uio.h>
#include <errno.h>

int
udp_serve(
	int			port
)
{
	int			s;
	struct sockaddr_in	addr;

	s = socket(
		PF_INET,
		SOCK_DGRAM,
		0
	);

	if( s < 0 )
		return -1;

	addr.sin_family		= AF_INET;
	addr.sin_port		= htons( port );
	addr.sin_addr.s_addr	= INADDR_ANY;

	if( bind( s, (struct sockaddr*) &addr, sizeof(addr) ) < 0 )
		return -1;

	return s;
}



int
udp_poll(
	int			fd,
	int			usec
)
{
	int			rc;
	fd_set			fds;

	struct timeval		tv;

	tv.tv_usec	= usec;
	tv.tv_sec	= usec / 1000000;

	FD_ZERO( &fds );
	FD_SET( fd, &fds );

	rc = select(
		fd + 1,
		&fds,
		0,
		0,
		usec < 0 ? 0 : &tv
	);

	if( rc > 0 )
		return rc;
	if( rc == 0 )
		return 0;
	if( errno == EINTR )
		return 0;

	/* Really an error... */
	return -1;
}
	
int
udp_read(
	int			fd,
	host_t *		addr,
	void *			buf,
	int			max_len
)
{
	int			rc;
	socklen_t		addr_len = sizeof( *addr );

	rc = recvfrom(
		fd,
		buf,
		max_len,
		0,
		(struct sockaddr*) addr,
		&addr_len
	);

	if( rc < 0 )
		return -1;

	return rc;
}


int
udp_send(
	int			fd,
	const host_t *		host,
	uint32_t		type,
	const void *		buf,
	int			len
)
{	struct timeval		now;
	gettimeofday( &now, 0 );

	return udp_send_raw( fd, host, type, &now, buf, len );
}


int
udp_send_raw(
	int			fd,
	const host_t *		host,
	uint32_t		type,
	const struct timeval *	now,
	const void *		buf,
	int			len
)
{
	struct iovec		vec[3];
	struct msghdr		hdr;

	vec[0].iov_base		= (void*) now;
	vec[0].iov_len		= sizeof(*now);

	vec[1].iov_base		= (void*) &type;
	vec[1].iov_len		= sizeof(type);

	vec[2].iov_base		= (void*) buf;
	vec[2].iov_len		= len;

	hdr.msg_name		= (void*) host;
	hdr.msg_namelen		= sizeof( *host );
	hdr.msg_iov		= vec;
	hdr.msg_iovlen		= 3;
#ifdef WIN32
	hdr.msg_accrights	= NULL;
	hdr.msg_accrightslen	= 0;
#else
	hdr.msg_control		= 0;
	hdr.msg_controllen	= 0;
	hdr.msg_flags		= 0;
#endif

	return sendmsg( fd, &hdr, 0 );
}


int
udp_self(
	int			fd,
	host_t *		self
)
{
	socklen_t		unused_len = sizeof(*self);

	return getsockname(
		fd,
		(struct sockaddr*) self,
		&unused_len
	);
}


char *
udp_parse(
	char *			buf,
	struct timeval **	when,
	uint32_t *		type
)
{
	if( when )
		*when = (struct timeval*) buf;
	buf += sizeof( struct timeval );

	if( type )
		*type = *(uint32_t*) buf;
	buf += sizeof( uint32_t );

	return buf;
}



