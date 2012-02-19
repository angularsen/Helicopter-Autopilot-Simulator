function Xdot = sixdofe(inp)
% given as the input the body forces, body moments,
%inertias, mass, gravity, and the states, this will compute the
%6-dof rigid body equations of motion using euler angles.
%INPUT:
%[X Y Z L M N Ixx Iyy Izz Ixz m g u v w p q r phi theta psi North East Down]
%
%OUTPUT:
% . . . . . .  .    .    .    .    .    .
%[u v w p q r phi theta psi North East Down]
%
%	Xdot = sixdofe(input)

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

phi = inp(19);
theta = inp(20);
psi = inp(21);

North = inp(22);
East = inp(23);
Down = inp(24);

% Generate the DCM
 DCM = eulerdcm([phi theta psi]);
 
% Generate the euler angle Wx matrix
OM = eulerwx([p q r]); 

J = [Ixx 0 -Ixz;...
      0 Iyy 0;...
      -Ixz 0 Izz];

invJ = inv(J);

% Generate the Strapdown matrix 
E = eulstrap([phi theta psi]);

Vb = [u;v;w];
wb = [p;q;r];
G = [0;0;g];
Fb = [X;Y;Z];
Tb = [L;M;N];

Vb_dot = -OM*Vb + DCM*G + Fb/m; 	%3
omb_dot = -invJ*OM*J*wb + invJ*Tb; 	%3
PHIdot = E*wb;		  					%3
Ve = DCM'*Vb;					  		%3

Xdot = [Vb_dot' omb_dot' PHIdot' Ve'];