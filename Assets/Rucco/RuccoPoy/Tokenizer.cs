namespace RuccoPoyLang
{
    public class Tokenizer
    {
        // Tokenizer
        readonly private string input;
        private int index;
        private char currentCharacter;

        // Location
        private int line, character;

        public Tokenizer(string input)
        {
            this.input = input;
            index = 0;
            currentCharacter = input[index];

            line = 1;
            character = 0;
        }

        private void GetNextCharacter()
        {
            // Character
            index++;

            if (index < input.Length)
                currentCharacter = input[index];
            else
                currentCharacter = '\0';

            // Location
            if (currentCharacter == '\n')
            {
                line++;
                character = 0;
            }
            else
                character++;
        }

        private bool Peak(char character)
        {
            if (input[index + 1] == character)
            {
                GetNextCharacter();
                return true;
            }
            else
                return false;
        }

        // Check methods
        private bool IsBlank() =>
            currentCharacter == ' '  ||
            currentCharacter == '\n' ||
            currentCharacter == '\r' ||
            currentCharacter == '\t' ||
            currentCharacter == '\v';
        
        private bool IsNumber() =>
            currentCharacter >= '0' &&
            currentCharacter <= '9';

        private bool IsAlphabet() =>
            currentCharacter >= 'A' &&
            currentCharacter <= 'Z' ||
            currentCharacter >= 'a' &&
            currentCharacter <= 'z';

        // Build/Skip Methods
        private void SkipBlanks()
        {
            while (IsBlank())
                GetNextCharacter();
        }

        private void SkipComments()
        {
            while (currentCharacter != '\n' && currentCharacter != '?' && currentCharacter != '\0')
                GetNextCharacter();
            GetNextCharacter();
        }

        private string BuildWord()
        {
            void ToCaps()
            {
                if (currentCharacter >= 'a' && currentCharacter <= 'z')
                    currentCharacter = (char)(currentCharacter - 32);
            }

            string word = "";

            SkipBlanks();

            if (IsAlphabet() || currentCharacter == '_')
            {
                ToCaps();

                word += currentCharacter;
                GetNextCharacter();

                while (IsAlphabet() || IsNumber() || currentCharacter == '_')
                {
                    ToCaps();

                    word += currentCharacter;
                    GetNextCharacter();
                }
            }

            return word;
        }

        private string BuildNumber()
        {
            string number = "";

            while (IsNumber())
            {
                number += currentCharacter;
                GetNextCharacter();

                if (currentCharacter == '.')
                {
                    number += currentCharacter;
                    GetNextCharacter();

                    while (IsNumber())
                    {
                        number += currentCharacter;
                        GetNextCharacter();
                    }
                }
            }

            return number;
        }

        private string BuildString(char stringStart)
        {
            char endChar = stringStart;

            int depth = 0; // bracket depth { 1 { 2 { 3 } } { 2 } }

            switch (stringStart)
            {
                case '"':
                case '\'':
                    break;

                case '{':
                    endChar = '}';
                    break;
            }

            string stringLiteral = "";

            while (currentCharacter != endChar || depth > 0)
            {
                if (currentCharacter == '\0')
                    Error.Throw(ErrorOrigin.Tokenizer, "String wasn't closed", new Location(line, character));

                if (currentCharacter == '\\') // Escape characters
                {
                    GetNextCharacter(); // eats '\'

                    if (currentCharacter != endChar)
                    {
                        switch (currentCharacter)
                        {
                            case 'n':
                                stringLiteral += '\n';
                                break;

                            case 't':
                                stringLiteral += '\t';
                                break;

                            case '\\':
                                stringLiteral += '\\';
                                break;

                            default:
                                Error.Throw(
                                    ErrorOrigin.Tokenizer,
                                    "Escapecharacter '" + currentCharacter + "' isn't supported",
                                    new Location(line, character));
                                break;
                        }
                    }
                    else
                        stringLiteral += endChar; 

                    GetNextCharacter(); // eats escaped character
                }
                else if (currentCharacter == '{' && endChar == '}')
                {
                    stringLiteral += currentCharacter;
                    GetNextCharacter();
                    depth++;
                }
                else if (currentCharacter == '}' && endChar == '}')
                {
                    stringLiteral += currentCharacter;
                    GetNextCharacter();
                    depth--;
                }
                else
                {
                    stringLiteral += currentCharacter;
                    GetNextCharacter();
                }
            }

            GetNextCharacter();

            return stringLiteral;
        }

        public Token GetNextToken()
        {
            while (currentCharacter != '\0')
            {
                if (IsBlank()) // Skip blanks
                {
                    SkipBlanks();
                    continue;
                }

                else if (currentCharacter.Equals('?')) // Skip comments
                {
                    GetNextCharacter();
                    SkipComments();
                    continue;
                }

                else if (IsNumber()) // Building numbers
                    return new Token(TokenType.Number, BuildNumber(), new Location(line, character));

                else if (currentCharacter.Equals('"') || currentCharacter.Equals('\'')) // Building strings
                {
                    char closing = currentCharacter;
                    GetNextCharacter();
                    return new Token(TokenType.String, BuildString(closing), new Location(line, character));
                }

                else if (currentCharacter.Equals('{')) // Building escaped strings
                {
                    GetNextCharacter();
                    return new Token(TokenType.String, BuildString('{'), new Location(line, character));
                }

                else if (currentCharacter.Equals('#')) // Varnames
                {
                    GetNextCharacter();
                    return new Token(TokenType.Varname, BuildWord(), new Location(line, character));
                }

                else if (currentCharacter.Equals('@')) // CustomCommand names
                {
                    GetNextCharacter();
                    return new Token(TokenType.CustomCommand, BuildWord(), new Location(line, character));
                }

                else if (IsAlphabet() || currentCharacter.Equals('_'))
                {
                    string word = BuildWord();

                    return word switch
                    {
                        "VALUE" => new Token(TokenType.Attribute, word, new Location(line, character)),
                        _       => new Token(TokenType.ConsoleCommand, word, new Location(line, character)),
                    };
                }

                else
                {
                    switch (currentCharacter)
                    {
                        case '*':
                            if (Peak('='))
                            {
                                GetNextCharacter();
                                return new Token(TokenType.AssMul, "*=", new Location(line, character));
                            }

                            else
                            {
                                GetNextCharacter();
                                return new Token(TokenType.Multiplication, "*", new Location(line, character));
                            }

                        case '/':
                            if (Peak('='))
                            {
                                GetNextCharacter();
                                return new Token(TokenType.AssDiv, "/=", new Location(line, character));
                            }

                            else
                            {
                                GetNextCharacter();
                                return new Token(TokenType.Division, "/", new Location(line, character));
                            }

                        case '+':
                            if (Peak('='))
                            {
                                GetNextCharacter();
                                return new Token(TokenType.AssAdd, "+=", new Location(line, character));
                            }

                            else if (Peak('+'))
                            {
                                GetNextCharacter();
                                return new Token(TokenType.Increment, "++", new Location(line, character));
                            }

                            else
                            {
                                GetNextCharacter();
                                return new Token(TokenType.Addition, "+", new Location(line, character));
                            }

                        case '-':
                            if (Peak('='))
                            {
                                GetNextCharacter();
                                return new Token(TokenType.AssSub, "-=", new Location(line, character));
                            }

                            else if (Peak('-'))
                            {
                                GetNextCharacter();
                                return new Token(TokenType.Decrement, "--", new Location(line, character));
                            }

                            else
                            {
                                GetNextCharacter();
                                return new Token(TokenType.Subtraction, "-", new Location(line, character));
                            }

                        case '&':
                            if (Peak('='))
                            {
                                GetNextCharacter();
                                return new Token(TokenType.AssAmp, "&=", new Location(line, character));
                            }

                            else if (Peak('&'))
                            {
                                GetNextCharacter();
                                return new Token(TokenType.And, "&&", new Location(line, character));
                            }

                            else
                            {
                                GetNextCharacter();
                                return new Token(TokenType.Ampersand, "&", new Location(line, character));
                            }

                        case '=':
                            if (Peak('='))
                            {
                                GetNextCharacter();
                                return new Token(TokenType.Equal, "==", new Location(line, character));
                            }
                            else
                            {
                                GetNextCharacter();
                                return new Token(TokenType.Assignment, "=", new Location(line, character));
                            }

                        case '(':
                            GetNextCharacter();
                            return new Token(TokenType.ParentheseOpen, "(", new Location(line, character));

                        case ')':
                            GetNextCharacter();
                            return new Token(TokenType.ParentheseClose, ")", new Location(line, character));

                        case '[':
                            GetNextCharacter();
                            return new Token(TokenType.SquareBracketOpen, "[", new Location(line, character));

                        case ']':
                            GetNextCharacter();
                            return new Token(TokenType.SquareBracketClose, "]", new Location(line, character));

                        case ';':
                            GetNextCharacter();
                            return new Token(TokenType.Semicolon, ";", new Location(line, character));

                        case ':':
                            GetNextCharacter();
                            return new Token(TokenType.Colon, ":", new Location(line, character));

                        case ',':
                            GetNextCharacter();
                            return new Token(TokenType.Comma, ",", new Location(line, character));

                        case '!':
                            if (Peak('='))
                            {
                                GetNextCharacter();
                                return new Token(TokenType.NotEqual, "!=", new Location(line, character));
                            }
                            else
                            {
                                GetNextCharacter();
                                return new Token(TokenType.Not, "!", new Location(line, character));
                            }

                        case '<':
                            if (Peak('='))
                            {
                                GetNextCharacter();
                                return new Token(TokenType.SmallerOrEqual, "<=", new Location(line, character));
                            }
                            else
                            {
                                GetNextCharacter();
                                return new Token(TokenType.SmallerThan, "<", new Location(line, character));
                            }

                        case '>':
                            if (Peak('='))
                            {
                                GetNextCharacter();
                                return new Token(TokenType.LargerOrEqual, ">=", new Location(line, character));
                            }
                            else
                            {
                                GetNextCharacter();
                                return new Token(TokenType.LargerThan, ">", new Location(line, character));
                            }

                        case '|':
                            if (Peak('|'))
                            {
                                GetNextCharacter();
                                return new Token(TokenType.Or, "||", new Location(line, character));
                            }
                            else
                            {
                                GetNextCharacter();
                                return new Token(TokenType.VerticalBar, "|", new Location(line, character));
                            }

                        case '%':
                            GetNextCharacter();
                            return new Token(TokenType.Modulus, "%", new Location(line, character));

                        default:
                            Error.Throw
                            (
                                ErrorOrigin.Tokenizer,
                                $"Unexpected character '{currentCharacter}'",
                                new Location(line, character)
                            );
                            break;
                    }
                }
            }
            return new Token(TokenType.EndOfFile, "", new Location(line, character));
        }
    }
}
