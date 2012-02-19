function value=lim(input, min, max)
% This will perform the limit function on
% a value entered.  If the value exceeds
% the upper or lower bounds, the value is
% clamped by these bounds.
%
% value = lim(input, min, max)

if( input >= max )
   value = max;
elseif( input <= min )
   value = min;
else
   value = input;
end
