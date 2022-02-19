using UnityEngine;
using RuccoPoyLang;

class LocationOf : CustomCommand
{
    LocationOf() : base
    (
        "LOCATION_OF",

        new string[]
        {
            "Returns the location of GameObject",
            "Modifies the location if X, Y, and Z positions are given",

            DescDemandedParam("GameObject name", 1),
            DescOptionalParam("X position", 2),
            DescOptionalParam("Y position", 3),
            DescOptionalParam("Z position", 4)
        }
    )
    {}

    public override string Command(string[] parameters)
    {
        DemandParamAmt(parameters, 1);

        string output = "";

        GameObject[] gameObjects = FindObjectsOfType<GameObject>();

        DemandParam(parameters, 0, "You need to give the name of the GameObject");

        foreach (GameObject gameObject in gameObjects)

            if (parameters[0] == gameObject.name)
            {
                // no parameters call doesn't work
                if (IsParamAmt(parameters, 4))

                    gameObject.transform.localPosition = new Vector3(
                        gameObject.transform.localPosition.x + float.Parse(parameters[1]),
                        gameObject.transform.localPosition.y + float.Parse(parameters[2]),
                        gameObject.transform.localPosition.z + float.Parse(parameters[3])
                    );

                output += gameObject.transform.position;
            }

        return output;
    }
}