function DCM = eulerDCM(eul)
% This will create a 3x3 DCM from a row vector of euler
% angles, [phi theta psi].
%
%	DCM = eulerDCM([phi theta psi])

phi = eul(1);
theta = eul(2);
psi = eul(3);

sphi = sin(phi);
cphi = cos(phi);
stheta = sin(theta);
ctheta = cos(theta);
spsi = sin(psi);
cpsi = cos(psi);

Pit = [ctheta 0 -stheta;...
      0 1 0;...
      stheta 0 ctheta];
   
Rol = [1 0 0;...
      0 cphi sphi;...
      0 -sphi cphi];
Yaw = [cpsi spsi 0;...
      -spsi cpsi 0;...
       0 0 1];
 DCM = Rol*Pit*Yaw;
