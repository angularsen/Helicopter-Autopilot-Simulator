#!/usr/bin/make
# $Id: Makefile,v 2.8 2003/03/22 18:09:53 tramm Exp $
#
# Rev2 control board and IMU makefile
#

ARCH		= atmega163
CC		= /usr/bin/avr-gcc -mmcu=$(ARCH)
LD		= $(CC)
SIZE		= avr-size

SERIAL_FLAGS	=							\
	-dprog=avr910							\
	-dpart=auto							\
	-dserial=/dev/ttyS0						\
	-dspeed=38400							\

ISP_FLAGS	=							\
	-dprog=stk200							\

UISP		= /usr/bin/uisp -v
#UISP_FLAGS	= $(ISP_FLAGS)
UISP_FLAGS	= $(SERIAL_FLAGS)

CFLAGS		=							\
	-O4								\
	-W								\
	-Wall								\

LDFLAGS		=							\
	-L.								\
	-lavr								\
	-lm								\

all:									\
	loader.srec							\
	mainloop.srec							\

install: mainloop.install


mainloop.srcs	=							\
	mainloop.c							\
	uart.c								\
	adc.c								\
	servo.c								\

AHRS_EXTRA	=							\
	ahrs.c								\
	imu.c								\
	pid.c								\
	calib.c								\
	mat.c								\

NO=\
	soft_uart.c							\
	bob.c								\
	lcd.c								\


mainloop.objs	=							\
	$(mainloop.srcs:.c=.o)						\

mainloop.elf: $(mainloop.objs) libavr.a


libavr.srcs	=							\
	string.c							\

libavr.objs	=							\
	$(libavr.srcs:.c=.o)						\

libavr.a: $(libavr.objs)
	$(RM) $@
	$(AR) qcv $@ $(libavr.objs)



pcm.srcs	=							\
	pcm.c								\
	string.c							\
	uart.c								\

pcm.objs	=							\
	$(pcm.srcs:.c=.o)						\

pcm.elf: $(pcm.objs)


#
# Boot loader is always installed over the ISP
#
loader.srcs	=							\
	loader.c							\
	assembly.S							\

loader.objs	=							\
	$(foreach a,$(loader.srcs),$(basename $a).o)			\


loader.elf: $(loader.objs)
	$(LD)								\
		$(LDFLAGS)						\
		$($(@:.elf=).objs)					\
		-Ttext 0x3C00						\
		-o $@							\

	$(SIZE) $@

loader.install: loader.srec erase
	$(UISP) $(ISP_FLAGS) --upload if="$<"


#
# General rules
#
%.elf:
	$(LD)								\
		$($(@:.elf=).objs)					\
		-o $@							\
		$(LDFLAGS)						\

	$(SIZE) $@


%.srec: %.elf
	avr-objcopy -O srec $< $@

%.install: %.srec
	# stk200 needs to be erased first
	$(UISP) $(SERIAL_FLAGS) --erase
	$(UISP) $(SERIAL_FLAGS) --upload if=$<

erase:
	$(UISP) $(ISP_FLAGS) --erase

clean:
	rm -f *.srec *.elf a.out core *.o *.a


#
# Dependencies
#
mainloop.o:								\
	mainloop.c							\
	timer.h								\
	uart.h								\
	string.h							\
	soft_uart.h							\
	servo.h								\
	ppm.h								\
	adc.h								\
	led.h								\
	tach.h								\
	button.h							\

adc.o:									\
	adc.c								\
	adc.h								\
	average.h							\

string.o:								\
	string.c							\
	string.h							\
	uart.h								\

uart.o:									\
	uart.c								\
	uart.h								\

lcd.o:									\
	lcd.c								\
	lcd.h								\

loader.o:								\
	loader.c							\
	led.h								\
	button.h							\

soft_uart.o:								\
	soft_uart.c							\
	soft_uart.h							\
	string.h							\

