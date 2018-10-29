# Furhat .net Library
We have created a .net dll library file that extends functionality and facilitates communication with the Furhat robot within a C# environment. This library communicates with 

## Using the library
This library was created for the Furhat version that uses IRISTK.

## Behaviors
When the wizard system starts, an excel spreadsheet containing all dialog acts used in the interaction is loaded into memory. This spreadsheet is easily editable by non-technical collaborators and for each dialog act there are several lines that contain several behaviour implementations for the same dialog act, see Figure 13. To execute a dialog act, one of its multiple lines is chosen at random optimizing for the ones that were less chosen in the past. The behaviour line that is chosen is then parsed and a behaviour planner outputs the right commands at the right time to the robot. Our behaviour lines are inspired by BML (Kopp, et al., 2006) and allow mixing verbal and non-verbal behaviours in a format that is compact to author.

Our wizard interface enables the simulation of decision making by allowing the wizard to select associated buttons in the interface with the use of a mouse or touchscreen interface. These buttons have in the text label the name of the dialog acts. In the edit phase, buttons can be associated with a piece, a board quadrant, a hint square, a player icon or can be placed anywhere in the interface alongside a label. When clicking on a piece, a quadrant, a square hint or a player icon, the wizard interface reveals the buttons associated to them and the wizard must drag the mouse to the appropriate option to select it. These selections shift the gaze to the focus of attention and execute the behaviour from there. When clicking on a button that is not associated with one of the mentioned interface element one click is sufficient and the focus of attention is always assumed to be the other player in the interaction

## Spreadsheet with available behaviors

## Extra Available Commands
Our external dll to communicate with 