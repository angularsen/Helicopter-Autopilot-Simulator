/**
 *  $Id: testclient.cpp,v 2.0 2002/09/22 02:07:32 tramm Exp $
 */
#include "Observer.h"
#include <iostream>

using namespace std;

void handler(
	uint32_t		type,
	const void *		buf,
	size_t			buflen,
	void *			user_data
)
{
	cout << "Received type " << type << " of len " << buflen << endl;
}


int main()
{
	Observer::Server c;

	cout << "Connecting" << endl;
	c.connect( "localhost", 2002 );

	c.subscribe( 1024, handler, 0 );

	cout << "Asking for 1024" << endl;
	c.sendme( 1024 );
	c.send( 1024, 0, 0 );

	while(1)
	{
		c.poll( -1 );
		c.handle();
	}
}
