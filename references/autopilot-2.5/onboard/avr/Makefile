#!/usr/bin/make
# $Id: Makefile,v 2.0 2002/09/22 02:10:16 tramm Exp $

LIB		= libavr.a
all: $(LIB)

include ../Makefile.common

LIBOBJS		=					\
	uart.o						\
	string.o					\
	servo.o						\
	accel.o						\
	adc.o						\
	pulse.o						\
	periodic.o					\

$(LIB):		$(LIBOBJS)
	$(RM) -f $@
	$(AR) crs $@ $(LIBOBJS)

avr.h:							\
	servo.h						\
	button.h					\
	timer.h						\
	string.h					\
	adc.h						\
	accel.h						\

servo.o:						\
	servo.c						\
	servo.h						\
	timer.h						\

accel.o:						\
	accel.c						\
	accel.h						\
	average.h					\
	timer.h						\

uart.o:							\
	uart.c						\
	uart.h						\

adc.o:							\
	adc.c						\
	adc.h						\
	average.h					\

string.o:						\
	string.c					\
	string.h					\
	uart.h						\

pid.o:							\
	pid.c						\
	avr.h						\
	pid.h						\

pulse.o:						\
	pulse.c						\
	pulse.h						\
	average.h					\

periodic.o:						\
	periodic.c					\
