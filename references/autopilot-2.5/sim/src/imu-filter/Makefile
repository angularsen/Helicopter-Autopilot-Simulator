#!/usr/bin/make
# $Id: Makefile,v 2.10 2003/03/14 19:44:43 tramm Exp $

#################################
#
# All things that we will build
#
BINS		=							\
	flyer								\
	gps-flyer							\
	ahrs								\
	gpsins								\

LIBS		=							\
	libimu-filter							\

TESTS		=							\
	test-gps							\

#
# The sensor processing library reads sensor data from the serial
# port or stdin.
#
libimu-filter.srcs	=						\
	IMU.cpp								\
	GPS.cpp								\
	AHRS.cpp							\
	INS.cpp								\
	Radio.cpp							\
	imu-filter.cpp							\


#
# ahrs is a simple AHRS filter that uses the AHRS object to produce
# an attitude estimate from the inputs, either a real IMU or a state
# server.
#
ahrs.srcs		=						\
	ahrs.cpp							\

ahrs.libs		=						\
	libimu-filter.a							\
	libmat.a							\
	libstate.a							\

#
# gpsins is a simple GPS aided INS to uses the GPS object to produce
# an attitude and position estimate from the inputs.
#
gpsins.srcs		=						\
	gpsins.cpp							\


gpsins.libs		=						\
	libimu-filter.a							\
	libmat.a							\
	libstate.a							\

#
# flyer processes the actual state data and runs the three axis (AHRS)
# flight controller on it.
#
flyer.srcs	=							\
	flyer.cpp							\

flyer.libs	=							\
	libimu-filter.a							\
	libmat.a							\
	libstate.a							\
	libcontroller.a							\
	libgetoptions.a							\

#
# gps-flyer attempts to run the GPS aided INS on the inputs.
#
gps-flyer.srcs	=							\
	gps-flyer.cpp							\

gps-flyer.libs	=							\
	libimu-filter.a							\
	libmat.a							\
	libcontroller.a							\




#
# test-gps simulates a stationary GPS
#
test-gps.srcs	=							\
	test-gps.cpp							\

test-gps.libs	=							\
	libimu-filter.a							\
	libmat.a							\


test-2d.srcs	=							\
	test-2d.cpp							\

test-2d.libs	=							\
	libimu-filter.a							\
	libmat.a							\


include ../Makefile.common

