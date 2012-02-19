#ifndef _Observer_h_
#define _Observer_h_

#include <map>
#include <vector>
#include <stdint.h>
#include <sys/time.h>
#include <sys/types.h>
#include <sys/socket.h>
#include <netinet/in.h>

namespace Observer
{

typedef struct sockaddr_in	client_t;


struct msg_hdr_t
{
	uint32_t		command;
	uint32_t		type;
	struct timeval		tv;
};


typedef enum {
	NOP		= 0,
	ACK,
	NACK,
	SENDME,
	NOSEND,
	OBJECT
} command_t;


class Server
{
public:
	Server(
		int			port		= 0
	);

	~Server();

	bool
	connect(
		const char *		hostname	= 0,
		int			port		= 0
	);

	bool
	poll(
		int			usec		= 0
	);

	bool
	handle();

	void
	send(
		uint32_t		msgtype,
		const void *		buf,
		size_t			buflen
	);

	/*
	 *  Local handlers
	 */
	typedef void (*handler_t)(
		uint32_t		msgtype,
		const void *		buf,
		size_t			buflen,
		void *			user_data
	);

	void
	subscribe(
		uint32_t		msgtype,
		handler_t		handler,
		void *			user_data
	);

	void
	unsubscribe(
		uint32_t		msgtype
	);


	/*
	 *  Requests to the server
	 */
	void
	sendme(
		uint32_t		msgtype
	);

	void
	nosend(
		uint32_t		msgtype
	);


private:
	int			sock;
	client_t		server;

	typedef std::vector<
		client_t
	> client_list_t;

	typedef std::map<
		uint32_t,
		client_list_t		
	> client_map_t;

	client_map_t	clients;

	ssize_t
	read(
		void *			buf,
		size_t			bufmax,
		client_t *		client
	);

	bool
	write(
		client_t *		dest,
		const void *		buf,
		size_t			buflen
	);

	void
	resend(
		uint32_t		msgtype,
		const void *		buf,
		size_t			buflen
	);

	void
	do_sendme(
		uint32_t		msgtype,
		client_t *		client
	);

	void
	do_nosend(
		uint32_t		msgtype,
		client_t *		client
	);


	/*
	 *  Object handlers
	 */
	typedef std::pair<
		handler_t,
		void*
	> handler_pair_t;

	typedef std::map<
		uint32_t,
		handler_pair_t
	> handler_map_t;

	handler_map_t		handlers;
};

}
#endif
