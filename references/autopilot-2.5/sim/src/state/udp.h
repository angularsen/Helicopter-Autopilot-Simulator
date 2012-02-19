/* -*- indent-tabs-mode:T; c-basic-offset:8; tab-width:8; -*- vi: set ts=8:
 * $Id: udp.h,v 1.5 2003/03/25 17:42:49 tramm Exp $
 *
 * (c) Aaron Kahn
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
#ifndef _state_udp_h_
#define _state_udp_h_

#include "macros.h"
#ifndef WIN32
#include <stdint.h>
#endif
#include <sys/types.h>
#include <sys/socket.h>
#include <netinet/in.h>
#include <arpa/inet.h>

#ifdef WIN32
typedef unsigned int uint32_t;
#endif

/* Defined in /usr/include/linux/in.h */
typedef struct sockaddr_in host_t;

BEGIN_DECLS

extern int
udp_serve(
	int			port
);


extern int
udp_poll(
	int			fd,
	int			usec
);


extern int
udp_read(
	int			fd,
	host_t *		src,
	void *			buf,
	int			max_len
);


extern int
udp_send(
	int			fd,
	const host_t *		dest,
	uint32_t		type,
	const void *		buf,
	int			max_len
);


extern int
udp_send_raw(
	int			fd,
	const host_t *		dest,
	uint32_t		type,
	const struct timeval *	timestamp,
	const void *		buf,
	int			max_len
);



extern int
udp_self(
	int			fd,
	host_t *		self
);


extern char *
udp_parse(
	char *			buf,
	struct timeval **	when,
	uint32_t *		type
);


#ifdef __cplusplus
#include <iostream>

static inline std::ostream &
operator << (
	std::ostream &		out,
	const host_t &		host
)
{
	return out
		<< inet_ntoa( host.sin_addr )
		<< ":"
		<< ntohs( host.sin_port );
}
#endif

END_DECLS

#endif
