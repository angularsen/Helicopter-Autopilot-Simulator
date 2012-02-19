/* -*- indent-tabs-mode:T; c-basic-offset:8; tab-width:8; -*- vi: set ts=8:
 * $Id: getoptions.h,v 1.2 2003/03/12 17:53:42 tramm Exp $
 *
 * (c) Trammell Hudson <hudson@rotomotion.com>
 *
 *	http://getoptions.sourceforge.net/
 *
 **************
 *
 *  getoptions is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2 of the License, or
 *  (at your option) any later version.
 *
 *  getoptions is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with getoptions; if not, write to the Free Software
 *  Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
 *
 */

#ifndef _getoptions_h_
#define _getoptions_h_

/**
 *  $Id: getoptions.h,v 1.2 2003/03/12 17:53:42 tramm Exp $
 *
 * C implementation of Perl style Getopt::Long module.
 *
 * Arguments specifications can be:
 *
 * ! The option does not take an argument and may be negated
 *   Expects an int *.
 *
 * + The option does not take an argument and will be incremented
 *   by one every time it appears on the command line.  Expects an
 *   int *.
 *
 * & The option does not take an argument and the function pointer
 *   passed in will be called when ever this option is specified.
 *   int (*handler)( void )
 *
 * [:=][sidf&](@N)?
 *
 * : The argument is optional.  If it is omitted, the empty
 *   string will be assigned to strings and zero to numerics.
 *
 * = The argument is required.
 *
 * s String argument.    Expects char **.
 * i Integral argument.  Expects int *.
 * f Float argument.     Expects float *.
 * d Double argument.    Expects double *.
 * & Handler argument.   Expects a function pointer of the form:
 *		int (*handler)( char * )
 *
 * @N  May be called up to N times.  Expects an additional int *
 *     for the number of items recorded.  This is an optional modifier.
 *
 * Successful handlers should return 0.  Any other return code is used
 * to indicate an error and will be passed to the caller of getoptions.
 *
 * After the descriptor, getoptions should be passed one or two pointers
 * for the variables that will be filled in by the parser.
 *
 ****************
 *
 * Example usage:
 *
 * void help( char *type );
 *
 * int vals[10];
 * int num_vals;
 * int rc;
 *
 * rc = getoptions(
 *	&argc,
 *	&argv,
 *	"v|verbose+",		&verbose,
 *	"d|debug!",		&debug,
 *	"s|sums=i@10",		vals, &num_vals,
 *	"h|help&",		help
 * );
 *
 * if( rc != 0 ) { // Error handling }
 * 
 */


#define GETOPTIONS_NOMATCH	127

extern
#ifdef __cplusplus
"C"
#endif
int
getoptions(
	int *			argc,
	char ***		argv,
	...
);

#endif
