#ifndef _imu_viewer_h_
#define _imu_viewer_h_

#include "frame.h"

class UserInterface;

extern UserInterface *gui;

void update_adc(char * imuline);
void update_ppm(char * imuline);
void zero_axis(int axis);
void select_serial(int port);
void connect_serial();
void disconnect_serial();
#endif
