function [X,Y,Z]=llh2ecef(lat,long,alt)
%   This function will convert latitude, longitude, and 
% altitude.  Lat and long are in degrees with -lat=south
% of equator; -long=west of PM; altitude is 
% in meters.  Return on this function is X,Y,Z in ECEF 
% coordinates.  The dimentions are in meters.  Based on WGS-84
% lat/long informtion.
% [X,Y,Z]=llh2ecef(lat,long,alt)

a=6378137.0; %m
b=6356752.3142; %m
f=(a-b)/a;
e=sqrt(2*f-f^2);

N=a/sqrt(1-e^2*sin(lat*pi/180)^2);

Z=(N*(1-e^2)+alt)*sin(lat*pi/180);
X=(N+alt)*cos(lat*pi/180)*cos(long*pi/180);
Y=(N+alt)*cos(lat*pi/180)*sin(long*pi/180);
