#!/usr/bin/make
# $Id: Makefile,v 1.1 2002/09/27 04:51:03 tramm Exp $


all: spi-bridge.install


#
# spi-bridge is the Vector 2X -> 8 bit SPI converter
#
spi-bridge.elf: spi-bridge.S


%.elf: %.S
	avr-gcc                                                         \
		-mmcu=attiny12                                          \
		-nostartfiles                                           \
		-nostdlib                                               \
		-o $@							\
		$<							\


%.srec: %.elf
	avr-objcopy -O srec $< $@

#
# tinystick is the trainer interface
#
tinystick.elf: tinystick.S uart.S

erase:
	uisp -dprog=stk200 -v --erase

%.install: %.srec erase
	uisp -dprog=stk200 -dno-poll -v --upload if=$<


