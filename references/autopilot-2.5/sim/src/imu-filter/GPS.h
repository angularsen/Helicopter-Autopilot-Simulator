/* -*- indent-tabs-mode:T; c-basic-offset:8; tab-width:8; -*- vi: set ts=8:
 *  $Id: GPS.h,v 1.2 2002/09/29 22:00:23 tramm Exp $
 *
 * (c) Trammell Hudson <hudson@rotomotion.com>
 *
 * GPS decoder object
 *
 * Takes NMEA lines and produces position / velocity values.
 *
 * GPGGA sentence details from:
 *	http://home.mira.net/~gnb/gps/nmea.html#gga
 *
 * Sample sentences from Cm3-SV6:
 * $GPGGA,020314.0,3902.848,N,07706.833,W,1,4,002.3,,M,-033,M,,*50
 * $GPVTG,159,T,169,M,04.1,N,07.7,K*48 
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
#ifndef _GPS_H_
#define _GPS_H_


namespace imufilter
{

class GPS
{
public:
	GPS();


	void
	update(
		const char *		line
	);


	/* These come from GPGGA sentences */
	int			time;		// seconds past midnight

	double			latitude;	// N=+, S=-
	double			longitude;	// E=+, W=-

	int			quality;	// 0=invalid, 1=gps, 2=dgps
	int			num_sats;

	double			hdop;		// meters
	double			altitude;	// meters, if available
	double			wgs_alt;	// meters

	/* These are GPVTG sentences */
	double			track;		// magnetic
	double			ground_speed;	// km/h
	
private:

	void
	gpgga_update(
		const char *		line
	);


	void
	gpvgt_update(
		const char *		line
	);

};

}
#endif
