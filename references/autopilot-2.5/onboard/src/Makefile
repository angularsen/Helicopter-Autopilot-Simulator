#!/usr/bin/make
# $Id: Makefile,v 2.0 2002/09/22 02:10:18 tramm Exp $

# Default program to build and install
PROGRAM		= flybywire

all: $(PROGRAM).srec

include ../Makefile.common
install:								\
	$(PROGRAM).srec							\
	$(PROGRAM).install						\


$(PROGRAM:=.o): ../avr/libavr.a

mainloop.o:								\
	mainloop.c							\
	$(INCDIR)/timer.h						\
	$(INCDIR)/uart.h						\
	$(INCDIR)/string.h						\
	$(INCDIR)/servo.h						\
	$(INCDIR)/accel.h						\
	$(INCDIR)/pulse.h						\
	$(INCDIR)/adc.h							\


#
# tinystick is the trainer interface
#
tinystick.elf: tinystick.S uart.S
	avr-gcc								\
		-mmcu=attiny12						\
		-nostartfiles						\
		-nostdlib						\
		tinystick.S						\
		uart.S							\
		-o $@
tinystick.install: tinystick.srec erase
	$(PROG) -dno-poll --upload if=tinystick.srec
	


KEY=x
data:
	@perl -nle '							\
		push @data, { /([a-zA-z]+)=([0-9A-F]+)/g };		\
		END { print map { hex( $$_->{$(KEY)}) . "\n" } @data }	\
	'
