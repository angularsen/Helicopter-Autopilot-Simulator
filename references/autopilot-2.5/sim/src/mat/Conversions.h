/******************************************************
*
*	This file contains some useful matrix and general
*	math utilities.  The maximum size of the matrix which
*	can be used is set at 11x11.  
*
*	Author: Aaron Kahn, Suresh Kannan, Eric Johnson
*	copyright 2001
*
* @pkg			matlib
* @version		$Revision: 2.0 $
*
*******************************************************/

#ifndef _CONVERSIONS_H_
#define _CONVERSIONS_H_


#define C_IN2M          0.0254          /* inches to meters */
#define C_FT2M          0.3048          /* (C_IN2M*12) feet to meters */
#define C_M2FT          3.280839895     /* (1.0/C_FT2M) */
#define C_YD2M          0.9144          /* (C_FT2M*3) yards to meters */

#define C_IN2M_F        0.0254f         /* inches to meters */
#define C_FT2M_F        0.3048f         /* (C_IN2M*12) feet to meters */
#define C_M2FT_F        3.280839895f    /* (1.0/C_FT2M) */
#define C_YD2M_F        0.9144f         /* (C_FT2M*3) yards to meters */


#define C_NMI2M         1852.0          /* nautical miles to meters */
#define C_MI2M          1609.344        /* (C_FT2M*5280) miles to meters */

#define C_NMI2M_F       1852.0f         /* nautical miles to meters */
#define C_MI2M_F        1609.344f       /* (C_FT2M*5280) miles to meters */

#define C_G0MPERSEC2    9.80665         /* g0 in m/s^2 */
#define C_G0            32.17404856     /* (C_G0MPERSEC2/C_FT2M) 
                                            standard gravity */
#define C_P0N_M2        1.01325e5       /* p0 in N/m^2 */
#define C_P0            14.692125       /* (C_P0N_M2*1.450e-4)
                                            standard atmospheric pressure */

#define C_G0MPERSEC2_F  9.80665f        /* g0 in m/s^2 */
#define C_G0_F          32.17404856f    /* (C_G0MPERSEC2/C_FT2M) 
                                            standard gravity */
#define C_P0N_M2_F      1.01325e5f      /* p0 in N/m^2 */
#define C_P0_F          14.692125f      /* (C_P0N_M2*1.450e-4)
                                            standard atmospheric pressure */

#define C_LBM2KG        0.45359237      /* lb mass */
#define C_KG2LBM        2.204622622     /* (1.0/C_LBM2KG) */
#define C_LBF2N         4.448221615     /* (C_LBM2KG*C_G0MPERSEC2) lb force */
#define C_SLUG2KG       14.59390294     /* (C_LBM2KG*C_G0) slugs */

#define C_LBM2KG_F      0.45359237f     /* lb mass */
#define C_KG2LBM_F      2.204622622f    /* (1.0/C_LBM2KG) */
#define C_LBF2N_F       4.448221615f    /* (C_LBM2KG*C_G0MPERSEC2) lb force */
#define C_SLUG2KG_F     14.59390294f    /* (C_LBM2KG*C_G0) slugs */


/* math constants */
/* from CRC Standard Mathematical Tables, 27th edition, 1984 */

#define C_PI    3.14159265358979323846264338327950288419716939937511
#define C_ROOT2 1.41421356237309504880168872420969807856967187537695
#define C_ROOT3 1.73205080756887729352744634150587236694280525381039
#define C_E 2.71828182845904523536028747135266249775724709369996

#define C_PI_F    3.14159265358979323846264338327950288419716939937511f
#define C_ROOT2_F 1.41421356237309504880168872420969807856967187537695f
#define C_ROOT3_F 1.73205080756887729352744634150587236694280525381039f
#define C_E_F 2.71828182845904523536028747135266249775724709369996f


#define C_PIOTWO         1.57079632679489661923
#define C_TWOPI          6.28318530717958647692

#define C_PIOTWO_F       1.57079632679489661923f
#define C_TWOPI_F        6.28318530717958647692f

#define C_DEG2RAD        0.017453292519943295
#define C_DEG2RAD_F      0.017453292519943295f
#define C_RAD2DEG       57.295779513082323
#define C_RAD2DEG_F     57.295779513082323f
                      
#define C_MM2M           0.001
#define C_MM2M_F         0.001f
#define C_M2MM        1000.0
#define C_M2MM_F      1000.0f

#define C_IN2M           0.0254
#define C_IN2M_F         0.0254f
#define C_M2IN          39.37007874
#define C_M2IN_F        39.37007874f

#define C_IN2MM         25.4
#define C_IN2MM_F       25.4f
#define C_MM2IN          0.03937007874
#define C_MM2IN_F        0.03937007874f

#define C_FPS2KT         0.5924838
#define C_FPS2KT_F       0.5924838f
#define C_KT2FPS         1.68780986
#define C_KT2FPS_F       1.68780986f

#define C_SQIN2SQFT      0.00694444444444444444444
#define C_SQIN2SQFT_F    0.00694444444444444444444f
#define C_SQFT2SQIN    144.0
#define C_SQFT2SQIN_F  144.0f

#define C_GPM2CFS        0.0022280093
#define C_GPM2CFS_F      0.0022280093f
#define C_CFS2GPM      448.83117
#define C_CFS2GPM_F    448.83117f

#define C_DEGF0_R        459.69
#define C_DEGC0_T        273.16
#define C_DEGC0_DEGF      32.0
#define C_DEGF_PER_DEGC    1.8


#define C_C2K           273.16
#define C_C2K_F         273.16f
#define C_F2R           459.69
#define C_F2R_F         459.69f

#define C_G_CONST       1.068944098e-09   /*6.6732e-11*CUBE(C_M2FT)/C_KG2LBM*/
#define C_EARTH_MASS    1.317041554e25    /*5.974e24*C_KG2LBM   */
#define C_N0_AVOGADRO   6.02205e23
#define C_R_IDEAL_SU    8.31434
#define C_K_BOLTZMANN   1.380622e-23
#define C_C_LIGHT       983571194.2       /*2.9979250e+8*C_M2FT*/
#define C_ECHARGE       1.6021917e-19

#define C_DOFA          0.080719353       /*1.293*C_KG2LBM/CUBE(C_M2FT)  */
#define C_DOFH2O        62.427960576      /*1.000e3*C_KG2LBM/CUBE(C_M2FT)*/
#define C_STOFH2O       75.6
#define C_VOFH2O        1.787e-3
#define C_SOUND0VEL     1087.598425       /* 331.5*C_M2FT */
#define C_SOUND20VEL    1126.64042        /* 343.4*C_M2FT */

#define C_G_CONST_F     1.068944098e-09f  /*6.6732e-11*CUBE(C_M2FT)/C_KG2LBM*/
#define C_EARTH_MASS_F  1.317041554e25f   /*5.974e24*C_KG2LBM   */
#define C_N0_AVOGADRO_F 6.02205e23f
#define C_R_IDEAL_SU_F  8.31434f
#define C_K_BOLTZMANN_F 1.380622e-23f
#define C_C_LIGHT_F     983571194.2f      /*2.9979250e+8*C_M2FT*/
#define C_ECHARGE_F     1.6021917e-19f

#define C_DOFA_F        0.080719353f      /*1.293*C_KG2LBM/CUBE(C_M2FT)  */
#define C_DOFH2O_F      62.427960576f     /*1.000e3*C_KG2LBM/CUBE(C_M2FT)*/
#define C_STOFH2O_F     75.6f
#define C_VOFH2O_F      1.787e-3f
#define C_SOUND0VEL_F   1087.598425f      /* 331.5*C_M2FT */
#define C_SOUND20VEL_F  1126.64042f       /* 343.4*C_M2FT */

#define C_WGS84_a		6378137.0		  /* WGS-84 semimajor axis (m) */
#define C_WGS84_a_F		6378137.0f
#define C_WGS84_b		6356752.3142	  /* WGS-84 semiminor axis (m) */
#define C_WGS84_b_F		6356752.3142f
#define C_WIE			7.2321151467e-05  /* WGS-84 earth rotation rate (rad/s) */
#define C_WIE_F			7.2321151467e-05f

#endif

