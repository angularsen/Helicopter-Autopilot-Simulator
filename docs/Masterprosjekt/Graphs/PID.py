import pylab
from pylab import *

def clamp (a, min, max ) :

    if a < min: 
       return min
    if a > max:
        return max
    return a    
    
    
def solve (yLabel, xLabel, s, v, Kp, Kd, Ki, Kf, target, dt, samples) :
    
    tRes = [t * dt for t in range(samples)]
    sRes = [0 * dt for t in range(samples)]
    vRes = [0 for i in range(samples)]
    aRes = [0 for i in range(samples)]
    uRes = [0 for i in range(samples)]
    targetRes = [target for i in range(samples)]
        
    e = None
    ie = 0
    for i in range(samples):
        
        if e is None:
            ePrev = 0
            e = 0
        else:
            ePrev = e
            e = s - target
            
        de = (e-ePrev)/dt   # derivative error
        ie += e             # integral error

        u = Kp*e + Kd*de + Ki*ie
        u = clamp(u, -1, 1)
        friction = Kf*v
        a = 4.0*u - friction
        
        aRes[i] = a
        vRes[i] = v
        sRes[i] = s
        uRes[i] = u * 100
        t = tRes[i]
        
        # Calculate next state
        ds = v*dt + 0.5*a*dt**2  # Linear acceleration
        v = ds/dt
        s += ds
        

    pylab.xlabel(xLabel)
    pylab.ylabel(yLabel)



    
    plot(tRes, targetRes, color="black", label="B")
    plot(tRes, sRes, color="blue", label="position", label="position")
    plot(tRes, vRes, color="yellow", label="velocity")
    #plot(tRes, uRes, label="throttle")
    legend(loc=(0.75,0))

    #pylab.ylim(0, 150)

    
def solveU (yLabel, xLabel, s, v, Kp, Kd, Ki, Kf, target, dt, samples) :
    
    tRes = [t * dt for t in range(samples)]
    sRes = [0 * dt for t in range(samples)]
    vRes = [0 for i in range(samples)]
    aRes = [0 for i in range(samples)]
    uRes = [0 for i in range(samples)]
    targetRes = [target for i in range(samples)]
        
    e = None
    ie = 0
    for i in range(samples):
        
        if e is None:
            ePrev = 0
            e = 0
        else:
            ePrev = e
            e = s - target
            
        de = (e-ePrev)/dt   # derivative error
        ie += e             # integral error

        u = Kp*e + Kd*de + Ki*ie
        u = clamp(u, -1, 1)
        friction = Kf*v
        a = 4.0*u - friction

        aRes[i] = a
        vRes[i] = v
        sRes[i] = s
        uRes[i] = u
        t = tRes[i]
        
        # Calculate next state
        ds = v*dt + 0.5*a*dt**2  # Linear acceleration
        v = ds/dt
        s += ds
        

    pylab.xlabel(xLabel)
    pylab.ylabel(yLabel)



    plot(tRes, uRes, color="red", label="throttle")
    legend(loc=(0.75,0))

    #pylab.ylim(0, 150)