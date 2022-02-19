namespace RuccoPoyLang
{
    public enum TokenType
    {
        // data
        Number, String, Varname,

        // keywords used by console commands
        Attribute,

        // +, -
        Addition, Subtraction,

        // *, /, %
        Multiplication, Division, Modulus,

        Assignment, // for assignment        (=)
        Ampersand,  // for string operations (&)

        // +=, -=, *=, /=, &=
        AssAdd, AssSub, AssMul, AssDiv, AssAmp,

        // ++, --
        Increment, Decrement,

        // (, )
        ParentheseOpen, ParentheseClose,

        // [, ]
        SquareBracketOpen, SquareBracketClose,

        // :, ;
        Colon, Semicolon,

        // @
        CustomCommand, ConsoleCommand,

        // ,
        Comma,

        // !, <, >, ==, <=, >=, !=
        Not, SmallerThan, LargerThan, Equal, SmallerOrEqual, LargerOrEqual, NotEqual,

        // &&, ||
        And, Or,

        // |
        VerticalBar,

        NotDefined, // for tokens that is created in parser
        EndOfFile
    }

    public class Token
    {
        public TokenType type { get; }
        public string value { get; }

        public Location location { get; }

        public Token(TokenType type, string value, Location location)
        {
            this.type = type;
            this.value = value;
            this.location = location;
        }

        public Token(Token token)
        {
            this.type = token.type;
            this.value = token.value;
            this.location = token.location;
        }

        public override string ToString() =>
            Print.Item("Token", Print.SubContent(new string[] { Print.Value("Type", type.ToString()), Print.Value("Value", value) }));
        
    }
}
