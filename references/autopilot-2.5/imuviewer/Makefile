#!/usr/bin/make

all:	bin

bin:	imuviewer

CFLAGS		=							\
	-g								\
	-pg								\
	-O3								\
	-W								\
	-Wall								\
	-pedantic							\
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
	`fltk-config --ldflags`						\

imuviewer.srcs	=							\
	frame.cpp							\
	main.cpp							\
	GlStripChart.cpp						\

imuviewer.objs	= $(imuviewer.srcs:.cpp=.o)


frame.cpp: frame.fl
	fluid -c -o frame.cpp $<

imuviewer: $(imuviewer.objs)
	$(CXX)								\
		-o $@							\
		$(imuviewer.objs)						\
		$(fltk.ldflags)						\
		-L/usr/X11R6/lib -lX11					\
		$(gl.ldflags)						\


