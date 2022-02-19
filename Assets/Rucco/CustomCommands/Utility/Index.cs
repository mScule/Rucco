using RuccoPoyLang;

public class Index : CustomCommand
{
    public Index() : base
    (
        "INDEX",

        new string[]
        {
            "Returns the element at the index in given variable",
            DescDemandedParam("Item", 1),
            DescDemandedParam("Index", 2)
        }
    )
    {}

    public override string Command(string[] parameters)
    {
        DemandParamAmt(parameters, 2);
        DemandParam(parameters, 0, "Parameter Item can't be empty.");
        DemandParam(parameters, 1, "Parameter Index can't be empty.");

        return $"{parameters[0][int.Parse(parameters[1])]}";
    }
}