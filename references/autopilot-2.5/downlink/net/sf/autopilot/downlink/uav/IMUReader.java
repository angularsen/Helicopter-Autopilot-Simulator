/* -*- indent-tabs-mode:T; c-basic-offset:8; tab-width:8; -*- vi: set ts=8:
	* $Id: IMUReader.java,v 1.2 2002/07/26 03:34:17 dennisda Exp $
	*
	*  (c) Dennis D'Annunzio <ciogeneral@positivechanges.com>
	*
	*************
	*
	*  This file is part of the autopilot downlink package.
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

package net.sf.autopilot.downlink.uav;

import java.io.File;
import java.io.FileInputStream;

public class IMUReader extends Thread {
        public static final String IMU_SERIAL_DEVICE = "/dev/ttySA0";
        // public static final String IMU_SERIAL_DEVICE="c:\\sample.imu.data.txt";
        private static final int BUFFER_SIZE = 300;
        private static final int LINE_SIZE = 125;
        TelemetryServer systemHandle;

        public IMUReader(TelemetryServer systemHandle) {
                super();
                this.systemHandle = systemHandle;
        }

        public void run() {
                System.out.println("IMUReader.run()");
                String line;
                boolean doRun = true;
                // try scope is a bit wide, could be more useful otherwise
                try {
                        File inputDevice = new File(IMU_SERIAL_DEVICE);
                        FileInputStream inputStream = new FileInputStream(inputDevice);
                        ASCIIWriter[] asciiWriters;
                        UDPWriter[] udpWriters;
                        byte[] inputBuffer = new byte[BUFFER_SIZE];
                        byte[] tempLine = new byte[LINE_SIZE];
                        byte[] tempBuffer = new byte[LINE_SIZE];
                        byte[] combBuffer = new byte[BUFFER_SIZE + LINE_SIZE];
                        int amtRead = 0;
                        int i;
                        int offset = 0;
                        String outputLine;
                        while (doRun) {
                                // read a block of input into a buffer
                                amtRead = inputStream.read(inputBuffer);
                                if (-1 == amtRead) {
                                        // we have hit the EOF
                                        System.exit(1);
                                }
                                // copy the remainder of the last line into the main buffer
				int l = 0;
				for (int j = 0; j < offset; j++, l++) {
					combBuffer[l] = tempBuffer[j];
				}
				// copy the data read into the main buffer

				for (int j = 0; j < amtRead; j++, l++) {
					combBuffer[l] = inputBuffer[j];
				}
				i = 0;
                                boolean runLoop = true;
                                while (runLoop) {
                                        int k = 0;
                                        // while we havn't seen a CR, and we not at the end
                                        // of the buffer
					while ((combBuffer[i] != 10) && (k < tempLine.length) && (i < l)) {
						if (combBuffer[i] != 32) {
							  tempLine[k++] = combBuffer[i];
						}
						i++;
					}
                                        if (i == l) {
                                                // this is a partial line that should be carried forward
                                                // to next batch.  Copy incomplete line section back
                                                // into buffer and set offset for read op
						for (int j = 0; j < k; j++) {
							tempBuffer[j] = tempLine[j];
						}
						offset = k;
                                                runLoop = false;
                                                //	System.out.println(new String(inputBuffer,0,k));
                                        } else {
                                                // output line to clients
						if (k>0) k--;
						tempLine[k] = 10;
						i++;

                                                outputLine = new String(tempLine, 0, k);
                                                //	System.out.println(outputLine);
                                                udpWriters = (UDPWriter[]) systemHandle.getUDPWriters();
                                                if (udpWriters[0].isActive()) {
                                                  udpWriters[0].sendLine(outputLine);
                                                }
                                                asciiWriters = (ASCIIWriter[]) systemHandle.getASCIIWriters();
                                                for (int j = 0; j < asciiWriters.length; j++) {
                                                  if (asciiWriters[j].isActive()) {
                                                  asciiWriters[j].sendLine(outputLine);
                                                  }
                                                }
                                        }
                                        // when I'm reading from a file for devel., slow down the parser
                                        // like the prod one is IO bound
                                        //this.sleep(150);
                                }
                                Thread.sleep(1);
                        }
                }
                catch (Exception io) {
                        // this is a bad situation to be in
                        System.out.println("IMUReader, main try block exception=" + io);
                }
        }
}
