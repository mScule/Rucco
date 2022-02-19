Creating a custom command

  POY:

  To create custom commands for Poy,
  you need to create a class that inherits from CustomCommand class.
  You need to create constructor that gives the base constructor 
  a key that is used for accessing the command,
  and the description, that will be shown at doc_custom_commands command.
  Next you override the Command method, and define the desired action inside it.
  You use the parameters array for using the paratmeters given to the custom command
  inside the enviroment where the Poy is implemented in.

  Beware!
  
   -Every custom command returns a value inside poy! Default return value is a empty string.

   -The key of the custom command HAS TO BE IN CAPS, since every id is turned into caps by
	the tokenizer!


  RUDECO:

  After you created the custom command class,
  drag it into the rudeco game object in unity.
  after that, drag the script into the rudeco custom command interface.

  To test if the poy found the custom command, write :doc_custom_commands into the rudeco.

Example

using PoyLang;

public class Test : CustomCommand
{
	public Test()
		: base("TEST_CUSTOM_COMMAND", new string[] { "returns word \"Test\" " })
	{ }

	public override string Command(string[] parameters)
	{
		return "Custom command works";
	}
}

