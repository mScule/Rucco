using System.Collections.Generic;

namespace RuccoPoyLang
{
    public class Interpreter
    {
        readonly private Parser parser;
        readonly private Node tree;

        private Dictionary<string, Variable> varList;
        readonly private Dictionary<string, CustomCommand> customCommands;

        readonly private string
            docPath = @"Assets\Rucco\RuccoPoy\Documentation\",
            libPath = @"Assets\Rucco\Libraries\";

        private string
            mainOut, // Main output string, that output shows
            scndOut; // For output that is loaded into the input field

        public Interpreter
        (
            Parser parser,
            Dictionary<string, Variable> varList,
            Dictionary<string, CustomCommand> customCommands
        )
        {
            this.parser = parser;
            tree = this.parser.Parse();

            this.varList = varList;
            this.customCommands = customCommands;
        }

        // Helper methods
        private float Float(Variable number, Location location)
        {
            float result = 0.0f;

            bool isFloat =
                number.type == VariableType.Float &&
                float.TryParse(number.value, out result);

            if (isFloat)
                return result;

            else
                Error.Throw
                (
                    ErrorOrigin.Interpreter,
                    "variable type needs to be a float",
                    location
                );

            return 0.0f;
        }

        private float Float(string number, Location location)
        {
            if (float.TryParse(number, out float result))
                return result;

            else
                Error.Throw
                (
                    ErrorOrigin.Interpreter,
                    "value needs to be a float",
                    location
                );

            return 0.0f;
        }

        private bool IsNumber(string input)
        {
            if (input != null)
            {
                foreach (char c in input)
                {
                    if (c == '.' || c >= '0' && c <= '9')
                        continue;
                    else
                        return false;
                }
                return true;
            }
            return false;
        }

        private Variable TryGetVariable(string name, Location location)
        {
            if (varList.ContainsKey(name))
                return varList[name];
            else
                Error.Throw
                (
                    ErrorOrigin.Interpreter,
                    $"Variable \"{name}\" doesn't exist",
                    location
                );
            return null;
        }

        // Visiting methods
        private string StatementList(Node node)
        {
            string outputStream = "";

            foreach (Node child in node.children)
                outputStream += Visit(child);

            return outputStream;
        }

        private string Value(Node node)
        {
            switch (node.content.type)
            {
                case TokenType.Number:
                case TokenType.String:
                    return node.content.value;

                case TokenType.Varname:
                    return TryGetVariable(node.content.value, node.content.location).value;
            }
            return "";
        }

        private void DemandParamAmt(Node node, int amt)
        {
            if (node.children.Length < amt)
                Error.Throw
                (
                    ErrorOrigin.Interpreter,
                    $"Command demands {amt} parameters, but {node.children.Length} was given",
                    node.content.location
                );
        }

        private string StringOperation(Node node) =>
            Visit(node.children[0]) + Visit(node.children[1]);

        private string BinaryOperation(Node node) => node.content.type switch
        {
            TokenType.Addition =>
                Float(Visit(node.children[0]), node.children[0].content.location)
                +
                Float(Visit(node.children[1]), node.children[1].content.location)
                + string.Empty,

            TokenType.Subtraction =>
                Float(Visit(node.children[0]), node.children[0].content.location)
                -
                Float(Visit(node.children[1]), node.children[1].content.location)
                + string.Empty,

            TokenType.Multiplication =>
                Float(Visit(node.children[0]), node.children[0].content.location)
                *
                Float(Visit(node.children[1]), node.children[1].content.location)
                + string.Empty,

            TokenType.Division =>
                 Float(Visit(node.children[0]), node.children[0].content.location)
                 /
                 Float(Visit(node.children[1]), node.children[1].content.location)
                 + string.Empty,

            _ => "",
        };

        private string UnaryOperation(Node node) => node.content.type switch
        {
            TokenType.Addition =>
                +Float(Visit(node.children[0]), node.children[0].content.location) + "",

            TokenType.Subtraction =>
                -Float(Visit(node.children[0]), node.children[0].content.location) + "",

            _ => "",
        };

        private string CommandStatement(Node node)
        {
            // children 0 is the command

            switch (node.children[0].type)
            {
                case NodeType.ConsoleCommand:

                    switch (node.children[0].content.value)
                    {
                        // Input output
                        case "OUT":
                            return Out(node);
                        case "LIBRARY":
                            return Library(node);
                        case "EDIT":
                            return Edit(node);

                        // Controlflow
                        case "WHEN":
                            return When(node);
                        case "CHAIN":
                            return "Not implemented yet";

                        // Code evaluation and functions
                        case "RUN":
                            return Run(node);
                        case "TASK":
                            return Task(node);

                        // Loops
                        case "TIMES":
                            return Times(node);
                        case "STEP":
                            return Step(node);
                        case "FOR_EACH":
                            return ForEach(node);

                        // Datahandilng
                        case "INTEGER":
                            return Integer(node);
                        case "INJECT":
                            return Inject(node);

                        // Deletion
                        case "DELETE":
                            return Delete(node);
                        case "DELETE_ALL":
                            return DeleteAll();
                        case "REMOVE_BY_KEY":
                            return RemoveByKey(node);
                        case "REMOVE_BY_VALUE":
                            return RemoveByValue(node);
                        case "REMOVE_BY_INDEX":
                            return RemoveByIndex(node);

                        // Listing commands
                        case "LIST_VARIABLES":
                            return ListVariables();
                        case "LIST_LIBRARIES":
                            return ListLibraries();
                        case "LIST_CONSOLE_COMMANDS":
                            return ListConsoleCommands();
                        case "LIST_CUSTOM_COMMANDS":
                            return ListCustomCommands();

                        // Help and documentation
                        case "HELP":
                            return Help();
                        case "DOC_GENERAL":
                            return DocGeneral();
                        case "DOC_COMMANDS":
                            return DocCommands();
                        case "DOC_CONSOLE_COMMANDS":
                            return DocConsoleCommands();
                        case "DOC_CUSTOM_COMMANDS":
                            return DocCustomCommands();
                        case "DOC_VARIABLES":
                            return DocVariables();
                        case "DOC_DATATYPES":
                            return DocDatatypes();
                        case "DOC_OPERATIONS":
                            return DocOperations();
                        case "DOC_OUTPUT":
                            return DocOutput();
                        case "DOC_BOOLEANS":
                            return DocBooleans();
                        case "DOC_ARRAYS":
                            return DocArrays();

                        default:
                            Error.Throw(
                                ErrorOrigin.Interpreter,
                                "Console command " + node.children[0].content.value + " is not supported, " +
                                "see list of supported commands with :list_console_commands;", node.content.location);
                            break;
                    }
                    break;

                case NodeType.CustomCommand:
                    return CustomCommand(node);
            }

            return "";
        }

        private void AssignmentStatement(Node node)
        {
            // children 0 is the identifier, and children 1 is the value

            // Number assignment statements
            void NumberAssignment() // =
            {
                VariableType type;
                string givenValue = Visit(node.children[1]);

                if (IsNumber(givenValue))
                    type = VariableType.Float;
                else
                    type = VariableType.String;

                varList[node.children[0].content.value].value = givenValue;
                varList[node.children[0].content.value].type = type;
            }

            void NumberAssignmentAmpersand() // &=
            {
                varList[node.children[0].content.value].value =
                    varList[node.children[0].content.value].value + Visit(node.children[1]);

                varList[node.children[0].content.value].type = VariableType.String;
            }

            void NumberAssignmentAddition() // +=
            {
                varList[node.children[0].content.value].value =
                    Float(varList[node.children[0].content.value].value, node.children[0].content.location)
                    +
                    Float(Visit(node.children[1]), node.children[1].content.location)
                    + string.Empty;
            }

            void NumberAssignmentSubtraction() // -=
            {
                varList[node.children[0].content.value].value =
                    Float(varList[node.children[0].content.value].value, node.children[0].content.location)
                    -
                    Float(Visit(node.children[1]), node.children[1].content.location)
                    + string.Empty;
            }

            void NumberAssignmentMultiplication() // *=
            {
                varList[node.children[0].content.value].value =
                    Float(varList[node.children[0].content.value].value, node.children[0].content.location)
                    *
                    Float(Visit(node.children[1]), node.children[1].content.location)
                    + string.Empty;
            }

            void NumberAssignmentDivision() // /= 
            {
                varList[node.children[0].content.value].value =
                    Float(varList[node.children[0].content.value].value, node.children[0].content.location)
                    /
                    Float(Visit(node.children[1]), node.children[1].content.location)
                    + string.Empty;
            }

            // String assignment statements

            void StringAssignment() // =
            {
                VariableType type;
                string givenValue = Visit(node.children[1]);

                if (IsNumber(givenValue))
                    type = VariableType.Float;
                else
                    type = VariableType.String;

                varList[node.children[0].content.value].value = givenValue;
                varList[node.children[0].content.value].type = type;
            }

            void StringAssignmentAmpersand() // &=
            {
                varList[node.children[0].content.value].value =
                    varList[node.children[0].content.value].value + Visit(node.children[1]);

                varList[node.children[0].content.value].type = VariableType.String;
            }

            // If variable already exists
            if (varList.ContainsKey(node.children[0].content.value))
            {
                switch (varList[node.children[0].content.value].type)
                {
                    case VariableType.Float:

                        switch (node.content.type)
                        {
                            case TokenType.Assignment:
                                NumberAssignment();
                                break;

                            case TokenType.AssAmp:
                                NumberAssignmentAmpersand();
                                break;

                            case TokenType.AssAdd:
                                NumberAssignmentAddition();
                                break;

                            case TokenType.AssSub:
                                NumberAssignmentSubtraction();
                                break;

                            case TokenType.AssMul:
                                NumberAssignmentMultiplication();
                                break;

                            case TokenType.AssDiv:
                                NumberAssignmentDivision();
                                break;
                        }
                        break;

                    case VariableType.String:

                        switch (node.content.type)
                        {
                            case TokenType.Assignment:
                                StringAssignment();
                                break;

                            case TokenType.AssAmp:
                                StringAssignmentAmpersand();
                                break;
                        }
                        break;
                }
            }

            else // Declares new variable
            {
                switch (node.content.type)
                {
                    case TokenType.Assignment:
                        VariableType type;

                        string givenValue = Visit(node.children[1]);

                        if (IsNumber(givenValue))
                            type = VariableType.Float;
                        else
                            type = VariableType.String;

                        varList.Add
                            (node.children[0].content.value,
                            new Variable(type, givenValue));

                        break;

                    default:
                        Error.Throw
                            (ErrorOrigin.Interpreter,
                            "You must init the variable with some value" +
                            "in order to use other types of assignment operators",
                            node.content.location);
                        break;
                }
            }
        }

        private void IncrementOperation(Node node)
        {
            // children 0 is identifier

            TryGetVariable(node.children[0].content.value, node.children[0].content.location).value =
                Float(varList[node.children[0].content.value], node.children[0].content.location)
                + 1 + string.Empty;
        }

        private void DecrementOperation(Node node)
        {
            // children 0 is identifier

            TryGetVariable(node.children[0].content.value, node.children[0].content.location).value =
                Float(varList[node.children[0].content.value], node.children[0].content.location)
                - 1 + string.Empty;
        }

        private string BooleanStatement(Node node)
        {
            string
                a = Visit(node.children[0]),
                b = Visit(node.children[1]);

            switch (node.content.type)
            {
                case TokenType.SmallerThan:
                    if (Float(a, node.content.location) < Float(b, node.content.location))
                        return "1";
                    else
                        return "0";

                case TokenType.LargerThan:
                    if (Float(a, node.content.location) > Float(b, node.content.location))
                        return "1";
                    else
                        return "0";

                case TokenType.SmallerOrEqual:
                    if (Float(a, node.content.location) <= Float(b, node.content.location))
                        return "1";
                    else
                        return "0";

                case TokenType.LargerOrEqual:
                    if (Float(a, node.content.location) >= Float(b, node.content.location))
                        return "1";
                    else
                        return "0";

                case TokenType.Equal:
                    if (a == b)
                        return "1";
                    else
                        return "0";

                case TokenType.NotEqual:
                    if (a != b)
                        return "1";
                    else
                        return "0";
            }

            return null;
        }

        private string BooleanAnd(Node node)
        {
            string
                a = Visit(node.children[0]),
                b = Visit(node.children[1]);

            if (a == "1" && b == "1") return "1";
            else return "0";
        }

        private string BooleanOr(Node node)
        {
            string
                a = Visit(node.children[0]),
                b = Visit(node.children[1]);

            if (a == "1" || b == "1") return "1";
            else return "0";
        }

        // Console commands (Core commands)
        private string When(Node node)
        {
            DemandParamAmt(node.children[1], 2);

            string boolean  = Visit(node.children[1].children[0]);
            string mainCode = Visit(node.children[1].children[1]);
            string secondaryCode = "";

            if (node.children[1].children.Length > 2)
                secondaryCode = Visit(node.children[1].children[2]);

            // Condition is true. Run given code
            if (boolean.Equals("1"))
            {
                Tokenizer tokenizer = new Tokenizer(mainCode);
                Parser parser = new Parser(tokenizer);
                Interpreter interpreter = new Interpreter(parser, varList, customCommands);

                mainOut += interpreter.Interprete()[RuccoPoyOuts.mainOut];
            }

            // Condition is false. Run secondary code if it exists
            else if (boolean.Equals("0") && secondaryCode != "")
            {
                Tokenizer tokenizer = new Tokenizer(secondaryCode);
                Parser parser = new Parser(tokenizer);
                Interpreter interpreter = new Interpreter(parser, varList, customCommands);

                mainOut += interpreter.Interprete()[RuccoPoyOuts.mainOut];
            }
            return "";
        }

        private string Run(Node node)
        {
            int parameters = 1;

            string runOut = "";

            foreach (Node parameter in node.children[parameters].children)
            {
                string input = Visit(parameter);

                Tokenizer tokenizer = new Tokenizer(input);
                Parser parser = new Parser(tokenizer);
                Interpreter interpreter = new Interpreter(parser, varList, customCommands);

                runOut += interpreter.Interprete()[RuccoPoyOuts.mainOut];
            }

            mainOut += runOut;
            return runOut;
        }

        // Step #i, From, To, Step, {};
        private string Step(Node node)
        {
            DemandParamAmt(node.children[1], 5);

            string
                indexName = node.children[1].children[0].content.value,
                codeBlock = Visit(node.children[1].children[4]);

            float
                startValue = Float(Visit(node.children[1].children[1]), node.children[1].children[1].content.location),
                endValue   = Float(Visit(node.children[1].children[2]), node.children[1].children[2].content.location),
                step       = Float(Visit(node.children[1].children[3]), node.children[1].children[3].content.location);

            string Evaluate(float index)
            {

                // Adding index to the varlist
                if (!varList.ContainsKey(indexName))
                    varList.Add(indexName, new Variable(VariableType.Float, index + ""));

                // Updating the value of the index
                else
                    varList[indexName].value = index + "";

                // Interpreting
                Tokenizer tokenizer     = new Tokenizer(codeBlock);
                Parser parser           = new Parser(tokenizer);
                Interpreter interpreter = new Interpreter(parser, varList, customCommands);

                return interpreter.Interprete()[RuccoPoyOuts.mainOut];
            }

            if (step.Equals(0))
            {
                mainOut += Evaluate(startValue);
                return string.Empty;
            }    

            // Turns the step around so the for loop won't jam
            if (step < 0)
                step -= step * 2;

            if (startValue < endValue)
                for (float i = startValue; i < endValue; i += step)
                    mainOut += Evaluate(i);

            else if (startValue > endValue)
                for (float i = startValue; i > endValue; i -= step)
                    mainOut += Evaluate(i);

            return string.Empty;
        }

        private string Times(Node node)
        {
            DemandParamAmt(node.children[1], 2);

            int parameters = 1;

            int times = (int)

                Float
                (
                    Visit(node.children[parameters].children[0]),
                    node.children[parameters].children[0].content.location
                );

            string loopOutput = "";
            string loopInput = Visit(node.children[parameters].children[1]);

            if (times < 0)
                times -= -times;

            while (times > 0)
            {
                Tokenizer tokenizer     = new Tokenizer(loopInput);
                Parser parser           = new Parser(tokenizer);
                Interpreter interpreter = new Interpreter(parser, varList, customCommands);

                loopOutput += interpreter.Interprete()[RuccoPoyOuts.mainOut];
                times--;
            }
            mainOut += loopOutput;
            return loopOutput;
        }

        private string Out(Node node)
        {
            foreach (Node parameter in node.children[1].children)
                mainOut += Visit(parameter);
            return string.Empty;
        }

        private string Edit(Node node)
        {
            foreach (Node parameter in node.children[1].children)
            {
                if(varList.ContainsKey(parameter.content.value))
                {
                    switch(varList[parameter.content.value].type)
                    {
                        case VariableType.Float:
                            scndOut += $"\n#{parameter.content.value} = {varList[parameter.content.value].value};";
                            break;

                        case VariableType.String:
                            scndOut += $"\n#{parameter.content.value} = {{{varList[parameter.content.value].value}}};";
                            break;
                    }
                }
            }
            return string.Empty;
        }

        private string Library(Node node)
        {
            int parameters = 1;

            string files = "";

            int i = 0;

            try
            {
                foreach (Node path in node.children[parameters].children)
                {
                    string[] file = System.IO.File.ReadAllLines(libPath + path.content.value);

                    foreach (string line in file)
                        files += line + '\n';

                    i++;
                }
            }

            catch
            {
                Error.Throw
                (
                    ErrorOrigin.Interpreter,
                    "path " + node.children[parameters].children[i].content.value + " doesn't exist",
                    node.children[parameters].children[i].content.location
                );
            }

            return files;
        }

        private string Task(Node node)
        {
            DemandParamAmt(node.children[1], 1);

            int parameters = 1;

            string
                givenTask = Visit(node.children[parameters].children[0]),
                subProcess = "";

            int i = 1;

            // Finding value keywords
            Tokenizer checkForValues = new Tokenizer(givenTask);
            Token token = checkForValues.GetNextToken();

            while(token.type != TokenType.EndOfFile)
            {
                if (token.type.Equals(TokenType.Attribute) && token.value.Equals("VALUE"))
                {
                    DemandParamAmt(node.children[parameters], i + 1);
                    subProcess += '{' + Visit(node.children[parameters].children[i]) + '}';
                    i++;
                }
                else if (token.type.Equals(TokenType.CustomCommand))
                    subProcess += '@' + token.value + ' ';
                else if (token.type.Equals(TokenType.Varname))
                    subProcess += '#' + token.value + ' ';
                else if (token.type.Equals(TokenType.String))
                    subProcess += '{' + token.value + '}';
                else
                    subProcess += token.value + ' ';

                token = checkForValues.GetNextToken();
            }

            // Interpreting task
            Dictionary<string, Variable> closure = new Dictionary<string, Variable>(varList);

            Tokenizer tokenizer = new Tokenizer(subProcess);
            Parser parser = new Parser(tokenizer);
            Interpreter interpreter = new Interpreter(parser, closure, customCommands);

            return interpreter.Interprete()[RuccoPoyOuts.mainOut];
        }

        private string Integer(Node node)
        {
            DemandParamAmt(node.children[1], 1);
            return (int)Float(Visit(node.children[1].children[0]), node.content.location) + "";
        }

        private string Inject(Node node)
        {
            DemandParamAmt(node.children[1], 1);

            string
                injectable = Visit (node.children[1].children[0]),

                injected = "",
                varName = "";

            bool injection = false;

            foreach (char c in injectable)
            {
                sequential:
                
                if (c.Equals('#') && !injection)
                {
                    injection = true;
                    continue;
                }

                else if (injection)
                {
                    if
                    (
                        c.Equals('_') ||
                        c >= 'A' && c <= 'Z' ||
                        c >= 'a' && c <= 'z' ||
                        c >= '0' && c <= '9'
                    )
                        varName += char.ToUpper(c);

                    else
                    {
                        injected += TryGetVariable
                        (
                            varName,
                            node.children[1].children[0].content.location
                        ).value;

                        varName = "";
                        injection = false;

                        if(c.Equals('#'))
                            goto sequential;
                        else
                            injected += c;
                    }
                    continue;
                }

                else injected += c;
            }

            return injected;
        }

        private string Delete(Node node)
        {
            int parameters = 1;

            foreach (Node variable in node.children[parameters].children)
                varList.Remove(variable.content.value);

            return string.Empty;
        }

        private string DeleteAll()
        {
            List<string> varNames = new List<string>();

            foreach (KeyValuePair<string, Variable> variable in varList)
                varNames.Add(variable.Key);

            foreach (string varName in varNames)
                varList.Remove(varName);

            return string.Empty;
        }

        // Array functions

        private ArrayItem[] Array(string rawArray)
        {
            List<ArrayItem> array = new List<ArrayItem>();

            string firstValue, secondValue;

            Tokenizer tokenizer = new Tokenizer(rawArray);
            Token currentToken = tokenizer.GetNextToken();

            void GetNext() =>
                currentToken = tokenizer.GetNextToken();

            void Match(TokenType type)
            {
                if (currentToken.type.Equals(type))
                {
                    GetNext();
                    return;
                }

                Error.Throw(
                    ErrorOrigin.Interpreter,
                    $"Unexpected tokentype in array {currentToken.type}. Waited for {type}"
                );
            }

            while (currentToken.type != TokenType.EndOfFile)
            {
                Match(TokenType.SquareBracketOpen);
                firstValue = currentToken.value;
                GetNext();

                if (currentToken.type == TokenType.Colon)
                {
                    GetNext();
                    secondValue = currentToken.value;
                    GetNext();
                    Match(TokenType.SquareBracketClose);
                    array.Add(new ArrayItem(firstValue, secondValue));
                }
                else
                {
                    Match(TokenType.SquareBracketClose);
                    array.Add(new ArrayItem(null, firstValue));
                }
            }

            return array.ToArray();
        }

        private string ArrayAccess(Node node)
        {
            List<string> accesses = new List<string>();

            void StrippedVisit(Node child)
            {
                if (child.type.Equals(NodeType.ArrayAccess))
                {
                    foreach (Node child2 in child.children)
                        StrippedVisit(child2);
                }
                else
                    accesses.Add(Visit(child));
            }

            StrippedVisit(node);

            string arrayItem = accesses[0];

            for(int i = 1; i < accesses.Count; i++)
            {
                bool isIndex = int.TryParse(accesses[i], out int index);

                if(isIndex)
                    arrayItem = Index(arrayItem, index);

                else
                    arrayItem = Key(arrayItem, accesses[i]);
            }

            return arrayItem;
        }

        //remove_key #array_name, "key1", "key2", "key3" ...;
        private string RemoveByKey(Node node)
        {
            string rawArray = Visit(node.children[1].children[0]);

            List<string> removableKeys = new List<string>();

            for (int i = 1; i < node.children[1].children.Length; i++)
                removableKeys.Add(Visit(node.children[1].children[i]));

            ArrayItem[] arrayItems = Array(rawArray);

            string newArray = "";

            foreach (ArrayItem item in arrayItems)
                if (!removableKeys.Contains(item.key))
                    newArray += item.ToString();

            return newArray;
        }

        // Basically almost identical with "RemoveByKey", Maybe these
        // will be combined somehow
        private string RemoveByValue(Node node)
        {
            string rawArray = Visit(node.children[1].children[0]);

            List<string> removableKeys = new List<string>();

            for (int i = 1; i < node.children[1].children.Length; i++)
                removableKeys.Add(Visit(node.children[1].children[i]));

            ArrayItem[] arrayItems = Array(rawArray);

            string newArray = "";

            foreach (ArrayItem item in arrayItems)
                if (!removableKeys.Contains(item.value))
                    newArray += item.ToString();

            return newArray;
        }

        private string RemoveByIndex(Node node)
        {
            string rawArray = Visit(node.children[1].children[0]);
            ArrayItem[] array = Array(rawArray);
            string newArray = "";

            List<string> removableKeys = new List<string>();

            // Adding every parameter for removableKeys
            for (int i = 1; i < node.children[1].children.Length; i++)
                removableKeys.Add(Visit(node.children[1].children[i]));

            List<int> removableIndexes = new List<int>();

            foreach(string removableKey in removableKeys)
            {
                bool isIndex;
                isIndex = int.TryParse(removableKey, out int index);

                if (isIndex)
                    removableIndexes.Add(index);
            }

            for (int i = 0; i < array.Length; i++)
            {
                bool found = false;

                foreach (int index in removableIndexes)
                    if (i.Equals(index))
                    {
                        found = true;
                        break;
                    }

                if (!found)
                    newArray += array[i].ToString();
            }

            return newArray;
        }

        //item #array_name, index or "key"
        private string Index(string rawArray, int index)
        {
            ArrayItem[] array = Array(rawArray);

            if (index <= array.Length - 1)
                return array[index].value;

            Error.Throw(
                ErrorOrigin.Interpreter,
                $"Array {rawArray} doesn't contain index {index}");
            return null;
        }

        private string Key(string rawArray, string key)
        {
            ArrayItem[] array = Array(rawArray);

            foreach (ArrayItem item in array)
                if (key.Equals(item.key))
                    return item.value;

            Error.Throw(
                ErrorOrigin.Interpreter,
                $"Array {rawArray} doesn't contain key {key}");
            return null;
        }

        private string ForEach(Node node)
        {
            DemandParamAmt(node.children[1], 3);

            ArrayItem[] array = Array(Visit(node.children[1].children[0]));

            string
                varName = node.children[1].children[1].content.value,
                code = Visit(node.children[1].children[2]);

            foreach(ArrayItem item in array)
            {
                // Copying current varlist to temporary one
                Dictionary<string, Variable> tempVarList = new Dictionary<string, Variable>(varList){};

                // Current item is added to the varlist
                tempVarList.Add(varName, new Variable(VariableType.String, item.value));

                // Interpreting code
                Tokenizer tokenizer = new Tokenizer(code);
                Parser parser = new Parser(tokenizer);
                Interpreter interpreter = new Interpreter(parser, tempVarList, customCommands);

                // Output
                mainOut += interpreter.Interprete()[RuccoPoyOuts.mainOut];
            }

            return string.Empty;
        }

        // Console commands (Listing commands)
        private string ListVariables()
        {
            string variableList = "";

            if (varList.Count != 0)
                foreach (KeyValuePair<string, Variable> variable in varList)
                    variableList += variable.Key + " | " + variable.Value.type.ToString() +
                        " (" + variable.Value.value + ")\n\n";
            else
                variableList = "empty";

            return variableList;
        }

        private string ListLibraries()
        {
            string output = "";

            string[] libs = System.IO.Directory.GetFileSystemEntries(libPath, "*.rp");
            output += "\n**LIBRARIES**\n\n";

            foreach (string lib in libs)
                output += lib.Remove(0, libPath.Length) + "\n\n";

            return output;
        }

        private string ListCustomCommands()
        {
            string customCommandList = "\n**LIST OF CUSTOM COMMANDS**\n\n";

            if (customCommands.Count > 0)
                foreach (KeyValuePair<string, CustomCommand> customCommand in customCommands)
                    customCommandList += "$R*$W " + customCommand.Key + "\n\n";
            else
                customCommandList += "No avaliable custom commands";

            return customCommandList;
        }

        private string ListConsoleCommands() =>
            ReadFile(@"list_console_commands.txt");

        // Console commands (Help and documetation commands)
        private string DocCustomCommands()
        {
            string customCommandDoc = "**CUSTOM COMMANDS**\n\n";

            if (customCommands.Count != 0)
                foreach (KeyValuePair<string, CustomCommand> command in customCommands)
                    customCommandDoc += $"{command.Key} {command.Value.Description}\n\n";
            else
                customCommandDoc += "No avaliable custom commands\n\n";

            return customCommandDoc;
        }

        private string DocConsoleCommands() =>
            ReadFile(@"console_commands.txt");

        private string Help() =>
            ReadFile(@"help.txt");

        private string DocGeneral() =>
            ReadFile(@"general.txt");

        private string DocCommands() =>
            ReadFile(@"commands.txt");

        private string DocVariables() =>
            ReadFile(@"variables.txt");

        private string DocDatatypes() =>
            ReadFile(@"datatypes.txt");

        private string DocOperations() =>
            ReadFile(@"operations.txt");

        private string DocOutput() =>
            ReadFile(@"output.txt");

        private string DocBooleans() =>
            ReadFile(@"booleans.txt");

        private string DocArrays() =>
            ReadFile(@"arrays.txt");

        // Custom command related
        private string CustomCommand(Node node)
        {
            int command = 0, parameters = 1;

            // CustomCommand List contains command with given key
            if (customCommands.ContainsKey(node.children[command].content.value))
            {
                List<string> visitedChildren = new List<string>();

                // Parameters given
                if (node.children[parameters] != null)
                {
                    foreach (Node child in node.children[parameters].children)
                        visitedChildren.Add(Visit(child));

                    return customCommands[node.children[command].content.value].Command(visitedChildren.ToArray());
                }
            }

            // CustomCommand List doesn't contain command with given key
            else
                Error.Throw(
                    ErrorOrigin.Interpreter,
                    "Custom command " + node.children[command].content.value + " is not supported, " +
                    "see list of supported custom commands with :list_custom_commands;", node.content.location);

            return "";
        }

        private string ReadFile(string path) =>
            System.IO.File.ReadAllText(docPath + path);

        // Reads through AST
        private string Visit(Node node)
        {
            switch (node.type)
            {
                case NodeType.Value:
                    return Value(node);

                case NodeType.CommandStatement:
                    return CommandStatement(node);

                case NodeType.AssignmentOperation:
                    AssignmentStatement(node);
                    break;

                case NodeType.BinaryOperation:
                    return BinaryOperation(node);

                case NodeType.UnaryOperation:
                    return UnaryOperation(node);

                case NodeType.StringOperation:
                    return StringOperation(node);

                case NodeType.IncrementOperation:
                    IncrementOperation(node);
                    break;

                case NodeType.DecrementOperation:
                    DecrementOperation(node);
                    break;

                case NodeType.ArrayAccess:
                    return ArrayAccess(node);

                case NodeType.BooleanStatement:
                    return BooleanStatement(node);

                case NodeType.BooleanAnd:
                    return BooleanAnd(node);

                case NodeType.BooleanOr:
                    return BooleanOr(node);

                case NodeType.StatementList:
                    return StatementList(node);

                case NodeType.Empty:
                    return "";
            }
            return null;
        }

        public string[] Interprete()
        {
            Visit(tree);
            return new string[] { mainOut, scndOut };
        }
    }
}