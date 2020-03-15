# Nurikabe solver in Unity (C#)

This program solves [nurikabe puzzles](https://en.wikipedia.org/wiki/Nurikabe_(puzzle)) using a semi-brute force approach.

In short the problem is based around having NxN field of boxes which can either be part called island or sea boxes and given the sizes of every island and its start location you need to fill the field. And there are some other rules that allow this problem to only have one solution.

The central part of my solution is using backtracking and going through all the possible combinations for each island at the time, while checking for violations of the basic rules of the puzzle. So the island combination is chosen if no violation occurs if it's placed. If every island combination violates the rules we know there was an error in some previously selected island and we revert the state back to before we selected the previous island and try new combinations.

Doing so ensures we eventually find a solution much faster than using completely brute force approach of trying randomly coloring boxes until we have a solution. However this is my solution will still take a significant amount of time for some puzzles because of the magnitude of possible combinations for larger islands. Adding more rules in between island guesses to fill out more boxes would be needed to speed up the process.

## Getting Started

If you're just interested in the code you'll find all the classes [here](Nurikabe/Assets/Scripts).

I have made the program in [Unity 3D](https://unity3d.com/) which is a game engine, because it allowed me to visualize the nurikabe field and run it step by step which helped with debugging.

If you wish to try it out yourself you may take the classes and refactor them a bit to work in your environment, or you can open the project with Unity. The easiest way to do so would be so go to the Nurikabe/Assets/Scenes folder and run MainScene with Unity.

Then pressing the play button at the top-center of the window is all you need to do to test it. 

To set your own nurikabe puzzle you may add a .txt file following the syntax of the test files which are under Nurikabe/Assets/TestFiles (first row is number of rows and columns, subsequent rows are location of islands and size of islands). The text file than needs to be dragged to the place of Grid File component of Main script attached to the Manager object (which you find under scene hierarchy) - in other words replace the test3 file.

![](demo.gif)

### Prerequisites

The Unity version I was using is 2018.2.11f1 Personal  but the only thing that was used was the UI components so I thnik it should work in most of the newer versions.

## License

This project is licensed under the MIT License - see the [LICENSE.md](LICENSE.md) file for details


