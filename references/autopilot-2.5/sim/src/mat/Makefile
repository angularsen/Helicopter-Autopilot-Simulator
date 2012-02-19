#!/usr/bin/make
# $Id: Makefile,v 2.1 2002/09/30 15:20:49 tramm Exp $

#################################
#
# All things that we will build
#
LIBS		=							\
	libmat								\

TESTS		=							\
	benchmark							\

NO=\
	testmat								\
	testpos								\
	testkalman							\
	testnav								\


#
# Our matrix math library
#
libmat.srcs	=							\
	Nav.cpp								\
	Vector_Rotate.cpp						\
	Quat.cpp							\
	SixDOF.cpp							\

NO=\
	rk4.cpp								\
	old-matlib.cpp							\
	vect.cpp							\
	old-quat.cpp							\
	old-nav_funcs.cpp						\


#####################################
#
# Programs
#

#CFLAGS += -pg
#LDFLAGS += -pg


#
# Benchmark times common operations
#
benchmark.srcs	= benchmark.cpp
benchmark.libs	= libmat.a

#
# Compare the matrix manipulation to the old version library
#
testmat.srcs	=							\
	testmat.cpp							\
	old-matlib.cpp							\
	old-quat.cpp							\
	old-nav_funcs.cpp						\
	old-vect.cpp							\

testmat.libs	= libmat.a

#
# Test the position conversion code
#
testpos.srcs	=							\
	testpos.cpp							\

testpos.libs	= libmat.a

# Bring in our generic rules and targets
include ../Makefile.common
