function [d,r] = dms2deg(deg, min, sec)
%	This will convert a latitude or longitude from 
% degrees:minutes:seconds to decimal degrees or radians.
%
% [deg,rad] = dms2deg(deg, min, sec)

d = abs(deg)+min/60+sec/3600;
if( deg < 0 )
   d = -d;
end
r = d*pi/180;
