function ab = alphabeta(inp)
% THis will compute alpha and beta given a vector or u,v,w
% INPUT:
% inp [u v w]
%
% OUTPUT
% ab [alpha beta] (rad)
%
%	ab = alphabeta(inp)

u = inp(1);
v = inp(2);
w = inp(3);

V = sqrt(u^2 + v^2 + w^2);
if(V > 1)
	alpha = atan2(w,u);
   beta = atan2(v, V);
else
   alpha = 0;
   beta = 0;
end


ab = [alpha beta];