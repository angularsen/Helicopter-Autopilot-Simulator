/**
 *  $Id: anim.c,v 2.0 2002/09/22 02:07:30 tramm Exp $
 *
 * (c) Bram Stolk
 * (c) Trammell Hudson
 *
 */
#include <stdio.h>
#include <stdlib.h>
#include <unistd.h>
#include <string.h>

#include <GL/gl.h>

static FILE *			file;

/**
 *  Open the animation file and prepare to write into it
 */
int
start_animation(
	const char *		filename
)
{
	if( file )
		fclose( file );

	file = fopen( filename, "wb" );
	if( !file )
		return -1;

	return 0;
}


/**
 *  Dump the frame buffer into a ppm array.
 *
 */
unsigned char *
fb2ppm(
	int 			w,
	int 			h
)
{
	unsigned char *		fbuf;
	unsigned char *		rbuf;
	int			y;

	if( !(fbuf = malloc( sizeof(unsigned char) * w*h*3 ) ) )
		return 0;

	if( !(rbuf = malloc( sizeof(unsigned char) * w*h*3 ) ) )
	{
		free( fbuf );
		return 0;
	}

	glReadPixels(
		0, 0,
		w, h,
		GL_RGB,
		GL_UNSIGNED_BYTE,
		(GLvoid*) fbuf
	);

	/* Transpose the order */
	for( y=0 ; y<h ; y++ )
	{
		int chunk = 3 * w;

		memcpy(
			rbuf + y*chunk,
			fbuf + (h - 1 - y) * chunk,
			chunk
		);
	}

	free( fbuf );
	return rbuf;
}


/**
 *  Write a frame of animation
 */
int
save_frame(
	int			x,
	int			y
)
{
	unsigned char *		buf;

	if( !file )
		return -1;

	buf = fb2ppm( x, y );
	if( !buf )
		return -1;

	fwrite( buf, x * y * 3, 1, file );
	fflush( file );
	free( buf );

	return 0;
}
