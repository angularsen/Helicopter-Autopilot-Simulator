/* -*- indent-tabs-mode:T; c-basic-offset:8; tab-width:8; -*- vi: set ts=8:
 * $Id: assembly.S,v 2.2 2003/02/27 05:05:28 tramm Exp $
 *
 * Assembly routines to read/write the program flash memory and
 * the EEPROM for the boot loader.
 *
 * (c) 1996-1998 Atmel Corporation
 * (c) Bryce Denney
 * (c) 2002 Trammell Hudson <hudson@rotomotion.com>
 *
 *************
 *
 *  This file is part of the autopilot onboard code package.
 *  
 *  Autopilot is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2 of the License, or
 *  (at your option) any later version.
 *  
 *  Autopilot is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *  
 *  You should have received a copy of the GNU General Public License
 *  along with Autopilot; if not, write to the Free Software
 *  Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
 *
 */

/*
#include <iom16.h>
*/

#define _SPM    .word 0x95E8      ;Workaround for not having enhanced core Assembler in EWAAVR 2.25

/* Not defined in avr-gcc 3.3? Copied from older <iom163.h> files */
#define SPMCR	0x37
#define SPMEN	0x00

.global	write_page
write_page:
        RCALL   WAIT_SPMEN  ;Wait for SPMEN flag cleared
        MOV     R31,R25         
        MOV     R30,R24     ;move adress to z pointer (R31=ZH R30=ZL)
        OUT     SPMCR,R22   ;argument 2 decides function
        _SPM                ;Store program memory
        .word      0xFFFF      ;For future part compatibility, instruction alignment
        NOP
        RJMP    WAIT_SPMEN ;Wait for SPMEN flag cleared

.global fill_temp_buffer
fill_temp_buffer:
// fixed parameters

        RCALL   WAIT_SPMEN  ;Wait for SPMEN flag cleared

        MOV     R31,R23     ;move adress to z pointer (R31=ZH R30=ZL)   
        MOV     R30,R22     
        MOV     R1,R25      ;move data to reg 0 and 1      
        MOV     R0,R24
 
        LDI     R18,(1<<SPMEN)
        OUT     SPMCR,R18   ;argument 2 decides function
        _SPM                ;Store program memory
        .word      0xFFFF      ;For future part compatibility, instruction alignment
        NOP
        RCALL   WAIT_SPMEN  ;Wait for SPMEN flag cleared

#ifdef ENABLE_READ

.global read_program_memory
read_program_memory:
// fix parameter passing
        RCALL    WAIT_SPMEN

        MOV     R31,R25     ;R31=ZH R30=ZL
        MOV     R30,R24     ;move adress to z pointer
        SBRC    R22,0       ;read lockbits? (second argument=0x09)
                            ;if so, place second argument in SPMEN register
        OUT     SPMCR,R22   ;argument 2 decides function
        
        LPM                 

        MOV     R24,R0      ;read LSB           
        INC     R30

        LPM                 

        MOV     R25,R0      ;read MSB (ignored when reading lockbits)

        RET
#endif

#ifdef ENABLE_FUSE_BITS
.global write_lock_bits
write_lock_bits:
// fixed parameter passing
        RCALL   WAIT_SPMEN  ;Wait for SPMEN flag cleared

        MOV     R0,R24   
        LDI     R17,((1<<BLBSET)|(1<<SPMEN))

        OUT     SPMCR,R17   ;argument 2 decides function

        _SPM                ;write lockbits

        .word      0xFFFF      ;For future part compatibility, instruction alignment
        NOP
        
        RCALL   WAIT_SPMEN  ;Wait for SPMEN flag cleared
#endif

WAIT_SPMEN:
        
        RET

