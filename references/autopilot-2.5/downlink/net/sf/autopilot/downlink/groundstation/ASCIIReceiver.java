/* -*- indent-tabs-mode:T; c-basic-offset:8; tab-width:8; -*- vi: set ts=8:
	* $Id: ASCIIReceiver.java,v 1.1 2002/07/14 04:31:52 dennisda Exp $
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

import java.net.Socket;
import java.io.BufferedInputStream;
import java.io.ObjectInputStream;
import java.net.DatagramSocket;
import java.net.DatagramPacket;
import java.net.MulticastSocket;
import java.net.InetAddress;
import java.io.BufferedReader;
import java.io.InputStreamReader;
import java.util.Observable;

/**
 * @testcase test.net.sf.autopilot.downlink.groundstation.TestASCIIReceiver 
 */
public class ASCIIReceiver extends Thread {
        /** @supplierCardinality 1 */
        FrameController frameController;
        IMULine imuLine;

        /** @link dependency */

    /*# LineParser lnkLineParser; */

        public ASCIIReceiver(FrameController frameController) {
                super();
                this.frameController = frameController;
        }

        //	public static void main(String [] argv) {
        //		new ClientReceiver().start();
        //   }
        public void run() {
                //	String server_name="127.0.0.1"; // need to put this into a runtime config class or use a deployment desc or properties file
                String server_name = "10.255.0.77";
                try {
                        // every so often reset the connection
                        long start = System.currentTimeMillis();
                        long current;
                        while (true) {
                                Socket socket = new Socket(server_name, 12346);
                                System.out.println("Connected to " + server_name + "!");
                                BufferedReader inputBuffer = new
                                    BufferedReader(new InputStreamReader(socket.getInputStream()));
                                // ObjectInputStream is = new ObjectInputStream(new BufferedInputStream(socket.getInputStream()));
                                int i = 0;
                                int j = 0;
                                boolean run = true;
                                long connectionStart = System.currentTimeMillis();
                                String line;
                                while (run) {
                                        while ((((line = inputBuffer.readLine()) == null)) && run == true);
                                        if (!run) continue;
                                        current = System.currentTimeMillis();
                                        if ((current - start) > 1000) {
                                                start = current;
                                                frameController.setRefreshRate(j + "");
                                                j = 0;
                                        }
                                        j++;
                                        imuLine = LineParser.parse(line);
                                        frameController.getFlightLog().add(imuLine);
                                        frameController.setParam(imuLine);
                                        //	if(imuLine instanceof GPADCLine) {
                                        //		System.out.println(( (GPADCLine) imuLine).toString());
                                        //	}
                                        //if ( (current-connectionStart) > 3200 ) {
                                        //	run=false;
                                        //	}
                                        //	System.out.println(line);
                                }
                                socket.close();
                        }
                } catch (Exception e) {
                        System.out.println("Exception = " + e);
                }
        }
}
