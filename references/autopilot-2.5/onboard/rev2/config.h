/*
 * $Id: config.h,v 1.1 2003/02/20 16:53:52 tramm Exp $
 *
 * Configuration file for the different feature sets and
 * pin outs
 */

#ifndef _REV2_CONFIG_H_
#define _REV2_CONFIG_H_

#define VERSION		2.4

/***********************************************************************
 *
 * Feature selection
 *
 */

/*
 * Low speed servo banks driven by the 16 bit timer overflows.
 * Uses a 4017 connected to the OCRnw pins and a reset pin.
 */
#define SERVO_BANK_A
#define SERVO_BANK_B


/*
 * High speed servos driven by the 16 bit timer overflows.
 * Directly connects to the outputs.  No reset pin necessary
 */
#define SERVO_HS_A
#define SERVO_HS_B


/*
 * PPM / PCM decoding is selectable, although PCM decoding is
 * very buggy right now.
 */
#define PCM_INPUT
#define PPM_INPUT


/***********************************************************************
 *
 * Consistency checks
 *
 */

#if defined( SERVO_HS_A ) && defined( SERVO_BANK_A )
#error "Only one of SERVO_HS_A and SERVO_BANK_A may be defined"
#endif

#if defined( SERVO_HS_B ) && defined( SERVO_BANK_B )
#error "Only one of SERVO_HS_B and SERVO_BANK_B may be defined"
#endif

#if defined( PCM_INPUT ) && defined( PPM_INPUT )
#error "Only one of PCM_INPUT and PPM_INPUT may be defined"
#endif


/***********************************************************************
 *
 * Board revision specific configurations, with pin assignments
 *
 */

#if VERSION == 2.2
/**
 *  2.2 board specific configurations
 */


#elif VERSION == 2.4

/**
 *  2.4 board specific configurations
 */
#ifdef SERVO_BANK_B
#error "2.4 has no servo bank B"
#endif

#else
#error "Unknown board revision"
#endif

#endif
