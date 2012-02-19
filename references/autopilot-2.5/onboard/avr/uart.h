/* -*- indent-tabs-mode:T; c-basic-offset:8; tab-width:8; -*- vi: set ts=8:
 * $Id: uart.h,v 2.0 2002/09/22 02:10:16 tramm Exp $
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

#include <sig-avr.h>
#include <interrupt.h>
#include <io.h>


/*************************************************************************
 *
 *  UART code.
 *
 * We only enable interrupts during the 2048 usec window between
 * accelerometer pulse transitions, so we are dropping input
 * characters.
 * 
 */
extern void
init_uart( void );

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
uint8_t				tx_head;
volatile uint8_t		tx_tail;

static inline uint8_t
uart_send_empty( void )
{
	return tx_head == tx_tail;
}

#endif
