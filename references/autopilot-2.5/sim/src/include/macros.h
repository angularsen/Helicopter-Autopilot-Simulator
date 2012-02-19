/* -*- indent-tabs-mode:T; c-basic-offset:8; tab-width:8; -*- vi: set ts=8:
 * $Id: macros.h,v 2.1 2003/03/08 05:21:52 tramm Exp $
 *
 * (c) Aaron Kahn
 * (c) Trammell Hudson
 *
 * Common macros
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

#ifndef _MACROS_H_
#define _MACROS_H_

#ifdef __cplusplus
#include <iostream>
#include <sys/types.h>

namespace util
{
#endif

/*
 *  Wrappers to ensure that all C functions are not mangled by the
 * C++ compilers.
 */
#ifdef __cplusplus
#define BEGIN_DECLS		extern "C" {
#define END_DECLS		}
#else
#define BEGIN_DECLS		/* Declarations */
#define END_DECLS		/* End declarations */
#endif


/*
 *  Wrapper to portably declare a parameter as unused
 */
#ifdef __cplusplus
#define UNUSED( name )		/* name */
#else
#define UNUSED( name )		name __attribute__ ((unused))
#endif

/*
 *  General math macros
 */
static double
limit(
	double			value,
	double			min,
	double			max
)
{
	if( value < min )
		return min;
	if( value > max )
		return max;
	return value;
}


static double
max(
	double			a,
	double			b
)
{
	if( a < b )
		return b;
	else
		return a;
}


static double
min(
	double			a,
	double			b
)
{
	if( a < b )
		return a;
	else
		return b;
}

static double
sqr(
	double			x
)
{
	return x * x;
}


/*
 *  C++ STL list manipulation stuff.
 *
 * We advance the iterator above the block so that if the delete
 * the iterator, we are not lost in the container.
 */
#ifdef __cplusplus

#define _FOR_ALL_BODY( i, block )					\
	int			_index	= 0;				\
	_iterator		_begin	= _c.begin();			\
	_iterator		_end	= _c.end();			\
									\
	for(								\
		_iterator _i = _begin ;					\
		_i != _end ;						\
		_index++						\
	)								\
	{								\
		const int		i ## _index = _index;		\
		_iterator		i = _i++;			\
									\
		/* Avoid unused variable warnings */			\
		(void) i;						\
		(void) i ## _index;					\
		block;							\
	}								\


#define FOR_ALL( type, i, container, block )				\
	do {								\
		typedef type::iterator		_iterator;		\
		type &			_c	= (container);		\
									\
		_FOR_ALL_BODY( i, block )				\
	} while(0)							\


#define FOR_ALL_CONST( type, i, container, block )			\
	do {								\
		typedef type::const_iterator	_iterator;		\
		const type &		_c	= (container);		\
									\
		_FOR_ALL_BODY( i, block )				\
	} while(0)							\


/*
 * Some handy C++ output methods for different types
 */
static inline std::ostream &
operator << (
	std::ostream &		out,
	const struct timeval &	time
)
{
	return out
		<< time.tv_sec
		<< ":"
		<< time.tv_usec;
}


#endif

#ifdef __cplusplus
}; /* namespace util */
#endif

#endif
