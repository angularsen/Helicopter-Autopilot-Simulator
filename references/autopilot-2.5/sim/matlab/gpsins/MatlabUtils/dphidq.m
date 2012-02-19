function C = dphidq(q)
%   This will compute the derivative of the Euler
% angle phi wrt q0 to q3 quaternions.
% The result is a row vector dphi/dq0 ... dphi/dq3.
% The input is a row vector [q0 ... q3].

q0 = q(1);
q1 = q(2);
q2 = q(3);
q3 = q(4);

% PHI section
err = 2/((1-2*(q1^2 + q2^2))^2 + (2*(q2*q3 + q0*q1))^2);
C(1,1) = q1*(1-2*(q1^2 + q2^2))*err;
C(1,2) = (q0*(1-2*(q1^2 + q2^2)) + 2*q1*(2*(q2*q3 + q0*q1)))*err;
C(1,3) = (q3*(1-2*(q1^2 + q2^2)) + 2*q2*(2*(q2*q3 + q0*q1)))*err;
C(1,4) = q2*(1-2*(q1^2 + q2^2))*err;

