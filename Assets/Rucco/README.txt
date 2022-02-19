RUCCO is a cheat console that can be embedded into unity game.
You can add features to the RUCCO console by creating custom commands.
Is meant for the player to mess with. It uses modified Poy interpreter as
a main interface. Poy as a language is little bit like batch script syntaxwise,
and has some useful features.

Installation

  Copy this folder and it's contents into "Assets" folder inside desired
  Unity project folder.

Usage

  Add Rudeco prefab into the scene,
  and press F1 to open it when the game is on.
  
Creating expansions with custom commands

  Create C# class inside the custom commands folder, and inherit the class
  from CustomCommand class. Further documentation on how to create
  custom command, read the README.txt inside the CustomCommands folder.