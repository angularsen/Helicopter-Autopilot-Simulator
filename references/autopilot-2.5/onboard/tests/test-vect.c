/**
 *  $Id: test-vect.c,v 2.0 2002/09/22 02:10:18 tramm Exp $
 */

#ifdef AVR
#include <io.h>
#else
#include <stdio.h>
#endif

#include "../avr/vector.h"

void bar( void )
{
	/* Nothing */
}

void
test_zero( void )
{
	v_t			v;
	m_t			m;

	v_zero( v );
	m_zero( m );
}


void
test_add( void )
{
	static v_t		b;
	static v_t		a;


	v_add( a, a, b, 3 );
}


void
test_madd( void )
{
	static m_t		a;
	static m_t		b;

	m_add( a, a, b, 3, 3 );
}


void
test_mult( void )
{
	static m_t		a;
	static m_t		b;
	static m_t		c;

	m_mult( c, a, b, 4, 4, 4 );
}

static inline void
mprint(
	const char *		title,
	m_t			M,
	iter_t			m,
	iter_t			n
)
{
	iter_t			i;
	iter_t			j;

	printf( "%s=\n", title );

	for( i=0 ; i<m ; i++ )
	{
		printf( "[" );
		for( j=0 ; j<n ; j++ )
			printf( " %lf", M[i][j] );
		printf( " ]\n" );
	}
}

int main( void )
{
	v_t			a = { 3, 4, 5, 6 };
	v_t			b = { 3, 9, 2, -1 };
	v_t			c;

	m_t			ma = {
		{ 3, 4, 5 },
		{ 2, 1, 9 },
		{ 1, 12, -1 },
	};

	m_t			mb = {
		{ 3, 4, 9 },
		{ 7, 5, -1 },
		{ 1, 1, 9 },
	};

	m_t			mc;

	mprint( "a", ma, 3, 3 );

	m33_inv( mb, ma );
	mprint( "inv", mb, 3, 3 );

	m_mult( mc, ma, mb, 3, 3, 3 );
	mprint( "eye", mc, 3, 3 );
}
	
