#!/usr/bin/make
# $Id: Makefile,v 2.0 2002/09/22 02:07:30 tramm Exp $

BINS		=							\
	heli-panel							\

#
# heli-panel draws the AI / DG / VSI / AS instrument
#
heli-panel.srcs	=							\
	heli-panel.cpp							\
	instruments.cpp							\

heli-panel.libs	=							\
	libstate.a							\

heli-panel.ldflags	=						\
	$(GLFLAGS)							\


include ../Makefile.common

