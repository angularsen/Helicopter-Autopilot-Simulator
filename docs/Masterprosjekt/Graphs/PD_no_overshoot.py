import PID
import pylab
from pylab import *

s=0
v=0
Kp=-1.0/100
Kd=-20.0/100
Ki=0
Kf=0.1
target = 100
dt=0.1
samples=2000
yLabel = "Position [m], Velocity [m/s]"
xLabel = "Time [s]"

PID.solve(yLabel, xLabel, s, v, Kp, Kd, Ki, Kf, target, dt, samples)

pylab.ylim(0, 120)

show()