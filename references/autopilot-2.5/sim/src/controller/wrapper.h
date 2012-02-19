/**
 *  $Id: wrapper.h,v 2.0 2002/09/22 02:07:30 tramm Exp $
 *
 * C wrapper for the Controller object.
 *
 * You only get one and it is very simple...
 */

#ifndef _wrapper_h_
#define _wrapper_h_

#ifdef __cplusplus
extern "C" {
#endif


extern int
controller_reset(
	const char *		hostname,
	int			port
);


extern void
controller_set(
	double			north,
	double			east,
	double			down,
	double			heading
);


extern int
controller_connected( void );


extern int
controller_step(
	double			*north,
	double			*east,
	double			*down,
	double			*roll,
	double			*yaw,
	double			*heading
);


#ifdef __cplusplus
}
#endif

#endif
