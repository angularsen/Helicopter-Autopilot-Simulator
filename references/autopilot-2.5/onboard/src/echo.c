/**
 *  $Id: echo.c,v 2.0 2002/09/22 02:10:18 tramm Exp $
 *
 * Just read from the serial port and write it back out.
 * Nothing to it, but a great test of the boards.
 */
#include "avr.h"

int main( void )
{
	avr_init();

	puts( "echo: Send me data" );
	putnl();

	while( 1 )
	{
		uint8_t c;

		while( getchar( &c ) < 0 )
			;

		puthex( time() );
		puts( ": read: " );
		putc( c );
		putnl();
	}

	return 0;
}
