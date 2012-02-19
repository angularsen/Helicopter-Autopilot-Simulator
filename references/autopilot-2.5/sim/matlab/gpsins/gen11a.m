function A = gen11a(uvw, pqr, Q, g)
%   This will generate the A matrix for the GPS aided INS
%   Inputs:
%   uvw = [u; v; w]
%   pqr = [p; q; r]
%   Q = [q0; q1; q2; q3]
%   g = g


u = uvw(1);
v = uvw(2);
w = uvw(3);

p = pqr(1);
q = pqr(2);
r = pqr(3);

q0 = Q(1);
q1 = Q(2);
q2 = Q(3);
q3 = Q(4);


C = quatdcm([q0 q1 q2 q3]);
Wxq = quatwx([p q r]);
Wx = eulerwx([p q r]);

AA = zeros(3,3);
AB = C';
AC = [-2*v*q3+2*w*q2 2*v*q2+2*w*q3 -4*u*q2+2*v*q1+2*w*q0 -4*u*q3-2*v*q0+2*w*q1;...
      2*u*q3-2*w*q1 2*u*q2-4*v*q1-2*w*q0 2*u*q1+2*w*q3 2*u*q0-4*v*q3+2*w*q2;...
      -2*u*q2+2*v*q1 2*u*q3+2*v*q0-4*w*q1 -2*u*q0+2*v*q3-4*w*q2 2*u*q1+2*v*q2];
AD = zeros(3,1);

AE = zeros(3,3);
AF = -Wx;
AG = [-2*g*q2 2*g*q3 -2*g*q0 2*g*q1;...
      2*g*q1 2*g*q0 2*g*q3 2*g*q2;...
      0 -4*g*q1 -4*g*q2 0];
AH = [C(1,3); C(2,3); C(3,3)];

AI = zeros(4,3);
AJ = zeros(4,3);
AK = Wxq;
AL = zeros(4,1);

AM = zeros(1,11);

A = [AA AB AC AD;...
      AE AF AG AH;...
      AI AJ AK AL;...
      AM];
