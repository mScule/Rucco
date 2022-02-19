using RuccoPoyLang;

public class ShowIterationTime : CustomCommand
{
    public ShowIterationTime() : base
    (
        "SHOW_ITERATION_TIME",

        new string []
        {
            "set 1 to show, 0 otherwise",
            "By shutting off the iteration timer, you will maybe get little performance boost"
        }
    )
    {}

    public override string Command(string[] parameters)
    {
        DemandParamAmt(parameters, 1);
        DemandParam(parameters, 0, "You need to give 1 or 0");

        switch(parameters[0])
        {
            case "1":
                Rucco.instance.showIterationTime = true;
                break;
            case "0":
                Rucco.instance.showIterationTime = false;
                break;

            default:
                Error.Throw(ErrorOrigin.CustomCommand, "the value can only be either 1 or 0");
                break;
        }

        return string.Empty;
    }
}