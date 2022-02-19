using RuccoPoyLang;
using UnityEngine;

public class RawInput : CustomCommand
{
    public RawInput() : base
    (
        "INPUT",

        new string[]
        {
            "Checks if given key is pressed or not",
            DescDemandedParam("Keycode", 1)
        }
    )
    {}

    public override string Command(string[] parameters)
    {
        DemandParamAmt(parameters, 1);
        DemandParam(parameters, 0, "You need to give keycode as a parameter");
        if (Input.GetKey(parameters[0]))
            return "1";
        return "0";
    }
}