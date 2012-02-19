#include <iostream>
#include <FL/gl.h>
#include <FL/fl_draw.h>
#include <FL/glut.h>
#include "GlStripChart.h"

using namespace std;

/*
 * draw each character in a stroke font
 */
static void
drawText(
	const char *		message
)
{
	while( *message )
		glutStrokeCharacter(
			GLUT_STROKE_MONO_ROMAN,
			*message++
		);
}


/*
 * generic string drawing routine, 2-D
 */
static void
showMessage(
	GLfloat			x,
	GLfloat			y,
	const char *		message,
	float			scale
)
{
	glPushMatrix();
	glTranslatef( x, y, 0.0 );
	glScalef( 0.0001, 0.0001, 0.0001 );
	glScalef( scale, scale, scale );
	drawText( message );
	glPopMatrix();
}

void 
GlStripChart::draw() 
{
    if (!valid()) {
        initialize_gl();
    }
    glClear(GL_COLOR_BUFFER_BIT);

    double w = this->w();
    double h = this->h();
    double verticalUnit = h / zoom;
    double horizontalUnit = w / 2000;
    glColor3f(0.0f, 0.0f, 0.0f);
    
    showMessage(0.0f,0.0f,"TEST",1.2);

    glBegin(GL_LINE_STRIP);

    for (int i = 0; i < buffer.size(); i++) {
            int adcValue = buffer[i];
            int d = getNormalizedValue(adcValue);
            int y = (int)((h / 2) + d * verticalUnit);
            int x = (int)(w - (horizontalUnit * i));

            glVertex3f( (float) x, (float) y, 0.0f);

    }

    glEnd();
    glFlush();
}

void 
GlStripChart::setBaseline(
    int value
)
{
    baseline=value;
}

void
GlStripChart::setZoom(
    int value
)
{
    zoom=value;
}

int 
GlStripChart::getNormalizedValue(
    int value
)
{
    return value - baseline;
}

void
GlStripChart::addMeasurement(
    int adc_val
)
{
    if (buffer.size()>2000) 
        buffer.pop_back();
    
    buffer.push_front(adc_val);
}


GlStripChart::GlStripChart(
	int			X,
	int			Y,
	int			W,
	int			H,
	const char *		L
) :
	Fl_Gl_Window		(X, Y, W, H, L)
{
}

void
GlStripChart::initialize_gl()
{
	const int		W = this->w();
	const int		H = this->h();

	glClearColor(0.0f, 0.0f, 1.0f, 1.0f);
    mouseX=0;
    mouseY=0;

    glViewport(0,0,W,H);
    glMatrixMode(GL_PROJECTION);
    glLoadIdentity();

    glOrtho(0.0f, W, 0.0f, H, 1.0, -1.0);
    glMatrixMode(GL_MODELVIEW);
    glLoadIdentity();
}


