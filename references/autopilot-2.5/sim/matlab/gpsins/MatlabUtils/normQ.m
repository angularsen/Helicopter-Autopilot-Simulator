function Q = normQ(quat)
% This will provide a normalized quaternion
% row vector given a quaternion row vector,
% [q0 q1 q2 q3].
%
%	Q = normQ([q0 q1 q2 q3])

q0 = quat(1);
q1 = quat(2);
q2 = quat(3);
q3 = quat(4);

q02 = q0*q0;
q12 = q1*q1;
q22 = q2*q2;
q32 = q3*q3;

invsqr = sqrt(q02 + q12 + q22 + q32);

q0 = q0/invsqr;
q1 = q1/invsqr;
q2 = q2/invsqr;
q3 = q3/invsqr;

Q = [q0 q1 q2 q3];