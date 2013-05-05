DDR-USB-Dance-Pad-Driver
========================

This is a USB dance pad driver for Dance Dance Revolution (Or StepMania).  It uses an Arduino to connect to the dance pad and communicates with it using the existing Arduino serial driver.


Usage
=====
There are two parts to this project: The Arduino part and the C# part (for the PC).

Arduino Part:
	
	First, download the Arduino Software if you haven't already:
		http://arduino.cc/en/Main/Software
		
	Wire your dancepad such that one wire from each switch is connected to ground, 
		and the second wire of each switch goes to the following pins of the Arduino:
			Up Button 		=  Pin 2
			Down Button 	=  Pin 3
			Left Button 	=  Pin 4
			RightButton 	=  Pin 5
			X Button 		=  Pin 6
			O Button 		=  Pin 7
			
		These pins are defined in DDRUsbPadDriver/DDRPad_Arduino/ddrpad_pde.ino so you can change them 
			if you're using your Arduino for multiple things and they conflict.  
			Just preserve the order so you don't have to also modify the C# code.
			The Arduino code is configured so each of those pins has a 20k pullup resister so each of the
			pins are grounded while their respective button is not pressed and draw no current.  When they
			are pressed, they are pulled up to +5V over a that resister.  The Arduino sends the state of 
			each of the buttons repeatedly over serial to the C# application on the other side, which 
			emulates button presses for each so that applications such as StepMania can pick them up.
			
C# Part:

	The C# code can be compiled in Visual Studio or Visual C# Express just by opening the solution and pressing Build and Run.
		Plug the dance pad into the PC before running it as it checks on startup for the COM port and picks the last one that was
		plugged in.
		
	After running it, open StepMania (or whatever program you're using to dance =D ) and reassign key mappings to your new buttons.
