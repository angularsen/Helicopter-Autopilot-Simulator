function [X,P] = ahrsupdate14(measAttitude, xyz, uvw, Q, delrate, g, P)
% This will update the KF gains with an AHRS-like measurement
%
% [X,P] = ahrsupdate(measAttitude, xyz, uvw, Q, g, P)
%
%   Inputs:
%   measAttitude = [phi; theta; psi]
%   xyz = [X; Y; Z] (est)
%   uvw = [u; v; w] (est)
%   Q = [q0; q1; q2; q3] (est)
%	 delrate = [delp; delq; delr] (est)
%   g = g (est)
%   P = coverience matrix
%
% X = [x; y; z; u; v; w; q0; q1; q2; q3; delp; delq; delr; g];
% P = updated coverience matrix

global CompSD;

x = [xyz; uvw; Q; delrate; g];

R = diag([CompSD^2 CompSD^2 CompSD^2]);

% converting heading into quaterinon
eul = quat2euler(Q');


C = zeros(3,14);
C(1, [7:10]) = dphidq(Q');
C(2, [7:10]) = dthetadq(Q');
C(3, [7:10]) = dpsidq(Q');

% get the E matrix
E = C*P*C' + R;

% compute the kalman gain
K = P*C'*inv(E);

err(1,1) = turndirection(measAttitude(1,1), eul(1,1));
err(2,1) = turndirection(measAttitude(2,1), eul(1,2));
err(3,1) = turndirection(measAttitude(3,1), eul(1,3));
%disp([measAttitude(3,1) eul(1,3) err(3,1)]*180/pi);
% update the state vector
x = x + K*err;

% update the coverience matrix
P = P - K*C*P;

X = x;

function error = turndirection(command, current)
if( current > pi/2 & command < -pi/2 )
   error = 2*pi + command - current;
elseif( current < -pi/2 & command > pi/2 )
   error = -2*pi + command - current;
else
   error = command - current;
end
