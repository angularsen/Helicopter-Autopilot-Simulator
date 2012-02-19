function [lat, lon, h] = ecef2llh(x, y, z)
%function [lat, lon, h] = ecef2llh(x, y, z)
%
%Converts ECEF coordinates to latitude, longitude, height
%
%Input coordinates in meters. Output angles in degrees, height in meters.
%

a=6378137.0; %m
b=6356752.3142; %m
f=(a-b)/a;
e=sqrt(2*f-f^2);

lon=atan2(y,x)*180/pi;

h=0;
N=a;
hold=100;
while(hold ~= h)
   hold=h;
   sinphi=z/(N*(1-e^2)+h);
   phi=atan((z+e^2*N*sinphi)/(sqrt(x^2+y^2)));
   N=a/sqrt(1-e^2*sin(phi)^2);
   hN=sqrt(x^2+y^2)/cos(phi);
   h=(hN-N);
   %txt=sprintf('phi= %0.3f; h= %0.3f',phi,h);
   %disp(txt);
end

h;
lat=phi*180/pi;
