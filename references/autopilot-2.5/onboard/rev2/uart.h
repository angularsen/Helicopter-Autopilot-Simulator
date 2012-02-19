/* -*- indent-tabs-mode:T; c-basic-offset:8; tab-width:8; -*- vi: set ts=8:
 * $Id: uart.h,v 2.2 2003/03/22 18:11:47 tramm Exp $
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

#ifndef _UART_H_
#define _UART_H_

#include <avr/signal.h>
#include <avr/io.h>
#include <avr/interrupt.h>
#include <inttypes.h>


/*************************************************************************
 *
 *  UART code.
 */
extern void
uart_init( void );

extern void
uart_task( void );


extern uint8_t
getc(
	uint8_t *		c
);


extern void
putc(
	uint8_t			c
);


/*
 * The UART queue structure is exposed here
 */
extern uint8_t			tx_head;
extern volatile uint8_t		tx_tail;

static inline uint8_t
uart_send_empty( void )
{
	return tx_head == tx_tail;
}

#endif
