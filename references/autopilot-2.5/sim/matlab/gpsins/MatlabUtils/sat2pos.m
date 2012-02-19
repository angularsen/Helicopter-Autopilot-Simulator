function [llh, ecef] = sat2pos(sat_eph, orig_sat_range)
%	This function will input the ephermerus data from 4 satillites as well
% as the psudorange to each satillite.  The output is the latitude,
% longitude, altitude as will as the ECEF position of the observer.
%
%	INPUTS
%  sat_eph[ SV1 X, SV1 Y, SV1 Z;
%           SV2 X, SV2 Y, SV2 Z;
%           SV3 X, SV3 Y, SV3 Z;
%           SV4 X, SV4 Y, SV4 Z]; (ECEF m)
%
%  sat_range[ SV1 psudorange;
%             SV2 psudorange;
%             SV3 psudorange;
%             SV4 psudorange]; (m)
%
%	OUPUTS
%  llh[ latitude; 
%       longitude;
%       altitude]; (deg) (m)
%
%  ECEF[ X;
%        Y;
%        Z]; (m)

%initialize the arrays
drdx=zeros(4,1);
drdy=zeros(4,1);
drdz=zeros(4,1);
H=zeros(4,4);
error=1;
a=0;
X=[0;0;0;0];

%main loop
while(error > 1E-9)
   Xold=X;
   a=a+1;
   
   %compute psudo-range
   
   for n=1:4  
     	 sat_range(n,1)=sqrt((sat_eph(n,1)-X(1,1))^2+(sat_eph(n,2)-X(2,1))^2+...
        	 (sat_eph(n,3)-X(3,1))^2);
   end
   
   %compute H parts and H
	for n=1:4
      drdx(n,1)=-(sat_eph(n,1)-X(1,1))/sat_range(n,1);
      drdy(n,1)=-(sat_eph(n,2)-X(2,1))/sat_range(n,1);
      drdz(n,1)=-(sat_eph(n,3)-X(3,1))/sat_range(n,1);
   	H(n,:)=[drdx(n,1) drdy(n,1) drdz(n,1) 1];   
   end
   
   
   dSat_range=orig_sat_range - sat_range;
   dX=inv(H'*H)*H'*dSat_range;
   X=Xold+dX;
   
   error=norm(dX([1:3],1));
   value(a,1)=error;
   
end

[lat,lon,h]=ecef2llh(X(1,1),X(2,1),X(3,1));

llh = [lat;lon;h];
ecef = X([1:3],1);
