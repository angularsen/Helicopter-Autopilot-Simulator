function C = dthetadq(q)
%   This will compute the derivative of the Euler
% angle theta wrt q0 to q3 quaternions.
% The result is a row vector dtheta/dq0 ... dtheta/dq3.
% The input is a row vector [q0 ... q3].

q0 = q(1);
q1 = q(2);
q2 = q(3);
q3 = q(4);


% THETA section
err = -1/sqrt(1 - (2*(q1*q3 - q0*q2))^2);
C(1,1) = -2*q2*err;
C(1,2) = 2*q3*err;
C(1,3) = -2*q0*err;
C(1,4) = 2*q1*err;

