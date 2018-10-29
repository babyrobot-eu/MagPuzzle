# ARToolkit
For object or piece detection [ARToolkit](https://artoolkit.org/) is used and already contained in the repository. We use it to recognize the fiducial markers on the MagPuzzle pieces. Beacuse of the AR integration we are using 2 displays. In display 1, we draw 3D cubes in the detected locations in space. Using the 3d objects we can perform geometric operations and estimate where each piece is placed on the board. The debug window (shown in figure 7) is not used during wizarding but is useful to debug and visualize the tracking accuracy. The location of each piece is also drawn in our main display in the centre-left position of the wizard window so that the wizard is always aware of the state of the board. Note that in this case we are designing this part of the interface specifically for the MagPuzzle setup, but this part of the interface can be replaced by other objects or boards placed in a table between a user and a robot.

## Calibration
Whenever the board or the camera is moved around you need to recalibrate the scenario. To facilitate the calibration of the board we added a button on the top right corner of the wizard interface. When that button is pressed a [calibration.csv](calibration.csv) file with the estimated distance from the camera of each of the 16 squares is created. When you press the button you have to have all the 6 pieces precisely located on the board in the following configuration.  
-Robot-  
|X  0  0  X|   
|0  0  X  0|   
|0  X  0  0|  
|X  0  0  X|   
The code responsible for this calibration is contained in the public function CalibratePiecePositions in [WizardManager.cs](Assets/Scripts/WizardManager.cs).