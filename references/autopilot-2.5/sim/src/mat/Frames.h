/* -*- indent-tabs-mode:T; c-basic-offset:8; tab-width:8; -*- vi: set ts=8:
 * $Id: Frames.h,v 2.0 2002/09/22 02:07:32 tramm Exp $
 *
 * (c) Aaron Kahn
 * (c) Trammell Hudson
 *
 * Reference frame and unit computation classes.
 * Thar be dragons!
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
#ifndef _Frames_h_
#define _Frames_h_

#include <mat/Vector.h>
#include <mat/Vector_Rotate.h>

namespace libmat
{

class Frame
{
public:
	class Body {};
	class NED {};
	class ECEF {};
};


/*
 *  This is repeated for each unit, but they can not have a common
 * base class.
 */
#define MAKE_UNIT( TYPE )						\
template<								\
	class			Frame					\
>									\
class TYPE								\
{									\
public:									\
	Vector<3>		v;					\
									\
	TYPE() {}							\
									\
	TYPE(								\
		const Vector<3> &	v				\
	) : v(v)							\
	{								\
	}								\
									\
	TYPE(								\
		const double &		v0,				\
		const double &		v1,				\
		const double &		v2				\
	) : v(v0,v1,v2)							\
	{								\
	}								\
									\
	void								\
	fill()								\
	{								\
		this->v.fill();						\
	}								\
									\
									\
	double								\
	mag2() const							\
	{								\
		return this->v.mag2();					\
	}								\
									\
	double								\
	mag() const							\
	{								\
		return this->v.mag();					\
	}								\
									\
	double								\
	operator [] (							\
		int			index				\
	) const								\
	{								\
		return this->v[index];					\
	}								\
									\
	double &							\
	operator [] (							\
		int			index				\
	) {								\
		return this->v[index];					\
	}								\
									\
	TYPE &								\
	operator += (							\
		const TYPE &		f				\
	) {								\
		this->v += f.v;						\
		return (*this);						\
	}								\
									\
	TYPE &								\
	operator -= (							\
		const TYPE &		f				\
	) {								\
		this->v -= f.v;						\
		return (*this);						\
	}								\
									\
	TYPE &								\
	operator *= (							\
		const double &		s				\
	) {								\
		this->v *= s;						\
		return (*this);						\
	}								\
									\
	TYPE &								\
	operator /= (							\
		const double &		s				\
	) {								\
		this->v /= s;						\
		return (*this);						\
	}								\
									\
	const TYPE							\
	operator + (							\
		const TYPE &		a				\
	) const								\
	{								\
		return TYPE(*this) += a;				\
	}								\
									\
	const TYPE							\
	operator * (							\
		const double &		s				\
	) const								\
	{								\
		return TYPE(*this) *= s;				\
	}								\
									\
	const TYPE							\
	operator / (							\
		const double &		s				\
	) const								\
	{								\
		return TYPE(this) /= s;					\
	}								\
}

/*
 *  Angular units
 */
MAKE_UNIT( Moment );
MAKE_UNIT( RateDot );
MAKE_UNIT( Rate );
MAKE_UNIT( Angle );

/*
 *  Linear positions
 */
MAKE_UNIT( Force );
MAKE_UNIT( Accel );
MAKE_UNIT( Velocity );
MAKE_UNIT( Position );


/**
 *  Class to handle rotations
 */
template<
	class		To_Frame,
	class		From_Frame
>
class Rotate
{
public:
	const Matrix<3,3>	m;
	Rotate(
		const Matrix<3,3> &	m
	) :
		m(m)
	{
	}

	Rotate(
		const Angle<To_Frame> &	v
	) :
		m( eulerDC( v.v ) )
	{
	}

	Rotate(
		const Angle<From_Frame> &v
	) :
		m( eulerDC( v.v ).transpose() )
	{
	}

#if 0
	Rotate(
		const Quat<To_Frame> & q
	) :
		m( quatDC( q.v ) )
	{
	}

	Rotate(
		const Quat<From_Frame> & q
	) :
		m( quatDC( q.v ).transpose() )
	{
	}
#endif
};


/*
 *  Output a unit
 */
template<
	class			Frame
>
std::ostream &
operator << (
	std::ostream &		out,
	const Force<Frame> &	f
)
{
	return out << f.v;
}



/*
 *  Translate from one frame to another
 */
#define ROTATE_UNIT( TYPE )						\
template<								\
	class			To_Frame,				\
	class			From_Frame				\
>									\
const TYPE< To_Frame >							\
operator * (								\
	const Rotate< To_Frame, From_Frame > & r,			\
	const TYPE< From_Frame > & v					\
)									\
{									\
	return TYPE< To_Frame >( r.m * v.v );				\
}									\
									\
template<								\
	class		To_Frame,					\
	class		From_Frame					\
>									\
const TYPE< To_Frame >							\
rotate(									\
	const TYPE< From_Frame > &	unit,				\
	const Angle< To_Frame > &	angle				\
) {									\
	const Rotate< To_Frame, From_Frame > r( angle );		\
	return TYPE< To_Frame >( r * unit );				\
}									\
									\
template<								\
	class		To_Frame,					\
	class		From_Frame					\
>									\
const TYPE< To_Frame >							\
rotate(									\
	const TYPE< From_Frame > &	unit,				\
	const Angle< From_Frame > &	angle				\
) {									\
	const Rotate< To_Frame, From_Frame > r( angle );		\
	return TYPE< To_Frame >( r * unit );				\
}									\


ROTATE_UNIT( Force )
ROTATE_UNIT( Accel )
ROTATE_UNIT( Velocity )
ROTATE_UNIT( Position )

ROTATE_UNIT( Moment )
ROTATE_UNIT( RateDot )
ROTATE_UNIT( Rate )
ROTATE_UNIT( Angle )

}

#endif
