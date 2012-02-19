function [X,P] = gpsUpdate(measNED, measVel, xyz, uvw, Q, g, P)
% This will update the KF gains with a compass heading update
%
% [X,P] = gpsUpdate(measNED, measVel, xyz, uvw, Q, P)
% It is assumed the the measured position is in local XYZ, NED-axis system
% and measVel is in UVW-body-axis system.
%
% Inputs:
%   measNED = [X; Y; Z]
%   measVel = [Xdot; Ydot; Zdot]
%   xyz = [X; Y; Z] (est)
%   uvw = [u; v; w] (est)
%   Q = [q0; q1; q2; q3] (est)
%   g = g (est)
%   P = coverience matrix
%
% X = [x; y; z; u; v; w; q0; q1; q2; q3; g];
% P = updated coverience matrix

global PosSD VelSD;

x = [xyz; uvw; Q; g];

dcm = quatdcm(Q');

R = [PosSD^2 0 0 0 0 0;...
      0 PosSD^2 0 0 0 0;...
      0 0 PosSD^2 0 0 0;...
      0 0 0 VelSD^2 0 0;...
      0 0 0 0 VelSD^2 0;...
      0 0 0 0 0 VelSD^2];

% make the c matrix
C = [eye(6,6) zeros(6,5)];

% get the E matrix
E = C*P*C' + R;

% compute the kalman gain
K = P*C'*inv(E);

% update the state vector
x = x + K*([measNED; measVel] - C*x);

% update the coverience matrix
P = P - K*C*P;

X = x;