/* -*- indent-tabs-mode:T; c-basic-offset:8; tab-width:8; -*- vi: set ts=8:
 * $Id: imu.h,v 1.1 2002/10/22 01:38:36 tramm Exp $
 *
 * IMU globals
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
#ifndef _avr_imu_h_
#define _avr_imu_h_

#include <inttypes.h>

#define IMU_INDEX_P		7
#define IMU_INDEX_Q		6
#define IMU_INDEX_R		5

#define IMU_INDEX_AX		3
#define IMU_INDEX_AY		4

extern void
imu_update( void );

extern void
imu_init( void );

#endif
