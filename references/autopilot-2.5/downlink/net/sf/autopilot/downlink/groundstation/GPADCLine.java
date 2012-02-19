/* -*- indent-tabs-mode:T; c-basic-offset:8; tab-width:8; -*- vi: set ts=8:
	* $Id: GPADCLine.java,v 1.1 2002/07/14 04:31:52 dennisda Exp $
	*
	*  (c) Dennis D'Annunzio <ciogeneral@positivechanges.com>
	*
	*************
	*
	*  This file is part of the autopilot simulation package.
	*  http://autopilot.sf.net
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

package net.sf.autopilot.downlink.groundstation;

import java.io.Serializable;
import java.util.Date;

public class GPADCLine extends IMULine implements Serializable {
        int[] params = new int[8];

        public GPADCLine(String rawLine, int p1, int p2, int p3, int p4, int p5, int p6, int p7, int p8) {
                super("");
                params[0] = p1;
                params[1] = p2;
                params[2] = p3;
                params[3] = p4;
                params[4] = p5;
                params[5] = p6;
                params[6] = p7;
                params[7] = p8;
        }

        public int getParam(int i) { return params[i]; }

        public String toString() {
                StringBuffer returnString = new StringBuffer("");
                returnString.append("<GPADCLine> p1=");
                returnString.append(params[0]);
                returnString.append(",p2=");
                returnString.append(params[1]);
                returnString.append(",p3=");
                returnString.append(params[2]);
                returnString.append(",p4=");
                returnString.append(params[3]);
                returnString.append(",p5=");
                returnString.append(params[4]);
                returnString.append(",p6=");
                returnString.append(params[5]);
                returnString.append(",p7=");
                returnString.append(params[6]);
                returnString.append(",p8=");
                returnString.append(params[7]);
                returnString.append(" </GPADCLine>");
                return returnString.toString();
        }
}
