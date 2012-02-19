#!/usr/bin/make
# $Id: Makefile,v 1.8 2003/03/12 18:15:08 tramm Exp $

all:	bin

bin:	ground

sim.root	= ../../../sim

CFLAGS		=							\
	-g								\
	-pg								\
	-O3								\
	-W								\
	-Wall								\
	-pedantic							\
	-I$(sim.root)/src						\
	-I$(sim.root)/src/include					\
	-I.								\

CXXFLAGS	= $(CFLAGS)

gl.ldflags	=							\
	-L/usr/X11R6/lib						\
	-lGL								\
	-lGLU								\
	-lglut								\
	-lXi								\
	-lXmu								\

fltk.ldflags	=							\
	-lfltk_gl							\
	-lfltk								\



ground.srcs	=							\
	gui.cpp								\
	main.cpp							\
	Horizon.cpp							\
	Joystick.cpp							\

ground.objs	= $(ground.srcs:.cpp=.o)


gui.cpp: gui.fl
	fluid -c $<

ground: $(ground.objs)
	$(CXX)								\
		-o $@							\
		$(ground.objs)						\
		$(fltk.ldflags)						\
		-L/usr/X11R6/lib -lX11					\
		-L$(sim.root)/lib					\
		-lstate							\
		-ljoystick						\
		-lgetoptions						\
		$(gl.ldflags)						\


clean:
	$(RM) *.o *.a
