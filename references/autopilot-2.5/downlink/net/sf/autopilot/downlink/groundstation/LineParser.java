/* -*- indent-tabs-mode:T; c-basic-offset:8; tab-width:8; -*- vi: set ts=8:
	* $Id: LineParser.java,v 1.1 2002/07/14 04:31:52 dennisda Exp $
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

import java.util.Vector;
import java.io.Serializable;
import java.util.StringTokenizer;
import java.util.Enumeration;
import org.apache.regexp.*;

public class LineParser implements Serializable {
        // take a line from the IMU and generate a java object

	/*
     *  IMU ADC raw values
     */

        final static String GPADC_HDR = "$GPADC";
        public final static int GPADC = 0;

    /*
     *  IMU mode info
     */

        public final static int GPMOD = 1;

    /*
     *  enging RPM
     */

        public final static int GPRPM = 2;
        final static String GPRPM_HDR = "$GPRPM";

    /*
     *  radio RX PPM signals
     */

        public final static int GPPPM = 3;

    /*
     *  GPAXY (computed values)
     */

        public final static int GPAXY = 4;

    /*
     *  GPRAT  (computed values)
     */

        public final static int GPRAT = 5;

    /*
     *  GPS NMEA string
     */

        public final static int GPGGA = 6;

    /*
     *  IMU code id string
     */

        public final static int GPID = 7;
        public static String GPID_HDR = "$Id";

    /*
     * UNKNOWN IMU sentence
     */

        public final static int UNKNOWN = 99;
        //regex stuff is from www.apache.org REGEX java library
        // Pre-compiled regular expression '^\$GPADC,([ABCDEF0123456789\+\-]+),([ABCDEF0123456789\+\-]+),([ABCDEF0123456789\+\-]+),([ABCDEF0123456789\+\-]+),([ABCDEF0123456789\+\-]+),([ABCDEF0123456789\+\-]+),([ABCDEF0123456789\+\-]+),([ABCDEF0123456789\+\-]+),\*00$'
        private static char[] GPADCPatternInstructions =
                {
        0x007c, 0x0000, 0x0206, 0x005e, 0x0000, 0x0003, 0x0041,
        0x0007, 0x000a, 0x0024, 0x0047, 0x0050, 0x0041, 0x0044,
        0x0043, 0x002c, 0x0028, 0x0001, 0x0003, 0x007c, 0x0000,
        0x0034, 0x005b, 0x0011, 0x0025, 0x0041, 0x0041, 0x0042,
        0x0042, 0x0043, 0x0043, 0x0044, 0x0044, 0x0045, 0x0045,
        0x0046, 0x0046, 0x0030, 0x0030, 0x0031, 0x0031, 0x0032,
        0x0032, 0x0033, 0x0033, 0x0034, 0x0034, 0x0035, 0x0035,
        0x0036, 0x0036, 0x0037, 0x0037, 0x0038, 0x0038, 0x0039,
        0x0039, 0x002d, 0x002d, 0x007c, 0x0000, 0x0006, 0x0047,
        0x0000, 0xffd8, 0x007c, 0x0000, 0x0003, 0x004e, 0x0000,
        0x0003, 0x0029, 0x0001, 0x0003, 0x0041, 0x0001, 0x0004,
        0x002c, 0x0028, 0x0002, 0x0003, 0x007c, 0x0000, 0x0034,
        0x005b, 0x0011, 0x0025, 0x0041, 0x0041, 0x0042, 0x0042,
        0x0043, 0x0043, 0x0044, 0x0044, 0x0045, 0x0045, 0x0046,
        0x0046, 0x0030, 0x0030, 0x0031, 0x0031, 0x0032, 0x0032,
        0x0033, 0x0033, 0x0034, 0x0034, 0x0035, 0x0035, 0x0036,
        0x0036, 0x0037, 0x0037, 0x0038, 0x0038, 0x0039, 0x0039,
        0x002d, 0x002d, 0x007c, 0x0000, 0x0006, 0x0047, 0x0000,
        0xffd8, 0x007c, 0x0000, 0x0003, 0x004e, 0x0000, 0x0003,
        0x0029, 0x0002, 0x0003, 0x0041, 0x0001, 0x0004, 0x002c,
        0x0028, 0x0003, 0x0003, 0x007c, 0x0000, 0x0034, 0x005b,
        0x0011, 0x0025, 0x0041, 0x0041, 0x0042, 0x0042, 0x0043,
        0x0043, 0x0044, 0x0044, 0x0045, 0x0045, 0x0046, 0x0046,
        0x0030, 0x0030, 0x0031, 0x0031, 0x0032, 0x0032, 0x0033,
        0x0033, 0x0034, 0x0034, 0x0035, 0x0035, 0x0036, 0x0036,
        0x0037, 0x0037, 0x0038, 0x0038, 0x0039, 0x0039, 0x002d,
        0x002d, 0x007c, 0x0000, 0x0006, 0x0047, 0x0000, 0xffd8,
        0x007c, 0x0000, 0x0003, 0x004e, 0x0000, 0x0003, 0x0029,
        0x0003, 0x0003, 0x0041, 0x0001, 0x0004, 0x002c, 0x0028,
        0x0004, 0x0003, 0x007c, 0x0000, 0x0034, 0x005b, 0x0011,
        0x0025, 0x0041, 0x0041, 0x0042, 0x0042, 0x0043, 0x0043,
        0x0044, 0x0044, 0x0045, 0x0045, 0x0046, 0x0046, 0x0030,
        0x0030, 0x0031, 0x0031, 0x0032, 0x0032, 0x0033, 0x0033,
        0x0034, 0x0034, 0x0035, 0x0035, 0x0036, 0x0036, 0x0037,
        0x0037, 0x0038, 0x0038, 0x0039, 0x0039, 0x002d, 0x002d,
        0x007c, 0x0000, 0x0006, 0x0047, 0x0000, 0xffd8, 0x007c,
        0x0000, 0x0003, 0x004e, 0x0000, 0x0003, 0x0029, 0x0004,
        0x0003, 0x0041, 0x0001, 0x0004, 0x002c, 0x0028, 0x0005,
        0x0003, 0x007c, 0x0000, 0x0034, 0x005b, 0x0011, 0x0025,
        0x0041, 0x0041, 0x0042, 0x0042, 0x0043, 0x0043, 0x0044,
        0x0044, 0x0045, 0x0045, 0x0046, 0x0046, 0x0030, 0x0030,
        0x0031, 0x0031, 0x0032, 0x0032, 0x0033, 0x0033, 0x0034,
        0x0034, 0x0035, 0x0035, 0x0036, 0x0036, 0x0037, 0x0037,
        0x0038, 0x0038, 0x0039, 0x0039, 0x002d, 0x002d, 0x007c,
        0x0000, 0x0006, 0x0047, 0x0000, 0xffd8, 0x007c, 0x0000,
        0x0003, 0x004e, 0x0000, 0x0003, 0x0029, 0x0005, 0x0003,
        0x0041, 0x0001, 0x0004, 0x002c, 0x0028, 0x0006, 0x0003,
        0x007c, 0x0000, 0x0034, 0x005b, 0x0011, 0x0025, 0x0041,
        0x0041, 0x0042, 0x0042, 0x0043, 0x0043, 0x0044, 0x0044,
        0x0045, 0x0045, 0x0046, 0x0046, 0x0030, 0x0030, 0x0031,
        0x0031, 0x0032, 0x0032, 0x0033, 0x0033, 0x0034, 0x0034,
        0x0035, 0x0035, 0x0036, 0x0036, 0x0037, 0x0037, 0x0038,
        0x0038, 0x0039, 0x0039, 0x002d, 0x002d, 0x007c, 0x0000,
        0x0006, 0x0047, 0x0000, 0xffd8, 0x007c, 0x0000, 0x0003,
        0x004e, 0x0000, 0x0003, 0x0029, 0x0006, 0x0003, 0x0041,
        0x0001, 0x0004, 0x002c, 0x0028, 0x0007, 0x0003, 0x007c,
        0x0000, 0x0034, 0x005b, 0x0011, 0x0025, 0x0041, 0x0041,
        0x0042, 0x0042, 0x0043, 0x0043, 0x0044, 0x0044, 0x0045,
        0x0045, 0x0046, 0x0046, 0x0030, 0x0030, 0x0031, 0x0031,
        0x0032, 0x0032, 0x0033, 0x0033, 0x0034, 0x0034, 0x0035,
        0x0035, 0x0036, 0x0036, 0x0037, 0x0037, 0x0038, 0x0038,
        0x0039, 0x0039, 0x002d, 0x002d, 0x007c, 0x0000, 0x0006,
        0x0047, 0x0000, 0xffd8, 0x007c, 0x0000, 0x0003, 0x004e,
        0x0000, 0x0003, 0x0029, 0x0007, 0x0003, 0x0041, 0x0001,
        0x0004, 0x002c, 0x0028, 0x0008, 0x0003, 0x007c, 0x0000,
        0x0034, 0x005b, 0x0011, 0x0025, 0x0041, 0x0041, 0x0042,
        0x0042, 0x0043, 0x0043, 0x0044, 0x0044, 0x0045, 0x0045,
        0x0046, 0x0046, 0x0030, 0x0030, 0x0031, 0x0031, 0x0032,
        0x0032, 0x0033, 0x0033, 0x0034, 0x0034, 0x0035, 0x0035,
        0x0036, 0x0036, 0x0037, 0x0037, 0x0038, 0x0038, 0x0039,
        0x0039, 0x002d, 0x002d, 0x007c, 0x0000, 0x0006, 0x0047,
        0x0000, 0xffd8, 0x007c, 0x0000, 0x0003, 0x004e, 0x0000,
        0x0003, 0x0029, 0x0008, 0x0003, 0x0041, 0x0004, 0x0007,
        0x002c, 0x002a, 0x0030, 0x0030, 0x0024, 0x0000, 0x0003,
        0x0045, 0x0000, 0x0000,
    };

        //  private static RE GPADCPattern = new RE(new REProgram(GPADCPatternInstructions));
        public static int whatType(String line) {
                if (line.startsWith(GPADC_HDR))
                        return GPADC;
                else if (line.startsWith(GPRPM_HDR))
                        return GPRPM;
                else if (line.startsWith(GPID_HDR))
                        return GPID;
                else
                        return UNKNOWN;
        }

        public static IMULine parse(String inputLine) {
                IMULine returnObject = new IMULine(inputLine);
                if (GPADC == whatType(inputLine)) {
                        //	System.out.println("GPADC line="+inputLine);
                        int[] pInts = new int[8];
                        RE GPADCPattern = null;
                        try {
                                GPADCPattern = new RE("^\\$GPADC,([ABCDEF0123456789]+),([ABCDEF0123456789]+),([ABCDEF0123456789]+),([ABCDEF0123456789]+),([ABCDEF0123456789]+),([ABCDEF0123456789]+),([ABCDEF0123456789]+),([ABCDEF0123456789]+)");
                        } catch (RESyntaxException reSyntax) {
                                // error
                                System.out.println("GPADC re syntax error");
                        }
                        //if (GPADCPattern.match(inputLine)) {
                        if (GPADCPattern.match(inputLine)) {
                                //				System.out.println("matches");
                                int numParens = GPADCPattern.getParenCount();
                                //				System.out.println("parenCount="+numParens);
                                try {
                                        pInts[0] = Integer.parseInt(GPADCPattern.getParen(1), 16);
                                } catch (Exception formate) {
                                        pInts[0] = -1;
                                }
                                try {
                                        pInts[1] = Integer.parseInt(GPADCPattern.getParen(2), 16);
                                } catch (Exception formate) {
                                        pInts[1] = -1;
                                }
                                try {
                                        pInts[2] = Integer.parseInt(GPADCPattern.getParen(3), 16);
                                } catch (Exception formate) {
                                        pInts[2] = -1;
                                }
                                try {
                                        pInts[3] = Integer.parseInt(GPADCPattern.getParen(4), 16);
                                } catch (Exception formate) {
                                        pInts[3] = -1;
                                }
                                try {
                                        pInts[4] = Integer.parseInt(GPADCPattern.getParen(5), 16);
                                } catch (Exception formate) {
                                        pInts[4] = -1;
                                }
                                try {
                                        pInts[5] = Integer.parseInt(GPADCPattern.getParen(6), 16);
                                } catch (Exception formate) {
                                        pInts[5] = -1;
                                }
                                try {
                                        pInts[6] = Integer.parseInt(GPADCPattern.getParen(7), 16);
                                } catch (Exception formate) {
                                        pInts[6] = -1;
                                }
                                try {
                                        pInts[7] = Integer.parseInt(GPADCPattern.getParen(8), 16);
                                } catch (Exception formate) {
                                        pInts[7] = -1;
                                }
                                returnObject = new GPADCLine(inputLine, pInts[0], pInts[1], pInts[2], pInts[3], pInts[4],
                                    pInts[5], pInts[6], pInts[7]);
                        } else {
                                // we have a problem here, probably serial port error
                                // or other data stream obsfucating factor, just don't
                                // do anything and the method will return a null object
                        }
                }
                else if (GPRPM == whatType(inputLine)) {
                        //   System.out.println("GPRPM"); // debug output
                }
                else if (GPID == whatType(inputLine)) {
                        //     System.out.println("GPID");  //debug output
                }
                return returnObject;
        }
}
