using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class OutputViewer : MonoBehaviour
{
    [SerializeField] ConsoleOutput consoleOutput;
    [SerializeField] Text footer;

    public int ch { get => cursorPosX; }
    public int ln { get => cursorPosY; }

    int cursorPosX, cursorPosY;

    // Output that's not filtered by the cursor
    string[] rawOutput;

    void Start() =>
        cursorPosX = cursorPosY = 0;

    void Update()
    {
        if (Input.GetKey(KeyCode.LeftControl))
        {
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                if(cursorPosY != 0)
                    cursorPosY--;
                FormatOutput();
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                cursorPosY++;
                FormatOutput();
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                cursorPosX++;
                FormatOutput();
            }
            else if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                if(cursorPosX != 0)
                    cursorPosX--;
                FormatOutput();
            }
        }

        else if (Input.GetKey(KeyCode.LeftShift))
        {
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                if (cursorPosY > 5)
                    cursorPosY -= 5;
                else
                    cursorPosY = 0;

                FormatOutput();
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                cursorPosY += 5;
                FormatOutput();
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                cursorPosX += 5;
                FormatOutput();
            }
            else if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                if (cursorPosX > 5)
                    cursorPosX -= 5;
                else
                    cursorPosX = 0;

                FormatOutput();
            }
        }
    }

    private void UpdateFooter()
    {
        footer.text = "[ ln: " + ln + " ch: " + ch + " ]"; 
    }

    private void FormatOutput()
    {
        string fOutput = "";

        List<string> temp = new List<string>();
        for (int i = cursorPosY; i < rawOutput.Length; i++)
            temp.Add(rawOutput[i]);

        foreach(string line in temp)
        {
            if (cursorPosX < line.Length)
                for (int i = cursorPosX; i < line.Length; i++)
                    fOutput += line[i];
            fOutput += '\n';
        }

        consoleOutput.Print(fOutput);

        UpdateFooter();
    }

    public void Print(string output)
    {
        List<string> lines = new List<string>();
        string line = "";

        foreach(char c in output)
        {
            switch(c)
            {
                case '\n':
                    lines.Add(line);
                    line = "";
                    continue;

                default:
                    line += c;
                    continue;
            }
        }

        if (line != "")
            lines.Add(line);

        rawOutput = lines.ToArray();
        FormatOutput();
    }
}
