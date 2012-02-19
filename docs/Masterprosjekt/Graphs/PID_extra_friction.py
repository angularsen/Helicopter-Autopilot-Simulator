import PID
import pylab
from pylab import *

s=0
v=0
Kp=-100.0/100
Kd=-20.0/5
Ki=-1.0/100000
Kf=2
target = 100
dt=0.1
samples=2000

#yLabel = "Position [m], Velocity [m/s]"
#xLabel = "Time [s]"

yLabel = "Throttle"
xLabel = "Time [s]"

#PID.solve(yLabel, xLabel, s, v, Kp, Kd, Ki, Kf, target, dt, samples)
PID.solveU(yLabel, xLabel, s, v, Kp, Kd, Ki, Kf, target, dt, samples)

#pylab.ylim(0, 150)
pylab.ylim(0, 1.1)

show()