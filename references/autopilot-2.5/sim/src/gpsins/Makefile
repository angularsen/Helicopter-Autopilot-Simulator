#!/usr/bin/make
# $Id: Makefile,v 2.0 2002/09/22 02:07:30 tramm Exp $

#################################
#
# All things that we will build
#
BINS		=							\
	gpsins-sim							\

LIBS		=							\
	libgpsins							\


#LDFLAGS		+= -pg

#
# GPS+INS simulation
#
gpsins-sim.srcs	=							\
	gpsins-sim.cpp							\

gpsins-sim.libs	=							\
	libgpsins.a							\
	libmat.a							\

#
# GPS+INS object library
#
libgpsins.srcs	=							\
	GPSINS.cpp							\

NOTYET=\
	compass_update.cpp						\
	gps_update.cpp							\
	imu_update.cpp							\

include ../Makefile.common

