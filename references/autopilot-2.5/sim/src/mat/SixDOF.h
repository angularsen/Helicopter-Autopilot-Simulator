/* -*- indent-tabs-mode:T; c-basic-offset:8; tab-width:8; -*- vi: set ts=8:
 * $Id: SixDOF.h,v 2.0 2002/09/22 02:07:32 tramm Exp $
 *
 * (c) Aaron Kahn
 * (c) Trammell Hudson
 *
 * Six Degree of freedom simulation code.
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
#ifndef _mat_SixDOF_h_
#define _mat_SixDOF_h_

#include <mat/Vector.h>
#include <mat/Matrix.h>

namespace libmat
{

class SixDOF
{
public:
	SixDOF(
		double			m,
		double			Ixx,
		double			Iyy,
		double			Izz,
		double			Ixz
	);

	void step(
		double			dt,
		double			g,
		const Vector<3> &	force,
		const Vector<3> &	lmn
	);

	Vector<3>		force;
	Vector<3>		uvw;
	Vector<3>		xyz;

	Vector<3>		pqr;
	Vector<3>		theta;

private:
	const double		m;
	const Matrix<3,3>	J;
	const Matrix<3,3>	inv_J;
};

}
#endif
