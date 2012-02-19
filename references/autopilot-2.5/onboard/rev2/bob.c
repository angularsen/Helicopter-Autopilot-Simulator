/**
 *  $Id: bob.c,v 2.0 2002/09/22 02:10:17 tramm Exp $
 *
 */
#include <io.h>
#include <progmem.h>

#include "soft_uart.h"
#include "string.h"

#define ROWS		10
#define COLS		27

static void
bob_putc(
	char		a0
)
{
	soft_uart_putc( a0 );
	soft_uart_block();
}

/*
 *  Clear the screen
 */
void bob_clear( void )
{
	bob_putc( '{' );
	bob_putc( 'A' );
}


/*
 *  goto xy position
 */
void bob_goto(
	const uint8_t		col,
	const uint8_t		row
)
{
	bob_putc( '{' );
	bob_putc( col / 10 + '0' );
	bob_putc( col % 10 + '0' );
	bob_putc( row / 10 + '0' );
	bob_putc( row % 10 + '0' );
}


/*
 * Height in the range 0 to 66, with 11 rows and 6 steps per row.
 * The step characters are:
 */
void bob_bar(
	uint8_t			height,
	uint8_t			col
)
{
	uint8_t			row;
	uint8_t			step = 66;

	for( row = 0 ; row <= 11 ; row++, step -= 6 )
	{
		bob_goto( col, row );

		if( step > height )
			bob_putc( ' ' );

		else
		if( step + 6 > height )
		{
			bob_putc( '{' );
			bob_putc( 0x72 + height - step );
			bob_putc( 27 );
		}
		else
		{
			bob_putc( '{' );
			bob_putc( 0x77 );
			bob_putc( 27 );
		}
	}

}


void _draw_paddle(
	uint8_t			row,
	uint8_t			old,
	uint8_t			new
)
{
	uint8_t			i;

	bob_goto( row, old );
	for( i=0 ; i<6 ; i++ )
		bob_putc( ' ' );

	bob_goto( row, new );
	for( i=0 ; i<6 ; i++ )
		bob_putc( '-' );
}


static uint8_t		old_p1;
static uint8_t		old_p2;

void draw_paddles(
	int8_t			p1,
	int8_t			p2
)
{
	if( p1 > COLS - 6 )
		p1 = COLS - 6;
	if( p2 > COLS - 6 )
		p2 = COLS - 6;
	if( p1 < 0 )
		p1 = 0;
	if( p2 < 0 )
		p2 = 0;

	if( p1 != old_p1 )
	{
		_draw_paddle( 0, old_p1, p1 );
		old_p1 = p1;
	}

	if( p2 != old_p2 )
	{
		_draw_paddle( ROWS, old_p2, p2 );
		old_p2 = p2;
	}
}


/*
 */
uint8_t draw_ball( void )
{
	static int8_t		dx = 1;
	static int8_t		dy = 1;
	static int8_t		pos_x = 15;
	static int8_t		pos_y = 2;

	if( pos_x == COLS || pos_x == 0 )
		dx = -dx;

	if( pos_y == ROWS )
		return 2;

	if( pos_y == ROWS - 1 )
	{
		/* Check for collision with player 2 */
		int delta = pos_x - old_p2;
		if( 0 < delta && delta < 6 )
			dy = -dy;
	}

	if( pos_y == 0 )
		return 1;

	if( pos_y == 1 )
	{
		/* Check for collision with player 1 */
		int8_t delta = pos_x - old_p1;
		if( 0 < delta && delta < 6 )
			dy = -dy;
	}

	bob_goto( pos_y, pos_x );
	bob_putc( ' ' );

	pos_x += dx;
	pos_y += dy;

	bob_goto( pos_y, pos_x );
	bob_putc( 'o' );

	return 0;
}



void pong( void )
{
	uint8_t			i;
	uint8_t			c;

	bob_clear();
	while( !getc( &c ) )
		;

	for( i=0 ; i<30 ; i++ )
	{
		uint8_t			loser;

		puts( "p=" );
		put_uint8_t( i );
		putnl();

		draw_paddles( i, COLS - i );

		loser = draw_ball();
		if( loser )
			return;

		while( !getc( &c ) )
			;
	}
}
