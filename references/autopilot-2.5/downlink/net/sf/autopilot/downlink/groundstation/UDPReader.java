/* -*- indent-tabs-mode:T; c-basic-offset:8; tab-width:8; -*- vi: set ts=8:
	* $Id: UDPReader.java,v 1.1 2002/07/14 04:31:52 dennisda Exp $
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
import java.util.Arrays;

public class UDPReader extends Thread {
        public static final int UDP_DATA_PORT = 12367;
        private DatagramSocket socket;
        private InetAddress remoteHost;
        private int remotePort;
        // change this to the ip of your helicopter
        String server_name = "10.255.0.77";

        /** @supplierCardinality 1 */
        FrameController frameController;
        IMULine imuLine;

        /** @link dependency */

	/*# LineParser lnkLineParser; */

        public UDPReader(FrameController frameController) {
                super();
                this.frameController = frameController;
                try {
                        remoteHost = InetAddress.getByName(server_name);
                        remotePort = UDP_DATA_PORT;
                } catch (Exception e) {
                        //ee-gads
                }
        }

        //	public static void main(String [] argv) {
        //		new ClientReceiver().start();
        //   }
        public void run() {
                try {
                        socket = new DatagramSocket();
                } catch (Exception e) {
                        //blah
                }
                byte[] outData = new byte[1];
                byte[] inData = new byte[1024];
                DatagramPacket outPacket = new DatagramPacket(outData, outData.length, remoteHost, remotePort);
                DatagramPacket inPacket = new DatagramPacket(inData, inData.length);
                try {
                        // every so often reset the connection
                        long start = System.currentTimeMillis();
                        long current;
                        while (true) {
                                // send first "ping" packet
                                Arrays.fill(outData, (byte)2);
                                //	outData = {2};
                                outPacket.setData(outData);
                                outPacket.setLength(outData.length);
                                socket.setSoTimeout(5000);
                                try {
                                        socket.send(outPacket);
                                        //						foundServer=true;
                                } catch (java.io.InterruptedIOException ioe) {
                                        System.out.println("SOCKETTIME on SERVERFINE");
                                }
                                boolean run = true;
                                long connectionStart = System.currentTimeMillis();
                                long start2 = System.currentTimeMillis();
                                String line;
                                int j = 0;
                                while (run) {
                                        current = System.currentTimeMillis();
                                        if ((current - start) > 1000) {
                                                start = current;
                                                try {
                                                  socket.send(outPacket);
                                                } catch (Exception e) {
                                                  System.out.print("ping packet send error. e=" + e);
                                                }
                                                // send ping packet to keep server sending data
                                                // to us
                                        }
					current = System.currentTimeMillis();
                                        if ((current - start2) > 1000) {
                                                start2 = current;
                                                frameController.setRefreshRate(j + "");
                                                j = 0;
                                        }
                                        j++;
                                        // receive data
                                        inPacket.setData(inData);
                                        inPacket.setLength(inData.length);
                                        try {
                                                socket.setSoTimeout(1000);
                                                socket.receive(inPacket);
                                                line = new String(inPacket.getData(), 0, inPacket.getLength());
                                                imuLine = LineParser.parse(line);
                                                frameController.getFlightLog().add(imuLine);
                                                frameController.setParam(imuLine);
                                                //   if (imuLine instanceof GPADCLine) {
                                                //     System.out.println(((GPADCLine)imuLine).toString());
                                                //   }
                                                frameController.setParam(imuLine);
                                        } catch (Exception eee) {
                                                System.out.println("Oh no!!");
                                                System.out.println("Eee=" + eee);
                                        }
                                }
                                //	socket.close();
                        }
                } catch (Exception e) {
                        System.out.println("Exception = " + e);
                }
        }
}
