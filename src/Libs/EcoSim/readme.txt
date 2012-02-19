Virtual Environment Simulator
CSC411 Project Readme (May 7, 2009)

Justin Stoecker (jstoecker911@gmail.com)
===============================================================

This application was built using Microsoft XNA Game Studio 2.0
and upgraded to use XNA Game Studio 3.0.  The source included in
this package is the 3.0 version.


Hardware / OS Requirements
---------------------------------------------------------------
-Operating System: Microsoft Windows XP (SP 2) or Vista
-Graphics card: must support Shader Model 3.0


Dependencies
---------------------------------------------------------------
-Microsoft .NET Framework 3.5 Redistributable
http://msdn.microsoft.com/en-us/netframework/cc378097.aspx

-Microsoft XNA Framework Redistributable 3.0
http://www.microsoft.com/downloads/details.aspx?FamilyId=6521D889-5414-49B8-AB32-E3FFF05A4C50&displaylang=en

-DirectX 9.0c
http://www.microsoft.com/downloads/details.aspx?FamilyId=2DA43D38-DB71-4C1B-BC6A-9B6652CD92A3&displaylang=en


To run the program:
---------------------------------------------------------------
"EcoSim/EcoSim/bin/x86/Release" contains the compiled executable
-Use Sim.exe to start the application

Note: "Sim.exe.config" is an XML file that has a few parameters you can
modify, such as the resolution.


Controls
---------------------------------------------------------------

Camera: these controls only apply in the "world screen"

W - translate camera forward
S - translate camera back
A - translate camera left
D - translate camera right
Q - rotate camera left
E - rotate camera right
R - rotate camera down
F - rotate camera up

Mouse
Left - 
	mouse is over gui controls: uses buttons and other controls
	build mode on: places objects
	build mode off: selects objects
Right - point light object is selected: orders the point light to move to
	where the mouse intersects the terrain (if a valid path exists)
Hold Left - moves slider knobs
Hold Right - rotates the camera

Mouse: world screen

Misc. Keys
p - pauses the world clock
- - turns the world clock back rapidly
+ - turns the world clock ahead rapidly
b - toggles "building" mode
esc - deselects current target if one exists
enter - toggles the console; executes console command if the console has one
del - clears the console if it is open


Build Mode
---------------------------------------------------------------
When activated, build mode allows you to place objects in the world using the mouse.
You can add objects by left-clicking somewhere on the terrain mesh. The first 8 objects
will always be point lights; subsequent clicks will add bounce balls.
