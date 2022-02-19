using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

using RuccoPoyLang;

public class Rucco : MonoBehaviour
{
    readonly string
        version     = "0.1.0",
        updateNotes = "RuccoPoy",
        patchNotes  = "";

    RuccoPoy console;
    Dictionary<string, CustomCommand> customCommands;

    private bool backgroundProcess = false;

    [Header("Header")]
    [SerializeField] private Text header;

    [Header("Input & Output")]
    [SerializeField] private InputField input;
    [SerializeField] private OutputViewer output;

    // Contains the full output
    private string
        outputBuffer = "",
        lastSecondOut = "";

    [Header("Window")]
    [SerializeField] Image background;
    [SerializeField] GameObject consoleWindow;

    /// <summary>For the game for checking if the console window is active or not</summary>
    public bool ConsoleWindowState { get => consoleWindow.activeSelf; }

    // CustomCommands in the scene
    private CustomCommand[] commands;

    /// <summary>Singleton access for the Rucco Console</summary>
    public static Rucco instance;

    [Header("Iteration Timer")]
    public bool showIterationTime = true;
    [SerializeField] private Text iterationTimer;

    private void CustomCommandInit()
    {
        foreach (CustomCommand command in commands)
            customCommands.Add(command.Key, command);
    }

    // For debugging, and is called somewhere in C# code
    public void Run(string commands)
    {
        if (!backgroundProcess)
        {
            console.Interprete(commands);
            output.Print(console.Output[RuccoPoyOuts.mainOut] + '\n');
        }
    }

    public void ClearOutput() =>
        outputBuffer = "";

    private void ClearInput()
    {
        input.Select();
        input.text = "";
    }

    private void IterationTimer(string text)
    {
        if(iterationTimer.text != text)
            iterationTimer.text = text;
    }
    
    private void Interprete()
    {
        if(showIterationTime)
        {
            // Start iteration timer
            System.Diagnostics.Stopwatch iterationTime =
                System.Diagnostics.Stopwatch.StartNew();
        
            // Do iteration
            console.Interprete(input.text);

            // Stopping timer and collecting the results
            iterationTime.Stop();
            System.TimeSpan ts = iterationTime.Elapsed;

            // Output
            outputBuffer += console.Output[RuccoPoyOuts.mainOut] + '\n';
            output.Print(outputBuffer);

            IterationTimer($"[time : {ts.TotalMilliseconds}ms]");
        }
        else
        {
            // Straight interprete and output
            console.Interprete(input.text);
            outputBuffer += console.Output[RuccoPoyOuts.mainOut] + '\n';
            output.Print(outputBuffer);

            IterationTimer($"[iterartion timer off]");
        }
    }

    private void SetBackgroundProcess()
    {
        if (backgroundProcess)
        {
            backgroundProcess = false;
            input.readOnly = false;
            input.textComponent.color = Color.white;
            header.text = "F1 exit ~ F2 clear inputfield ~ F5 run code ~ F10 set background process";
        }
        else
        {
            backgroundProcess = true;
            input.readOnly = true;
            input.textComponent.color = Color.grey;
            header.text = "F1 exit ~ F2 clear inputfield ~ F5 LOCKED ~ F10 end background process";
        }
    }

    private void Start()
    {
        // Collect every CustomCommand from current scene
        commands = FindObjectsOfType<CustomCommand>();

        if (instance == null)
        {
            // Initializing console
            customCommands = new Dictionary<string, CustomCommand>();
            CustomCommandInit();

            console = new RuccoPoy(customCommands);

            outputBuffer +=
                "RUCCO Cheating console\n" +
                $"Version: {OutputColors.Cyan(version)}\n" +
                $"Update Notes:\n{updateNotes}\n" +
                $"Patch Notes:\n{patchNotes}\n\n" +

                $"Write {OutputColors.Magenta("out help")} to get started\n";

            output.Print(outputBuffer);

            instance = this;
        }
    }

    private void Update()
    {
        // Open/close Rucco window (F1)
        if (Input.GetKeyDown(KeyCode.F1))
        {
            switch (consoleWindow.activeSelf)
            {
                case false:
                    consoleWindow.SetActive(true);
                    background.enabled = true;
                    break;

                case true:
                    consoleWindow.SetActive(false);
                    background.enabled = false;
                    break;
            }
        }

        // Clear input field (F2)
        if (consoleWindow.activeSelf && Input.GetKeyDown(KeyCode.F2))
            ClearInput();

        // Run code (F5)
        if (consoleWindow.activeSelf && Input.GetKeyDown(KeyCode.F5) && !backgroundProcess)
            Interprete();

        // Set background process (F10)
        if (consoleWindow.activeSelf
            && Input.GetKeyDown(KeyCode.F10))
            SetBackgroundProcess();

        if (backgroundProcess)
            Interprete();

        // For edit command
        if (console.Output != null)
        {
            if (
                console.Output.Length == 2 &&
                console.Output[RuccoPoyOuts.secondaryOut] != null &&
                console.Output[RuccoPoyOuts.secondaryOut] != lastSecondOut)
            {
                input.text += console.Output[RuccoPoyOuts.secondaryOut];
                lastSecondOut = console.Output[RuccoPoyOuts.secondaryOut];
            }
        }
    }
}