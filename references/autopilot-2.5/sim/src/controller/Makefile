#!/usr/bin/make
# $Id: Makefile,v 2.1 2003/03/13 22:52:19 tramm Exp $

BINS		=							\
	hover								\
	simple-hover							\
	rpv								\

LIBS		=							\
	libcontroller							\

SWIG		=							\
	Controller							\

#
# The controller library implements PID loops and other things
# to keep the helicopter under control.
#
libcontroller.srcs	=						\
	Attitude.cpp							\
	Guidance.cpp							\
	PID.cpp								\
	wrapper.cpp							\

#
# simple-hover is a program to demonstrate the C bindings of the
# controller library.  It doesn't do much other than hover at (0,0,-5,0)
#
simple-hover.srcs	=						\
	simple-hover.c							\

simple-hover.libs	=						\
	libcontroller.a							\
	libstate.a							\
	libmat.a							\

#
# hover is the simple program to do just that.
#
hover.srcs	=							\
	hover.cpp							\

hover.libs	=							\
	libcontroller.a							\
	libgetoptions.a							\
	libstate.a							\
	libmat.a							\


#
# rpv uses the joystick library to read commands and then
# gives those commands to the controller library to implement
#
rpv.srcs	=							\
	rpv.cpp								\

rpv.libs	=							\
	libcontroller.a							\
	libstate.a							\
	libjoystick.a							\
	libmat.a							\

include ../Makefile.common

#
# SWIG interface for Perl bindings
#

Controller.so.libs	=						\
	libcontroller.a							\
	libmat.a							\
	libstate.a							\

