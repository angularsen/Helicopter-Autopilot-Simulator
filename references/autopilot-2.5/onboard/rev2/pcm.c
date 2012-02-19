/* -*- indent-tabs-mode:T; c-basic-offset:8; tab-width:8; -*- vi: set ts=8:
 * $Id: pcm.c,v 1.22 2003/02/03 05:30:50 tramm Exp $
 *
 * Decode a PCM signal on the ICP pin.
 *
 * (c) 2002 Trammell Hudson <hudson@swcp.com>
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
/**
 * The CPU clock is 8 MHz.
 * The bit clock is 150 usec or 150 * 8 == 1200 ticks.
 */
#define		CLOCK		(8ul)	/* Mhz */
#define		BIT_CLOCK	(150ul * CLOCK)	


/**
 *  Define VERBOSE to get raw field/frame data instead of
 * processed packets
 */
#undef VERBOSE
#undef RAW_DATA_PACKETS

#include <io.h>

#include "timer.h"
#include "uart.h"
#include "string.h"
#include "memory.h"


/*
 * Input is on the ICP (Port D pin 6).  These inlines wrap access to
 * the control registers to make it clearer what is happening.
 */
#define			PCM_PUD		PORTD
#define			PCM_PORT	PIND
#define			PCM_DDR		DDRC
#define			PCM_PIN		6

static inline void
capture_falling_edge( void )
{
	cbi( TCCR1B, ICES1 );
}


static inline void
capture_rising_edge( void )
{
	sbi( TCCR1B, ICES1 );
}



static inline void
capture_clear_flag( void )
{
	outp( 1 << ICF1, TIFR );
}


static inline uint8_t
capture_flag( void )
{
	return bit_is_set( TIFR, ICF1 );
}

static inline uint16_t
capture_wait( void )
{
	while( !capture_flag() )
		;

	return __inw( ICR1L );
}


/*
 * We use the 16 bit Timer 1B for our software uart.
 * These inlines just wrap access to the registers.
 */
static inline void
compare_disable( void )
{
	cbi( TIMSK, OCIE1B );
}


static inline void
compare_enable( void )
{
	sbi( TIMSK, OCIE1B );
}


static inline void
compare_set( 
	uint16_t		when
)
{
	__outw( when, OCR1BL );
}


static inline uint16_t
compare_get( void )
{
	return __inw( OCR1BL );
}

static inline void
compare_clear_flag( void )
{
	outp( 1 << OCF1B, TIFR );
}



static void
pcm_init( void )
{
	/* No noise cancelation */
	cbi( TCCR1B, ICNC1 );

	/* Set ICP to input, internal pull up */
	sbi( PCM_PUD, PCM_PIN );
	cbi( PCM_DDR, PCM_PIN );

	/* Disable timer overflow for now and run at Clk/32 */
	cbi( TIMSK, TOIE2 );
	outp( 0x02, TCCR2 );

	/* Disable the OC1B servo bank */
	compare_disable();

	/* We will trigger on port C */
	outp( 0xFF, DDRC );
	outp( 0x00, PORTC );

	/* We have to enable to ADC to sink the opamp output */
	outp( 0x00, PORTA );
	outp( 0x00, DDRA );
}


static uint16_t		sync_offset;
static uint16_t		pcm_byte;
static volatile uint8_t	pcm_ready;
static uint8_t		bit_counter;

/*
 * Store the fields here until the can be processed
 */

struct pcm_packet
{
	uint16_t	pos;
	uint8_t		aux;
	uint8_t		diff;
	uint8_t		crc;
};

static struct pcm_packet	packets[4];
static uint16_t			raw[4];
static uint16_t			pos[9];


/*
 * Wait for a sync pulse of 3 ms
 */
static void
wait_for_sync(
	uint8_t			hi,
	uint8_t			flag
)
{
	uint16_t		diff;
	uint16_t		stop;

	outp( 0x00, PORTC );

	do {
		uint16_t		start;

		/*
		 * Catch the start of the sync pulse.
		 * Rising for hi sync, falling for low sync
		 */
		if( hi )
			capture_rising_edge();
		else
			capture_falling_edge();

		capture_clear_flag();
		start = capture_wait();

		/*
		 * Catch the end of the sync pulse.
		 * Falling for hi sync, rising for lo sync.
		 */
		if( hi )
			capture_falling_edge();
		else
			capture_rising_edge();

		capture_clear_flag();
		stop = capture_wait();

		diff = stop - start;

	} while( diff < 2500 * CLOCK );

#if 0
	/* Wait for the next edge */
	if( hi )
		capture_rising_edge();
	else
		capture_falling_edge();

	capture_clear_flag();
	stop = capture_wait();
#endif

	outp( flag, PORTC );

	/*
	 * The bit clock is always 300 useconds long.
	 * We don't actually need the length of the sync pulse.
	 */
	compare_set( stop + sync_offset );
	compare_clear_flag();

	/* Reset our counters for the interrupt routine */
	bit_counter	= 10;
	pcm_ready	= 0;

	/* Enable our interrupt */
	compare_enable();
}



/**
 *  Sample the bits when ever our timer interrupt fires
 */
SIGNAL( SIG_OUTPUT_COMPARE1B )
{
	static uint16_t	byte;
	uint16_t	temp;

	sbi( PORTC, 7 );

	/* Push it onto our byte */
	temp = byte << 1;
	if( bit_is_set( PCM_PORT, PCM_PIN ) )
		temp |= 1;


	/* Interrupt us in 150 usec */
	compare_set( compare_get() + BIT_CLOCK );

	if( --bit_counter == 0 )
	{
		pcm_ready	= 1;
		pcm_byte	= temp;
		bit_counter	= 10;
		byte		= 0;
	} else {
		byte = temp;
	}

	cbi( PORTC, 7 );
}


/*
 * Convert the 10 bit encodings to 6 bit data packets.
 */
static uint8_t
ten2six(
	uint16_t		word
)
{
	uint8_t			high	= (word & 0x300) >> 8;
	uint8_t			low	= (word & 0x0FF) >> 0;

	if( high == 0x03 )
	{
		if( low == 0xF8 ) return 0x00; /* 1111111000 */
		if( low == 0xF3 ) return 0x01; /* 1111110011 */
		if( low == 0xE3 ) return 0x02; /* 1111100011 */
		if( low == 0xE7 ) return 0x03; /* 1111100111 */
		if( low == 0xC7 ) return 0x04; /* 1111000111 */
		if( low == 0xCF ) return 0x05; /* 1111001111 */
		if( low == 0x8F ) return 0x06; /* 1110001111 */
		if( low == 0x9F ) return 0x07; /* 1110011111 */

		if( low == 0x3F ) return 0x0B; /* 1100111111 */
		if( low == 0x1F ) return 0x0C; /* 1100011111 */
		if( low == 0x0F ) return 0x0D; /* 1100001111 */
		if( low == 0x87 ) return 0x0E; /* 1110000111 */
		if( low == 0xC3 ) return 0x0F; /* 1111000011 */

		if( low == 0xCC ) return 0x14; /* 1111001100 */
		if( low == 0x9C ) return 0x15; /* 1110011100 */
		if( low == 0x3C ) return 0x16; /* 1100111100 */
		if( low == 0x33 ) return 0x17; /* 1100110011 */
		if( low == 0xF0 ) return 0x18; /* 1111110000 */
		if( low == 0xE0 ) return 0x19; /* 1111100000 */
		if( low == 0x83 ) return 0x1A; /* 1110000011 */
		if( low == 0x07 ) return 0x1B; /* 1100000111 */
		if( low == 0x1C ) return 0x1C; /* 1100011100 */
		if( low == 0x98 ) return 0x1D; /* 1110011000 */
		if( low == 0x8C ) return 0x1E; /* 1110001100 */
		if( low == 0x38 ) return 0x1F; /* 1100111000 */

		if( low == 0x30 ) return 0x2C; /* 1100110000 */
		if( low == 0x18 ) return 0x2D; /* 1100011000 */
		if( low == 0x0C ) return 0x2E; /* 1100001100 */
		if( low == 0x03 ) return 0x2F; /* 1100000011 */

		if( low == 0xC0 ) return 0x35; /* 1111000000 */
		if( low == 0x80 ) return 0x36; /* 1110000000 */
		if( low == 0x00 ) return 0x37; /* 1100000000 */
 	} else
	if( high == 0x00 )
	{
		if( low == 0xFF ) return 0x08; /* 0011111111 */
		if( low == 0x7F ) return 0x09; /* 0001111111 */
		if( low == 0x3F ) return 0x0A; /* 0000111111 */

		if( low == 0xFC ) return 0x10; /* 0011111100 */
		if( low == 0xF3 ) return 0x11; /* 0011110011 */
		if( low == 0xE7 ) return 0x12; /* 0011100111 */
		if( low == 0xCF ) return 0x13; /* 0011001111 */
		
		if( low == 0xC7 ) return 0x20; /* 0011000111 */
		if( low == 0x73 ) return 0x21; /* 0001110011 */
		if( low == 0x67 ) return 0x22; /* 0001100111 */
		if( low == 0xE3 ) return 0x23; /* 0011100011 */
		if( low == 0xF8 ) return 0x24; /* 0011111000 */
		if( low == 0x7C ) return 0x25; /* 0001111100 */
		if( low == 0x1F ) return 0x26; /* 0000011111 */
		if( low == 0x0F ) return 0x27; /* 0000001111 */
		if( low == 0xCC ) return 0x28; /* 0011001100 */
		if( low == 0xC3 ) return 0x29; /* 0011000011 */
		if( low == 0x63 ) return 0x2A; /* 0001100011 */
		if( low == 0x33 ) return 0x2B; /* 0000110011 */

		if( low == 0x3C ) return 0x30; /* 0000111100 */
		if( low == 0x78 ) return 0x31; /* 0001111000 */
		if( low == 0xF0 ) return 0x32; /* 0011110000 */
		if( low == 0xE0 ) return 0x33; /* 0011100000 */
		if( low == 0xC0 ) return 0x34; /* 0011000000 */

		if( low == 0x60 ) return 0x38; /* 0001100000 */
		if( low == 0x70 ) return 0x39; /* 0001110000 */
		if( low == 0x30 ) return 0x3A; /* 0000110000 */
		if( low == 0x38 ) return 0x3B; /* 0000111000 */
		if( low == 0x18 ) return 0x3C; /* 0000011000 */
		if( low == 0x1C ) return 0x3D; /* 0000011100 */
		if( low == 0x0C ) return 0x3E; /* 0000001100 */
		if( low == 0x07 ) return 0x3F; /* 0000000111 */
	}

	return 0xFF;
}


static void
get_packet(
	uint8_t *		packet,
	uint8_t			invert
)
{
	uint8_t			byte_counter = 0;

	while( byte_counter < 4 )
	{
		uint8_t			byte;

		while( !pcm_ready )
			;

		pcm_ready	= 0;

		byte		= ten2six(
			invert ? ~pcm_byte : pcm_byte
		);

		raw[ byte_counter ] = pcm_byte;
		packet[ byte_counter++ ] = byte;
#ifdef VERBOSE
		put_uint12_t( pcm_byte );
#endif
	}
}


static void
process_packet(
	struct pcm_packet *	p,
	const uint8_t *		data
)
{
	p->aux	= (data[0] & 0x30) >> 4;

	p->diff	= (data[0] & 0x0F) >> 0;

	p->pos	= (data[1] & 0xFF) << 4
		| (data[2] & 0x3C) >> 2;

	p->crc	= (data[2] & 0x03) << 6
		| (data[3] & 0x3F) >> 0;

	if( data[0] == 0xFF
	||  data[1] == 0xFF
	||  data[2] == 0xFF
	||  data[3] == 0xFF
	)
	{
		p->aux = 0xFF;
	}

#ifdef RAW_DATA_PACKETS
{
	uint8_t			i;

	for( i=0 ; i<4 ; i++ )
	{
		if( data[i] == 0xFF )
		{
			putc( 'X' );
			put_uint12_t( raw[i] );
		} else
			put_uint8_t( data[i] );
		putc( ' ' );
	}
}
#else
/*
	if( p->aux == 0xFF )
		putc( '!' );

	puts( " a=" );
	putc( hexdigit( p->aux ) );

	puts( " d=" );
	putc( hexdigit( p->diff ) );

	puts( " p=" );
	put_uint12_t( p->pos );

	puts( " c=" );
	put_uint8_t( p->crc );
*/
#endif
}


static void
update_pos(
	const struct pcm_packet *	packet,
	uint8_t			chan1,
	uint8_t			chan2
)
{
	if( packet->aux == 0xFF )
	{
		putc( '!' );
		return;
	}

	pos[chan1] = packet->pos;
	pos[chan2] += (int8_t) packet->diff - 8;
}


static void
get_frame(
	struct pcm_packet *	packets,
	uint8_t			invert,
	uint8_t			field
)
{
	uint8_t			data[4];
	struct pcm_packet *	packet;

	get_packet( data, invert );
	packet = &packets[0];
	process_packet( packet, data );
	update_pos( packet,
		field == 1 ? 0 : 1,
		field == 1 ? 1 : 0
	);

	put_uint12_t( pos[0] ); putc( ' ' );
	put_uint12_t( pos[1] ); putc( ' ' );


	get_packet( data, invert );
	packet = &packets[1];
	process_packet( packet, data );
	update_pos( packet,
		field == 1 ? 2 : 3,
		field == 1 ? 3 : 2
	);

	put_uint12_t( pos[2] ); putc( ' ' );
	put_uint12_t( pos[3] ); putc( ' ' );

	get_packet( data, invert );
	packet = &packets[2];
	process_packet( packet, data );
	update_pos( packet,
		field == 1 ? 4 : 5,
		field == 1 ? 5 : 4
	);

	put_uint12_t( pos[4] ); putc( ' ' );
	put_uint12_t( pos[5] ); putc( ' ' );


	get_packet( data, invert );
	packet = &packets[3];
	process_packet( packet, data );
	update_pos( packet,
		field == 1 ? 6 : 7,
		field == 1 ? 7 : 6
	);

	put_uint12_t( pos[6] ); putc( ' ' );
	put_uint12_t( pos[7] ); putc( ' ' );

	/* Disable our interrupt */
	compare_disable();
	putnl();
}




static void
get_field1( void )
{
	/* Offset of -35 and skip the first six bits of header */
	sync_offset	= BIT_CLOCK * 7 - 35 * CLOCK;

	wait_for_sync( 0, 1 );
	puts( "1:" );
	get_frame( packets, 1, 1 );

	/* Offset of -35 and skip the first eight bytes of header */
	sync_offset	= BIT_CLOCK * 9 - 35 * CLOCK;

	wait_for_sync( 0, 2 );
	puts( "2:" );
	get_frame( packets, 1, 2 );
}


static void
get_field2( void )
{
	/* Offset of +25 and skip the first six bits of header */
	sync_offset	= BIT_CLOCK * 6 + 25 * CLOCK;

	wait_for_sync( 1, 4 );
	puts( "3:" );
	get_frame( packets, 0, 1 );

	/* Offset of +25 and skip the first eight bits of header */
	sync_offset	= BIT_CLOCK * 8 + 25 * CLOCK;

	wait_for_sync( 1, 8 );
	puts( "4:" );
	get_frame( packets, 0, 2 );
}



int main( void )
{
	timer_init();
	uart_init();
	pcm_init();
	sei();

	puts( "PCM decoder" );
	putnl();

	while( 1 )
	{
		get_field1();
		get_field2();
	}
}
