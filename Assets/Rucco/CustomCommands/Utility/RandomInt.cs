using RuccoPoyLang;
using UnityEngine;

public class RandomInt : CustomCommand
{
    public RandomInt() : base
    (
        "RANDOM_INT",

        new string[]
        {
            "Returns random integer value.",

            DescDemandedParam("Min value", 1),
            DescDemandedParam("Max value", 2)
        }
    )
    {}

    public override string Command(string[] parameters)
    {
        DemandParamAmt(parameters, 2);
        DemandParam(parameters, 0, "Min value is needed");
        DemandParam(parameters, 1, "Max value is needed");

        return $"{Random.Range(int.Parse(parameters[0]), int.Parse(parameters[1]))}";
    }
}