/* -*- indent-tabs-mode:T; c-basic-offset:8; tab-width:8; -*- vi: set ts=8:
 * $Id: commands.h,v 1.1 2003/03/08 05:29:25 tramm Exp $
 *
 * (c) Trammell Hudson
 *
 * Commands understood by the simulator
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
#ifndef _sim_commands_h_
#define _sim_commands_h_


#ifdef __cplusplus
namespace libstate
{
#endif

enum {
	COMMAND_NOP		= 0,
	COMMAND_OPEN		= 1,
	COMMAND_ACK		= 2,
	COMMAND_CLOSE		= 3,
	
	AHRS_STATE		= 40,
	AHRS_DT			= 41,

	ATTITUDE_GAIN_YAW	= 50,
	ATTITUDE_GAIN_ROLL	= 51,
	ATTITUDE_GAIN_PITCH	= 52,
	ATTITUDE_GAIN_COLL_U	= 53,
	ATTITUDE_GAIN_COLL_D	= 53,
	GUIDANCE_GAIN_X		= 55,
	GUIDANCE_GAIN_Y		= 56,
	GUIDANCE_GAIN_Z		= 57,

	SIM_QUIT		= 10,
	SIM_RESET		= 11,
	SIM_SET			= 12,

	SERVO_ROLL		= 20,
	SERVO_PITCH		= 21,
	SERVO_YAW		= 22,
	SERVO_COLL		= 23,
	SERVO_THROTTLE		= 24,
	SERVO_GYRO_GAIN		= 25,
	SERVO_EXTRA_0		= 26,
	SERVO_EXTRA_1		= 27,
	SERVO_EXTRA_2		= 28,
	SERVO_EXTRA_3		= 29,

	JOYSTICK		= 30,
	PPM			= 31,

	COMMAND_MAX		= 256
};

#ifdef __cplusplus
}
#endif

#endif
