/**
 *  $Id: texture.cpp,v 2.0 2002/09/22 02:07:30 tramm Exp $
 *
 * Texture mapping for the ground
 *
 * Source code example from:
 *
 *	http://www.nullterminator.net/gltexture.html
 */


#include <stdint.h>
#include <cstdio>
#include <cmath>
#include <GL/gl.h>
#include <GL/glu.h>
#include <GL/glut.h>

#include "graphics.h"
#include <mat/Quat.h>
#include <mat/Vector_Rotate.h>
#include <mat/Conversions.h>

using namespace libmat;
using namespace std;


// load a 256x256 RGB .RAW file as a texture
unsigned int
load_raw_texture(
	const char *		filename,
	int			wrap
)
{
	GLuint			texture;
	const int		width	= 768;
	const int		height	= 512;

	cerr << "Openning " << filename << " for reading" << endl << endl;

	// open texture data
	FILE *			file = fopen( filename, "rb" );
    	if( !file )
		return 0;


	// allocate buffer
	uint8_t			data[ width * height * 3 ];

	// read texture data
	fread( data, width * height * 3, 1, file );
	fclose( file );

	// allocate a texture name
	glGenTextures( 1, &texture );

	// select our current texture
	glBindTexture( GL_TEXTURE_2D, texture );

        glTexEnvi( GL_TEXTURE_ENV, GL_TEXTURE_ENV_MODE, GL_DECAL );
#if 0
        glEnable( GL_TEXTURE_2D );
        glPixelStorei( GL_UNPACK_ALIGNMENT, 1);
        glTexParameteri( GL_TEXTURE_2D, GL_TEXTURE_WRAP_S, GL_REPEAT);
        glTexParameteri( GL_TEXTURE_2D, GL_TEXTURE_WRAP_T, GL_REPEAT);
        glTexParameteri( GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, GL_LINEAR );
        glTexParameteri( GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER,
GL_LINEAR_MIPMAP_NEAREST );

        gluBuild2DMipmaps(
		GL_TEXTURE_2D,
		3,
		width,
		height,
		GL_RGB,
		GL_UNSIGNED_BYTE,
		data
	);


#else
	// select modulate to mix texture with color for shading
	glTexEnvf(
		GL_TEXTURE_ENV,
		GL_TEXTURE_ENV_MODE,
		GL_MODULATE
	);

	// when texture area is small, bilinear filter the closest mipmap
	glTexParameterf(
		GL_TEXTURE_2D,
		GL_TEXTURE_MIN_FILTER,
               	GL_LINEAR_MIPMAP_NEAREST
	);

	// when texture area is large, bilinear filter the first mipmap
	glTexParameterf(
		GL_TEXTURE_2D,
		GL_TEXTURE_MAG_FILTER,
		GL_LINEAR
	);

	// if wrap is true, the texture wraps over at the edges (repeat)
	//       ... false, the texture ends at the edges (clamp)
	glTexParameterf(
		GL_TEXTURE_2D,
		GL_TEXTURE_WRAP_S,
		wrap ? GL_REPEAT : GL_CLAMP
	);

	glTexParameterf(
		GL_TEXTURE_2D,
		GL_TEXTURE_WRAP_T,
		wrap ? GL_REPEAT : GL_CLAMP
	);

	// build our texture mipmaps
	gluBuild2DMipmaps(
		GL_TEXTURE_2D,
		3,
		width,
		height,
               	GL_RGB,
		GL_UNSIGNED_BYTE,
		data
	);
#endif

	return texture;
}


void
apply_texture()
{
	static int texture = 0;

	cerr << "applying texture: " << texture << endl;

	if( !texture )
		texture = 0 && load_raw_texture( "./src/heli-3d/grass.bmp", 1 );

	GLfloat groundAmbient[4]	= { 0.02, 0.30, 0.10, 1.00 };

	glEnable( GL_LIGHTING );
	glEnable( GL_TEXTURE_2D );

	glMaterialfv(
		GL_FRONT,
		GL_AMBIENT_AND_DIFFUSE,
		groundAmbient
	);
        
	glBegin( GL_QUADS );
	glNormal3f( 0.0, 1.0, 0.0 );
        
	for( int i=-31200; i <= 31200; i+=2000 )
	{
		for( int j=-31200; j <= 31200; j+=2000 )
		{
			glTexCoord2f( 0.0, 0.0 );
			glVertex3f( i, -0.01, j );
        
			glTexCoord2f( 1.0, 0.0 ); 
			glVertex3f( i+2000.0, -0.01, j);

			glTexCoord2f( 1.0, 1.0 );
			glVertex3f( i+2000.0, -0.01f, j+2000.0 );

			glTexCoord2f( 0.0, 1.0 );
			glVertex3f( i, -0.01f, j+2000.0 );
		}
	}

	glEnd();

	glDisable( GL_TEXTURE_2D );
}

