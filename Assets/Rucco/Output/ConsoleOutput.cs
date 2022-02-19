using UnityEngine.UI;
using UnityEngine;

public class ConsoleOutput : MonoBehaviour
{
    [SerializeField] Text outputWhite;
    [SerializeField] Text outputRed;
    [SerializeField] Text outputGreen;
    [SerializeField] Text outputBlue;
    [SerializeField] Text outputYellow;
    [SerializeField] Text outputCyan;
    [SerializeField] Text outputMagneta;

    public string DollarSign { get => "$$"; }

    //[SerializeField] WindowResize windowResize;

    public void Clear()
    {
        outputWhite.text  =
        outputRed.text    = outputGreen.text = outputBlue.text    =
        outputYellow.text = outputCyan.text  = outputMagneta.text = "";
    }

    public void Print(string input)
    {
        //windowResize.Resize(input);

        bool markup = false;

        string[] outputs =
        {
            "", // White   0
            "", // Red     1
            "", // Green   2
            "", // Blue    3
            "", // Yellow  4
            "", // Cyan    5
            "", // Magneta 6
        };

        byte textColor = 0;

        byte
            white = 0,
            red = 1,    green = 2, blue = 3,
            yellow = 4, cyan = 5,  magneta = 6;

        foreach (char c in input)
        {
            if (markup)
            {
                switch (c)
                {
                    case 'w': case 'W':
                        textColor = white;
                        break;

                    case 'r': case 'R':
                        textColor = red;
                        break;

                    case 'g': case 'G':
                        textColor = green;
                        break;

                    case 'b': case 'B':
                        textColor = blue;
                        break;

                    case 'y': case 'Y':
                        textColor = yellow;
                        break;

                    case 'c': case 'C':
                        textColor = cyan;
                        break;

                    case 'm': case 'M':
                        textColor = magneta;
                        break;

                    case '$':
                        //escaping '$'
                        for (int i = 0; i < 7; i++)
                        {
                            if (i == textColor)
                                outputs[i] += c;
                            else
                                outputs[i] += ' ';
                        }
                        break;
                }
                markup = false;
                continue;
            }
            if (c == '$')
            {
                markup = true;
                continue;
            }

            // Adds spaces, newlines, tabs, and vertical tabs to every output
            if (c == ' ' || c == '\n' || c == '\t' || c == '\v')
                for (int i = 0; i < 7; i++)
                    outputs[i] += c;

            // Adds characters only to selected output
            else
            {
                for (int i = 0; i < 7; i++)
                {
                    if (i == textColor)
                        outputs[i] += c;
                    else
                        outputs[i] += ' ';
                }
            }
        }

        outputWhite.text   = outputs[white];

        outputRed.text     = outputs[red];
        outputGreen.text   = outputs[green];
        outputBlue.text    = outputs[blue];

        outputYellow.text  = outputs[yellow];
        outputCyan.text    = outputs[cyan];
        outputMagneta.text = outputs[magneta];
    }
}