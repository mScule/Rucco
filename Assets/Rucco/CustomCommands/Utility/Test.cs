using RuccoPoyLang;

public class CommandTemplate : CustomCommand
{
    public CommandTemplate() : base
    (
        "COMMAND_TEMPLATE",

        new string[]
        {
            "Template for CustomCommand",
            DescNoParameters()
        }
    )
    { }

    public override string Command(string[] parameters) =>
        "Hello world!";
}