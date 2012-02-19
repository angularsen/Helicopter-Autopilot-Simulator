/* -*- indent-tabs-mode:T; c-basic-offset:8; tab-width:8; -*- vi: set ts=8:
	* $Id: AiPanel.java,v 1.1 2002/07/14 04:31:52 dennisda Exp $
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

import java.awt.BasicStroke;
import java.awt.Color;
import java.awt.Font;
import java.awt.Graphics;
import java.awt.Graphics2D;
import java.awt.RenderingHints;
import java.awt.Shape;
import java.awt.Stroke;
import java.awt.font.FontRenderContext;
import java.awt.font.TextLayout;
import java.awt.geom.AffineTransform;
import java.awt.geom.Arc2D;
import java.awt.geom.Ellipse2D;
import java.awt.geom.Line2D;
import java.awt.geom.Rectangle2D;
import javax.swing.JPanel;
import java.awt.Dimension;

public class AiPanel extends JPanel {
        double pitch = 0.0;
        double roll = 0.0;
        double heading = 0.0;
        double xScale = 1.0;
	double yScale = 1.0;
	RefreshThread refreshThread;

	public AiPanel() {
		refreshThread = new RefreshThread(this);
		refreshThread.start();
	}

	public void paintComponent(Graphics graphics) {
		super.paintComponent(graphics);
                transformPaintMethod(graphics);
        }

        public Dimension preferredSize() {
                int x = (int)(400.0 * xScale);
                int y = (int)(400.0 * yScale);
                return (new Dimension(x, y));
        }

        public void thPaintMethod(Graphics graphics) {
                Graphics2D graphics2d = (Graphics2D)graphics;
                graphics2d.setRenderingHint(RenderingHints.KEY_ANTIALIASING, RenderingHints.VALUE_ANTIALIAS_ON);
                graphics2d.setPaint(Color.black);
                Ellipse2D.Double var_double = new Ellipse2D.Double(0.0, 0.0, 300.0, 300.0);
                graphics2d.draw(var_double);
                graphics2d.setPaint(Color.blue);
                Arc2D.Double var_double_0_ = new Arc2D.Double(0.0, 0.0, 300.0, 300.0, 0.0 + (((AiPanel)this).roll - ((AiPanel)this).pitch),
                    180.0 + 2.0 * ((AiPanel)this).pitch, 1);
                graphics2d.draw(var_double_0_);
                graphics2d.fill(var_double_0_);
                graphics2d.setPaint(Color.green);
                Arc2D.Double var_double_1_ = new Arc2D.Double(0.0, 0.0, 300.0, 300.0, 0.0 + (((AiPanel)this).roll - ((AiPanel)this).pitch),
                    -180.0 - 2.0 * ((AiPanel)this).pitch, 1);
                graphics2d.draw(var_double_1_);
                graphics2d.fill(var_double_1_);
        }

        public void transformPaintMethod(Graphics graphics) {
                Graphics2D graphics2d = (Graphics2D)graphics;
                AffineTransform affinetransform = graphics2d.getTransform();
                Shape shape = graphics2d.getClip();
                graphics2d.setRenderingHint(RenderingHints.KEY_ANTIALIASING, RenderingHints.VALUE_ANTIALIAS_ON);
                graphics2d.scale(((AiPanel)this).xScale, ((AiPanel)this).yScale);
	     //   graphics2d.setPaint(Color.black);
	     //   Rectangle2D.Double var_double = new Rectangle2D.Double(0.0, 0.0, 470.0, 470.0);
             //   graphics2d.draw(var_double);
                Stroke stroke = graphics2d.getStroke();
                graphics2d.setPaint(Color.darkGray);
                graphics2d.setStroke(new BasicStroke(21.0F, 1, 1));
                Ellipse2D.Double var_double_2_ = new Ellipse2D.Double(12.0, 12.0, 337.0, 337.0);
                graphics2d.draw(var_double_2_);
                graphics2d.setStroke(stroke);
                Ellipse2D.Double var_double_3_ = new Ellipse2D.Double(21.0, 21.0, 318.0, 318.0);
                graphics2d.clip(var_double_3_);
                graphics2d.setPaint(Color.blue);
                Rectangle2D.Double var_double_4_ = new Rectangle2D.Double(20.0, 20.0, 470.0, 470.0);
                graphics2d.draw(var_double_4_);
		graphics2d.fill(var_double_4_);

		graphics2d.rotate(Math.toRadians(((AiPanel)this).roll), 170.0, 170.0);
		graphics2d.translate(0.0, ((AiPanel)this).pitch * 2.6667);

                graphics2d.setPaint(Color.green);
                Rectangle2D.Double var_double_5_ = new Rectangle2D.Double(0.0, 170.0, 570.0, 570.0);
                graphics2d.draw(var_double_5_);
                graphics2d.fill(var_double_5_);
                graphics2d.setPaint(Color.white);
                graphics2d.setStroke(new BasicStroke(2.0F, 1, 1));
                graphics2d.draw(new Line2D.Double(140.0, 170.0, 200.0, 170.0));
                graphics2d.draw(new Line2D.Double(120.0, 200.0, 220.0, 200.0));
                graphics2d.draw(new Line2D.Double(120.0, 140.0, 220.0, 140.0));
                graphics2d.draw(new Line2D.Double(120.0, 110.0, 220.0, 110.0));
                graphics2d.draw(new Line2D.Double(120.0, 230.0, 220.0, 230.0));
                graphics2d.draw(new Line2D.Double(120.0, 80.0, 220.0, 80.0));
                graphics2d.draw(new Line2D.Double(120.0, 260.0, 220.0, 260.0));
                FontRenderContext fontrendercontext = graphics2d.getFontRenderContext();
//                Font font = new Font("Serif", 0, 13);
//                TextLayout textlayout = new TextLayout("15", font, fontrendercontext);
//                textlayout.draw(graphics2d, 98.0F, 114.0F);
//                textlayout.draw(graphics2d, 229.0F, 114.0F);
                graphics2d.setClip(shape);
                graphics2d.setTransform(affinetransform);
//                graphics2d.scale(((AiPanel)this).xScale, ((AiPanel)this).yScale);
//                graphics2d.setPaint(Color.white);
//                graphics2d.setStroke(new BasicStroke(2.2F, 1, 1));
//                graphics2d.setColor(Color.black);
//                graphics2d.draw(new Line2D.Double(60.0, 180.0, 300.0, 180.0));
//                graphics2d.setTransform(affinetransform);
        }

        public void setPitch(double d) {
                ((AiPanel)this).pitch = d;
        }

        public void setRoll(double d) {
                ((AiPanel)this).roll = d;
        }

        public void setHeading(double d) {
                ((AiPanel)this).heading = d;
        }

        public void setXScale(double d) {
                ((AiPanel)this).xScale = d;
        }

        public void setYScale(double d) {
                ((AiPanel)this).yScale = d;
	}

	class RefreshThread extends Thread {
                long last;
                long current;
                boolean run = true;
		AiPanel aiPanel;

		public RefreshThread(AiPanel aiPanel) {
                        super();
			this.aiPanel = aiPanel;
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

				aiPanel.repaint();
				aiPanel.invalidate();
			}
                }
	} // end refershthread

}
