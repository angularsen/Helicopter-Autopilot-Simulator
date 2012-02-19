/* -*- indent-tabs-mode:T; c-basic-offset:8; tab-width:8; -*- vi: set ts=8:
 * $Id: timer.h,v 2.0 2002/09/22 02:07:31 tramm Exp $
 *
 * (c) Trammell Hudson
 *
 * High performance timing functions
 *
 *************
 *
 *  This file is part of the autopilot simulation package.
 *
 *  For more details:
 *
 *	http://autopilot.sourceforge.net/
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
#ifndef _TIMER_H_
#define _TIMER_H_

#include <stdio.h>
#include <sys/time.h>

typedef struct timeval stopwatch_t;


/**
 *  Inline functions to help keep track of time.
 * This one starts a stopwatch.
 */
static void
start(
	stopwatch_t *		t
)
{
	gettimeofday( t, 0 );
}


/**
 *  This one stops a stopwatch and returns the number of microseconds
 * since the timer was started.  It is safe to call it multiple times
 * with the same stopwatch_t, so you can use it like a lap counter, too.
 */
static unsigned long
stop(
	stopwatch_t *		t
)
{
	stopwatch_t		end;
	gettimeofday( &end, 0 );

	while( end.tv_sec > t->tv_sec )
	{
		end.tv_sec --;
		end.tv_usec += 1000000;
	}

	return end.tv_usec - t->tv_usec;
}


/**
 *  Handy macro to compare the runtimes (and perhaps efficiency)
 * of two similar procedures.  Inspired by the Perl 'use Benchmark'
 * module.
 */
#define time_these( iters, name1, block1, name2, block2 )		\
	do {								\
		stopwatch_t		timer;				\
		const unsigned long	_iters = iters;			\
		unsigned long		time1;				\
		unsigned long		time2;				\
									\
		printf( "\nTiming %lu iterations of %s and %s\n",	\
			_iters,						\
			name1,						\
			name2						\
		);							\
									\
		start( &timer );					\
		for( unsigned long i = 0 ; i < _iters ; i++ )		\
		{							\
			block1;						\
		}							\
		time1 = stop( &timer );					\
									\
		start( &timer );					\
		for( unsigned long i = 0 ; i < _iters ; i++ )		\
		{							\
			block2;						\
		}							\
		time2 = stop( &timer );					\
									\
		double	time_per1 = (double)time1 / (double)_iters;	\
		double	time_per2 = (double)time2 / (double)_iters;	\
									\
		printf( "%s: Total = %lu => %lf usec each\n",		\
			name1,						\
			time1,						\
			time_per1					\
		);							\
									\
		printf( "%s: Total = %lu => %lf usec each\n",		\
			name2,						\
			time2,						\
			time_per2					\
		);							\
									\
		printf( "%3.3lf%% change\n",				\
			100.0 * (time_per1 - time_per2) / time_per2	\
		);							\
	} while(0)							\


#endif
