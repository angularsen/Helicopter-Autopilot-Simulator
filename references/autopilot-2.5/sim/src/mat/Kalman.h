/* -*- indent-tabs-mode:T; c-basic-offset:8; tab-width:8; -*- vi: set ts=8:
 * $Id: Kalman.h,v 2.0 2002/09/22 02:07:32 tramm Exp $
 *
 * (c) Aaron Kahn
 * (c) Trammell Hudson
 *
 * Kalman filtering with the new math library
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
#ifndef _Kalman_h_
#define _Kalman_h_

#include <mat/Vector.h>
#include <mat/Matrix.h>
#include <mat/Matrix_Invert.h>

namespace libmat
{


template<
	const int		n,
	const int		m,
	class			T
>
void Kalman(
	Matrix<n,n,T> &		P,
	Vector<n,T> &		X,
	const Matrix<m,n,T> &	C,
	const Matrix<m,m,T> &	R,
	const Vector<m,T> &	err,
	Matrix<n,m,T> &		K
)
{
	const Matrix<n,m,T>	C_transpose( C.transpose() );

	Matrix<m,m,T>		E( R );
	E += mult3( C, P, C_transpose );

	K = mult3( P, C_transpose,  invert( E ) );

	X += K * err;

	P -= mult3( K, C, P );
}

}
#endif
