%module Controller
%{
#include <controller/wrapper.h>
%}


%include typemaps.i

int
controller_connected( void );


int
controller_reset(
	const char *	hostname,
	int		port
);


void
controller_set(
	double		north,
	double		east,
	double		down,
	double		heading
);


int
controller_step(
	double		*OUTPUT,	// north
	double		*OUTPUT,	// east
	double		*OUTPUT,	// down
	double		*OUTPUT,	// roll
	double		*OUTPUT,	// pitch
	double		*OUTPUT		// heading
);

