#!/usr/bin/make
# $Id: Makefile,v 2.0 2002/09/22 02:07:28 tramm Exp $

MAKE		= gmake

all:									\
	srcdir								\

srcdir:
	$(MAKE) -C ./src

clean:
	$(MAKE) -C src clean
	-$(RM) -f core

depend:
	$(MAKE) -C src depend
