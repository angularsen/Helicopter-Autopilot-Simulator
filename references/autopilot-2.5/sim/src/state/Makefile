#!/usr/bin/make
# $Id: Makefile,v 2.1 2002/10/13 01:32:16 tramm Exp $

LIBS		=							\
	libstate							\

BINS		=							\
	log2txt								\


#
# Our state transmission library
#
libstate.srcs	=							\
	state.c								\
	client.c							\
	udp.c								\
	Server.cpp							\



#
# log2txt converts v2 log files into text form
#
log2txt.srcs	=							\
	log2txt.c							\

log2txt.libs	=							\
	libstate.a							\


include ../Makefile.common

