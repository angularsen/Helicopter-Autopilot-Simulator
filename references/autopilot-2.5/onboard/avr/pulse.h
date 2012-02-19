/* -*- indent-tabs-mode:T; c-basic-offset:8; tab-width:8; -*- vi: set ts=8:
 * $Id: pulse.h,v 2.0 2002/09/22 02:10:16 tramm Exp $
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

#ifndef _PULSE_H_
#define _PULSE_H_

#include <sig-avr.h>
#include <interrupt.h>
#include <io.h>


/*************************************************************************
 *
 * Pulse counting inputs for N1 and N2 tach
 *
 * We only count pulses during the 2048 usecond windows, so we have
 * 8192 useconds of counting per 20ms duty cycle.  This value should
 * be indicative of the actual pulse value, so the value output can
 * be multiplied by 256 to produce RPM.
 *
 * I hope.
 */
#define PULSE_SAMPLES	4u
extern uint16_t		pulse_sum[2];

extern void
init_tach( void );


extern void
pulse_avg( void );

#endif
