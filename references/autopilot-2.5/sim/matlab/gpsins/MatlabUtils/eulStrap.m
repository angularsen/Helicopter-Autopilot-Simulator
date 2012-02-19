function E = eulStrap(eul)
% This will generate the euler angle strapdowm matrix
% given a row vector of the euler angles, [phi theta psi].
%
%	E = eulStrap(eul)

phi = eul(1);
theta = eul(2);
psi = eul(3);

sphi = sin(phi);
cphi = cos(phi);
stheta = sin(theta);
ctheta = cos(theta);

E = [1 sphi*tan(theta) cphi*tan(theta);...
      0 cphi -sphi;...
      0 sphi/ctheta cphi/ctheta];

