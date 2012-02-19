#!/usr/bin/make
# $Id: Makefile,v 2.0 2002/09/22 02:07:31 tramm Exp $

#################################
#
# All things that we will build
#
BINS		=							\
	heli-sim							\

LIBS		=							\
	libsim								\


#LDFLAGS		+= -pg

#
# Our core simulation library
#
libsim.srcs	=							\
	Heli.cpp							\
	Servo.cpp							\
	Blade.cpp							\
	Gear.cpp							\
	wind_model.cpp							\
	wgs_model.cpp							\
	Fin.cpp								\
	FlatEarth.cpp							\
	Forces.cpp							\

NO=\
	gravity_model.cpp						\
	imu_model.cpp							\


#
# heli-sim is the math model core that spawns the clients that
# receive inertial state data and input flight control commands
#
heli-sim.srcs	=							\
	heli-sim.cpp							\

heli-sim.libs	=							\
	libsim.a							\
	libmat.a							\
	libstate.a							\

include ../Makefile.common

