/**
 *  $Id: testmain.cpp,v 2.0 2002/09/22 02:07:32 tramm Exp $
 */
#include "Observer.h"
#include <iostream>

using namespace std;

int main( void )
{
	Observer::Server s( 2002 );

	cout << "Waiting for connections" << endl;
	while( 1 )
	{
		s.poll( -1 );
		cout << "Got data" << endl;
		s.handle();
	}
}

#if 0
class Sensor
{
	static const uint32_t	type	= 0xDEAD;

public:
	Sensor(
		Observer &	o
	) :
		handler( this, o, type )
	{
	}

	bool
	operator() (
		const Observer::header_t *	hdr,
		const void *			buf,
		size_t				buf_len
	) {
		cerr << "Sensor data!" << endl;
		return true;
	}


private:
	Handler<Sensor>		handler;
};


int main()
{
	Observer o( 2002 );
	Sensor s( o );

	while( 1 )
	{
		if( !o.poll( 500000 ) )
			continue;

		if( !o() )
		{
			cerr << "Read failed?" << endl;
			continue;
		}

		cerr << "Got an object..." << endl;
	}
}
#endif
