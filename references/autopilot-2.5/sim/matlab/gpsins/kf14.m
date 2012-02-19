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
CompSD = 0.3;
PosSD = 0.5;
VelSD = 0.1;

% setup the process noise matrix (Q)
Q = diag([0 0 0 0.1 0.1 0.1 0.00001 0.00001 0.00001 0.00001 0.010 0.010 0.010 0.001]);

% setup the state estimate vector [xyz; uvw; Q; delrate; g]
Xest = [0; 0; 0; 0; 0; 0; 1; 0; 0; 0; 1*deg2rad; 0; 0; 32.2];
XYZest = Xest([1:3], 1);
UVWest = Xest([4:6], 1);
Qest = Xest([7:10], 1);
delRateest = Xest([11:13], 1);
Gest = Xest(14,1);

% setup the truth simulation vector and inertias
% u v w p q r phi theta psi n e d
Xtrue = [0; 0; 0; 0; 0; 0; 10*pi/180; 0; 0; 0; 0; 0];
Ix = 1;
Iy = 1;
Iz = 1;
Ixz = 0;
m = 1;
G = 32.2;

% setup the coverience matrix
P = eye(14);
%P(11,11) = 0;
%P(12,12) = 0;
%P(13,13) = 0;
%P(14,14) = 0;

dt = 0.01;
tf = 90;

% setup some arrays for saving data
REC = zeros(tf/dt, 16);
RECnoise = zeros(tf/dt, 16);
RECest = zeros(tf/dt, 14);


for n = 1:tf/dt
   t = (n-1)*dt;
   
   % simulate the truth
   F = eulerdcm(Xtrue(7:9))*[0;0;-G*m];
   Fx = F(1) + 0.0;
   Fy = F(2) + 0.0*cos(0.3*t);
   Fz = F(3) + 0.0*cos(0.3*t);
   L = 0.00*sin(0.3*t);
   M = 0.01*cos(0.3*t);
   N = 0.01;
   Xtruedot = sixdofe([Fx Fy Fz L M N Ix Iy Iz Ixz m G Xtrue']);
   Xtrue = Xtrue + Xtruedot'*dt;
   Xtrue(7,1) = tohrev(Xtrue(7,1));
   Xtrue(8,1) = tohrev(Xtrue(8,1));
   Xtrue(9,1) = tohrev(Xtrue(9,1));
   AccelTrue = [Xtruedot(1);Xtruedot(2);Xtruedot(3)] + F + eulerwx(Xtrue(4:6))*Xtrue(1:3);
   
   % Add noise to the true values to simulate the data
   XYZtrue = Xtrue([10:12], 1) + 2*randn(3,1)*sin(0.2*t);
   UVWtrue = Xtrue([1:3], 1) + 0.2*randn(3,1)*cos(0.2*t);
   PQRtrue = Xtrue([4:6], 1) + 2.3*deg2rad*randn(3,1) + [3;2;8]*deg2rad;
   THETAtrue = Xtrue([7:9], 1);
   Atrue = AccelTrue + 1.9*randn(3,1);
   
   %%%%%%%%% THE FILTER STARTS HERE %%%%%%%%%
   
   % Propogate the INS states
   [Xest, Xestdot] = propogate_state14(XYZest, UVWest, Atrue, PQRtrue, Qest, delRateest, Gest, dt);
   
   XYZest = Xest([1:3], 1);
   UVWest = Xest([4:6], 1);
   Qest = normq(Xest([7:10], 1)')';
   delRateest = Xest([11:13], 1);
   Gest = Xest(14,1);
   
   % Genarte the A matrix
   A = gen14a(UVWest, PQRtrue, Qest, delRateest, Gest);
   
   % Propogate the converience matrix P
   Pdot = A*P + P*A' + Q;
   P = P + Pdot*dt;
   
   % GPS update once per second
   if( mod(t, 1) == 0 )
      [Xest, P] = gpsupdate14(XYZtrue, UVWtrue, XYZest, UVWest, Qest, delRateest, Gest, P);
      
      XYZest = Xest([1:3], 1);
	   UVWest = Xest([4:6], 1);
      Qest = normq(Xest([7:10], 1)')';
	   delRateest = Xest([11:13], 1);
   	Gest = Xest(14,1);
      txt = sprintf('GPS Update at Time %f', t);
      disp(txt);
   end
   
   % AHRS-like attitude update at 5 Hz
   if( mod(t, 0.2) == 0 )
      angles(1,1) = -atan2(Atrue(2,1), -Atrue(3,1));
      angles(2,1) = asin(sat(Atrue(1,1)/Gest, -1, 1));
      angles(3,1) = THETAtrue(3,1);
      [Xest, P] = ahrsupdate14(angles, XYZest, UVWest, Qest, delRateest, Gest, P);
      
      XYZest = Xest([1:3], 1);
	   UVWest = Xest([4:6], 1);
      Qest = normq(Xest([7:10], 1)')';
      delRateest = Xest([11:13], 1);
      Gest = Xest(14,1);
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
   RECest(n, 11:13) = Xest([11:13], 1)'*rad2deg; %est gyro bias
   RECest(n, 14) = Xest(14,1); %est gravity
end

%%%%%% PLOT THE DATA %%%%%%

figure(1);
subplot(3,1,1);
plot(RECnoise(:,1), RECnoise(:,2), 'c:', REC(:,1), REC(:,2), 'b', RECest(:,1), RECest(:,2), 'r'); grid on;
title('UVW vs. Time');
ylabel('U ft/s');
subplot(3,1,2);
plot(RECnoise(:,1), RECnoise(:,3), 'c:', REC(:,1), REC(:,3), 'b', RECest(:,1), RECest(:,3), 'r'); grid on;
ylabel('V ft/s');
subplot(3,1,3);
plot(RECnoise(:,1), RECnoise(:,4), 'c:', REC(:,1), REC(:,4), 'b', RECest(:,1), RECest(:,4), 'r'); grid on;
ylabel('W ft/s');
xlabel('Time sec');
legend('Corrupt', 'Truth', 'Est');

figure(2);
subplot(3,1,1);
plot(RECnoise(:,1), RECnoise(:,5), 'c:', REC(:,1), REC(:,5), 'b', RECnoise(:,1),RECnoise(:,5)-RECest(:,11),'r'); grid on;
title('PQR vs. Time');
ylabel('p deg/s');
subplot(3,1,2);
plot(RECnoise(:,1), RECnoise(:,6), 'c:', REC(:,1), REC(:,6), 'b', RECnoise(:,1),RECnoise(:,6)-RECest(:,12),'r'); grid on;
ylabel('q deg/s');
subplot(3,1,3);
plot(RECnoise(:,1), RECnoise(:,7), 'c:', REC(:,1), REC(:,7), 'b', RECnoise(:,1),RECnoise(:,7)-RECest(:,13),'r'); grid on;
ylabel('r deg/s');
xlabel('Time sec');
legend('Corrupt', 'Truth', 'Corrected');

figure(3);
subplot(3,1,1);
plot(REC(:,1), REC(:,8), 'b', RECest(:,1), RECest(:,5), 'r'); grid on;
title('Attitude vs. Time');
ylabel('Roll deg');
subplot(3,1,2);
plot(REC(:,1), REC(:,9), 'b', RECest(:,1), RECest(:,6), 'r'); grid on;
ylabel('Pitch deg');
subplot(3,1,3);
plot(REC(:,1), REC(:,10), 'b', RECest(:,1), RECest(:,7), 'r'); grid on;
ylabel('Yaw deg');
xlabel('Time sec');
legend('Truth', 'Est');

figure(4);
subplot(3,1,1);
plot(RECnoise(:,1), RECnoise(:,11), 'c:', REC(:,1), REC(:,11), 'b', RECest(:,1), RECest(:,8), 'r'); grid on;
title('NED vs. Time');
ylabel('North ft');
subplot(3,1,2);
plot(RECnoise(:,1), RECnoise(:,12), 'c:', REC(:,1), REC(:,12), 'b', RECest(:,1), RECest(:,9), 'r'); grid on;
ylabel('East ft');
subplot(3,1,3);
plot(RECnoise(:,1), RECnoise(:,13), 'c:', REC(:,1), REC(:,13), 'b', RECest(:,1), RECest(:,10), 'r'); grid on;
ylabel('Down ft');
xlabel('Time sec');
legend('Corrupt', 'Truth', 'Est');

figure(5);
subplot(3,1,1);
plot(RECnoise(:,1), RECnoise(:,14), 'c:', REC(:,1), REC(:,14), 'b'); grid on;
title('Body Accel vs. Time');
ylabel('X accel ft/s/s');
subplot(3,1,2);
plot(RECnoise(:,1), RECnoise(:,15), 'c:', REC(:,1), REC(:,15), 'b'); grid on;
ylabel('Y accel ft/s/s');
subplot(3,1,3);
plot(RECnoise(:,1), RECnoise(:,16), 'c:', REC(:,1), REC(:,16), 'b'); grid on;
ylabel('Z accel ft/s/s');
xlabel('Time sec');
legend('Corrupt', 'Truth');

figure(6);
subplot(3,1,1);
plot(RECest(:,1), RECest(:,11)); grid on;
title('Estimated Gyro Bias');
ylabel('\delta p deg/s');
subplot(3,1,2);
plot(RECest(:,1), RECest(:,12)); grid on;
ylabel('\delta q deg/s');
subplot(3,1,3);
plot(RECest(:,1), RECest(:,13)); grid on;
ylabel('\delta r deg/s');
xlabel('Time sec');
   