#!/usr/bin/make
# $Id: Makefile,v 2.0 2002/09/22 02:07:31 tramm Exp $

#
BINS		=							\
	heli-stick							\
	calibrate							\

TESTS		=							\


LIBS		=							\
	libjoystick							\


#
# Joystick processing library
#
libjoystick.srcs	=						\
	joystick.c							\
	trainer.c							\



#
# calibrate reads the stick and write the configuration file for
# heli-stick
#
calibrate.srcs	=							\
	calibrate.c							\

calibrate.libs	=							\
	libjoystick.a							\

#
# heli-stick reads the joystick and writes commands to the simulator
#
heli-stick.srcs	=							\
	heli-stick.c							\

heli-stick.libs	=							\
	libjoystick.a							\
	libstate.a							\

include ../Makefile.common

