/* -*- indent-tabs-mode:T; c-basic-offset:8; tab-width:8; -*- vi: set ts=8:
 * $Id: Server.h,v 2.2 2003/03/13 22:51:30 tramm Exp $
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

#ifndef _state_Server_h_
#define _state_Server_h_

#include <state/state.h>
#include <state/commands.h>

#include <state/udp.h>
#include <map>
#include <string>
#include <vector>

namespace libstate
{

class Server
{
public:
	Server(
		int			port = 0
	);

	Server(
		const char *		server_hostname,
		int			server_port,
		int			port = 0
	);

	~Server();


	/*
	 * Connect and send data to a server (not us)
	 */
	void
	connect(
		const char *		hostname,
		int			port
	);

	void
	send_command(
		int			type
	);

	void
	send_parameter(
		int			type,
		double			value
	);

	/*
	 * Send a packet to all clients.
	 */
	void
	send_packet
	(
		int			type,
		const void *		buf,
		size_t			len
	);


	/*
	 * Check for a waiting packet
	 */
	int
	poll(
		int			usec = 1
	);


	/*
	 * Returns the type of packet received, if valid.
	 */
	int
	get_packet();


	void
	add_client(
		const host_t *		src
	);

	void
	del_client(
		const host_t *		src
	);


	typedef void		(*handler_t)(
		void *			priv,
		const host_t *		src,
		int			type,
		const struct timeval *	when,
		const void *		data,
		size_t			len
	);



	void
	handle(
		int			type,
		handler_t		func,
		void *			data
	) {
		this->handlers[type].func = func;
		this->handlers[type].data = data;
	}

	/* You can select(2) on this file descriptor */
	int			sock;

	/* Call back from Fltk or other mainloops */
	static void
	update(
		int			fd,
		void *			server
	);

	/* Simple handler to fill in a state variable */
	static void
	process_ahrs(
		void *			priv,
		const host_t *		src,
		int			type,
		const struct timeval *	when,
		const void *		data,
		size_t			len
	);

private:
	host_t			server;

	void
	serve(
		int			port = 0
	);

	typedef std::vector<host_t>	clientmap_t;
	clientmap_t		clients;


	struct {
		handler_t		func;
		void *			data;
	} handlers[ 256 ];


};

}
#endif
