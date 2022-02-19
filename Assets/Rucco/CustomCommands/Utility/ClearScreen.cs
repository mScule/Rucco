using RuccoPoyLang;

public class ClearScreen : CustomCommand
{

    public ClearScreen() : base
    (
        "CLEAR_SCREEN",

        new string[]
        {
            "Clears output buffer",
            DescNoParameters()
        }
    )
    {}

    public override string Command(string[] parameters)
    {
        Rucco.instance.ClearOutput();
        return string.Empty;
    }
}