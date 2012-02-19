function plot3vec(fignum, time, data, ylabel1, ylabel2, ylabel3, x_label, plottitle)
%	This will plot data from a vector of length (:,3) with a time 
% vector of length (:,1).  Other input parameters are the 3 y-axis
% labels and the x axis label.  Also, there is the plot title.
%
% plot2vec(fignum, time, data, ylabel1, ylabel2, ylabel3, xlabel, plottitle)

figure(fignum);
subplot(3,1,1); plot(time, data(:,1)); grid on;
title(plottitle);
ylabel(ylabel1);
subplot(3,1,2); plot(time, data(:,2)); grid on;
ylabel(ylabel2);
subplot(3,1,3); plot(time, data(:,3)); grid on;
ylabel(ylabel3);
xlabel(x_label);