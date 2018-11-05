# Responsive Autonomous Joint Attention System
For generating the autonomous gaze behavior of our robot, several gameObjects communicate with a central [Autonomous Gaze Behavior Script] (Assets/Scripts/AutonomousGazeBehavior.cs). This implementation follows a proposal that divides responsive joint attention gaze responsibilities between two concurrent layers (depicted in the picture below). 

![Picture of the gaze model](ResponsiveGazeDiagram.png)

## Proactive Gaze Layer
- The proactive layer uses the wizard's dialog act decisions (Decision Making) to generate the appropriate gaze that suits the dialog act. If dialog acts are associated with a square on the board or a quadrant, the robot looks at the associated place on the board. When dealing with dialog acts not associated with the board, the robot looks at the participant. Using the information provided by gaze sense our application tracks the user in real-time at 30 frames per second.
- Idle gaze shifts happen when the timer for any previous gaze shift ends. The system either maintains its gaze target or performs a gaze shift to a different target for a certain duration. 

## Responsive Gaze Layer
This layer uses the multimodal information from the user's speech, gaze and board moves to minimize the robot's idle time and generate responsive joint attention behavior. 
- When the speech recognizer returns real-time intermediate or final results, an event is triggered to simulate listening behavior.
- The robot looks at the positions of the board that the user looks at and looks back at users when gazed upon. 
- When participants move, place or remove pieces from the MagPuzzle board, the robot gazes at that location.

## Priorities and Timings
The selection of the duration and priorities of each gaze target can be easily defined in the public properties, in the Unity visual inspector.