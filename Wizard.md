# Wizard Interface
The wizard interface was created in [Unity3D](https://unity3d.com/) and C# is used for most of the scripting in the project. Although Unity allows for crossplatform development and deployment, some of the modules (Speech Recognition, GazeSense and Tobii) in this project  will only work in Windows 10.


## GameObjects
[GameObjects](https://docs.unity3d.com/ScriptReference/GameObject.html) are the baseclass for all objects in Unity3D and we use them to organize and divide our project.

### Speech Recognition
The Speech Recognition game object starts the [SpeechRecognition.cs](Assets/Scripts/SpeechRecognition.cs) script. This class initializes a Unity's [DictationRecognizer](https://docs.unity3d.com/ScriptReference/Windows.Speech.DictationRecognizer.html) object. This allows us to use Windows 10 Online Speech Recognition with a few lines of code within Unity. Don't forget to [enable Online Speech Recognition in Windows](https://privacy.microsoft.com/en-US/windows-10-speech-inking-typing-and-privacy-faq) (in the target language) to get this to work. Final results and intermediate results of speech recognition will result in updating the PlayerUI(described below) with the text that is being said. Intermediate speech recognition results or hypothesis are presented with a … punctuation at the end of the detected utterance and once the result is known the punctuation is removed. The result stays in the interface for a few more seconds or until a new recognition starts after detection of the end of utterance. Additionally, a child game object contains a [unity plugin](https://assetstore.unity.com/packages/tools/audio/rt-voice-pro-41068) that allows us to play back the text of speech recognition results back to the wizard in a 2X speed.

### Logging
The logging game object starts the [dbg.cs](Assets/Scripts/Dbg.cs) script. Dbg is a simple singleton class that can be accessed from any other class to log any given content to a 'Logs' folder. If a 'Logs' folder does not exist, it creates the folder automatically.

### ARToolkit
The ARToolkit game object starts the ARController and several ARMarker scripts developed in the [ARToolkit](https://github.com/artoolkit/arunity5) platform, [click here for more details](ARToolkit.md).

### Robot
The Robot game object starts 4 different scripts. 
- A [gazeDrawing.cs](Assets/Scripts/GazeDrawing.cs) script used to draw two lines from the robot's eyes to the point in the interaction where the robot is looking at. 
- A [Behaviors.cs](Assets/Scripts/Behaviors.cs) script that reads a tsv data file and can generate random (optimized for decreased repeatability) behaviors. 
- A [Furhat.cs](Assets/Scripts/Furhat.cs) .net library that allows us to communicate with the [Furhat robot](http://www.furhatrobotics.com) inside Unity3D, [click here for more details](Furhat.md). 
- A [AutonomousGazeBehavior.cs](Assets/Scripts/AutonomousGazeBehavior.cs) script used to generate autonomous responsive gaze behavior for the robot, [click here for more details](ResponsiveGaze.md).

### Main Camera
This gameobject contains the [Unity3D camera](https://docs.unity3d.com/ScriptReference/Camera.html) used for displaying the user interface in a secondary display. The main display is reserved for the ARToolkit camera. As such, to run this project you will require 2 different monitors (with a recommended resolution of 1080p).

### Canvas
This [game object](https://docs.unity3d.com/Manual/UICanvas.html) defines an area where all the interface elements are drawn. To facilitate organization we created different child game objects for each part of the interface.

#### Multimodal Perception
This game object contains several images that represent all the multimodal information captured by the system that can be perceived by the wizard. This information consists of the player's and robot's action and the MagPuzzle pieces. As such we divided it further into a RobotUI game object, a PlayerUI game object and several game objects for the squares on the board that represent the physical pieces.

- Associated with the RobotUI and PlayerUI game objects there are two interface elements that are controlled by other external scripts: line renderers that represent the gaze direction and a chatbox that represents the text of what the user or the robot is currently vocalizing.

- A squares game object

#### Gaze


#### Wizard Management


The interface is able to follow MagPuzzle pieces as they move and the wizard that is controlling it is able to guide users towards a target solution.

## Game Board

### Hardware


## Furhat Connection

## Logging