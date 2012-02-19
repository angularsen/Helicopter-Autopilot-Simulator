/* -*- indent-tabs-mode:T; c-basic-offset:8; tab-width:8; -*- vi: set ts=8:
	* $Id: TCClient.java,v 1.1 2002/07/14 04:31:52 dennisda Exp $
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

import javax.swing.JFrame;
import java.awt.event.WindowEvent;
import java.awt.Dimension;
import java.awt.BorderLayout;
import javax.swing.JPanel;
import java.awt.Rectangle;
import javax.swing.JLabel;
import javax.swing.JMenuBar;
import javax.swing.JMenu;
import javax.swing.JMenuItem;
import java.awt.event.ActionListener;
import java.awt.event.ActionEvent;
import javax.swing.JFileChooser;
import javax.swing.JList;
import javax.swing.JTextField;
import net.sf.autopilot.downlink.groundstation.efis.AiPanel;
import net.sf.autopilot.downlink.groundstation.efis.RTStripChart;

public class TCClient extends JFrame {
        /** Creates new form JFrame */
        public TCClient() {
                initGUI();
                pack();
        }

        /** This method is called from within the constructor to initialize the form. */
        private void initGUI() {
                jLabel1.setText("Refresh Rate: ");
                getContentPane().setLayout(new java.awt.BorderLayout());
                getContentPane().add(jPanel1, java.awt.BorderLayout.NORTH);
                getContentPane().add(jPanel2, java.awt.BorderLayout.CENTER);
                getContentPane().add(jPanel3, java.awt.BorderLayout.EAST);
                // set title
                setTitle("autopilot.sf.net, telemetry client (tc)");
                // add status bar
                getContentPane().add(statusBar, BorderLayout.SOUTH);
                // add menu bar
                JMenuBar menuBar = new JMenuBar();
                JMenu menuFile = new JMenu("File");
                menuFile.setMnemonic('F');
                // create Exit menu item
                JMenuItem fileExit = new JMenuItem("Exit");
                fileExit.setMnemonic('E');
                fileExit.addActionListener(
                    new ActionListener() {
                            public void actionPerformed(ActionEvent e) {
                                    System.exit(0);
                            }
                    });
                // create About menu item
                JMenu menuHelp = new JMenu("Help");
                menuHelp.setMnemonic('H');
                JMenuItem helpAbout = new JMenuItem("About");
                helpAbout.setMnemonic('A');
                helpAbout.addActionListener(
                    new ActionListener() {
                            public void actionPerformed(ActionEvent e) {
                                    AboutDialog1 aboutDialog = new AboutDialog1(TCClient.this, true);
                                    Dimension frameSize = getSize();
                                    Dimension aboutSize = aboutDialog.getPreferredSize();
                                    int x = getLocation().x + (frameSize.width - aboutSize.width) / 2;
                                    int y = getLocation().y + (frameSize.height - aboutSize.height) / 2;
                                    if (x < 0) x = 0;
                                    if (y < 0) y = 0;
                                    aboutDialog.setLocation(x, y);
                                    aboutDialog.setVisible(true);
                            }
                    });
                menuHelp.add(helpAbout);
                // create Open menu item
                final JFileChooser fc = new JFileChooser();
                JMenuItem openFile = new JMenuItem("Open");
                openFile.setMnemonic('O');
                openFile.addActionListener(
                    new java.awt.event.ActionListener() {
                            public void actionPerformed(java.awt.event.ActionEvent e) {
                                    int returnVal = fc.showOpenDialog(TCClient.this);
                                    if (returnVal == javax.swing.JFileChooser.APPROVE_OPTION) {
                                            java.io.File file = fc.getSelectedFile();
                                            // Write your code here what to do with selected file
                                    } else {
                                            // Write your code here what to do if user has canceled Open dialog
                                    }
                            }
                    });
                menuFile.add(openFile);
                menuFile.add(fileExit);
                menuBar.add(menuFile);
                menuBar.add(menuHelp);
                // sets menu bar
                setJMenuBar(menuBar);
                addWindowListener(
                    new java.awt.event.WindowAdapter() {
                            public void windowClosing(java.awt.event.WindowEvent evt) {
                                    exitForm(evt);
                            }
                    });
                jPanel1.add(jLabel1);
                jPanel1.add(jTextField1);
                jPanel2.setLayout(new javax.swing.BoxLayout(jPanel2, javax.swing.BoxLayout.Y_AXIS));
                jPanel2.add(param4StripChart);
                jPanel2.add(param5StripChart);
                jPanel2.add(param6StripChart);
                jPanel2.add(param7StripChart);
                jPanel2.add(param8StripChart);
		jPanel3.setLayout(new javax.swing.BoxLayout(jPanel3, javax.swing.BoxLayout.X_AXIS));
		jPanel3.add(jPanel5);
		jPanel4.setLayout(new javax.swing.BoxLayout(jPanel4, javax.swing.BoxLayout.Y_AXIS));
		jPanel3.add(jPanel4);
		jPanel4.add(aiPanel);
                jTextField1.setText("?");
                jTextField1.setEditable(false);
                jTextField1.setToolTipText("");
                jTextField1.setColumns(5);
                jTextField1.setBorder(javax.swing.BorderFactory.createCompoundBorder(null, null));
                jTextField1.setBackground(new java.awt.Color(255, 0, 0));
                jLabel2.setText("jLabel2");
                param4StripChart.setSize(400, 200);
                param4StripChart.setBaseline(16800);
                param4StripChart.setBorder(null);
                param5StripChart.setSize(400, 200);
                param5StripChart.setBaseline(18600);
                param5StripChart.setBorder(null);
                param6StripChart.setSize(400, 200);
                param6StripChart.setBaseline(16000);
                param6StripChart.setBorder(null);
                param7StripChart.setSize(400, 200);
                param7StripChart.setBaseline(14100);
                param7StripChart.setBorder(null);
                param8StripChart.setSize(400, 200);
                param8StripChart.setBaseline(13800);
                param8StripChart.setBorder(null);
                aiPanel.setPitch(0.0);
                aiPanel.setRoll(0.0);
		aiPanel.setXScale(.4);
                aiPanel.setYScale(.4);
                aiPanel.setSize(new java.awt.Dimension(160, 150));
                aiPanel.setBorder(null);
                aiPanel.setLayout(new java.awt.FlowLayout());
                jPanel5.setPreferredSize(new java.awt.Dimension(20,10));
                jPanel5.setBorder(null);
        }

        public void setRefreshRate(String text) {
                if (Integer.parseInt(text) > 30) {
                        jTextField1.setBackground(new java.awt.Color(0, 255, 0));
                } else if (Integer.parseInt(text) > 25) {
                        jTextField1.setBackground(new java.awt.Color(255, 255, 0));
                } else {
                        jTextField1.setBackground(new java.awt.Color(255, 0, 0));
                }
                jTextField1.setText(text);
                jTextField1.invalidate();
        }

        /** Exit the Application */
        private void exitForm(WindowEvent evt) {
                System.exit(0);
        }

        private BorderLayout layout = new BorderLayout();
        private JLabel statusBar = new JLabel("Ready");
        private JPanel jPanel1 = new JPanel();
        private JPanel jPanel2 = new JPanel();
	private JPanel jPanel3 = new JPanel();
	private JPanel jPanel4 = new JPanel();
	private JPanel jPanel5 = new JPanel();
        private JLabel jLabel1 = new JLabel();
        private JTextField jTextField1 = new JTextField();
        private JLabel jLabel2 = new JLabel();
        private RTStripChart param4StripChart = new RTStripChart(3);
        private RTStripChart param5StripChart = new RTStripChart(4);
        private RTStripChart param6StripChart = new RTStripChart(5);
        private RTStripChart param7StripChart = new RTStripChart(6);
        private RTStripChart param8StripChart = new RTStripChart(7);
	private AiPanel aiPanel = new AiPanel();

        public void setParam4(IMULine imuLine) {
                param4StripChart.setValue(imuLine);
        }

        public void setParam5(IMULine imuLine) {
                param5StripChart.setValue(imuLine);
        }

        public void setParam6(IMULine imuLine) {
                param6StripChart.setValue(imuLine);
        }

        public void setParam7(IMULine imuLine) {
                param7StripChart.setValue(imuLine);
        }

        public void setParam8(IMULine imuLine) {
                param8StripChart.setValue(imuLine);
        }

        public void setAIPitch(double pitch) {
                aiPanel.setPitch(pitch);
        }

        public void setAIRoll(double roll) {
                aiPanel.setRoll(roll);
        }
}
