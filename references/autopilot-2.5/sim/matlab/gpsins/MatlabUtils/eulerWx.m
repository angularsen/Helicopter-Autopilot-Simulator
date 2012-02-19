function Wx = eulerWx(rates)
% This will create a 3x3 Omega-cross matrix
% from a row vector of body axis rates, [p q r].
%
%	Wx = eulerWx([p q r])

p = rates(1);
q = rates(2);
r = rates(3);


Wx = [0 -r q;...
      r 0 -p;...
      -q p 0];

