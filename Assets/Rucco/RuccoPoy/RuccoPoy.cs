using System;
using System.Collections.Generic;

namespace RuccoPoyLang
{
    public static class RuccoPoyOuts
    {
        public const int
            mainOut      = 0,
            secondaryOut = 1;
    }

    public class RuccoPoy
    {
        private Dictionary<string, CustomCommand> customCommands;
        private static Dictionary<string, Variable> varList;

        private string[] output;
        public  string[] Output { get => output; }

        public RuccoPoy(Dictionary<string, CustomCommand> customCommands)
        {
            this.customCommands = customCommands;
            varList = new Dictionary<string, Variable>();
        }

        public void Interprete(string input)
        {
            try
            {
                Tokenizer   tokenizer   = new Tokenizer(input);
                Parser      parser      = new Parser(tokenizer);
                Interpreter interpreter = new Interpreter(parser, varList, customCommands);

                output = interpreter.Interprete();
            }

            catch (Exception e) 
            {
                output = new string[] { $"{e.Message}" };
            }
        }
    }
}
