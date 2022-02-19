using UnityEngine;
using UnityEngine.UI;

public class InputHighlight : MonoBehaviour
{
    [Header("Highlights")]
    [SerializeField] Text operators;
    [SerializeField] Text consoleCommands;
    [SerializeField] Text customCommands;
    [SerializeField] Text varnames;
    [SerializeField] Text numbers;
    [SerializeField] Text comments;

    [Header("InputField")]
    [SerializeField] Text inputField;
    string last = "";

    private void Update()
    {
        // Update highlighting only when its changed
        if (inputField.text == "")
        {
            operators.text =
            consoleCommands.text =
            customCommands.text =
            varnames.text =
            numbers.text =
            comments.text = "";
        }
        else if (last != inputField.text)
        {
            last = inputField.text;
            Highlight();
        }
    }

    void Highlight()
    {
        string input = inputField.text;
        int index = 0;
        char curChar = input[index];

        string[] highlights =
        {
            "", // Operators 0
            "", // ConsoleCommands 1
            "", // CustomCommands 2
            "", // Varnames 3
            "", // Numbers 4
            "", // Comments 5
        };

        int // highlights
            oprs    = 0,
            console = 1, custom = 2,
            vars    = 3, nums = 4,
            coms    = 5;

        // Helper functions
        void AddHighlight(string word, int highlight)
        {
            for (int i = 0; i < 6; i++)
            {
                if (i == highlight)
                {
                    highlights[i] += word;
                }
                else
                {
                    foreach (char c in word)
                    {
                        if (c == '\t' || c == '\n' || c == '\v')
                            highlights[i] += c;
                        else
                            highlights[i] += ' ';
                    }
                }
            }
        }

        void AddEmpty(string word)
        {
            for (int i = 0; i < 6; i++)
                foreach (char c in word)
                    highlights[i] += ' ';
        }

        void GetNextChar()
        {
            index++;

            if (index < input.Length)
                curChar = input[index];
            else
                curChar = '\0';
        }

        bool IsBlank()
        {
            return
                curChar == ' '  ||
                curChar == '\n' ||
                curChar == '\t' ||
                curChar == '\v' ;
        }

        bool IsNumber()
        {
            return
                curChar >= '0' &&
                curChar <= '9';
        }
        
        bool IsIdLetter()
        {
            return
                curChar >= 'a' && curChar <= 'z' ||
                curChar >= 'A' && curChar <= 'Z' ||
                curChar == '_' ||
                IsNumber();
        }

        void SkipBlanks()
        {
            while (IsBlank() && curChar != '\0')
            {
                for (int i = 0; i < 6; i++)
                    highlights[i] += curChar;
                GetNextChar();
            }
        }

        string BuildNumber()
        {
            string number = "";

            while (IsNumber())
            {
                number += curChar;
                GetNextChar();

                if (curChar == '.')
                {
                    number += curChar;
                    GetNextChar();

                    while (IsNumber())
                    {
                        number += curChar;
                        GetNextChar();
                    }
                }
            }
            
            return number;
        }

        string BuildWord()
        {
            string word = "";

            while (!IsBlank() && IsIdLetter() && curChar != '\0' && curChar != '\t' && curChar != '\n')
            {
                word += curChar;
                GetNextChar();
            }

            return word;
        }

        void BuildDynamic(char flag, int highlight)
        {
            string dynamic = "" + curChar;
            GetNextChar();

            while (IsBlank())
            {
                dynamic += curChar;
                GetNextChar();
            }

            dynamic += BuildWord();
            AddHighlight(dynamic, highlight);

        }

        void BuildString(char start, int highlight)
        {
            string stringOut = "" + curChar;
            GetNextChar();

            while(curChar != start && curChar != '\0')
            {
                stringOut += curChar;
                GetNextChar();
            }

            if(curChar.Equals(start))
            {
                stringOut += curChar;
                GetNextChar();
            }

            AddHighlight(stringOut, highlight);
        }
 
        char Peek()
        {
            if (index + 1 < input.Length)
                return input[index + 1];
            else
                return '\0';
        }

        bool TestChars(char c, int highlight)
        {
            if (curChar == c)
            {
                highlights[highlight] += c;
                GetNextChar();

                for (int i = 0; i < 6; i++)
                {
                    if (i != highlight)
                        highlights[i] += ' ';
                }
                return true;
            }
            return false;
        }

        bool TestChars2(char c1, char c2, int highlight)
        {
            if (curChar == c1 && Peek() == c2)
            {
                highlights[highlight] += c1 + "" + c2;
                GetNextChar();

                for (int i = 0; i < 6; i++)
                {
                    if (i != highlight)
                        highlights[i] += "  ";
                }
                GetNextChar();
                return true;
            }
            return false;
        }

        // Highlighting loop
        while (curChar != '\0')
        {

            // Blanks
            if (IsBlank())
            {
                SkipBlanks();
                continue;
            }

            // Comments
            else if (curChar == '?')
            {
                string comment = curChar + "";
                GetNextChar();

                while (curChar != '?' && curChar != '\n' && curChar != '\0')
                {
                    comment += curChar;
                    GetNextChar();
                }

                comment += curChar; // adding last char into the comment '?' or '\n'
                GetNextChar();

                AddHighlight(comment, coms);
                continue;
            }

            // Numbers
            else if (IsNumber())
            {
                string number = BuildNumber();
                AddHighlight(number, nums);
                continue;
            }

            // Dynamic
            switch (curChar)
            {
                case '@':
                    BuildDynamic(curChar, custom);
                    continue;

                // Old highlight for console command
                //case ':':
                //    BuildDynamic(curChar, console);
                //    continue;

                case '#':
                    BuildDynamic(curChar, vars);
                    continue;

                case '\'':
                case '"':
                    BuildString(curChar, nums);
                    continue;
            }

            // New highligh for console command
            if (IsIdLetter())
                BuildDynamic(curChar, console);

            // Operators
            if (TestChars('&', oprs))
            {
                TestChars('=', oprs); // &=
                continue;
            }
            else if (TestChars('+', oprs))
            {
                TestChars('+', oprs); // ++
                TestChars('=', oprs); // +=
                continue;
            }
            else if (TestChars('-', oprs))
            {
                TestChars('-', oprs); // --
                TestChars('=', oprs); // -=
                continue;
            }
            else if (TestChars('*', oprs))
            {
                TestChars('=', oprs); // *=
                continue;
            }
            else if (TestChars('/', oprs))
            {
                TestChars('=', oprs); // /=
                continue;
            }
            else if (TestChars('/', oprs))
            {
                TestChars('=', oprs); // /=
                continue;
            }

            // Escape characters
            else if (TestChars('\\', console))
            {
                if (TestChars('n', console))
                    continue;
                else if (TestChars('t', console))
                    continue;
                else if (TestChars('\\', console))
                    continue;
                else if (TestChars('"', console))
                    continue;
                else if (TestChars('}',  console))
                    continue;

                continue;
            }

            // Markup
            else if (curChar == '$')
            {
                if
                (
                TestChars2('$', 'r', custom)  ||
                TestChars2('$', 'R', custom)  ||

                TestChars2('$', 'g', coms)    ||
                TestChars2('$', 'G', coms)    ||

                TestChars2('$', 'y', nums)    ||
                TestChars2('$', 'Y', nums)    ||

                TestChars2('$', 'b', oprs)    ||
                TestChars2('$', 'B', oprs)    ||

                TestChars2('$', 'm', console) ||
                TestChars2('$', 'M', console) ||

                TestChars2('$', 'c', vars)    ||
                TestChars2('$', 'C', vars)
                )
                    continue;

                AddEmpty(curChar + "");
                GetNextChar();
                continue;
            }

            // Other
            string other = "" + curChar;
            GetNextChar();
            AddEmpty(other);
        }

        operators.text       = highlights[oprs];
        consoleCommands.text = highlights[console];
        customCommands.text  = highlights[custom];
        varnames.text        = highlights[vars];
        numbers.text         = highlights[nums];
        comments.text        = highlights[coms];
    }
}