#include <stdio.h>
#include <stdlib.h>
#include <stdint.h>
#include <unistd.h>

const double pi=3.14159265358979323846;

/* How many words/bytes in the header that we've "skipped" */
const int		header_bytes	= 10;
const int		header_words	= 6;



/*
 * THIS IS ONE INDEXED.  The Zodiac manual is also one indexed,
 * so this lets you use its values instead of the more rational
 * zero indexed.
 */
static uint16_t
getword(
	const uint8_t *		buf,
	int			word
)
{
	return 0
		| buf[word * 2 - 1] << 8
		| buf[word * 2 - 2] << 0;
}

static uint8_t
getbit(
	const uint8_t *		buf,
	int			word,
	int			bit
)
{
	word += bit / 16;
	bit %= 16;

	word = getword( buf, word );
	return word & (1<<bit);
}


static double
getdouble(
	const uint8_t *		buf,
	int			word,
	double			scale
)
{
	uint32_t		val = 0;

	val = getword( buf, word+1 );
	val <<= 16;
	val = getword( buf, word );

	return scale * (double)val;
}



static int
readall(
	int			fd,
	uint8_t *		buf,
	size_t			len
)
{
	ssize_t			read_len;

	while( len )
	{
		read_len = read( fd, buf, len );

		if( read_len < 0 )
			return -1;

		len -= read_len;
		buf += read_len;
	}

	return 0;
}


typedef struct
{
	unsigned		sync;
	unsigned		type;
	unsigned		len;
	unsigned		status;
	unsigned		checksum;
	uint8_t			valid;
} hdr_t;


typedef struct
{
	int			year;
	int			month;
	int			day;
	int			hour;
	int			minute;
	int			second;

	int			sats_used;
	int			fps;
	int			magnetic_variation;

	double			lat;
	double			lon;
	double			course;
	double			alt;
} gps_t;

int
compute_checksum(
	const uint8_t *		buf,
	int			len
)
{
	int			n;
	uint16_t		csum = 0;

	for( n=1 ; n <= len ; n ++ )
                csum += getword( buf, n );

        return csum == 0;
}


int
read_header(
	int			fd,
	hdr_t *			hdr,
	uint8_t *		buf
)
{
	/* Wait for 0xFF 0x81 */
	while(1)
	{
		if( readall( fd, &buf[0], 1 ) < 0 )
			return -1;
		if( buf[0] != 0xFF )
			continue;

		if( readall( fd, &buf[1], 1 ) < 0 )
			return -1;
		if( buf[1] != 0x81 )
			continue;

		break;
	}

	/* Get the rest of the header */
	if( readall( fd, &buf[2], 8 ) < 0 )
		return -1;

	hdr->sync	= getword( buf, 1 );
	hdr->type	= getword( buf, 2 );
	hdr->len	= getword( buf, 3 );
	hdr->status	= getword( buf, 4 );
	hdr->checksum	= getword( buf, 5 );

	if( compute_checksum( buf, 5 ) )
		hdr->valid	= 1;
	else
		hdr->valid	= 0;
		

	return 0;
}


void
position_msg(
	const uint8_t *		buf,
	gps_t *			gps
)
{
	printf( "Position status: %d sats %s %s %s\n",
		getword( buf, 12 ),
		getbit( buf, 11, 0 ) ? "Propagated" : "",
		getbit( buf, 11, 1 ) ? "Altitude used" : "",
		getbit( buf, 11, 2 ) ? "Differential" : ""
	);

	printf( "Time: %04d/%02d/%02d %02d:%02d:%02d\n",
		getword( buf, 21 ),
		getword( buf, 20 ),
		getword( buf, 19 ),

		getword( buf, 22 ),
		getword( buf, 23 ),
		getword( buf, 24 )
	);

	printf( "Lat: %f\n",
		getdouble( buf, 27, 10e-8 )
	);

	printf( "Lon: %f\n",
		getdouble( buf, 29, 10e-8 )
	);

	printf( "Alt: %f\n",
		getdouble( buf, 31, 10e-2 )
	);

	printf( "FPS: %d\n",
		getword( buf, 38 )
	);

	printf( "\n" );
}



void
channel_msg(
	const uint8_t *		buf,
	gps_t *			gps
)
{
	int			i;
	static unsigned		last_seq;
	unsigned		new_seq = getword( buf, 8 );

	printf( "Channel summary %s\n",
		new_seq == last_seq ? "OLD" : "new"
	);

	if( last_seq == new_seq )
		return;
	last_seq = new_seq;

	for( i=0 ; i<12 ; i++ )
	{
		uint16_t		status = getword( buf, 15 + 3*i );
		uint16_t		prn = getword( buf, 16 + 3*i );
		uint16_t		strength = getword( buf, 17 + 3*i );

		printf( "%2d: %s sat %d signal=%d: %s %s\n",
			i,
			status & 1 ? "Using" : "Not using",
			prn,
			strength,
			status & 2 ? "Ephemeris" : "",
			status & 4 ? "Valid" : ""
		);
	}
	printf( "\n" );
}


void
visible_msg(
	const uint8_t *		buf,
	gps_t *			gps
)
{
	int			i;
	int			visible = getword( buf, 14 );

	printf( "Visible satelites: %d\n",
		visible
	);

	for( i=0 ; i<visible ; i++ )
	{
		uint16_t		prn	= getword( buf, 15 + 3*i );
		uint16_t		azimuth	= getword( buf, 16 + 3*i );
		uint16_t		elev	= getword( buf, 17 + 3*i );

		printf( "%2d: az=%d el=%d\n",
			prn,
			azimuth,
			elev
		);
	}

	printf( "\n" );
}



void
measurement_msg(
	const uint8_t *		buf,
	gps_t *			gps
)
{
	int			i;

	printf( "Channel measurement message:\n" );

	printf( "\n" );
}



/*
 *  Our table of message types from the GPS and our handlers for them.
 */
typedef void (*msg_handler_t)(
	const uint8_t *		buf,
	gps_t *			gps
);


typedef struct
{
	uint16_t		type;		// Messgae number
	size_t			len;		// Expected words + header
	msg_handler_t		handler;	// Function pointer
	const char *		description;	// Free form
} message_t;

message_t handlers[] =
{
	{ 1000,  55, position_msg,	"Geodetic Position" },
	{ 1001,  54, 0,			"ECEF Position status" },
	{ 1002,  51, channel_msg,	"Satellite channel summary" },
	{ 1003,  51, visible_msg,	"Visible satellites" },
	{ 1005,  25, 0,			"Differential GPS status" },
	{ 1007, 154, measurement_msg,	"Channel measurement" },
	{ 1011,  59, 0,			"Receiver ID" },
	{ 1012,  22, 0,			"User-settings output" },
	{ 1100,  20, 0,			"Built-in test results" },
	{ 1102, 253, 0,			"Measurement time mark" },
	{ 1108,  20, 0,			"UTC Time mark pulse" },
	{ 1130,  21, 0,			"Serial port parameters" },
	{ 1135,  10, 0,			"EEPROM update" },
	{ 1136,  18, 0,			"EEPROM status" },
	{    0,   0, 0,			0 },
};



message_t *
find_handler(
	uint16_t		type
)
{
	message_t *		handler = &handlers[0];

	/* Find the message type in our table */
	while( handler->description )
	{
		if( handler->type == type )
			return handler;
		handler++;
	}

	return 0;
}

	

int main( void )
{
	hdr_t			hdr;
	gps_t			gps;
	int			fd = STDIN_FILENO;
	uint8_t			buf[ 512 ];


	while( 0 <= read_header( fd, &hdr, buf ) )
	{
		message_t *		handler;

		if( 0 )
		printf( "%s Message: type=%04x len=%d cs=%04x\n",
			hdr.valid ? "Good" : "BAD!",
			hdr.type,
			(int) hdr.len,
			hdr.checksum
		);

		if( !hdr.valid )
		{
			int			i;

			printf( "Bytes: " );
			for( i=0 ; i<header_bytes ; i++ )
				printf( " %02x", buf[i] );
			printf( "\n" );
			continue;
		}


		/* Lookup our message handler */
		handler = find_handler( hdr.type );
		if( !handler )
		{
			printf( "Unknown message type %04x\n", hdr.type );
			continue;
		}
	

		/*
		 * Sanity check that they have sent as much data as
		 * expected in the table and that we have enough space
		 * to store it.
		 */
		if( handler->len - header_words != hdr.len )
		{
			printf( "%04x: (%s) Len mismatch: %d != %d\n",
				handler->type,
				handler->description,
				handler->len,
				hdr.len
			);

			continue;
		}

		if( hdr.len * 2 + header_bytes > sizeof(buf) )
		{
			printf( "%04x: (%s) Too large: %d > %d\n",
				handler->type,
				handler->description,
				handler->len,
				sizeof(buf)
			);

			continue;
		}


		/*
		 *  Get the data
		 */
		if( 0 > readall( fd, buf + header_bytes, hdr.len * 2 ) )
			break;


		if( compute_checksum( buf + header_bytes , hdr.len ) != 0 )
		{
			printf( "%04x: (%s) Checksum mismatch\n",
				handler->type,
				handler->description
			);

			continue;
		}


		/*
		 *  Everything looks good.  Pass it to the handler
		 */
		if( handler->handler )
			handler->handler( buf, &gps );
		else
			printf( "%04x (%s): Unhandled %d words\n\n",
				handler->type,
				handler->description,
				handler->len
			);
	}

	return 0;
}
