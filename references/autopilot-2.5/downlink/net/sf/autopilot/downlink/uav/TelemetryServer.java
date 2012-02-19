/* -*- indent-tabs-mode:T; c-basic-offset:8; tab-width:8; -*- vi: set ts=8:
	* $Id: TelemetryServer.java,v 1.1 2002/07/14 04:31:52 dennisda Exp $
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

package net.sf.autopilot.downlink.uav;

import java.io.*;
import java.net.ServerSocket;
import java.net.Socket;

// IMU source input, data source generator
public class TelemetryServer {
        public static final int PORT = 12346;
        public static final int NUM_THREADS = 4;
        ASCIIWriter[] asciiWriters;
        UDPWriter[] udpWriters;

        public ASCIIWriter[] getASCIIWriters() { return asciiWriters; }

        public UDPWriter[] getUDPWriters() { return udpWriters; }

        IMUReader imuReader;

        public static void main(String[] args) {
                new TelemetryServer();
        }

        public IMUReader getIMUReader() { return imuReader; }

        public TelemetryServer() {
                System.out.println("Title line and welcome message");
                System.out.println("Starting Socket server...");
                ServerSocket serverSocket = null;
                Socket clientSocket;
                try {
                        serverSocket = new ServerSocket(PORT);
                } catch (IOException ioe) {
                        // bad place to be
                        System.out.println("Unable to create server socket... " + ioe);
                        System.exit(1);
                }
                // create the ascii sockets
                asciiWriters = new ASCIIWriter[4];
                for (int i = 0; i < NUM_THREADS; i++) {
                        asciiWriters[i] = new ASCIIWriter(this, serverSocket, i);
                        asciiWriters[i].start();
                }
                // give the ipaq a chance to finish creating threads
                try {
                        Thread.sleep(1000);
                } catch (InterruptedException ie) {
                        // oops!
                }
                udpWriters = new UDPWriter[1];
                udpWriters[0] = new UDPWriter(this);
                udpWriters[0].start();
                System.out.println("Starting IMUReader...");
                imuReader = new IMUReader(this);
                imuReader.start();
        }
}
