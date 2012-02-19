function Wx = quatWx(rates)
% This will create the quaternion strapdown matrix
% (4x4) from a row vector of body rates, [p q r].
%
%	Wx = quatWx([p q r])

p = rates(1);
q = rates(2);
r = rates(3);

Wx = [0 -p/2 -q/2 -r/2;...
      p/2 0 r/2 -q/2;...
      q/2 -r/2 0 p/2;...
      r/2 q/2 -p/2 0];
