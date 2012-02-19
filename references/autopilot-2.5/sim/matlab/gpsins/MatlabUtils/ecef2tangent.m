function Ce2t = ecef2tangent(latitude, longitude)
%	This function will generate the rotation matrix to 
% convert a vector in ECEF coordinates to local tangent
% plane coordinates at a given latitude and longitude.
% The latitude and longitude are to be in radians.
%
% Ce2t = ecef2tangent(latitude, longitude)

slat = sin(latitude);
slon = sin(longitude);
clat = cos(latitude);
clon = cos(longitude);

Ce2t = [-slat*clon -slat*slon clat;...
      -slon clon 0;...
      -clat*clon -clat*slon -slat];