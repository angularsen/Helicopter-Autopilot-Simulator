/* -*- indent-tabs-mode:T; c-basic-offset:8; tab-width:8; -*- vi: set ts=8:
 *  $Id: imu-filter.h,v 2.9 2003/03/14 19:43:53 tramm Exp $
 *
 * (c) Trammell Hudson <hudson@rotomotion.com>
 *
 * Board interface object
 *
 **************
 *
 *  This file is part of the autopilot simulation package.
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
#ifndef _IMU_filter_h_
#define _IMU_filter_h_

#include <imu-filter/IMU.h>
#include <imu-filter/GPS.h>
#include <imu-filter/AHRS.h>
#include <imu-filter/Radio.h>
#include "timer.h"
#include <iostream>
#include <map>

namespace imufilter
{

class IMU_filter
{
public:
	IMU_filter(
		int			serial_fd,
		bool			real_time,
		double			dt		= 32768.0 / 1000000.0
	);

	IMU			imu;
	AHRS			ahrs;
	Radio			radio;
	GPS			gps;
	double			heading;

	int			imu_samples;
	int			ahrs_samples;
	int			ppm_samples;
	int			heading_samples;
	int			gps_samples;

	double			time;

	bool
	logfile(
		const char *		file_name
	);

	bool
	step( void );


	/*
	 * File descriptor handlers
	 */
	typedef void (*handler_func)(
		int			fd,
		void *			priv
	);

	void
	add_fd(
		int			fd,
		handler_func		handler,
		void *			priv
	) {
		handler_t &		h = this->handlers[fd];

		h.handler		= handler;
		h.priv			= priv;
	}


	void
	remove_fd(
		int			fd
	) {
		this->handlers.erase( fd );
	}

private:
	void
	handle_adc(
		const char *		line
	);

	void
	handle_compass(
		const char *		line
	);

	void
	handle_angles(
		const char *		line
	);

	void
	handle_pqr(
		const char *		line
	);


	int			serial_fd;
	int			log_fd;

	const bool		real_time;
	const double		dt;
	stopwatch_t		start_time;


	struct handler_t {
		handler_func		handler;
		void *			priv;
	};
	typedef std::map<int,handler_t>	handler_map_t;
	handler_map_t		handlers;
};

}
#endif
