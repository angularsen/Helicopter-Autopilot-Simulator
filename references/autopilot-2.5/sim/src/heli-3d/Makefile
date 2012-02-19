#!/usr/bin/make
# $Id: Makefile,v 2.3 2003/03/25 17:36:58 tramm Exp $

#
BINS		=							\
	heli-3d								\
	splitppm							\

#
# Possible models:
# 0	Pod-n-boom
# 1	HH60
# 2	HH65 (with fenestron and cockpit)
#
MODEL		= 0


#
# heli-3d is the OpenGL 3D program that displays the model
# universe and the helicopter state
#
heli-3d.srcs	=							\
	heli-3d.cpp							\
	graphics.cpp							\
	anim.c								\
	viewpoint.cpp							\
	texture.cpp							\
	simview.cpp							\

graphics.cpp.cflags	=						\
	-DMODEL="$(MODEL)"						\

heli-3d.libs	=							\
	libgetoptions.a							\
	libmat.a							\
	libstate.a							\

heli-3d.ldflags	=							\
	$(fltk.ldflags)							\
	$(GLFLAGS)							\

heli-3d.cflags	=							\
	$(fltk.cflags)							\


fltk.cflags	= `fltk-config --cflags --use-gl`
fltk.ldflags	= `fltk-config --ldflags --use-gl`


#
# splitppm takes a large ppm file from heli-3d and produces
# individual ppm frames from it.
#
splitppm.srcs	=							\
	splitppm.cpp							\


include ../Makefile.common


simview.cpp: simview.fl

