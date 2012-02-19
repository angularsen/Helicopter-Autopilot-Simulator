function [X,Xdot] = propogate_state14(xyz, uvw, a, pqr, Q, delrate, g, dt)
% This will compute the X dot equations for the model
%   Inputs:
%   xyz = [X; Y; Z]
%   uvw = [u; v; w]
%   a = [ax; ay; az]
%   pqr = [p; q; r]
%   Q = [q0; q1; q2; q3]
%   delrate = [delp; delq; delr]
%   g = g
%   dt = dt
%
%	[X,Xdot] = propogate_state14(xyz, uvw, a, pqr, Q, delrate, g, dt);
%			  .  .  .  .  .  .   .   .   .   .   .     .     .    .
%	Xdot = [x; y; z; u; v; w; q0; q1; q2; q3; delp, delq, delr, g]
%	X = [x; y; z; u; v; w; q0; q1; q2; q3; delp, delq, delr, g]

Wxq = quatwx((pqr - delrate)');
Wx = eulerwx((pqr - delrate)');
dcm = quatdcm(Q');

NEDdot = dcm'*uvw;
uvwdot = a + dcm*[0;0;g] - Wx*uvw;
quatdot = Wxq*Q;
delratedot = zeros(3,1);
delg = 0;


Xdot = [NEDdot; uvwdot; quatdot; delratedot; delg];

NED = xyz + NEDdot*dt;
uvw = uvw + uvwdot*dt;
pqr = pqr;
quat = Q + quatdot*dt;
quat = (normq(quat'))';

X = [NED; uvw; quat; delrate; g];

