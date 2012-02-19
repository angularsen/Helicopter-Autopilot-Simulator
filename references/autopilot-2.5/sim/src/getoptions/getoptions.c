/* -*- indent-tabs-mode:T; c-basic-offset:8; tab-width:8; -*- vi: set ts=8:
 * $Id: getoptions.c,v 1.1 2003/03/12 17:34:21 tramm Exp $
 *
 * (c) Trammell Hudson <hudson@rotomotion.com>
 *
 * See the header <getoptions.h> for details on using the library or
 *
 *	http://getoptions.sourceforge.net/
 *
 * For a manpage
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

#include <stdio.h>
#include <stdarg.h>
#include <stdlib.h>
#include <stdint.h>
#include <string.h>
#include "getoptions.h"

static int
is_delimiter(
	char			a
)
{
	return a == '\0'
		|| a == '='
		|| a == '!'
		|| a == ':'
		|| a == '&'
		|| a == '+'
		|| a == '|'
	;
}


/**
 *  Perform multiple string compares, up to either | or other
 * option delimiting character
 */
static int
listcmp(
	const char *		option,
	const char *		descr
)
{
	const char *		s;

	/* Ignore leading - chars in option */
	while( *option == '-' )
		option++;

	s = option;

	while( 1 )
	{
		char			a = *descr;
		char			b = *s;

		/* If we're a match, keep looping */
		if( a == b )
		{
			descr++;
			s++;
			continue;
		}

		/* If we're out of option string, check our description */
		if( b == '\0' && is_delimiter( a ) )
			return 0;

		/* Find next delimiter */
		while( !is_delimiter( *descr ) )
			descr++;

		/* Return no match if we do not have another spelling */
		if( *descr != '|' )
			return -1;

		/* Skip delimiter */
		descr++;

		/* Restart option string */
		s = option;
	}
}


static int
parse_option(
	const char *		descr,
	const char *		option,
	char ***		argv,
	va_list *		ap
)
{
	const char *		type			= descr;
	char *			next_arg		= 0;
	int			not_arg 	 	= 0;
	int			incr			= 0;
	int			max			= 1;
	int			optional		= 0;
	int *			count			= 0;
	int *			i_arg			= 0;
	float *			f_arg			= 0;
	double *		d_arg			= 0;
	char **			s_arg			= 0;
	int			(*p_arg)( void )	= 0;
	int			(*h_arg)( char * )	= 0;

	/* Split the option into list of names and type specification */
	while( *type )
	{
		if( *type == '='
		||  *type == '+'
		||  *type == '!'
		||  *type == ':'
		||  *type == '&'
		)
			break;
		type++;
	}

	/* Decode the option type */
	switch( type[0] )
	{
	case '!' :
		not_arg = 1;
		i_arg = va_arg( *ap, int * );
		break;

	case '+' :
		incr = 1;
		i_arg = va_arg( *ap, int * );
		break;

	case '&' :
		p_arg = va_arg( *ap, int (*)( void ) );
		break;

	case ':' :
		optional = 1;
		/* Fall through */
	case '=' :
		switch( type[1] )
		{
		case 'i' :
			i_arg = va_arg( *ap, int * );
			break;
		case 'f' :
			f_arg = va_arg( *ap, float * );
			break;
		case 'd' :
			d_arg = va_arg( *ap, double * );
			break;
		case 's' :
			s_arg = va_arg( *ap, char ** );
			break;
		case '&' :
			h_arg = va_arg( *ap, int (*)( char * ) );
			break;
		default:
			goto failure;
		}

		switch( type[2] )
		{
		case '\0' :
			break;
		case '@' :
			count = va_arg( *ap, int * );
			max = atoi( &type[3] );
			break;
		default :
			goto failure;
		}

		break;

	default:
	failure:
		fprintf( stderr,
			"Unable to parse option description: '%s'\n",
			descr
		);

		return -1;
	}

	/*
	 * Look for leading "no" on not_arg options
	 */
	if( not_arg )
	{
		/* Skip leading - */
		while( *option == '-' )
			option++;

		if( option[0] == 'n' && option[1] == 'o' )
		{
			option += 2;
			not_arg = -1;
		}
	}


	if( listcmp( option, descr ) != 0 )
		return GETOPTIONS_NOMATCH;


	/*
	 * No argument required cases
	 */
	if( p_arg )
		return p_arg();

	if( incr )
	{
		(*i_arg)++;
		return 0;
	}

	if( not_arg )
	{
		(*i_arg) = not_arg < 0 ? 0 : 1;
		return 0;
	}


	/*
	 * Check for ones that have an array
	 */
	if( count )
	{
		if( *count >= max )
			return GETOPTIONS_NOMATCH;

		if( i_arg )
			i_arg += *count;
		if( f_arg )
			f_arg += *count;
		if( d_arg )
			d_arg += *count;
		if( s_arg )
			s_arg += *count;

		(*count)++;
	}

		
	/*
	 * Get the next argument and parse it.
	 */
	next_arg = *++*argv;

	if( optional && next_arg[0] == '-' )
	{
		(*argv)--;

		if( h_arg )
			return h_arg( 0 );

		if( s_arg )
		{
			(*s_arg) = "";
			return 0;
		}

		/* Let the typical case handle the numeric ones */
		next_arg = "0";
	}

	if( h_arg )
		return h_arg( next_arg );

	if( i_arg )
		(*i_arg) = atoi( next_arg );
	if( f_arg )
		(*f_arg) = atof( next_arg );
	if( d_arg )
		(*d_arg) = atof( next_arg );

	if( s_arg )
		(*s_arg) = next_arg;

	return 0;
}


int
getoptions(
	int *			argc_p,
	char ***		argv_p,
	...
)
{
	int			argc = *argc_p;
	char **			argv = *argv_p;
	int			rc = 0;
	const char *		option;

	while( (option = *++argv) )
	{
		va_list			ap;
		const char *		descr;
		argc--;

		/* Check for a non-option value */
		if( option[0] != '-' )
			goto end;

		/* Check for end of option marker */
		if( memcmp( option, "--", 3 ) == 0 )
		{
			argv++;
			argc--;
			goto end;
		}

		va_start( ap, argv_p );

		while( (descr = va_arg( ap, const char * )) != 0 )
		{
			rc = parse_option( descr, option, &argv, &ap );
			if( rc == GETOPTIONS_NOMATCH )
				continue;
			if( rc == 0 )
				goto found_one;

			if( rc != 0 )
				goto end;
		}

		fprintf( stderr,
			"Unknown option '%s'\n",
			option
		);

		rc = -1;
		break;

found_one:
		rc = 0;
		va_end( ap );
	}

end:
	*argc_p = argc;
	*argv_p = argv;
	return rc;
}
