%	Helpful transformation and simulation functions
%
% Transformation Matrix Tools.
%   eulerdcm         - Generate NED --> body rotation matrix from euler angles.
%   eulerwx          - Generate omega-cross matrix from body rates.
%   eulstrap         - Generate euler angle ODE matrix from euler angles.
%   quatdcm          - Generate NED --> body rotation matrix from quaternion.
%   quatwx           - Generate quaternion ODE matrix from body rates.
%   quaterr          - Generates an error vector from two quaternions.
%   dphidq           - Generate row-vector of derivatives [dphi/dq0...dphi/dq3]
%   dthetadq         - Generate row-vector of derivatives [dtheta/dq0...dtheta/dq3]
%   dpsidq           - Generate row-vector of derivatives [dpsi/dq0...dpsi/dq3]
%
% Conversion Tools.
%   alphabeta        - Generate alpha, beta angles from body velocities.
%   euler2quat       - Convert from euler angles to quaternion.
%   normq            - Normalize a quaternion to length 1.
%   quat2euler       - Convert from quaternion to euler angles.
%   tohrev           - Convert -2*pi <= angle <= 2*pi to -pi <= angle <= pi.
%
% Simulation Tools.
%   sixdofe           - Rigid body 6-DOF dynamics using euler angle propogation.
%   sixdofq           - Rigid body 6-DOF dynamics using quaternion propogation.
%
% GPS Position Tools.
%   ecef2llh          - Convert from ECEF coordinates to Lat, Lon, Altitude.
%   llh2ecef          - Convert from Lat, Lon, Altitude to ECEF coordinates.
%   sat2pos           - Generate a position solution from satillite information.
%
% Other Tools.
%   sat               - Limit function (min <= x <= max).
%   line              - Linear independant -> dependant mapping.