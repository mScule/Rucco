using System.Collections.Generic;

namespace RuccoPoyLang
{
    public class Parser
    {
        private Tokenizer tokenizer;
        private Token currentToken;

        public Parser(Tokenizer tokenizer)
        {
            this.tokenizer = tokenizer;
            currentToken = this.tokenizer.GetNextToken();
        }

        // For advancing without checkking the tokentype
        private void NextToken() =>
            currentToken = tokenizer.GetNextToken();

        // Match with specific error message
        private void Match(TokenType type, string message)
        {
            if (currentToken.type != type)
                Error.Throw(ErrorOrigin.Parser, message, currentToken.location);
            else
                currentToken = tokenizer.GetNextToken();
        }

        // Match with generic error message
        private void Match(TokenType type)
        {
            if (currentToken.type != type)
                Error.Throw(ErrorOrigin.Parser,
                    "Waited for type " + type + ". " +
                    "Given type " + currentToken.type,

                    currentToken.location
                );
            else
                currentToken = tokenizer.GetNextToken();
        }

        // Factor : ParentheseOpen Expression ParentheseClose
        //        | Addition
        //        | Subtraction
        //        | Number
        //        | Varname
        //        | CustomCommand
        //        | ConsoleCommand
        //        | ArrayItem

        private Node Factor()
        {
            Node factor;

            switch (currentToken.type)
            {
                case TokenType.ParentheseOpen:
                    NextToken();
                    factor = Expression();
                    Match(TokenType.ParentheseClose, "Waited for closing parenthese");
                    break;

                case TokenType.Addition:
                    Token addition = new Token(currentToken);
                    NextToken();

                    factor = new Node(
                        NodeType.UnaryOperation,
                        new Token(addition),
                        new Node[] { Factor() });
                    break;

                case TokenType.Subtraction:
                    Token subtraction = new Token(currentToken);
                    NextToken();

                    factor = new Node(
                        NodeType.UnaryOperation,
                        new Token(subtraction),
                        new Node[] { Factor() });
                    break;

                case TokenType.Number:
                    Token number = new Token(currentToken);
                    NextToken();

                    factor = new Node(NodeType.Value, new Token(number), null);
                    break;

                case TokenType.Varname:
                    Token varName = new Token(currentToken);
                    NextToken();

                    factor = new Node(NodeType.Value, new Token(varName), null);
                    break;

                case TokenType.CustomCommand:
                    Token customCommand = new Token(currentToken);
                    NextToken();

                    Node command = new Node(NodeType.CustomCommand, customCommand, null);
                    Node parameters = Parameters();

                    factor = new Node
                    (
                        NodeType.CommandStatement,
                        new Token(TokenType.NotDefined, null, currentToken.location),
                        new Node[] { command, parameters }
                    );
                    break;

                case TokenType.ConsoleCommand:
                    Token consoleCommand = new Token(currentToken);
                    NextToken();

                    Node cmd = new Node(NodeType.ConsoleCommand, consoleCommand, null);
                    Node param = Parameters();

                    factor = new Node
                    (
                        NodeType.CommandStatement,
                        new Token(TokenType.NotDefined, null, currentToken.location),
                        new Node[] { cmd, param }
                    );
                    break;

                default:
                    factor = Empty();
                    break;
            }
            return factor;
        }

        // ArrayAccess : Factor ("|" Factor)*
        private Node ArrayAccess()
        {
            Node factor = StringOperation();

            if (currentToken.type.Equals(TokenType.VerticalBar))
            {
                while (currentToken.type.Equals(TokenType.VerticalBar))
                {
                    Token verticalBar = currentToken;
                    NextToken();
                    factor = new Node(NodeType.ArrayAccess, verticalBar, new Node[] { factor, ArrayAccess() });
                }
                return factor;
            }
            else
                return factor;
        }

        // Term : ArrayAccess ((Multiplication|Division|Modulus) ArrayAccess)*
        private Node Term()
        {
            Node factor = Factor();

            while
            (
                currentToken.type == TokenType.Multiplication ||
                currentToken.type == TokenType.Division       ||
                currentToken.type == TokenType.Modulus
            )
            {
                Token opr = new Token(currentToken);

                NextToken();

                factor = new Node(
                    NodeType.BinaryOperation,
                    opr,
                    new Node[] { factor, Factor() });
                    break;   
            }
            return factor;
        }

        // Expression : Term ((Addition|Subtraction) Term)*
        private Node Expression()
        {
            Node term = Term();

            while
            (
                currentToken.type == TokenType.Addition ||
                currentToken.type == TokenType.Subtraction
            )
            {
                Token opr = new Token(currentToken);

                NextToken();
                term = new Node(
                    NodeType.BinaryOperation,
                    opr,
                    new Node[] { term, Term() });
                break;
            }
            return term;
        }

        // StringElement : String | Varname | Expression
        private Node StringElement()
        {
            Token stringElement = new Token(currentToken);

            switch (currentToken.type)
            {
                case TokenType.String:
                    NextToken();
                    return new Node(NodeType.Value, stringElement, null);

                default:
                    return Expression();
            }
        }

        // StringOperation : StringElement (Ampersand StringElement)*
        private Node StringOperation()
        {
            Node stringElement = StringElement();

            while (currentToken.type == TokenType.Ampersand)
            {
                Token stringOperation = new Token(currentToken);
                NextToken();

                stringElement = new Node(
                    NodeType.StringOperation,
                    stringOperation,
                    new Node[] { stringElement, StringElement() });
            }

            return stringElement;
        }

        // AssignmentStatement : Varname Assignment StringExpression
        //                     | Varname AssAdd     StringExpression
        //                     | VarName AssSub     StringExpression
        //                     | VarName AssMul     StringExpression
        //                     | VarName AssDiv     StringExpression
        //                     | VarName AssAmp     StringExpression
        //                     | VarName Increment
        //                     | VarName Decrement

        private Node AssignmentStatement()
        {
            Node varName = new Node(NodeType.Value, currentToken, null);
            Match(TokenType.Varname);
            Token assOpr = new Token(currentToken);

            switch (assOpr.type)
            {
                case TokenType.Assignment:
                case TokenType.AssAdd:
                case TokenType.AssSub:
                case TokenType.AssMul:
                case TokenType.AssDiv:
                case TokenType.AssAmp:
                    NextToken();
                    varName = new Node(
                        NodeType.AssignmentOperation,
                        assOpr,
                        new Node[] { varName, ArrayAccess() });
                    break;

                case TokenType.Increment:
                    NextToken();
                    return new Node(
                        NodeType.IncrementOperation,
                        assOpr,
                        new Node[] { varName });

                case TokenType.Decrement:
                    NextToken();
                    return new Node(
                        NodeType.DecrementOperation,
                        assOpr,
                        new Node[] { varName });

                default:
                    Error.Throw(
                        ErrorOrigin.Parser,
                        "Illegal operator " + currentToken.type + ". " +
                        "Supported assignment operators: " +
                        "++, --, +=, -=, *=, /=, &=, and =",
                        currentToken.location);
                    break;
            }
            return varName;
        }

        // BooleanStatement : StringOperation !  StringOperation
        //                  | StringOperation >  StringOperation
        //                  | StringOperation <  StringOperation
        //                  | StringOperation == StringOperation
        //                  | StringOperation >= StringOperation
        //                  | StringOperation <= StringOperation
        //                  | StringOperation != StringOperation

        private Node BooleanStatement()
        {
            Node a = ArrayAccess();

            Token comparisonOperator = currentToken;

            if 
            (
                comparisonOperator.type.Equals(TokenType.Not            ) ||
                comparisonOperator.type.Equals(TokenType.SmallerThan    ) ||
                comparisonOperator.type.Equals(TokenType.LargerThan     ) ||
                comparisonOperator.type.Equals(TokenType.SmallerOrEqual ) ||
                comparisonOperator.type.Equals(TokenType.LargerOrEqual  ) ||
                comparisonOperator.type.Equals(TokenType.Equal          ) ||
                comparisonOperator.type.Equals(TokenType.NotEqual       )      
            )
            {
                NextToken();
                Node b = ArrayAccess();
                return new Node(NodeType.BooleanStatement, comparisonOperator, new Node[] { a, b });
            }

            else
                return a;
        }

        // BooleanAnd : BooleanStatement ('&&' BooleanStatement)*
        private Node BooleanAnd()
        {
            Node stmt = BooleanStatement();

            if (stmt.type.Equals(NodeType.BooleanStatement))
            {
                while (currentToken.type.Equals(TokenType.And))
                {
                    Token and = currentToken;
                    NextToken();

                    stmt = new Node(NodeType.BooleanAnd, and, new Node[] { stmt, BooleanStatement() });
                }
                return stmt;
            }

            // If type returns as a string operation
            else
                return stmt;
        }

        // BooleanOr : BooleanAnd ('||') BooleanAnd)*
        private Node BooleanOr()
        {
            Node stmt = BooleanAnd();

            if (stmt.type.Equals(NodeType.BooleanAnd) || stmt.type.Equals(NodeType.BooleanStatement))
            {
                while (currentToken.type.Equals(TokenType.Or))
                {
                    Token or = currentToken;
                    NextToken();

                    stmt = new Node(NodeType.BooleanOr, or, new Node[] { stmt, BooleanAnd() });
                }
                return stmt;
            }
            else
                return stmt;
        }

        // Command : ConsoleCommand | CustomCommand
        private Node Command()
        {
            Token command = new Token(currentToken);

            switch (currentToken.type)
            {
                case TokenType.CustomCommand:
                    NextToken();
                    return new Node(NodeType.CustomCommand, command, null);

                case TokenType.ConsoleCommand:
                    NextToken();
                    return new Node(NodeType.ConsoleCommand, command, null);

                default:
                    Error.Throw
                    (
                        ErrorOrigin.Parser,
                        "Command statement needs to start with: a-z A-Z and _ " +
                        "(console), or @ (custom) flags",
                        currentToken.location
                    );
                    return null;
            }
        }

        // Parameter : (BooleanStatementList)*
        private Node Parameters()
        {
            List<Node> children = new List<Node> { BooleanOr() };

            while (currentToken.type == TokenType.Comma)
            {
                NextToken();
                children.Add(BooleanOr());
            }

            return new Node
            (
                NodeType.Parameters,
                new Token(TokenType.NotDefined, null, currentToken.location),
                children.ToArray()
            );
        }

        // CommandStatement : Command Parameter
        private Node CommandStatement()
        {
            Node command = Command(), parameters = Parameters();

            return new Node
            (
                NodeType.CommandStatement, 
                new Token(TokenType.NotDefined, null, currentToken.location), 
                new Node[] { command, parameters }
            );
        }

        // Empty :
        private Node Empty()
        {
            return new Node
            (
                NodeType.Empty, 
                new Token(TokenType.NotDefined, null, currentToken.location), 
                null
            );
        }

        // Statement : AssignmentStatement | CommandStatement | Empty
        private Node Statement()
        {
            switch (currentToken.type)
            {
                case TokenType.Varname:
                    return AssignmentStatement();

                case TokenType.CustomCommand:
                case TokenType.ConsoleCommand:
                    return CommandStatement();

                default:
                    return Empty();
            }
        }

        // StatementList : Statement (Semicolon Statement)*
        private Node StatementList()
        {
            List<Node> statements = new List<Node>() { Statement() };

            while (currentToken.type == TokenType.Semicolon)
            {
                NextToken();
                statements.Add(Statement());
            }

            return new Node
            (
                NodeType.StatementList, 
                new Token(TokenType.NotDefined, "", currentToken.location), 
                statements.ToArray()
            );
        }

        public Node Parse() => StatementList();
    }
}