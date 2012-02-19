/* -*- indent-tabs-mode:T; c-basic-offset:8; tab-width:8; -*- vi: set ts=8:
 * $Id: fast_float.h,v 1.2 2002/10/04 14:56:51 tramm Exp $
 *
 * (c) Trammell Hudson
 *
 * Fast float operations for FPU-less machines, such as the
 * StrongARM SA1110 in the Compaq iPAQ.
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
#ifndef _fast_float_h_
#define _fast_float_h_

namespace libmat
{


static inline
bool
is_zero(
	const double &		x
)
{
#ifdef NO_FPU
	const int32_t *		xp = (const int32_t*) &x;
	return xp[0] == 0 && xp[1] == 0;
#else
	return x == 0.0;
#endif
}


static inline
bool
is_zero(
	const float &		x
)
{
#ifdef NO_FPU
	const int32_t *		xp = (const int32_t*) &x;
	return xp[0] == 0;
#else
	return x == 0.0;
#endif
}


template<
	class			T
>
bool
is_zero(
	const T &		x
)
{
	return x == 0;
}


template<
	class			T
>
void
increment(
	T &			a,
	const T &		b
)
{
	if( is_zero( b ) )
		return;
	if( is_zero( a ) )
	{
		a = b;
		return;
	}

	a += b;
}


template<
	class			T
>
void
decrement(
	T &			a,
	const T &		b
)
{
	if( is_zero( b ) )
		return;
	if( is_zero( a ) )
	{
		a = -b;
		return;
	}

	a -= b;
}


};
#endif
