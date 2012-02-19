/* -*- indent-tabs-mode:T; c-basic-offset:8; tab-width:8; -*- vi: set ts=8:
 * $Id: calib.c,v 1.1 2002/10/21 23:32:59 tramm Exp $
 *
 * Code to recalibrate and store the accelerometer values in the
 * EEPROM on the Mega163.  We track the max/min values for each and
 * compute the bias for each and the average scale factor for both.
 *
 * (c) Bram Stolk <b.stolk@chello.nl>
 * (c) Trammell Hudson <hudson@rotomotion.com>
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
 * P 317  158
 * R 362  18A
 */

#include "calib.h"
#include "adc.h"
#include "lcd.h"
#include "button.h"
#include "memory.h"
#include <eeprom.h>



// Should be localized! Gravity is not constant over entire planet earth.
#define GRAVITY 9.78

float			accel_scale;
uint8_t			bias_ax;
uint8_t			bias_ay;

static uint8_t		_bias_ax	__attribute__ ((section(".eeprom")));
static uint8_t		_bias_ay	__attribute__ ((section(".eeprom")));
static float		_scale		__attribute__ ((section(".eeprom")));


static inline void
eeprom_write_float(
	int		addr,
	const float *	v
)
{
	const uint8_t *c = (uint8_t *) v;
	eeprom_wb( addr+0, c[0] );
	eeprom_wb( addr+1, c[1] );
	eeprom_wb( addr+2, c[2] );
	eeprom_wb( addr+3, c[3] );
}


static inline void
eeprom_read_float(
	uint16_t		addr,
	float *			v
)
{
	eeprom_read_block( v, addr, 4 );
}


void
calib_init( void )
{
	eeprom_read_float( (uint16_t) &_scale, &accel_scale );
	((int8_t) bias_ax) = eeprom_rb( (unsigned) &_bias_ax );
	((int8_t) bias_ay) = eeprom_rb( (unsigned) &_bias_ay );

	puts( "$GPCAL," );
	put_uint8_t( bias_ax );
	putc( ',' );
	put_uint8_t( bias_ay );
	putc( ',' );
	put_float( &accel_scale );
	putnl();
}


static void
render_uint12(
	uint8_t		pos,
	uint16_t	val
)
{
	lcd_buf[pos+2] = hexdigit( val & 0xF );
	val >>= 4;
	lcd_buf[pos+1] = hexdigit( val & 0xF );
	val >>= 4;
	lcd_buf[pos+0] = hexdigit( val & 0xF );
}

void
calib_perform(void)
{
	uint16_t	minrates[2] = { 65535, 65535 };
	uint16_t	maxrates[2] = {     0,     0 };

   	uint8_t		i;
	uint16_t	num_iterations_since_update = 0;

	memset( lcd_buf, ' ', 32 );
	lcd_buf[0]  = 'P';
	lcd_buf[16] = 'R';

	while( num_iterations_since_update < 60000 )
	{
		// AX and AY are sampled at adc 3+4
		for( i=0 ; i<2 ; i++ )
		{
			uint16_t	value = adc_samples[3+i];
			render_uint12( 16*i + 2, maxrates[i] );
			render_uint12( 16*i + 7, value );
			render_uint12( 16*i + 12, minrates[i] );
			
			if( value < minrates[i] )
			{
				minrates[i] = value;
				num_iterations_since_update = 0;
			}

			if( value > maxrates[i] )
			{
				maxrates[i] = value;
				num_iterations_since_update = 0;
			}
		}

		num_iterations_since_update++;
	}

	// the sample range should correspond from -g to g for both axes.
	// average the sample range of X and Y
	uint16_t delta2	= (maxrates[0] + maxrates[1])
			- (minrates[0] + minrates[1]);
	
	accel_scale = 4.0 * GRAVITY / delta2;

	bias_ax = (maxrates[0]+minrates[0])/2 - ACCEL_OFFSET;
	bias_ay = (maxrates[1]+minrates[1])/2 - ACCEL_OFFSET;

        eeprom_write_float((int) &_scale, &accel_scale );
	eeprom_wb( (int) &_bias_ax, bias_ax );
	eeprom_wb( (int) &_bias_ay, bias_ay );

	memset( lcd_buf, ' ', 32 );
	render_uint12(  0, bias_ax );
	render_uint12( 16, bias_ay );

}

