%	This is a test program to try out an 11-state
% GPS aided INS Kalman filter.  The estimated states
% are the XYZ (NED) positions in local nav frame.  The 
% body velocites u, v, w.  The body attitude in quaternion
% representation.  G is also estimated to account for any
% bias in the vertical accelerometer channel.
%
%	Author: Aaron Kahn
%  copyright 2002
clc;
clear;


global CompSD PosSD VelSD;

% some coversions
deg2rad = pi/180;
rad2deg = 180/pi;


% setup the standard deviations for the R matricies later
CompSD = 0.2;
PosSD = 0.8;
VelSD = 0.2;

% setup the process noise matrix (Q)
Q = diag([0 0 0 0.1 0.1 0.1 0.00001 0.00001 0.00001 0.00001 0.001]);

% setup the state estimate vector [xyz; uvw; Q; g]
Xest = [0; 0; 0; 0; 0; 0; 1; 0; 0; 0; 32.2];
XYZest = Xest([1:3], 1);
UVWest = Xest([4:6], 1);
Qest = Xest([7:10], 1);
Gest = Xest(11,1);

% setup the truth simulation vector and inertias
Xtrue = [0; 0; 0; 0; 0; 0; 0; 0; 0; 0; 0; 0];
Ix = 1;
Iy = 1;
Iz = 1;
Ixz = 0;
m = 1;
G = 32.2;

% setup the coverience matrix
P = eye(11);

dt = 0.01;
tf = 90;

% setup some arrays for saving data
REC = zeros(tf/dt, 16);
RECnoise = zeros(tf/dt, 16);
RECest = zeros(tf/dt, 10);


for n = 1:tf/dt
   t = (n-1)*dt;
   
   % simulate the truth
   F = eulerdcm(Xtrue(7:9))*[0;0;-G*m];
   Fx = F(1) + 0.0;
   Fy = F(2) + 0.01*cos(0.3*t);
   Fz = F(3) + 0.0*cos(0.3*t);
   L = 0.001*sin(0.3*t);
   M = 0.00*cos(0.3*t);
   N = 0.00;
   Xtruedot = sixdofe([Fx Fy Fz L M N Ix Iy Iz Ixz m G Xtrue']);
   Xtrue = Xtrue + Xtruedot'*dt;
   Xtrue(7,1) = tohrev(Xtrue(7,1));
   Xtrue(8,1) = tohrev(Xtrue(8,1));
   Xtrue(9,1) = tohrev(Xtrue(9,1));
   AccelTrue = [Xtruedot(1);Xtruedot(2);Xtruedot(3)] + F + eulerwx(Xtrue(4:6))*Xtrue(1:3);
   
   % Add noise to the true values to simulate the data
   XYZtrue = Xtrue([10:12], 1) + 0*randn(3,1);
   UVWtrue = Xtrue([1:3], 1) + 0.0*randn(3,1);
   PQRtrue = Xtrue([4:6], 1) + 0*deg2rad*randn(3,1) + 0.0*[0.1*deg2rad;0;0];
   THETAtrue = Xtrue([7:9], 1);
   Atrue = AccelTrue + 0*randn(3,1);
   
   %%%%%%%%% THE FILTER STARTS HERE %%%%%%%%%
   
   % Propogate the INS states
   [Xest, Xestdot] = propogate_state(XYZest, UVWest, Atrue, PQRtrue, Qest, Gest, dt);
   
   XYZest = Xest([1:3], 1);
   UVWest = Xest([4:6], 1);
   Qest = normq(Xest([7:10], 1)')';
   Gest = Xest(11,1);
   
   % Genarte the A matrix
   A = gena(UVWest, PQRtrue, Qest, Gest);
   
   % Propogate the converience matrix P
   Pdot = A*P + P*A' + Q;
   P = P + Pdot*dt;
   
   % GPS update once per second
   if( mod(t, 1) == 0 )
      [Xest, P] = gpsupdate(XYZtrue, UVWtrue, XYZest, UVWest, Qest, Gest, P);
      
      XYZest = Xest([1:3], 1);
	   UVWest = Xest([4:6], 1);
   	Qest = normq(Xest([7:10], 1)')';
      Gest = Xest(11,1);
      txt = sprintf('GPS Update at Time %f', t);
      disp(txt);
   end
   
   % Compass update 5 times per second
   if( mod(t, 0.2) == 0 )
      [Xest, P] = compassupdate(THETAtrue(3,1), XYZest, UVWest, Qest, Gest, P);
      
      XYZest = Xest([1:3], 1);
	   UVWest = Xest([4:6], 1);
   	Qest = normq(Xest([7:10], 1)')';
      Gest = Xest(11,1);
   end
   
   
   % Record measurement values
   RECnoise(n,1) = t;
   RECnoise(n, 2:4) = UVWtrue';						% body velocity
   RECnoise(n, 5:7) = PQRtrue'*rad2deg;			% pqr true
   RECnoise(n, 8:10) = THETAtrue'*rad2deg;		% true attitude
   RECnoise(n, 11:13) = XYZtrue';					% NED true position
   RECnoise(n, 14:16) = Atrue';				% true body acceleration
   
   
   % Record true values
   XYZtrue = Xtrue([10:12], 1);
   UVWtrue = Xtrue([1:3], 1);
   PQRtrue = Xtrue([4:6], 1);
   THETAtrue = Xtrue([7:9], 1);
   REC(n, 1) = t;
   REC(n, 2:4) = UVWtrue';						% body velocity
   REC(n, 5:7) = PQRtrue'*rad2deg;			% pqr true
   REC(n, 8:10) = THETAtrue'*rad2deg;		% true attitude
   REC(n, 11:13) = XYZtrue';					% NED true position
   REC(n, 14:16) = AccelTrue';				% true body acceleration
   
   % Record estimated values
   RECest(n,1) = t;
   RECest(n, 2:4) = Xest([4:6],1)';			% estimated body velocity
   RECest(n, 5:7) = (quat2euler(Xest([7:10],1)))*rad2deg; %estimated attitude
   RECest(n, 8:10) = Xest([1:3],1)';		% estimated NED position
end

%%%%%% PLOT THE DATA %%%%%%

figure(1);
subplot(3,1,1);
plot(RECnoise(:,1), RECnoise(:,2), 'c:', REC(:,1), REC(:,2), 'b', RECest(:,1), RECest(:,2), 'r');
title('UVW vs. Time');
ylabel('U ft/s');
subplot(3,1,2);
plot(RECnoise(:,1), RECnoise(:,3), 'c:', REC(:,1), REC(:,3), 'b', RECest(:,1), RECest(:,3), 'r');
ylabel('V ft/s');
subplot(3,1,3);
plot(RECnoise(:,1), RECnoise(:,4), 'c:', REC(:,1), REC(:,4), 'b', RECest(:,1), RECest(:,4), 'r');
ylabel('W ft/s');
xlabel('Time sec');

figure(2);
subplot(3,1,1);
plot(RECnoise(:,1), RECnoise(:,5), 'c:', REC(:,1), REC(:,5), 'b');
title('PQR vs. Time');
ylabel('p deg/s');
subplot(3,1,2);
plot(RECnoise(:,1), RECnoise(:,6), 'c:', REC(:,1), REC(:,6), 'b');
ylabel('q deg/s');
subplot(3,1,3);
plot(RECnoise(:,1), RECnoise(:,7), 'c:', REC(:,1), REC(:,7), 'b');
ylabel('r deg/s');
xlabel('Time sec');

figure(3);
subplot(3,1,1);
plot(REC(:,1), REC(:,8), 'b', RECest(:,1), RECest(:,5), 'r');
title('Attitude vs. Time');
ylabel('Roll deg');
subplot(3,1,2);
plot(REC(:,1), REC(:,9), 'b', RECest(:,1), RECest(:,6), 'r');
ylabel('Pitch deg');
subplot(3,1,3);
plot(REC(:,1), REC(:,10), 'b', RECest(:,1), RECest(:,7), 'r');
ylabel('Yaw deg');
xlabel('Time sec');

figure(4);
subplot(3,1,1);
plot(RECnoise(:,1), RECnoise(:,11), 'c:', REC(:,1), REC(:,11), 'b', RECest(:,1), RECest(:,8), 'r');
title('NED vs. Time');
ylabel('North ft');
subplot(3,1,2);
plot(RECnoise(:,1), RECnoise(:,12), 'c:', REC(:,1), REC(:,12), 'b', RECest(:,1), RECest(:,9), 'r');
ylabel('East ft');
subplot(3,1,3);
plot(RECnoise(:,1), RECnoise(:,13), 'c:', REC(:,1), REC(:,13), 'b', RECest(:,1), RECest(:,10), 'r');
ylabel('Down ft');
xlabel('Time sec');

figure(5);
subplot(3,1,1);
plot(RECnoise(:,1), RECnoise(:,14), 'c:', REC(:,1), REC(:,14), 'b');
title('Body Accel vs. Time');
ylabel('X accel ft/s/s');
subplot(3,1,2);
plot(RECnoise(:,1), RECnoise(:,15), 'c:', REC(:,1), REC(:,15), 'b');
ylabel('Y accel ft/s/s');
subplot(3,1,3);
plot(RECnoise(:,1), RECnoise(:,16), 'c:', REC(:,1), REC(:,16), 'b');
ylabel('Z accel ft/s/s');
xlabel('Time sec');


   
   