#include <iostream>

#include <mat/Frames.h>
#include <timer.h>

using namespace std;
using namespace libmat;

int main()
{
	Force<Frame::Body>	xyz( 1, 1, -1 );
	Force<Frame::NED>	ned;
	Angle<Frame::NED>	theta( 1.7, -1, 0 );

	cout << "XYZ=" << xyz << endl;

	ned = rotate( xyz, theta );
	cout << "NED=" << ned << endl;

	const Vector<3>		old_ned( rotate3( xyz.v, theta.v ) );
	cout << "OLD=" << old_ned << endl;

	return 0;
}
