#include <iostream>
#include <mat/Quat.h>

using namespace std;
using namespace libmat;

int main()
{
	cerr << "main()" << endl;

	Quat			theta( euler2quat( Vector<3>(0,0,0) ) );
	cerr << "Quat created" << endl << endl;

	Matrix<3,3>		dc1( quatDC( theta ) );
	cerr << "Matrix created" << endl << endl;

	const Matrix<3,3>	dc2( quatDC( theta ) );
	cerr << "const Matrix created" << endl << endl;

	const Matrix<3,3> &	dc3( quatDC( theta ) );
	cerr << "const Matrix & created" << endl << endl;

	const Matrix<3,3> &	dc4( quatDC( theta ) );
	cerr << "const Matrix & created" << endl << endl;

	const Matrix<3,3> &	dc5( dc4 );
	cerr << "const Matrix & created" << endl << endl;

	const Matrix<3,3> 	dc6( dc4 );
	cerr << "const Matrix  created" << endl << endl;


	cout
		<< "1=" << &dc1 << endl
		<< "2=" << &dc2 << endl
		<< "3=" << &dc3 << endl
		<< "4=" << &dc4 << endl
		<< "5=" << &dc5 << endl
		<< "6=" << &dc6 << endl
		<< endl;
}
