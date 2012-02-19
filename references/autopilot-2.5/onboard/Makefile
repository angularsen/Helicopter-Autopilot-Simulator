#!/usr/bin/make
# $Id: Makefile,v 2.0 2002/09/22 02:10:16 tramm Exp $
#
# Top level Makefile
#


all: bin
include Makefile.common


lib:
	$(MAKE) -C ./avr
bin: lib
	$(MAKE) -C ./src 

install: bin
	$(MAKE) -C ./src install


clean:
	$(MAKE) -C ./avr clean
	$(MAKE) -C ./src clean
