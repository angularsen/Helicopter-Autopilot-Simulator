function [X,Xdot] = propogate_state(xyz, uvw, a, pqr, Q, g, dt)
% This will compute the X dot equations for the model
%   Inputs:
%   xyz = [X; Y; Z]
%   uvw = [u; v; w]
%   a = [ax; ay; az]
%   pqr = [p; q; r]
%   Q = [q0; q1; q2; q3]
%   g = g
%   dt = dt
%
%	[X,Xdot] = propogate_state(xyz, uvw, a, pqr, Q, g, dt);
%			  .  .  .  .  .  .   .   .   .   .  . 
%	Xdot = [x; y; z; u; v; w; q0; q1; q2; q3; g]
%	X = [x; y; z; u; v; w; q0; q1; q2; q3; g]

Wxq = quatwx(pqr');
Wx = eulerwx(pqr');
dcm = quatdcm(Q');

NEDdot = dcm'*uvw;
uvwdot = a + dcm*[0;0;g] - Wx*uvw;
quatdot = Wxq*Q;

Xdot = [NEDdot; uvwdot; quatdot; 0];

NED = xyz + NEDdot*dt;
uvw = uvw + uvwdot*dt;
pqr = pqr;
quat = Q + quatdot*dt;
quat = (normq(quat'))';

X = [NED; uvw; quat; g];

