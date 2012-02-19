function value=linear(x, knownys, knownxs)
% This will compute an output value that is
% on a line determined by 4 input values,
% known x's, known y's.  The independant
% value is give.
%
% value = line(x, knownys, knownxs)
% knownys = [Y1 Y2]
% knownxs = [X1 X2]

Y1 = knownys(1,1);
Y2 = knownys(1,2);

X1 = knownxs(1,1);
X2 = knownxs(1,2);

value = (Y1 - Y2)/(X1 - X2)*x + (Y2 - (Y1 - Y2)/(X1 - X2)*X2);