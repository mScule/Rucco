using UnityEngine;
using RuccoPoyLang;

public class RuccoLogger : CustomCommand
{
    [SerializeField] private ConsoleOutput rudecoLogger;

    RuccoLogger() : base
    (
        "LOG",

        new string[]
        {
            "Outputs straight to ingame ui",
            "Returns also the output as a string"
        }
    )
    {}

    public override string Command(string[] parameters)
    {
        string output = "";

        foreach(string param in parameters)
            output += param;

        rudecoLogger.Print(output);
        return output;
    }
}
