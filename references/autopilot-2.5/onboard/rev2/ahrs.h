/* -*- indent-tabs-mode:T; c-basic-offset:8; tab-width:8; -*- vi: set ts=8:
 * $Id: ahrs.h,v 1.3 2002/10/20 03:15:54 tramm Exp $
 *
 * AHRS (Attitude and Heading Reference System)
 *
 * Optimized for smaller microcontrollers with limited memory.
 *
 * (c) 2002 Trammell Hudson <hudson@rotomotion.com>
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

#ifndef _avr_ahrs_h_
#define _avr_ahrs_h_

#include "mat.h"

extern float		ahrs_theta[2];
extern float		ahrs_pqr[2];
extern float		ahrs_accel[2];
extern uint8_t		ahrs_stage;
extern float		ahrs_trace;


/*
 *
 * The filter assumes that we are level when we start.  The initial
 * ahrs_pqr values are used to set the starting bias and the starting
 * attitude estimate is derived from the ahrs_accel values with the
 * accel2euler function.
 */
extern void
ahrs_init( void );


/*
 * Caller should set pqr with the raw (biased) pqr values in
 * ahrs_pqr[] and the two raw accelerometer values in ahrs_accel[];
 *
 * The estimate attitude is output into ahrs_theta in radians
 * and the unbiased pqr values are in ahrs_pqr.
 */
extern void
ahrs_update( void );


#endif
