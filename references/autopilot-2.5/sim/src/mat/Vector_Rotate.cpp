/* -*- indent-tabs-mode:T; c-basic-offset:8; tab-width:8; -*- vi: set ts=8:
 * $Id: Vector_Rotate.cpp,v 2.0 2002/09/22 02:07:32 tramm Exp $
 *
 * (c) Aaron Kahn
 * (c) Trammell Hudson
 *
 * Vector rotation code
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
#include <mat/Vector_Rotate.h>
#include <mat/Quat.h>
#include <cmath>

namespace libmat
{

using std::cos;
using std::sin;


/*
 *  Rotate a vector by one angle.  This is not thread safe, but
 * fast for repeatedly rotating about the same angle.
 */
const Vector<3>
rotate2(
	const Vector<3> &	v_in,
	const double		theta
)
{
	static double           cos_theta = 1.0;
	static double           sin_theta = 0.0;
	static double           last_theta = 0.0;
        
	if( last_theta != theta )
	{
                last_theta      = theta;
                cos_theta       = cos(theta);
                sin_theta       = sin(theta);
	}

	return Vector<3>(
		v_in[0] * cos_theta + v_in[1] * sin_theta,
		v_in[1] * cos_theta - v_in[0] * sin_theta,
		v_in[2]
	);
}

}
