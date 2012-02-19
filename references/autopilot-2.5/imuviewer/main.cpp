#include <fcntl.h>
#include <cstring>
#include <stdio.h>
#include <sys/types.h>
#include <unistd.h>
#include <string>
#include <termios.h>
#include "imu_viewer.h"

UserInterface *			gui = 0;
static int		        serial_fd=0;
static int              first_measurement=1;
static int              values[9];
static int              imu_samples=0;

void
init_gui(
	void
)
{
	
    /*gui->guiJoyID->value(0);
	gui->rollAxes->value(0);
	gui->pitchAxes->value(1);
	gui->collAxes->value(2);
	gui->yawAxes->value(3);*/
    
}
 
static 
int
nmea_split(
	const char *		line,
	int *			values,
	int			max_values
)
{
	int			i;

	// Skip NMEA style header
	line = index( line, ',' );
	if( !line )
		return 0;

	for( i=0 ; i<max_values ; i++ )
	{
		char *			end_ptr;

		values[i] = strtol( line+1, &end_ptr, 16 );

		line = end_ptr;

		if( ! *line )
			break;
	}

	return i;
}
   
/**
 *  Braindead code to read line at a time from the serial port.
 */
bool
read_char_line(
	int			fd,
	char *			buf,
	size_t			max_len
)
{
	size_t			i = 0;

	while(1)
	{
		char c;

		if( read( fd, &c, 1 ) <= 0 )
			return false;

		if( c == '\r' )
			continue;

		if( c == '\n' )
			break;

		buf[i++] = c;

		if( i > max_len - 1 )
			break;
	}

	buf[i] = '\0';

	return true;
}

static void
serial_handler(
	int			fd,
	void *			user_arg
)
{

	char			line[ 256 ];
	int			    len;

    if (serial_fd==0) 
        return;

	if( !read_char_line( serial_fd, line, sizeof(line) ) )
	{
		perror( "read_char" );
		return;
	}

	len = strlen( line );
	line[len++] = '\n';
	line[len] = '\0';

    if (gui->pause_raw_output->value()!=1) {
        gui->raw_serial_output->insert(line);
        Fl_Text_Buffer *textBuffer = gui->raw_serial_output->buffer();
        int numLines = textBuffer->count_lines(0, textBuffer->length());
        gui->raw_serial_output->scroll(numLines, 0);

        // ugly way to keep the buffer from growing unchecked
        if (numLines > 1000) {
            textBuffer->remove(0, (textBuffer->length()-100));
        }
    }

	if( strncmp( line, "$GPADC", 6 ) == 0 )
	{
	    imu_samples++;
        update_adc(line);
	} else

	if( strncmp( line, "$GPPPM", 6 ) == 0 )
	{
        update_ppm(line);
	} else

    {
		// Ignore the line for now
	}

}

#ifndef WIN32
static int
max(
	int			a,
	int			b
)
{
	if( a < b )
		return b;
	else
		return a;
}


static int
min(
	int			a,
	int			b
)
{
	if( a < b )
		return a;
	else
		return b;
}
#endif

void
zero_axis(
    int axis
)
{
    if (axis==0)
        gui->glstripchart->setBaseline(values[4]);
    else if (axis==3)
        gui->glstripchart1->setBaseline(values[6]);
    else if (axis==4)
        gui->glstripchart2->setBaseline(values[5]);
    else if (axis==5)
        gui->glstripchart3->setBaseline(values[7]);
    else if (axis==6)
        gui->glstripchart4->setBaseline(values[3]);
    else if (axis==7)
        gui->glstripchart5->setBaseline(values[2]);

    return;
}

void 
select_serial(
    int port
)
{
    if (port==1) {
        if (gui->select_serial_2->value()==1) {
            gui->select_serial_2->value(0);
        }
    } else if (port==2) {
        if (gui->select_serial_1->value()==1) {
            gui->select_serial_1->value(0);
        }
    }

}


void
update_adc(
    char * imuline
)
{
    static int max_values[9];
    static int min_values[9]={32000,32000,32000,32000,32000,32000,32000,32000,32000};

    int found = nmea_split(imuline,values,8);

    gui->adc_value_0->value(values[0]);
    gui->adc_value_1->value(values[1]);
    gui->adc_value_2->value(values[2]);
    gui->adc_value_3->value(values[3]);
    gui->adc_value_4->value(values[4]);
    gui->adc_value_5->value(values[5]);
    gui->adc_value_6->value(values[6]);
    gui->adc_value_7->value(values[7]);

    if (first_measurement) {
        gui->glstripchart->setBaseline(values[4]);
        gui->glstripchart1->setBaseline(values[6]);
        gui->glstripchart2->setBaseline(values[5]);
        gui->glstripchart3->setBaseline(values[7]);
        gui->glstripchart4->setBaseline(values[3]);
        gui->glstripchart5->setBaseline(values[2]);

        first_measurement=0;
    }

    // dynamically resize progress bars based
    // on incoming imu values
    for (int i=0; i<9; i++) {
        max_values[i]=max(max_values[i], values[i]);
    }
    
    for (int i=0; i<9; i++) {
        min_values[i]=min(min_values[i], values[i]);
    }

    gui->adc_bar_0->maximum(max_values[0]);
    gui->adc_bar_0->minimum(min_values[0]);
    gui->adc_bar_0->value(values[0]);
    gui->adc_bar_1->maximum(max_values[1]);
    gui->adc_bar_1->minimum(min_values[1]);
    gui->adc_bar_1->value(values[1]);
    gui->adc_bar_2->maximum(max_values[2]);
    gui->adc_bar_2->minimum(min_values[2]);
    gui->adc_bar_2->value(values[2]);
    gui->adc_bar_3->maximum(max_values[3]);
    gui->adc_bar_3->minimum(min_values[3]);
    gui->adc_bar_3->value(values[3]);
    gui->adc_bar_4->maximum(max_values[4]);
    gui->adc_bar_4->minimum(min_values[4]);
    gui->adc_bar_4->value(values[4]);
    gui->adc_bar_5->maximum(max_values[5]);
    gui->adc_bar_5->minimum(min_values[5]);
    gui->adc_bar_5->value(values[5]);
    gui->adc_bar_6->maximum(max_values[6]);
    gui->adc_bar_6->minimum(min_values[6]);
    gui->adc_bar_6->value(values[6]);
    gui->adc_bar_7->maximum(max_values[7]);
    gui->adc_bar_7->minimum(min_values[7]);
    gui->adc_bar_7->value(values[7]);
}


void
update_ppm(
    char * imuline
)
{
    int values[9];
    static max_values[9]={16000,16000,16000,16000,16000,16000,16000,16000,16000};
    static int min_values[9]={8000,8000,8000,8000,8000,8000,8000,8000,8000};

    int found = nmea_split(imuline,values,9);

    gui->ppm_value_0->value(values[0]);
    gui->ppm_value_1->value(values[1]);
    gui->ppm_value_2->value(values[2]);
    gui->ppm_value_3->value(values[3]);
    gui->ppm_value_4->value(values[4]);
    gui->ppm_value_5->value(values[5]);
    gui->ppm_value_6->value(values[6]);
    gui->ppm_value_7->value(values[7]);
    gui->ppm_value_8->value(values[8]);
    // dynamically resize progress bars based
    // on incoming ppm values
   /* for (int i=0; i<9; i++) {
        max_values[i]=max(max_values[i], values[i]);
    }
    
    for (int i=0; i<9; i++) {
        min_values[i]=min(min_values[i], values[i]);
    }*/

    gui->ppm_bar_0->maximum(max_values[0]);
    gui->ppm_bar_0->minimum(min_values[0]);
    gui->ppm_bar_0->value(values[0]);
    gui->ppm_bar_1->maximum(max_values[1]);
    gui->ppm_bar_1->minimum(min_values[1]);
    gui->ppm_bar_1->value(values[1]);
    gui->ppm_bar_2->maximum(max_values[2]);
    gui->ppm_bar_2->minimum(min_values[2]);
    gui->ppm_bar_2->value(values[2]);
    gui->ppm_bar_3->maximum(max_values[3]);
    gui->ppm_bar_3->minimum(min_values[3]);
    gui->ppm_bar_3->value(values[3]);
    gui->ppm_bar_4->maximum(max_values[4]);
    gui->ppm_bar_4->minimum(min_values[4]);
    gui->ppm_bar_4->value(values[4]);
    gui->ppm_bar_5->maximum(max_values[5]);
    gui->ppm_bar_5->minimum(min_values[5]);
    gui->ppm_bar_5->value(values[5]);
    gui->ppm_bar_6->maximum(max_values[6]);
    gui->ppm_bar_6->minimum(min_values[6]);
    gui->ppm_bar_6->value(values[6]);
    gui->ppm_bar_7->maximum(max_values[7]);
    gui->ppm_bar_7->minimum(min_values[7]);
    gui->ppm_bar_7->value(values[7]);

    gui->ppm_bar_8->maximum(max_values[8]);
    gui->ppm_bar_8->minimum(min_values[8]);
    gui->ppm_bar_8->value(values[8]);
}

static void
sync_handler(
    void *			user_arg
)
{
    gui->glstripchart->addMeasurement(gui->adc_value_4->value());
    gui->glstripchart1->addMeasurement(gui->adc_value_6->value());
    gui->glstripchart2->addMeasurement(gui->adc_value_5->value());
    gui->glstripchart3->addMeasurement(gui->adc_value_7->value());
    gui->glstripchart4->addMeasurement(gui->adc_value_3->value());
    gui->glstripchart5->addMeasurement(gui->adc_value_2->value());
    
    Fl::repeat_timeout(
        .01,
        sync_handler
    );

}

static void
status_handler(
    void * user_arg
    )
{
    static int last_sample_count=0;
    int hertz=0;
    char status_text[128];

    hertz=imu_samples-last_sample_count;

    sprintf(status_text, "   %d Hz", hertz);

    gui->connection_status->value(status_text);

    if (hertz>28) {
        gui->connection_status->color(FL_GREEN);
    } else if (hertz>25 && hertz<=28) {
        gui->connection_status->color(FL_YELLOW);
    } else gui->connection_status->color(FL_RED);

    last_sample_count=imu_samples;
    
    Fl::repeat_timeout(
        1,
        status_handler
        );
}

static void
anim_handler(
    void *			user_arg
)
{
    gui->glstripchart->redraw();
    gui->glstripchart->setZoom(gui->zaccel_zoom->value());
    gui->glstripchart1->redraw();
    gui->glstripchart1->setZoom(gui->xaccel_zoom->value());
    gui->glstripchart2->redraw();
    gui->glstripchart2->setZoom(gui->yaccel_zoom->value());
    gui->glstripchart3->redraw();
    gui->glstripchart3->setZoom(gui->yaw_zoom->value());
    gui->glstripchart4->redraw();
    gui->glstripchart4->setZoom(gui->pitch_zoom->value());
    gui->glstripchart5->redraw();
    gui->glstripchart5->setZoom(gui->roll_zoom->value());

    Fl::repeat_timeout(
#ifdef WIN32
        .11,
#else
        .05,
#endif
        anim_handler
    );
}

void
disconnect_serial()
{
    if (serial_fd) {
        close(serial_fd);
        Fl::remove_fd(serial_fd);
        serial_fd=0;
    }
}

void
connect_serial()
{
    struct termios term;
    string serial_device;

#ifdef WIN32
    if (gui->select_serial_1->value()==1) {
        serial_device="/dev/ttyS0";
    } else  {
        serial_device="/dev/ttyS1";
    }
#else
    serial_device=gui->serial_device_name->value();
#endif
	
    disconnect_serial();
    
    serial_fd = open( serial_device.c_str(), O_RDWR, 0666 );
	if( !serial_fd )
	{
		perror( serial_device.c_str() );
		return -1;
	}

    if (tcgetattr(serial_fd, &term)<0) {
        printf("tcgetattr error\n");
        return -1;
    }

    term.c_lflag &= ~(ECHO | ICANON | IEXTEN | ISIG );
    term.c_iflag &= ~(BRKINT | ICRNL | INPCK | ISTRIP | IXON );
    term.c_cflag &= ~(CSIZE | PARENB );
    term.c_cflag |= CS8;
    term.c_oflag &= ~(OPOST);
    term.c_cc[VMIN] = 1;
    term.c_cc[VTIME]=0;

    cfsetispeed(&term, B38400);
    cfsetospeed(&term, B38400);

    tcsetattr(serial_fd,TCSANOW, &term);

	Fl::add_fd(
		serial_fd,
		FL_READ,
		serial_handler
	);
}

int
main(
	int			argc,
	char **			argv
)
{


	Fl::gl_visual( FL_RGB );

	gui = new UserInterface();
	gui->make_window()->show( argc, argv );
 /*   gui->glstripchart->setBaseline(22400);
    gui->glstripchart1->setBaseline(18700);
    gui->glstripchart2->setBaseline(15300);
    gui->glstripchart3->setBaseline(8000);
    gui->glstripchart4->setBaseline(14000);
    gui->glstripchart5->setBaseline(17000);*/

#define IMU_ZOOM_MIN 1
#define IMU_ZOOM_MAX 1200
#define IMU_ZOOM_DEFAULT 600

    gui->zaccel_zoom->range(IMU_ZOOM_MIN, IMU_ZOOM_MAX);
    gui->zaccel_zoom->value(IMU_ZOOM_DEFAULT);
    gui->xaccel_zoom->range(IMU_ZOOM_MIN, IMU_ZOOM_MAX);
    gui->xaccel_zoom->value(IMU_ZOOM_DEFAULT);
    gui->yaccel_zoom->range(IMU_ZOOM_MIN, IMU_ZOOM_MAX);
    gui->yaccel_zoom->value(IMU_ZOOM_DEFAULT);
    gui->yaw_zoom->range(IMU_ZOOM_MIN, IMU_ZOOM_MAX);
    gui->yaw_zoom->value(IMU_ZOOM_DEFAULT);
    gui->pitch_zoom->range(IMU_ZOOM_MIN, IMU_ZOOM_MAX);
    gui->pitch_zoom->value(IMU_ZOOM_DEFAULT);
    gui->roll_zoom->range(IMU_ZOOM_MIN, IMU_ZOOM_MAX);
    gui->roll_zoom->value(IMU_ZOOM_DEFAULT);

	gui->serial_device_name->value("/dev/ttyS0");

    Fl_Text_Buffer textBuffer = Fl_Text_Buffer();
    gui->raw_serial_output->buffer(&textBuffer);

#ifdef WIN32
    gui->unix_serial->deactivate();
#else
    gui->win32_serial->deactivate();
#endif

    init_gui();
 
    Fl::add_timeout(
        .1,
        sync_handler
    );

    Fl::add_timeout(
#ifdef WIN32
        .11,
#else
        .05,
#endif
        anim_handler
    );

    Fl::add_timeout(
        1,
        status_handler
    );
    
	return Fl::run();
}
