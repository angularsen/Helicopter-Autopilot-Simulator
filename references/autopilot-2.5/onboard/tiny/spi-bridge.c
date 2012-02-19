#include <io.h>

/*
 * Configure ports:
 *
 * PB0 (Pin 5): Enable from V2x
 * PB1 (Pin 6): MOSI from V2x
 * PB2 (Pin 7): SCK from V2x
 *
 * PB3 (Pin 2): SCK out
 * PB4 (Pin 3): MOSI out
 *
 */

#define V2X_PORT	PINB
#define V2X_DDR		DDRB
#define V2X_PUD		PORTB
#define V2X_ENABLE	0
#define V2X_MOSI	1
#define V2X_SCK		2

#define SPI_PORT	PORTB
#define SPI_DDR		DDRB
#define SPI_MOSI	3
#define SPI_SCK		4


static inline uint8_t
get_bit( void )
{
	uint8_t			bit = 0;

	while( !bit_is_clear( V2X_PORT, V2X_SCK ) )
		;

	if( bit_is_set( V2X_PORT, V2X_MOSI ) )
		bit = 1;

	while( bit_is_clear( V2X_PORT, V2X_SCK ) )
		;

	return bit;
}

static inline uint8_t
get_byte( void )
{
	uint8_t	i;
	uint8_t	byte = 0;


	for( i=8 ; i ; i-- )
		byte = (byte << 1 ) | get_bit();

	return byte;
}


static inline void
put_byte(
	uint8_t			byte
)
{
	uint8_t			i;

	for( i = 8 ; i ; i-- )
	{
		if( byte & 0x10 )
			sbi( SPI_PORT, SPI_MOSI );
		else
			cbi( SPI_PORT, SPI_MOSI );

		cbi( SPI_PORT, SPI_SCK );
		byte <<= 1;
		sbi( SPI_PORT, SPI_SCK );
	}
}


static inline void
shuffle_bits( void )
{
	uint8_t			high;
	uint8_t			low;

	/*
	 *  Wait for enable from V2x
	 */
	while( !bit_is_clear( V2X_PORT, V2X_ENABLE ) )
		;

	while( bit_is_clear( V2X_PORT, V2X_ENABLE ) )
		;

	/*
	 *  Sync up with the clock
	 */
	high = get_byte();
	low = get_byte();

	/*
	 * Shuffle the bits so that the first high bit of each
	 * reflects which byte it was in.  We can only send 14 bits
	 * this way, but the V2X only outputs 12 anyway.
	 */
	high <<= 1;
	high |= low & 0x10;
	high |= 0x10;
	
	low &= 0x7F;

	/*
	 *  Resend on the output line
	 */
	put_byte( high );
	put_byte( low );
}


void main( void )
{
	cbi( V2X_DDR, V2X_ENABLE );
	cbi( V2X_DDR, V2X_MOSI );
	cbi( V2X_DDR, V2X_SCK );

	sbi( V2X_PUD, V2X_ENABLE );
	sbi( V2X_PUD, V2X_MOSI );
	sbi( V2X_PUD, V2X_SCK );

	sbi( SPI_DDR, SPI_MOSI );
	sbi( SPI_DDR, SPI_SCK );

	while(1)
		shuffle_bits();
}

