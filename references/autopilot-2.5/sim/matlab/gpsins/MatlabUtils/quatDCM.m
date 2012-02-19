function DCM = quatDCM(quat)
% This will create a 3x3 direction cosine matrix
% from a quaternion row vector, [q0 q1 q2 q3].
%
%	DCM = quatDCM([q0 q1 q2 q3])

q0 = quat(1);
q1 = quat(2);
q2 = quat(3);
q3 = quat(4);

q02 = q0*q0;
q12 = q1*q1;
q22 = q2*q2;
q32 = q3*q3;

DCM = [1-2*(q22 + q32) 2*(q1*q2 + q0*q3) 2*(q1*q3 - q0*q2);...
      2*(q1*q2 - q0*q3) 1-2*(q12 + q32) 2*(q2*q3 + q0*q1);...
      2*(q1*q3 + q0*q2) 2*(q2*q3 - q0*q1) 1-2*(q12 + q22)];
