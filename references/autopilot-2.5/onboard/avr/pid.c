/* -*- indent-tabs-mode:T; c-basic-offset:8; tab-width:8; -*- vi: set ts=8:
 * $Id: pid.c,v 2.0 2002/09/22 02:10:16 tramm Exp $
 *
 * (c) 2002 Trammell Hudson <hudson@swcp.com>
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
 *
 * Output is:
 *
 *	( Gp * p + Gi * i - Gd * d ) / G
 *
 * Where:
 *
 *	p		Delta of the desired value and the measured value
 *
 *	i		Integral of the errors
 *
 *	d		Derivative of the error
 *
 *	Gp, Gi, Gd	Gain coefficients for P, I and D
 *
 *	G		Global gain.  Gp, Gi and Gd are ints, so
 *			this allows for a rational scaling factor.
 *			16 or 32 are good values.
 *
 * The tunable values (G, Gp, Gi, Gd) are set with the tune() function.
 * The desired value  is set with the setpoint() function.
 * Samples are recorded with sample().
 * Output is retrieved with output().
 */
#include "avr.h"
#include "pid.h"

#define MAX_STREAMS		4

typedef struct stream_t
{
	int16_t			p;
	int16_t			i;
	int16_t			d;

	int16_t			setpoint;

	int16_t			P;
	int16_t			I;
	int16_t			D;
	int16_t			G;
} stream_t;

static stream_t streams[ MAX_STREAMS ];

void
sample(
	uint8_t			stream, 
	int16_t			measured
)
{
	stream_t *s = &streams[ stream ];

	int16_t		error;

	error	 = measured - s->setpoint;
	s->d	+= error - s->p;
	s->i	+= error;
	s->p	 = error;

/*
	puts( " m=" ); puthex( measured );
	puts( " s=" ); puthex( s->setpoint );
	puts( " p=" ); puthex( s->p );
	puts( " i=" ); puthex( s->i );
	puts( " d=" ); puthex( s->d );
	putnl();
*/
}


void
setpoint(
	uint8_t			stream,
	int16_t			value
)
{
	stream_t *s = &streams[ stream ];
	s->setpoint = value;
}


void
tune(
	uint8_t			stream,
	int16_t			g,
	int16_t			p,
	int16_t			i,
	int16_t			d
)
{
	stream_t *s = &streams[ stream ];
	s->G = g;
	s->P = p;
	s->I = i;
	s->D = d;
}


int16_t
output(
	uint8_t			stream
)
{
	stream_t *s = &streams[ stream ];
	int16_t rc = 0;

	if( s->P )
		rc += s->p * s->P;
	if( s->I )
		rc += s->i * s->I;
	if( s->D )
		rc += s->d * s->D;

	return rc / 32;
}
