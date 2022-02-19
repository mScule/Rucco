using UnityEngine;
using RuccoPoyLang;

class GetItemNames : CustomCommand
{
    GetItemNames() : base
    (
        "ALL_SCENE_ITEMS",

        new string[]
        {
            "Returns list containing all scene items",
            DescNoParameters()
        }
    )
    {}

    public override string Command(string[] parameters)
    {
        string output = "";

        GameObject[] gameObjects = FindObjectsOfType<GameObject>();

        foreach(GameObject gameObject in gameObjects)
            output += gameObject.name + '\n';
        
        return output;
    }
}