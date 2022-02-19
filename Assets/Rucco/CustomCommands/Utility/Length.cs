using RuccoPoyLang;

public class Length : CustomCommand
{
    public Length() : base
    (
        "LENGTH_OF",

        new string[]
        {
            "Returns the character length of the given item.",
            DescDemandedParam("Item", 1)
        }
    )
    {}

    public override string Command(string[] parameters)
    {
        DemandParamAmt(parameters, 1);
        DemandParam(parameters, 0, "Parameter Item can't be empty");
        return $"{parameters[0].Length}";
    }
}
