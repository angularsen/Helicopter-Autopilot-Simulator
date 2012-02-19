/* -*- indent-tabs-mode:T; c-basic-offset:8; tab-width:8; -*- vi: set ts=8:
	* $Id: FrameController.java,v 1.1 2002/07/14 04:31:52 dennisda Exp $
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

class FrameController {
        /** @supplierCardinality 1 */
        FlightLog flightLog;
        TCClient tcClient;

        /** @supplierCardinality 1 */
        UDPReader clientReceiver;

        public FrameController(TCClient tcClient) {
                this.tcClient = tcClient;
                flightLog = new FlightLog();
                //	clientReceiver = new ClientReceiver(this);
                clientReceiver = new UDPReader(this);
                try {
                        Thread.sleep(10);
                } catch (Exception e) {
                        //
                }
                clientReceiver.start();
        }

        public FlightLog getFlightLog() { return flightLog; }

        public TCClient getTCClient() { return tcClient; }

        public UDPReader getClientReceiver() { return clientReceiver; }

        protected void setTCClient(TCClient tcClient) { this.tcClient = tcClient; }

        protected void setFlightLog(FlightLog flightLog) { this.flightLog = flightLog; }

        protected void setClientReceiver(UDPReader clientReceiver) { this.clientReceiver = clientReceiver; }

        public void setRefreshRate(String rate) {
                tcClient.setRefreshRate(rate);
        }

        public void setParam4(IMULine imuLine) {
                tcClient.setParam4(imuLine);
        }

        public void setParam5(IMULine imuLine) {
                tcClient.setParam5(imuLine);
        }

        public void setParam6(IMULine imuLine) {
                tcClient.setParam6(imuLine);
        }

        public void setParam7(IMULine imuLine) {
                tcClient.setParam7(imuLine);
        }

        public void setParam8(IMULine imuLine) {
                tcClient.setParam8(imuLine);
        }

        public void setParam(IMULine imuLine) {
                setParam4(imuLine);
                setParam5(imuLine);
                setParam6(imuLine);
                setParam7(imuLine);
                setParam8(imuLine);
        }
}
