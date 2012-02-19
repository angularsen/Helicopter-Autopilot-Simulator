/* -*- indent-tabs-mode:T; c-basic-offset:8; tab-width:8; -*- vi: set ts=8:
	* $Id: TelemetryClient.java,v 1.1 2002/07/14 04:31:52 dennisda Exp $
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

import javax.swing.UIManager;
import java.awt.Dimension;
import java.awt.Toolkit;

public class TelemetryClient {
        TCClient frame;
        FrameController frameController;

        public TelemetryClient() {
                frame = new TCClient();
                frameController = new FrameController(frame);
                //Center the frame on screen
                Dimension screenSize = Toolkit.getDefaultToolkit().getScreenSize();
                Dimension frameSize = frame.getSize();
                frameSize.height = ((frameSize.height > screenSize.height) ? screenSize.height : frameSize.height);
                frameSize.width = ((frameSize.width > screenSize.width) ? screenSize.width : frameSize.width);
                frame.setLocation((screenSize.width - frameSize.width) / 2, (screenSize.height - frameSize.height) / 2);
                frame.setVisible(true);
        }

        public static void main(String[] argv) {
                // set up system Look&Feel
                try {
                        UIManager.setLookAndFeel(UIManager.getSystemLookAndFeelClassName());
                }
                catch (Exception e) {
                        e.printStackTrace();
                }
                new TelemetryClient();
        }
}
