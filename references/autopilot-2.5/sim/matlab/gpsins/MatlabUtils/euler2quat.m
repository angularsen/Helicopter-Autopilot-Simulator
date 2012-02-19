function Q = euler2quat(eul)
% Given a row vector of euler angles,
% [phi theta psi], this will return a row vector
% of quaternions, [q0 q1 q2 q3].
%
%	Q = euler2quat([phi theta psi])

phi = eul(1);
theta = eul(2);
psi = eul(3);

shphi0   = sin( 0.5*phi );
chphi0   = cos( 0.5*phi );
shtheta0 = sin( 0.5*theta );
chtheta0 = cos( 0.5*theta );
shpsi0   = sin( 0.5*psi );
chpsi0   = cos( 0.5*psi );
q0 =  chphi0*chtheta0*chpsi0 + shphi0*shtheta0*shpsi0;
q1 = -chphi0*shtheta0*shpsi0 + shphi0*chtheta0*chpsi0;
q2 =  chphi0*shtheta0*chpsi0 + shphi0*chtheta0*shpsi0;
q3 =  chphi0*chtheta0*shpsi0 - shphi0*shtheta0*chpsi0;

Q = [q0 q1 q2 q3];