function ret = sat(s,L,U)
% This is the saturation function.  
%
%       { U: s >= U
%  ret ={ s
%       { L: s <= L
%  
% ret = sat(s,L,U)

n = length(s);

[N,M] = size(s);
ret = zeros(N,M);
for j=1:n
	if s(j) <= L(j)
   	ret(j) = L(j);
	elseif s(j) >= U(j)
    	ret(j) = U(j);
	else
    	ret(j) = s(j);
    end
 end
 
