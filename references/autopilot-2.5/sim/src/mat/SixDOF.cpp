/* -*- indent-tabs-mode:T; c-basic-offset:8; tab-width:8; -*- vi: set ts=8:
 * $Id: SixDOF.cpp,v 2.3 2002/10/20 18:58:29 tramm Exp $
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
#include <mat/SixDOF.h>
#include <mat/Matrix_Invert.h>
#include <mat/Quat.h>
#include <mat/Nav.h>
#include <mat/Constants.h>

#include <iostream>

namespace libmat
{

SixDOF::SixDOF(
	double			m,
	double			Ixx,
	double			Iyy,
	double			Izz,
	double			Ixz
) :
	m			(m),
	J( Matrix<3,3>(
		Vector<3>(  Ixx, 0,   -Ixz ),
		Vector<3>( 0,    Iyy, 0 ),
		Vector<3>( -Ixz, 0,    Izz )
	) ),
	inv_J			( invert( J ) )
{
	if( isnan( inv_J[0][0] ) )
		std::cerr << "SixDOF: J matrix is uninvertible" << std::endl;
}


static inline
double
tohrev(
	double			angle
)
{
	if( angle > C_PI && angle <= 2 * C_PI )
		return angle - 2 * C_PI;
	if( angle < -C_PI && angle > -2 * C_PI )
		return 2 * C_PI + angle;
	return angle;
}


void
SixDOF::step(
	double			dt,
	double			g,
	const Vector<3> &	force,
	const Vector<3> &	lmn
)
{
	const Matrix<3,3>	DCM( eulerDC( this->theta ) );
	const Matrix<3,3>	OM( eulerWx( this->pqr ) );
	const Matrix<3,3>	E( euler_strapdown( this->theta ) );
	const Vector<3>		G( 0, 0, g );

	const Vector<3>		Vb_dot(
		 -OM * this->uvw + DCM * G + force / this->m
	);

	const Vector<3>		omb_dot(
		-this->inv_J * (OM * (this->J * this->pqr)) + this->inv_J * lmn
	);

	const Vector<3>		PHI_dot( E * this->pqr );
	const Vector<3>		Ve( DCM.transpose() * this->uvw );

	this->uvw	+= Vb_dot * dt; 
	this->xyz	+= Ve * dt;

	this->pqr	+= omb_dot * dt;
	this->theta	+= PHI_dot * dt;

	this->theta[0] = tohrev( this->theta[0] );
	this->theta[1] = tohrev( this->theta[1] );
	this->theta[2] = tohrev( this->theta[2] );

	// Compute the body force normally what would be seen by
	// a strapdown IMU.  The force on the body is not the same as the
	// accelerations felt by the IMU
	this->force	= Vb_dot + force + eulerWx( this->pqr ) * this->uvw;

}

}
