#!/usr/bin/make
# $Id: Makefile,v 2.1 2003/03/12 17:46:45 tramm Exp $

#################################
#
# All things that we will build
#

LIBDIRS		=							\
	getoptions							\
	controller							\
	joystick							\
	mat								\
	heli-sim							\
	state								\
	imu-filter							\

BINDIRS		=							\
	heli-sim							\
	state								\
	controller							\
	joystick							\
	heli-3d								\
	heli-panel							\
	imu-filter							\

NO		=							\
	viewer								\


all:	$(BINDIRS:=.all)

lib:	$(LIBDIRS:=.lib)

clean:	$(BINDIRS:=.clean)
clean:	$(LIBDIRS:=.clean)

depend:	$(BINDIRS:=.depend)
depend:	$(LIBDIRS:=.depend)


%.all:
	$(MAKE) -C "$*" all
.all:

%.lib:
	$(MAKE) -C "$*" lib
.lib:

%.clean:
	$(MAKE) -C "$*" clean
.clean:

%.depend:
	$(MAKE) -C "$*" depend
.depend:


