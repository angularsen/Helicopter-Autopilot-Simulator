package net.sf.autopilot.downlink.uav;

import java.net.InetAddress;

class UDPClient {
        InetAddress iNetAddress;
        long lastPacketTime;

        public UDPClient(InetAddress iNetAddress) {
                this.iNetAddress = iNetAddress;
        }

        public InetAddress getINetAddress() { return iNetAddress; }

        public void touch() {
                lastPacketTime = System.currentTimeMillis();
        }

        public boolean isAlive(long timeout) {
                if (System.currentTimeMillis() - lastPacketTime > timeout)
                        return true;
                else
                        return false;
        }
}
