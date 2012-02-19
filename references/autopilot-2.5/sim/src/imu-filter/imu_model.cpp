/* -*- indent-tabs-mode:T; c-basic-offset:8; tab-width:8; -*- vi: set ts=8:
 * $Id: imu_model.cpp,v 2.0 2002/09/22 02:07:31 tramm Exp $
 *
 * (c) Aaron Kahn
 * (c) Trammell Hudson
 *
 * This is an IMU emulation function.  Given some geometric information
 * on the position of the IMU relative the the position of the aircraft,
 * a new inertial solution can be solved.  The IMU that is being 
 * simulated is a full-state strapdown IMU.  It is capable of delivering
 * acceleration, velocity, position, angular rate, and attitude at the 
 * IMU's current location and orientation.  
 *
 * See the structures below for the input and output arguments.  All of the
 * input arguments must be defined.  Units are commented next to each of 
 * the values.
 *
 * Sources:
 * The Global Positioning System and Inertial Navigation
 * Farrel, Jay A., Barth, Matthew.
 *
 * Aircraft Control and Simulation
 * Stevens, Brian L., Lewis, Frank L.
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

#include <stdio.h>
#include <cmath>
#include <stdlib.h>
#include <string.h>

#include "imu_model.h"
#include "wgs_model.h"
#include "fe_model.h"
#include "macros.h"

#include <mat/Conversions.h>
#include <mat/rk4.h>


namespace sim
{

using namespace matlib;


void
imu_model(
	imu_inputs_def *	pIn,
	imu_outputs_def *	pOut
)
{
	// omega-cross matrix
	double			wx[MAXSIZE][MAXSIZE];

	// earth -> body transformation matrix
	double			cBE[MAXSIZE][MAXSIZE];

	// body -> earth transformation matrix
	double			cEB[MAXSIZE][MAXSIZE];

	// IMU -> body transformation matrix
	double			cBI[MAXSIZE][MAXSIZE];

	// body -> IMU transformation matrix
	double			cIB[MAXSIZE][MAXSIZE];

	// lat, long, alt of the cg (rad)(rad)(ft)
	double			LLHcg[MAXSIZE];

	double			tempM1[MAXSIZE][MAXSIZE];
	double			tempV1[MAXSIZE];
	double			tempV2[MAXSIZE];
	double			tempV3[MAXSIZE];

	int			i;
	int			j;
	double			lat;
	double			lon;

	// make some of the matricies that will be needed
	// make the omega-cross matrix 
	eulerWx(
		wx,
		pIn->cg_pqr[0],
		pIn->cg_pqr[1],
		pIn->cg_pqr[2]
	);

	// make the cBE matrix
	eulerDC(
		cBE,
		pIn->cg_THETA[0],
		pIn->cg_THETA[1],
		pIn->cg_THETA[2]
	);

	// make the cEB matrix
	transpose( cBE, cEB, 3, 3 );

	// make the cIB matrix
	for(i=0; i<3; ++i)
		for(j=0; j<3; ++j)
			cIB[i][j] = pIn->body2imu[i][j];

	// make the cBI matrix
	transpose( cIB, cBI, 3, 3 );

	// get the CG LLH values in ft
	for(i=0; i<3; ++i)
		tempV1[i] = pIn->cg_pos[i]*C_FT2M;

	ECEF2llh( tempV1, LLHcg );
	LLHcg[2] = LLHcg[2]*C_M2FT;
	

	lat = LLHcg[0];
	lon = LLHcg[1];


	// NOW DO SOME MATH

	// compute the accelerations at the IMU
	// a_imu = cIB(a_cg + alpha X pos + w X w X pos)
	cross( pIn->cg_alpha, pIn->cg2imu, tempV1 );
	MMmult(wx, wx, tempM1, 3, 3, 3);
	MVmult(tempM1, pIn->cg2imu, tempV2, 3, 3);
	VVadd(tempV1, tempV2, tempV3, 3);
	VVadd(tempV3, pIn->cg_accel, tempV1, 3);
	MVmult(cIB, tempV1, pOut->accel, 3, 3);

	// compute the velocity at the IMU
	// v_imu = cIB(v_cg + w X pos)
	MVmult(wx, pIn->cg2imu, tempV1, 3, 3);
	VVadd(pIn->cg_uvw, tempV1, tempV2, 3);
	MVmult(cIB, tempV2, pOut->uvw, 3, 3);

	// compute the position of the IMU
	// rotate the body cg->imu vector to TP coordinates
	MVmult(cEB, pIn->cg2imu, tempV1, 3, 3);
	// rotate the TP cg->imu vector into ECEF coordinates
	Tangent2ECEF(tempV2, tempV1, lat, lon);
	// compute the ECEF position of the IMU
	VVadd(tempV2, pIn->cg_pos, pOut->ECEFpos, 3);
	// compute the LLH position of the IMU
	for(i=0; i<3; ++i)
		tempV1[i] = pOut->ECEFpos[i]*C_FT2M;

	ECEF2llh(tempV1, pOut->LLHpos);
	pOut->LLHpos[2] = pOut->LLHpos[2]*C_M2FT;

	// compute the angular velocity of the IMU
	MVmult(cIB, pIn->cg_pqr, pOut->pqr, 3, 3);

	// compute the attitude of the IMU
	MMmult(cIB, cBE, tempM1, 3, 3, 3);
	pOut->THETA[0] = atan2(tempM1[1][2], tempM1[2][2]);
	pOut->THETA[1] = -asin(tempM1[0][2]);
	pOut->THETA[2] = atan2(tempM1[0][1], tempM1[0][0]);
}

}
