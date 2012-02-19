/* -*- indent-tabs-mode:T; c-basic-offset:8; tab-width:8; -*- vi: set ts=8:
 * $Id: old-quat.h,v 2.0 2002/09/22 02:07:32 tramm Exp $
 *
 * Quaternion manipulation routines
 *
 *	Author: Aaron Kahn, Suresh Kannan, Eric Johnson
 *	copyright 2001
 *	Portions (c) Trammell Hudson
 */

#ifndef _QUAT_H_
#define _QUAT_H_

namespace matlib
{

/*
 * This will construct a direction cosine matrix from 
 * euler angles in the standard rotation sequence 
 * [phi][theta][psi] from NED to body frame
 *
 * body = tBL(3,3)*NED
 */
extern void
eulerDC(
	double			tBL[MAXSIZE][MAXSIZE],
	double			phi,
	double			theta,
	double			psi
);

/*
 * This will construct a direction cosine matrix from 
 * quaternions in the standard rotation  sequence
 * [phi][theta][psi] from NED to body frame
 *
 * body = tBL(3,3)*NED
 * q(4,1)
 */
extern void
quatDC(
	double			tBL[MAXSIZE][MAXSIZE],
	const double *		q
);


/*
 * This will construct the euler omega-cross matrix
 * wx(3,3)
 * p, q, r (rad/sec)
 */
extern void
eulerWx(
	double			Wx[MAXSIZE][MAXSIZE],
	double			p,
	double			q,
	double			r
);

/*
 * This will construct the quaternion omega matrix
 * W(4,4)
 * p, q, r (rad/sec)
 */
extern void
quatW(
	double			W[MAXSIZE][MAXSIZE],
	double			p,
	double			q,
	double			r
);


/*
 * This will normalize a quaternion vector q
 * q/norm(q)
 * q(4,1)
 */
extern void
normq(
	double *		q
);


/*
 * This will convert from quaternions to euler angles
 * q(4,1) -> euler[phi;theta;psi] (rad)
 */
extern void
quat2euler(
	const double *		q,
	double *		euler
);


/*
 * This will convert from euler angles to quaternion vector
 * phi, theta, psi -> q(4,1)
 * euler angles in radians
 */
extern void
euler2quat(
	double *		q,
	double			phi,
	double			theta,
	double			psi
);



}

#endif
