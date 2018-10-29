# Wizard Interface
The wizard interface was created in [Unity3D](https://unity3d.com/) and C# is used for most of the scripting in the project. Although Unity allows for crossplatform development and deployment, some of the modules (Speech Recognition, GazeSense and Tobii) in this project  will only work in Windows 10.


## GameObjects
[GameObjects](https://docs.unity3d.com/ScriptReference/GameObject.html) are the baseclass for all objects in Unity3D and we use them to organize and divide our project.

### Speech Recognition
The Speech Recognition game object contains the [SpeechRecognition.cs](Assets/Scripts/SpeechRecognition.cs) script. This class initializes a Unity's [DictationRecognizer](https://docs.unity3d.com/ScriptReference/Windows.Speech.DictationRecognizer.html) object. This allows us to use Windows 10 Online Speech Recognition with a few lines of code within Unity. Don't forget to [enable Online Speech Recognition in Windows](https://privacy.microsoft.com/en-US/windows-10-speech-inking-typing-and-privacy-faq) (in the target language) to get this to work.

### Logging
The logging game object contains the [dbg.cs](Assets/Scripts/Dbg.cs) script. Dbg is a simple singleton class that can be accessed from any other class to logs any given content to a 'Logs' folder. If a 'Logs' folder does not exist, it creates the folder automatically.  

### Furhat Robot


### ARToolkit
### Main Camera
These two gameobjects are present in most Unity3D projects. We use a simple camera 

### Canvas
#### Wizard Management
### GameBoard
#### Gaze

### RTVoice

The interface is able to follow MagPuzzle pieces as they move and the wizard that is controlling it is able to guide users towards a target solution.

## Game Board

### Hardware

## Speech Recognition
Speech recognition results from the user are drawn in a text box on the bottom-left of the interface. Intermediate speech recognition results or hypothesis are presented with a … punctuation at the end of the detected utterance and once the result is known the punctuation is removed. The result stays in the interface for a few more seconds or until a new recognition starts after detection of the end of utterance.

## Furhat Connection

## Logging