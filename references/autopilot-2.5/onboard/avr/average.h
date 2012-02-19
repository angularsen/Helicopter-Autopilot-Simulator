/* -*- indent-tabs-mode:T; c-basic-offset:8; tab-width:8; -*- vi: set ts=8:
 * $Id: average.h,v 2.0 2002/09/22 02:10:16 tramm Exp $
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

#ifndef _AVERAGE_H_
#define _AVERAGE_H_

#include <sig-avr.h>
#include <interrupt.h>
#include <io.h>


/**
 *  Add a value to the averaging window.  The head index must
 * be advanced by the caller; this allows several concurrent samples
 * to share the same window.
 *
 * For performance reasons, max_samples should be a power of two.
 * It will also help with the computation of the actual value.
 */
static inline void
average(
	uint16_t		value,
	uint16_t *		sum,
	uint16_t *		samples,
	uint8_t			head,
	uint8_t			max_samples
)
{
	uint16_t *		store = &samples[head % max_samples];

	*sum += value - *store;
	*store = value;
}

#endif
