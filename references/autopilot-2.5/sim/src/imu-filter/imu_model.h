/* -*- indent-tabs-mode:T; c-basic-offset:8; tab-width:8; -*- vi: set ts=8:
 * $Id: imu_model.h,v 2.0 2002/09/22 02:07:31 tramm Exp $
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


#ifndef _IMU_MODEL_H_
#define _IMU_MODEL_H_

namespace sim
{

typedef struct
{
	// position vector from CG->IMU body axis [X Y Z] (ft)
	double			cg2imu[MAXSIZE];

	// rotation matrix body axis->IMU axis
	double			body2imu[MAXSIZE][MAXSIZE];

	// body axis accel. at CG [X Y Z] (ft/s/s)
	double			cg_accel[MAXSIZE];

	// body axis veloctiy at CG [X Y Z] (ft/s)
	double			cg_uvw[MAXSIZE];

	// ECEF pos. at CG [X Y Z] (ft)
	double			cg_pos[MAXSIZE];

	// body axis angular accel. at CG [Pdot Qdot Rdot] (rad/s/s)
	double			cg_alpha[MAXSIZE];

	// body axis angular rates at CG [P Q R] (rad/s)
	double			cg_pqr[MAXSIZE];

	// body axis attitude at CG [phi theta psi] (rad)
	double			cg_THETA[MAXSIZE];
} imu_inputs_def;


typedef struct
{
	// IMU axis accel at IMU [X Y Z] (ft/s/s)
	double			accel[MAXSIZE];

	// IMU axis velocity at IMU [X Y Z] (ft/s)
	double			uvw[MAXSIZE];

	// ECEF pos of IMU [X Y Z] (ft)
	double			ECEFpos[MAXSIZE];

	// LLH pos of IMU [latitude longitude altitude] (rad)(rad)(ft MSL + up)
	double			LLHpos[MAXSIZE];

	// IMU axis angular rate at IMU [P Q R] (rad/s)
	double			pqr[MAXSIZE];

	// IMU axis attitude at IMU [phi thata psi] (rad)
	double			THETA[MAXSIZE];
} imu_outputs_def;


extern void
imu_model(
	imu_inputs_def *	pIn,
	imu_outputs_def *	pOut
);

}

#endif  
