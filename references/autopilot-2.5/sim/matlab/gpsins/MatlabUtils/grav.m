function [gn,Gn]=grav(lat,alt)
%	This will compute the gravity and gravitation for a 
%	location on the earth.  Inputs are latitude in deg and
% 	altitude in meters.  Outputs are in meters/s^2.
%	Vectors for Gn and gn are in [north; east; down].
%	[gravity,gravitation]=grav(lat,alt)


[X,Y,Z]=llh2ecef(lat,90,alt);		%no long dependance so will make it on the Y axis
Rc=sqrt(X^2+Y^2+Z^2);	%computing radius from center of Earth
psi=asin(Z/Rc);	%computing psi (elevation from ECEF)

% Now for some constats

GM=3.9860015E14; %m^3/s^2
a=6378137.0; %m
b=6356752.3142; %m
f=(a-b)/a;
e=sqrt(2*f-f^2);
C2=-0.48416685E-3;
P2=sqrt(5)/2*(3*(sin(psi))^2-1);
J2=-sqrt(5)*C2;
Lnc=[-sin(lat*pi/180) 0 cos(lat*pi/180);...
      0 1 0;...
   cos(lat*pi/180) 0 sin(lat*pi/180)];
we=2*pi/(24*60*60); %rad/sec
N=a/sqrt(1-e^2*sin(lat*pi/180)^2);

%compute G (gravitational attraction in c frame)

Gc=(GM/Rc^2)*[-(a/Rc)^2*3*J2*sin(psi)*cos(psi);...
      0; 1-(3/2)*(a/Rc)^2*J2*(3*(sin(psi))^2-1)];

%compute G ("    " in n frame)

Gn=Gc;

%compute gravity (gravitational attraction + centripital accel. in n frame)

gn=Gn-we^2*(N+alt)*cos(lat*pi/180)*[sin(lat*pi/180);0;cos(lat*pi/180)];


