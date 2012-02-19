/* -*- indent-tabs-mode:T; c-basic-offset:8; tab-width:8; -*- vi: set ts=8:
 * $Id: lcd.c,v 2.3 2002/10/21 01:48:37 tramm Exp $
 *
 * This implements a Hitachi 44780 LCD controller interface on Port C
 * in 4 bit mode.  The interface is one way only -- the W line can be
 * tied to ground on the display.
 *
 * Largely based on code from Peter Fluery's LCD library.
 *
 * (c) 2002 Trammell Hudson <hudson@rotomotion.com>
 *
 * Portions:
 * (c) 2000 Peter Fleury <pfleury@gmx.ch>  http://jump.to/fleury
 *
 **************
 *
 * Rev 2.2 board and multicolor cable pinout:
 *
 * Port C   Cable    LCD  Function
 *  Vcc      Black    1    Vcc
 *  Ground   White    2    Vdd
 *  0        Grey     4    Reg select
 *  1        Purple   6    Enable
 *  2        Blue     11   D4
 *  3        Green    12   D5
 *  4        Yellow   13   D6
 *  5        Orange   14   D7
 *  6        Red      15   Button A
 *  7        Brown    16   Button B
 *
 * Additionally, LCD pin 5 (R/W) should be shorted to ground.  LCD pin 3
 * (Contrast) should be connected to the wiper of a 10k pot.  Button A
 * and B are included and should short to ground when closed.
 *
 *************
 *
 *  This file is part of the autopilot onboard code package.
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
#include <io.h>
#include <sig-avr.h>
#include "lcd.h"


/*
 *  Definitions for the hook up
 */
#define			LCD_PORT		PORTC
#define			LCD_DDR			DDRC

#define			E_PIN			1
#define			RS_PIN			0

#define			LCD_DDRAM		7

#define			ROWS			2
#define			COLS			16

/*
 *  Buffer for the outgoing characters.  This is written to by the
 * user directly.
 */
uint8_t			lcd_buf[ COLS * ROWS ];


/*
 *  Output a 500 ns pulse @ 8 Mhz to strobe the enable line
 * on the controller input
 */
static inline void
lcd_e_toggle( void )
{
	sbi( LCD_PORT, E_PIN );
	asm volatile( "nop" );
	asm volatile( "rjmp _PC_+0" );
	cbi( LCD_PORT, E_PIN );
}


/*
 *  Busy wait for atleast us microseconds.  We're likely to be
 * a bit longer than requested.
 */
static void
delay(
	uint16_t		us
)
{
	while( --us )
	{
		asm volatile( "nop" );
		asm volatile( "nop" );
		asm volatile( "nop" );
		asm volatile( "nop" );
		asm volatile( "nop" );
		asm volatile( "nop" );
	}
}


/**
 *  If we had a bidirectional port, this would wait for the display
 * to signal ready.  We don't, so we just wait 160 usec.
 */
static inline void
lcd_waitbusy( void )
{
	delay( 160 );
}


/*
 *  In 4-bit mode, we write the high nibble, strobe E then write
 * the low nibble and strobe E again.  Total time is about 2 us.
 */
static inline void
lcd_write(
	uint8_t			data,
	uint8_t			rs
)
{
	rs = rs ? 1<<RS_PIN : 0;

	outp( rs
		| ((data>>2) & (0x0F << 2))
		| (inp(LCD_PORT) & 0xC0),
		LCD_PORT
	);

	lcd_e_toggle();

	outp( rs
		| ((data<<2) & (0x0F << 2))
		| (inp(LCD_PORT) & 0xC0),
		LCD_PORT
	);

	lcd_e_toggle();
	outp( inp(LCD_PORT) & 0xC0, LCD_PORT );
}


void
lcd_command(
	uint8_t			cmd
)
{
	lcd_waitbusy();
	lcd_write( cmd, 0 );
}

static void
_lcd_nl( void )
{
	/* Go to the second line: 0x40 */
	lcd_command( (1<<LCD_DDRAM ) + 0x40 );
}
	


/*
 *  _lcd_putc() actually writes the data to the display.
 * It does not wait for the LCD to be non-busy, so it should
 * only be called if you are certain that there is nothing
 * going on.
 *
 * The interrupt routine calls it, but no one else should.
 */
static inline void
_lcd_putc(
	uint8_t			c
)
{
		lcd_write( c, 1 );
}


/*
 *  These are only used during the initialization routines, which
 * is why they are static inline.
 */
static inline void
lcd_clear( void )
{
	/* Clear the display */
	lcd_command( 0x01 );
}


static inline void
lcd_home( void )
{
	/* Home the cursor */
	lcd_command( 0x02 );
}




/*
 *  Magic dance to bring the LCD online
 */
void
lcd_init( void )
{
	uint16_t		i;

	/* Configure the port as an output */
	outp( 0xFF, LCD_DDR );
	outp( 0x00, LCD_PORT );

	/* Wait at least 16 ms for the display to initialize */
	for( i=0 ; i<64 ; i++ )
		delay( 1000 );

	/* Do the magic dance to bring it up in 8 bit mode */
	outp( 0x03 << 2, LCD_PORT );
	lcd_e_toggle();
	delay( 6000 );

	outp( 0x03 << 2, LCD_PORT );
	lcd_e_toggle();
	delay( 200 );

	outp( 0x03 << 2, LCD_PORT );
	lcd_e_toggle();
	delay( 200 );

	/* Go into four bit mode */
	outp( 0x02 << 2, LCD_PORT );
	lcd_e_toggle();

	/* Configure 8x2 lines 5x7 font */
	lcd_command( 0x28 );
	delay( 400 );

	lcd_clear();
	delay( 400 );

	lcd_home();
	delay( 400 );

	/* Increment and shift */
	lcd_command( 0x07 );
	delay( 400 );

	/* Shift more? */
	lcd_command( 0x18 );
	delay( 400 );

	/* On with all blinking features */
	lcd_command( 0x0C );
	delay( 400 );

	/* Enable our interrupt to process the buffers */
	sbi( TIMSK, OCIE2 );
}


/*
 *  Simplistic task to print output on the display
 */
SIGNAL( SIG_OUTPUT_COMPARE2 )
{
	static uint8_t		cur;

	outp( inp(OCR2) + 2, OCR2 );

	if( cur < COLS )
	{
		_lcd_putc( lcd_buf[ cur++ + 0 ] );
	}
	else if( cur == COLS )
	{
		_lcd_nl();
		cur = COLS+1;
	}
	else if( cur <= COLS * 2 )
	{
		_lcd_putc( lcd_buf[ cur++ - 1 ] );
	} else {
		lcd_home();
		cur = 0;
		outp( inp(OCR2) + 10, OCR2 );
	}
}
