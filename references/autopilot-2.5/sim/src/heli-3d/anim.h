/**
 *  $Id: anim.h,v 2.0 2002/09/22 02:07:30 tramm Exp $
 *
 * (c) Bram Stolk
 * (c) Trammell Hudson
 *
 *  Dump the frame buffer into a ppm array
 */
#ifndef _ANIM_H_
#define _ANIM_H_

#include "macros.h"

BEGIN_DECLS

extern int
start_animation(
	const char *		filename
);


extern int
save_frame(
	int			x,
	int			y
);


extern unsigned char *
fb2ppm(
	int 			w,
	int 			h
);

END_DECLS

#endif
