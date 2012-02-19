/**
 *  $Id: splitppm.cpp,v 2.0 2002/09/22 02:07:30 tramm Exp $
 *
 * (c) Bram Stolk
 * (c) Trammell Hudson
 *
 * Splits a packged PMM output into separate frames for making
 * mpeg movies.
 */
#include <cstdlib>
#include <cstdio>
#include <assert.h>

int main(int argc, char *argv[])
{
  if (argc!= 4)
  {
    fprintf(stderr,"Usage: %s file w h\n", argv[0]);
    exit(1);
  }

  char *fname = argv[1];
  int w = atoi(argv[2]);
  int h = atoi(argv[3]);
  assert(w>0);
  assert(h>0);
  
  FILE *f = fopen(fname,"rb");
  if (!f) 
  {
    fprintf(stderr,"Cannot open '%s' for reading\n", fname);
    exit(2);
  } 
  
  int framenr=0;
  int chunk = w*h*3;
  
  unsigned char *buf = new unsigned char [chunk];


  int retval;
  while(1)
  {
      retval = fread(buf, chunk, 1, f);
      if( !retval )
	break;

      char outname[128];
      sprintf(outname, "nb%04d.ppm", framenr);
      FILE *g=fopen(outname,"wb");
      fprintf(g,"P6 %d %d 255\n", w, h);
      fwrite(buf, chunk, 1, g);
      fclose(g);
      framenr++;
  }

  fprintf(stderr,"Wrote %d frames\n", framenr);
  return 0;
}

