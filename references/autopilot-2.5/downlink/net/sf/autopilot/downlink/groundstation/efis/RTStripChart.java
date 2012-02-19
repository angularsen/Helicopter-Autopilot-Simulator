/* -*- indent-tabs-mode:T; c-basic-offset:8; tab-width:8; -*- vi: set ts=8:
	* $Id: RTStripChart.java,v 1.1 2002/07/14 04:31:52 dennisda Exp $
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

package net.sf.autopilot.downlink.groundstation.efis;

import javax.swing.JPanel;
import java.awt.Graphics;
import java.util.LinkedList;
import java.awt.Dimension;
import java.awt.Color;
import java.awt.Component;
import javax.swing.JComponent;
import net.sf.autopilot.downlink.groundstation.GPADCLine;
import net.sf.autopilot.downlink.groundstation.IMULine;

/**
 * Real time strip chart that reads a datasource every X milliseconds and displays a line from the last point to the new
 * point.  The new data is always at the far right, with the data scrolling to the left.  A different component is used
 * for viewing archived data.
 */
public class RTStripChart extends JComponent {
        /** Creates new form RTStripChart */
        GPADCLine currentLine;
        LinkedList imuLinesSync = new LinkedList();
        RefreshThread refreshThread;
        int baseline;
        int param;

        public void setValue(IMULine currentLine) {
                if (currentLine instanceof GPADCLine) {
                        this.currentLine = (GPADCLine)currentLine;
                }
        }

        public Dimension preferredSize() {
                return (new Dimension(500, 100));
        }

        public RTStripChart(int param) {
                //setDoubleBuffered(true);
                refreshThread = new RefreshThread(this);
                refreshThread.start();
                this.param = param;
                initGUI();
        }

        public void drawStrip(Graphics g) {
                double w = getSize().getWidth();
                double h = getSize().getHeight();
                double verticalUnit = h / 10000;
                double horizontalUnit = w / 500;
                Color temp = g.getColor();
                g.setColor(Color.black);
                for (int i = 0; i < imuLinesSync.size(); i++) {
                        GPADCLine gpadcLine = (GPADCLine)imuLinesSync.get(i);
                        int d = getNormalizedValue(gpadcLine.getParam(param));
                        int y = (int)((h / 2) + d * verticalUnit);
                        int x = (int)(w - (horizontalUnit * i));
                        int lastD;
                        int lastX;
                        int lastY;
                        if (i == 0) {
                                lastD = getNormalizedValue(baseline);
                                lastX = (int)w;
                                lastY = (int)(h / 2);
                        } else {
                                lastD = getNormalizedValue(((GPADCLine)(imuLinesSync.get(i - 1))).getParam(param));
                                lastX = (int)(w - (horizontalUnit * i) + (horizontalUnit));
                                lastY = (int)((h / 2) + lastD * verticalUnit);
                        }
                        g.drawLine(lastX, lastY, x, y);
                }
                g.setColor(temp);
        }

        public void setBaseline(int baseline) {
                this.baseline = baseline;
        }

        int getNormalizedValue(int value) {
                int temp = value - baseline;
                return temp;
        }

	public void paint(Graphics g) {
		super.paint(g);
	        drawStrip(g);
        }

        class RefreshThread extends Thread {
                long last;
                long current;
                boolean run = true;
                RTStripChart rtStripChart;

                public RefreshThread(RTStripChart rtStripChart) {
                        super();
                        this.rtStripChart = rtStripChart;
                }

                public void run() {
                        last = System.currentTimeMillis();
                        current = System.currentTimeMillis();
                        while (run) {
                                // 10hZ update rate
                                while ((System.currentTimeMillis() - last) < 100) {
                                        try {
                                                this.sleep(20);
                                        } catch (InterruptedException ie) {
                                                //oops
                                        }
                                }
                                last = System.currentTimeMillis();
                                if (currentLine != null) {
                                        imuLinesSync.addFirst(currentLine);
                                        if (imuLinesSync.size() > 500) {
                                                imuLinesSync.removeLast();
                                        }
                                        rtStripChart.repaint();
                                        rtStripChart.invalidate();
                                }
                        }
                }
        }


        /** This method is called from within the constructor to initialize the form. */
        private void initGUI() {
                //setLayout(new java.awt.BorderLayout());
        }
}
