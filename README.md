![MagPuzzle Scenario Picture](DocsImages\logoBabyRobot.png)

This application was devoloped for the use case 1 of the [Baby Robot EU Project, grant number: 687831](http://babyrobot.eu/).

# MagPuzzle
![MagPuzzle Scenario Picture](scenario.PNG)

This application was designed and developed to study the effects of **responsive joint attention and mutual gaze** in Human Robot Interaction. MagPuzzle is a spatial reasoning cooperative task where a life-like social robot interacts with human participants. Participants visualize a three-dimensional cube and reconstruct it by placing and manipulating six different colored puzzle pieces on a two-dimensional board with a 4X4 grid. The task is comprised of three different sub-tasks or puzzles, with increasing levels of difficulty, where the maximum amount of pieces in a line (row or column) allowed for the solution varies. The social robot cooperates with participants to arrive at solutions in all three different puzzles. The robot guides users towards a solution mostly by using non-deterministic speech and joint attention behavior. The task was also purposefully designed so that the robot's guidance is more successful if joint attention is established with the robot, and so that the robot is not essential for the completion of the task. Participants can choose to cooperate with the robot but can also choose to ignore it and still be successful.


## Repository
This repository contains different two different systems that are integrated in the MagPuzzle application.
1. [A perception restricted wizard interface for the MagPuzzle scenario.](Wizard.md)
2. [An autonomous responsive joint attention system.](ResponsiveGaze.md)