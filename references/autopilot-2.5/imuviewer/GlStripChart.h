#ifndef _GlStripChart_h_
#define _GlStripChart_h_
#include <Fl/Fl.h>
#include <Fl/Fl_Window.h>
#include <Fl/Fl_Gl_Window.h>
#include <Fl/Fl_Box.h>
#include <deque>

using namespace std;
                           
class GlStripChart : public Fl_Gl_Window {
public:
    GlStripChart(int x,int y,int w,int h,const char *l=0);
    void draw();
    void initialize_gl();
    void addMeasurement(int adc_val);
    void setBaseline(int value);
    int getNormalizedValue(int value);
    void setZoom(int value);
private:
    int mouseX;
    int mouseY;
    deque<int> buffer;
    int baseline;
    int zoom;


};

#endif

