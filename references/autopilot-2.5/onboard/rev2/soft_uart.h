/* -*- indent-tabs-mode:T; c-basic-offset:8; tab-width:8; -*- vi: set ts=8:
 * $Id: soft_uart.h,v 2.0 2002/09/22 02:10:18 tramm Exp $
 *
 * Software UART for the Mega163.
 * Uses 1 timer and 1 external interrupt.
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

#ifndef _soft_uart_h_
#define _soft_uart_h_

extern uint8_t soft_uart_line[];
extern volatile uint8_t soft_uart_dir; /* 0 == incoming or idle, 1 == outgoing */

extern void
soft_uart_putc(
	uint8_t		c
);

static inline void
soft_uart_block( void )
{
	while( soft_uart_dir )
		;
}


extern void
soft_uart_init( void );

extern void
soft_uart_task( void );

#endif
