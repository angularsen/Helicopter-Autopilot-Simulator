function [N,E,D] = nedpts(llh1, llh2, lat, lon)
%	This function will compute the north, east, and down
% local tangent plane distances from point1 to point2 using
% a given latitude and longitude as the rotation for the
% local tangent plane.  The points 1 and 2 are given in 
% [latitude(rad), longitude(rad), altitude (MSL m)].  The rotation
% angles, latitude and longitude, are both in radians.
% The output distances are in meters.
%
%	[N,E,D] = nedpts(llh1, llh2, lat, lon)

ecef1 = llh2ecef(llh1(1)*180/pi, llh1(2)*180/pi, llh1(3));
ecef2 = llh2ecef(llh2(1)*180/pi, llh2(2)*180/pi, llh2(3));

ecef = ecef2 - ecef1;

C = ecef2tangent(lat, lon);

ned = C*ecef;

N = ned(1);
E = ned(2);
D = ned(3);