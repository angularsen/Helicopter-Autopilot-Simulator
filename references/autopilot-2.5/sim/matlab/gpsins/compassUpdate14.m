function [X,P] = compassUpdate14(measHeading, xyz, uvw, Q, delrate, g, P)
% This will update the KF gains with a compass heading update
%
% [X,P] = compassUpdate(measHeading, xyz, uvw, Q, g, P)
%
%   Inputs:
%   measHeading = measured heading
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

R = [CompSD^2];

tBL = quatdcm(Q');

% converting heading into quaterinon
eul = quat2euler(Q');

% make the c matrix
C = zeros(1,14);
c = dpsidq(Q');
C(1, [7:10]) = c;

% get the E matrix
E = C*P*C' + R;

% compute the kalman gain
K = P*C'*inv(E);

% update the state vector
x = x + K*(measHeading - eul(3));

% update the coverience matrix
P = P - K*C*P;

X = x;