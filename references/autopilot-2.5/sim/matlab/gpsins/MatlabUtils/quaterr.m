function Q = quaterr(q, p)
%	This will compute the difference between two quaternion
% vectors, q and p.  
%
% Q = quaterr(q1, q2)
%
% Q = 3x1 error matrix
% q and p = 4x1

q1 = q(1);
q2 = q(2);
q3 = q(3);
q4 = q(4);

p1 = p(1);
p2 = p(2);
p3 = p(3);
p4 = p(4);


sgn = 2*sign(q1*p1 + p2*p2 + q3*p3 + q4*p4);

Q = sgn*[-q1*p2 + q2*p1 + q3*p4 - q4*p3;...
      -q1*p3 - q2*p4 + q3*p1 + q4*p2;...
      -q1*p4 + q2*p3 - q3*p2 + q4*p1];
