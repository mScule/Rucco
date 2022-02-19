using System.Collections.Generic;

namespace RuccoPoyLang
{
    public enum NodeType
    {
        Empty, StatementList,

        Value, // Float, String, or Varname

        // Command related
        CommandStatement, ConsoleCommand, CustomCommand, Parameters,

        // Operations
        AssignmentOperation, BinaryOperation, UnaryOperation, StringOperation,
        IncrementOperation, DecrementOperation, ArrayAccess,

        BooleanStatement, BooleanAnd, BooleanOr,
    }

    public class Node
    {
        public NodeType type   { get; }
        public Token content   { get; }
        public Node[] children { get; }

        public Node(NodeType type, Token content, Node[] children)
        {
            this.type     = type;
            this.content  = content;
            this.children = children;
        }

        public override string ToString()
        {
            List<string> stringChildren = new List<string>();

            if (children != null)
                foreach (Node child in children)
                    stringChildren.Add(Print.Value("child", child.type.ToString()));
            else
                stringChildren.Add("no children");

            return Print.Item("Node",
                Print.SubContent(new string[] {
                    Print.Value("Type", type.ToString()),
                    Print.Value("Content", content.ToString()),
                    Print.Value("Children", Print.SubContent(stringChildren.ToArray()))
                })
            );
        }
    }
}