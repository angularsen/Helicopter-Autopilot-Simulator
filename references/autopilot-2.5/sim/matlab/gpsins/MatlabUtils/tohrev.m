function angle = tohrev(angle)
% This will take in an angle in radians between
% -2*pi and 2*pi and modulate it so that the output
% is from -pi to pi
%
%	angle = tohrev(angle)

if(angle > pi & angle <= 2*pi)
   angle = angle - 2*pi;
elseif(angle < -pi & angle > -2*pi)
   angle = 2*pi + angle;
else
   angle = angle;
end

   