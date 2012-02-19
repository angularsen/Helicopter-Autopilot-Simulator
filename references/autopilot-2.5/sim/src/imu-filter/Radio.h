/* -*- indent-tabs-mode:T; c-basic-offset:8; tab-width:8; -*- vi: set ts=8:
 *  $Id: Radio.h,v 2.1 2002/09/26 16:09:27 tramm Exp $
 *
 * (c) Trammell Hudson <hudson@rotomotion.com>
 *
 * PPM decoder object.
 *
 * Accepts signals from the radio and translates them into
 * servo commands.
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
#ifndef _Radio_h_
#define _Radio_h_

#include <unistd.h>


namespace imufilter
{

class Radio
{
public:
	Radio();

	int			collective;
	int			throttle;
	int			roll;
	int			pitch;
	int			yaw;

	int			mode;
	int			manual;
	int			extra;

	int			pulse_width;

	bool			ppm_valid;

	void
	update(
		const char *		line
	);

	int
	output(
		char *			line,
		size_t			len
	) const;


private:
	const int		pulse_width_index;

	const int		collective_index;
	const int		throttle_index;
	const int		roll_index;
	const int		pitch_index;
	const int		yaw_index;
	const int		extra_index;

	const int		manual_index;
	const int		manual_threshold;

	const int		mode_index;
	const int		mode_threshold_0;
	const int		mode_threshold_1;


};

}
#endif
