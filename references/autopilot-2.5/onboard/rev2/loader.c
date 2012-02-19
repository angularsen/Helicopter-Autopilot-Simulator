/* -*- indent-tabs-mode:T; c-basic-offset:8; tab-width:8; -*- vi: set ts=8:
 * $Id: loader.c,v 2.7 2003/03/22 17:38:20 tramm Exp $
 *
 * Allows an AVR with bootloader capabilities to write its own Flash/EEprom.
 * If the two buttons on the Rev2.2 board are held down at startup, the
 * board will enter programming mode.  Otherwise, a normal reset vector
 * is called, starting the user application.
 *
 * You can also send an escape character within the first second of
 * startup the enter the boot loader.
 *
 * Only the ATmega163 constants are in here, with a 2048 byte boot section.
 *
 * There are several compile time options that will reduce the size
 * of the executable if they are disabled:
 *
 *	ENABLE_EEPROM		Allow programming the EEPROM
 *	ENABLE_READ		Allow the programmer to read back the code
 *	ENABLE_FUSE_BITS	Allow the programmer to set the fuse bits
 *	ENABLE_BUTTON		Allow a button press on boot to start the loader
 *
 * With all of them turned off, the program just barely fits in a
 * 1k boot sector.
 * 
 * (c) 1996-1998 Atmel Corporation
 * (c) Bryce Denney
 * (c) 2002 Trammell Hudson <hudson@rotomotion.com>
 *
 * Fuse bits for a 1024 byte boot sector:
 *	df fa 7e ff ff 
 * uisp script:
ce
ss fuse
wr 0 df
wr 1 fa
wr 2 7e
wr 3 ff
wr 4 ff
du 0
q
 * 
 * Fuse bits for 2048 byte boot sector:
 * df f8 49 ff ff 
 *
 * uisp script to write them:
ce
ss fuse
wr 0 df
wr 1 f8
wr 2 49
wr 3 ff
wr 4 ff
du 0
q
 *
 * TODO: Come up with the 1024 byte boot sector fuse bits.
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

#include <avr/io.h>
#include <avr/signal.h>
#include <inttypes.h>

#ifdef ENABLE_BUTTON
#include "button.h"
#endif

#include "memory.h"

#define CLOCK		8
#define PARTCODE	0x66
#define sig_byte1	0x1E
#define sig_byte2	0x94
#define sig_byte3	0x02

/**
 * The start address for a 512 word boot sector is 0x3C00.
 * For a 1024 work boot sector it is 0x3800.
 *
 * This should be specified as -Ttext to ld in the Makefile.
 */
#define BOOT_SIZE	512ul

#define PAGESIZE	128ul
#define APP_END		(2 * ( 8192ul - BOOT_SIZE ))
#define APP_PAGES	(APP_END / PAGESIZE )

/*
 *  Program memory accessors are in assembly.S
 */
extern void
write_page(
	uint16_t		address,
	uint8_t			flag
);


#ifdef ENABLE_FUSE_BITS
extern void
write_lock_bits(
	uint8_t			val
);
#endif

#ifdef ENABLE_READ
extern uint16_t
read_program_memory(
	uint16_t		address,
	uint8_t			flag
);
#endif

extern void
fill_temp_buffer(
	uint16_t		data,
	uint16_t		addr
);




/*
 *  Simple UART functions that block until they are done.
 * We have nothing better to do in the boot loader...
 */
static void
putc(
	const unsigned char	c
)
{
	UDR = c;
	while( !bit_is_set( UCSRA, TXC ) )
		;
	sbi( UCSRA, TXC );
}

#ifdef ENABLE_PUTS
static void
_puts(
	const prog_char *	s
)
{
	unsigned char		c;
	while( (c = PRG_RDB( s++ )) )
		putc( c );
}

#define puts( s ) _puts( PSTR( s ) )
#endif

static unsigned char
getc( void )
{
	while( !bit_is_set( UCSRA, RXC ) )
		;
	return inp( UDR );
}


/*
 *  Bring the UART online at 38400 baud @ 8 Mhz.
 */
static inline void
uart_init( void )
{
#if CLOCK == 16
	UBRRL = 25;
#elif CLOCK == 8
	UBRR = 12;
#else
	#error "Unsupported clock!"
#endif
	sbi( UCSRB, RXEN );
	sbi( UCSRB, TXEN );
}


/*
 *  Jump to the user application
 */
typedef void (*reset)(void);

#define		run_user	((reset)0)



/*
 *  Our AVR910 compatible interface
 */
static inline void
process_char(
	unsigned char		c
)
{
#ifdef ENABLE_READ
	unsigned int		intval;
#endif
	unsigned char		output;

	static unsigned int	address;
	static unsigned int	data;
	static unsigned char	ldata;

	switch(c)
	{
	case 'A': /* Write address (MSB first) in word size increments*/
	{
		uint16_t	high = getc();
		address = ( (high<<8) | getc() ) << 1;

		goto new_line;
	}

	case 'c': /* Write program memory, low byte */
		ldata = getc();
		goto new_line;

	case 'C': /* Write program memory, high byte */
		data = ldata | ( getc() << 8 );
		fill_temp_buffer( data, address );
		address += 2;  
		goto new_line;
        
	case 'e': /* Chip erase of application section */
		for( address=0; address < APP_END; address += PAGESIZE)
			write_page( address, (1<<PGERS) + (1<<SPMEN) );
		goto new_line;

	case 'a': /* Auto increment */
	case 'n': /* Query multiword-write */
		output = 'Y';
		goto out_char;

	case 'M': /* Write many words at once */
	{
/*
 * Let PGSZ = size of one page of program memory, in words.
 * 
 * The 'M' is followed by the length byte, which must be
 * between 0 and the PGSZ.  The length specifies how many words of
 * data will follow.  After the length byte, there must an even
 * number of bytes of data:
 *   low  byte for address offset 0
 *   high byte for address offset 0
 *   low  byte for address offset 1
 *   high byte for address offset 1, etc.
 * There can be between 0 and PGSZ.  The exact number is determined
 * from the length byte.  The response sent back to the programmer is
 * a copy of the length byte, the address of the first word (2
 * bytes), then the address of the last word+2 (2 bytes), then a
 * return.
 *
 * NOTE: The 'M' command is equivalent to a series of 'c' and
 * 'C' commands with autoincrementing address.  You still must stop
 * at page boundaries and do the page write ('m').
 */
		unsigned char i, len = getc();
		unsigned int startaddr = address;

		for( i=0; i<len; i++ )
		{
			ldata = getc();
			data = ldata|(getc()<<8);
			fill_temp_buffer(data,address); //call asm routine. 
			address += 2;  
		}

		putc( len );
		putc( (startaddr >> 8 ) & 0xFF );
		putc( (startaddr >> 0 ) & 0xFF );
		putc( (address >> 8 ) & 0xFF );
		putc( (address >> 0 ) & 0xFF );
		goto new_line;
	}

#ifdef ENABLE_FUSE_BITS
	case 'l': /* Write lockbits */
		write_lock_bits( getc() );
		goto new_line;
#endif
       
	case 'm': /* Write page: Perform page write */
		write_page( address, (1<<PGWRT) + (1<<SPMEN) );
		// Fall through to goto new_line
        
	case 'P': /* Enter programming mode */
	case 'L': /* ??? */
		goto new_line;

	case 'p': /* ??? */
		output = 'S';
		goto out_char;
        
#ifdef ENABLE_READ
	case 'R': /* Read program memory */
		intval = read_program_memory( address, 0x00 );
		putc( intval>>8 & 0xFF ); /* send MSB */
		putc( intval>>0 & 0xFF ); /* send LSB */

		/* SPM uses Z pointer but the pointer is only 16bit */
		address += 2;
		break;
#endif

#ifdef ENABLE_EEPROM
	case 'D': /* Write to EEPROM? */
		__outw( address, EEARL );
		address++;
		outp( getc(), EEDR );
		sbi( EECR, EEMWE );
		while( bit_is_set( EECR, EEWE ) )
			;
		goto new_line;

	case 'd': /* Read from EEPROM? */
		__outw( address, EEARL );
		address++;
		sbi( EECR, EERE );
		putc( inp(EEDR) );
		break;
#endif

#ifdef ENABLE_FUSE_BITS
	case 'F': /* Read fuse bits */
		putc( read_program_memory( 0x0000, 0x09 ) );
		break;

	case 'r': /* Read lock bits */
		putc( read_program_memory( 0x0001, 0x09 ) );
		break;

	case 'N': /* Read high fuse bits */
		putc( read_program_memory( 0x0003, 0x09 ) );
		break;
#endif

	case 't': /* Return programmer type */
		putc( PARTCODE );
		putc( 0x00 );
		break;


	case 'x': /* ignored??? */
	case 'y': /* ignored??? */
	case 'T': /* ignored??? */
		getc();
		goto new_line;
       
	case 'S': /* Return software identifier */
	{
#ifdef ENABLE_PUTS
		puts( "AVRBOOT" );
#else
		const prog_char *s = PSTR( "AVRBOOT" );
		while( (c = PRG_RDB( s++ )) )
			putc( c );
#endif
		break;
	}
        
	case 'V': /* Return software version */
		putc( '2' );
		output = '4';
		goto out_char;

	case 'v': /* Return hardware version */
		putc( '1' );
		output = '0';
		goto out_char;

	case 's': /* Return signature byte */
		putc( sig_byte3 );
		putc( sig_byte2 );
		output = sig_byte1;
		goto out_char;

	case 'Z': /* Start application (never returns) */
		run_user();
		break;

	default:
		output = '?';
		goto out_char;
	}

	return;

new_line:
	output = '\r';
out_char:
	putc( output );
	return;
}


int
main( void )
{
	uint16_t i;
	uint8_t flash = 0;

#ifdef ENABLE_BUTTON
	button_init();
#endif

	uart_init();

	putc( '>' );
	putc( ' ' );

	/*
	 * They get a short while to let us know what they want to do.
	 * Either hit button 0 or send an escape character on the serial
	 * port.  Either way we go into flash programming mode.
	 *
	 * The delay is hard coded for roughly 1 second at 8 Mhz.
	 * Be quick!
	 */
	for( i=0 ; i < 65530 ; i++ )
	{
		uint16_t j;

#ifdef ENABLE_BUTTON
		if( !button_state( 0 ) )
		{
			flash = 1;
			break;
		}
#endif

		if( bit_is_set( UCSRA, RXC ) && inp(UDR) == 27 )
		{
			flash = 1;
			break;
		}

		for( j=0 ; j<1000 ; j++ )
			;
	}

	if( !flash )
		run_user();

	putc( '!' );

	while(1)
	{
		unsigned char c = getc();
		if( c == 27 )
			continue;
		process_char( c );
	}

	return 0; 
}
