function eul = quat2euler(quat)
% This will take a input quaternion row vector,
% [q0 q1 q2 q3], and output a row vector of euler
% angles, [phi theta psi]
%
% [phi theta psi] = quat2euler([q0 q1 q2 q3])

quat = normq(quat);

q0 = quat(1);
q1 = quat(2);
q2 = quat(3);
q3 = quat(4);
   
q02 = q0*q0;
q12 = q1*q1;
q22 = q2*q2;
q32 = q3*q3;

theta = real(-asin(2*(q1*q3 - q0*q2)));
phi = real(atan2( 2*(q2*q3 + q0*q1),(1-2*(q12 + q22)) ));
psi = real(atan2( 2*(q1*q2 + q0*q3),(1-2*(q22 + q32)) ));

eul = [phi theta psi];