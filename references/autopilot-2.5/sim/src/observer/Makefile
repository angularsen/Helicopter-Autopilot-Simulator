#!/usr/bin/make
# $Id: Makefile,v 2.0 2002/09/22 02:07:32 tramm Exp $

BINS		=							\
	testmain							\
	testclient							\

LIBS		=							\
	libobserver							\

#
libobserver.srcs	=						\
	Observer.cpp							\

#
testmain.srcs	=							\
	testmain.cpp							\

testmain.libs	=							\
	libobserver.a							\

#
testclient.srcs	=							\
	testclient.cpp							\

testclient.libs	=							\
	libobserver.a							\

include ../Makefile.common
