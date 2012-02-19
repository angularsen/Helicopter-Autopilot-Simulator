function A = gen14a(uvw, pqr, Q, delrate, g)
%   This will generate the A matrix for the GPS aided INS
%   Inputs:
%   uvw = [u; v; w]
%   pqr = [p; q; r]
%   Q = [q0; q1; q2; q3]
%   g = g


u = uvw(1);
v = uvw(2);
w = uvw(3);

p = pqr(1) - delrate(1);
q = pqr(2) - delrate(2);
r = pqr(3) - delrate(3);

q0 = Q(1);
q1 = Q(2);
q2 = Q(3);
q3 = Q(4);


% A matrix components from NED_dot equations.
AA = zeros(3,3);
AB = [1-2*(q2^2 + q3^2), 2*(q1*q2-q0*q3), 2*(q1*q3+q0*q2);...
      2*(q1*q2+q0*q3), 1-2*(q1^2+q3^2), 2*(q2*q3-q0*q1);...
      2*(q1*q3-q0*q2), 2*(q2*q3+q0*q1), 1-2*(q1^2+q2^2)];
AC = [-2*v*q3+2*w*q2, 2*v*q2+2*w*q3, -4*u*q2+2*v*q1+2*w*q0, -4*u*q3-2*v*q0+2*w*q1;...
      2*u*q3-2*w*q1, 2*u*q2-4*v*q1-2*w*q0, 2*u*q1+2*w*q3, 2*u*q0-4*v*q3+2*w*q2;...
      -2*u*q2+2*v*q1, 2*u*q3+2*v*q0-4*w*q1, -2*u*q0+2*v*q3-4*w*q2, 2*u*q1+2*v*q2];
AD = zeros(3,3);
AE = zeros(3,1);

% A matrix componentes from uvw_dot equations.
BA = zeros(3,3);
BB = [0, r, -q;...
      -r, 0, p;...
      q, -p, 0];
BC = [-2*g*q2, 2*g*q3, -2*g*q0, 2*g*q1;...
      2*g*q1, 2*g*q0, 2*g*q3, 2*g*q2;...
      0, -4*g*q1, -4*g*q2, 0];
BD = [0, w, -v;...
      -w, 0, u;...
      v, -u, 0];
BE = [2*(q1*q3-q0*q2);...
      2*(q2*q3+q0*q1);...
      1-2*(q1^2+q2^2)];

% A matrix components from q_dot equations
CA = zeros(4,3);
CB = zeros(4,3);
CC = [0, -p/2, -q/2, -r/2;...
      p/2, 0, r/2, -q/2;...
      q/2, -r/2, 0, p/2;...
      r/2, q/2, -p/2, 0];
CD = [q1/2, q2/2, q3/2;...
      -q0/2, q3/2, -q2/2;...
      -q3/2, -q0/2, q1/2;...
      q2/2, -q1/2, -q0/2];
CE = zeros(4,1);

% A matrix components from delrate_dot equations
DA = zeros(3,3);
DB = zeros(3,3);
DC = zeros(3,4);
DD = zeros(3,3);
DE = zeros(3,1);

% A matrix components from g_dot equations
EA = zeros(1,3);
EB = zeros(1,3);
EC = zeros(1,4);
ED = zeros(1,3);
EE = 0;

A = [AA, AB, AC, AD, AE;...
      BA, BB, BC, BD, BE;...
      CA, CB, CC, CD, CE;...
      DA, DB, DC, DD, DE;...
      EA, EB, EC, ED, EE];



