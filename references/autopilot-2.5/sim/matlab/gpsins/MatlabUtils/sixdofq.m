function Xdot = sixdofq(inp)
% given as the input the body forces, body moments,
%inertias, mass, gravity, and the states, this will compute the
%6-dof rigid body equations of motion using euler angles.
%INPUT:
%[X Y Z L M N Ixx Iyy Izz Ixz m g u v w p q r q0 q1 q2 q3 North East Down]
%
%OUTPUT:
% . . . . . . .  .  .  .    .     .    .  
%[u v w p q r q0 q1 q2 q3 North East Down]
%
%	Xdot = sixdofq(input)

X = inp(1);
Y = inp(2);
Z = inp(3);

L = inp(4);
M = inp(5);
N = inp(6);

Ixx = inp(7);
Iyy = inp(8);
Izz = inp(9);
Ixz = inp(10);

m = inp(11);
g = inp(12);

u = inp(13);
v = inp(14);
w = inp(15);

p = inp(16);
q = inp(17);
r = inp(18);

q0 = inp(19);
q1 = inp(20);
q2 = inp(21);
q3 = inp(22);

North = inp(23);
East = inp(24);
Down = inp(25);

% Nomalize the quaternion
quats = normq([q0 q1 q2 q3]);

% Generate the DCM 
DCM = quatdcm(quats); 

% Generate the euler Wx matrix
OM = eulerwx([p q r]);

J = [Ixx 0 -Ixz;...
      0 Iyy 0;...
      -Ixz 0 Izz];

detJ = Ixx*Izz - Ixz*Ixz;
invJ = [Izz/detJ 0 Ixz/detJ; 0 1/Iyy 0; Ixz/detJ 0 Ixx/detJ];

% Generate the Strapdown matrix 
E = quatwx([p q r]);

Vb = [u;v;w];
wb = [p;q;r];
G = [0;0;g];
Fb = [X;Y;Z];
Tb = [L;M;N];

Vb_dot = -OM*Vb + DCM*G + Fb/m; 	%3
omb_dot = -invJ*OM*J*wb + invJ*Tb; 	%3
quatdot = E*quats';		  					%3
Ve = DCM'*Vb;					  		%3

Xdot = [Vb_dot' omb_dot' quatdot' Ve'];