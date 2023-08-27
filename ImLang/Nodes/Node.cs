using ImLang.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using static ImLang.Enums;

namespace ImLang
{
    public abstract class Node { }

    public abstract class Statement : Node { }
    public abstract class LeafNode : Node { }

    /// <summary>
    /// The body of the source. Can only have function. Variable declarations in body are disabled for now
    /// </summary>
    public class BodyNode : Node
    {
        public BodyNode(IEnumerator<Token> iter)
        {
            iter.MoveNext();
            do
            {
                Functions.Add(iter.Current switch
                {
                    //{ TokenType: TokenType.VariableDeclaration } => new VariableDeclarationNode(iter),
                    { TokenType: TokenType.FunctionDeclaration } => new FunctionDeclarationNode(iter),
                    _ => throw new UnexpectedTokenException(iter.Current, TokenType.FunctionDeclaration)
                });
            }
            while (iter.MoveNext());
        }

        public List<Node> Functions { get; set; } = new List<Node>();
    }

    // Program made up of nodes
    // Nodes: Declarations and Statements
    // Declarations: Variables and Functions
    // Statements: variable assignment, function calls, if statements

    // DECLARATIONS
    // VariableDeclaration: Format is always "vartype (int|str|decimal) Identifier = Expression;"
    // FunctionDeclaration: Format is always "fn vartype (int|str|decimal|void) Identifier(arg0, arg1, ...) { Nodes[]; return Expression }"

    // STATEMENTS
    // VariableAssignment: Format is always "Identifier = Expression;"
    // FunctionCall: Format is  "Identifier(Expression);" - Note: a FunctionCall is both a statement and an expression
    // IfStatement: Format is "If (Expression) { Nodes[]; }"

    // Expression: Evaluated, either a Literal, an Identifier, a BinaryExpression, or a FunctionCall
    // Binary expression is simply a function call with two params, in the format LEFT OP RIGHT
    // FunctionCall is in the format FN Param1, Param2, ...

    /// <summary>
    /// Parse valid expressions. This is either a literal or a CallExpression
    /// An expression consists of a function and parameters
    /// Valid function are LITERAL, OPERATOR, and IDENTIFIER
    /// </summary>
    public class BinaryExpressionNode : Node
    {
        public BinaryExpressionNode(Token token)
        {
            Operator = token.Source switch
            {
                "+" => BinaryOperator.Add,
                "-" => BinaryOperator.Subtract,
                "*" => BinaryOperator.Multiply,
                "/" => BinaryOperator.Divide,
                _ => throw new NotImplementedException($"Operator {token.Source} doesnt exist at position {token.StartOffset}")
            };
        }
        public BinaryExpressionNode(Token token, Node left, Node right)
        {
            Operator = token.Source switch
            {
                "+" => BinaryOperator.Add,
                "-" => BinaryOperator.Subtract,
                "*" => BinaryOperator.Multiply,
                "/" => BinaryOperator.Divide,
                _ => throw new NotImplementedException($"Operator {token.Source} doesnt exist at position {token.StartOffset}")
            };

            Left = left;
            Right = right;
        }

        public BinaryOperator Operator;
        public Node Left { get; set; }
        public Node Right { get; set; }

        private static readonly List<TokenType> validTokens = new List<TokenType>()
        {
            TokenType.BracketOpen,
            TokenType.Identifier,
            TokenType.LiteralFloat,
            TokenType.LiteralInt,
            TokenType.LiteralString,
        };
    }

    public class VariableDeclarationStatement : Statement
    {
        /// <summary>
        /// Expect Identifier Operator Expression
        /// </summary>
        /// <param name="iter"></param>
        public VariableDeclarationStatement(IEnumerator<Token> iter, TokenGroup terminator = TokenGroup.LineSeparator)
        {
            Assert.TokenGroup(iter.Current, TokenGroup.Datatype);

            Datatype = new DatatypeNode(iter);

            iter.MoveNext();
            Identifier = new IdentifierNode(iter);

            iter.MoveNext();

            Parser.VariableDefinitions.Add(this);
            if (iter.Current.TokenGroup == terminator) return;

            Assignment = new AssignmentNode(iter);

            // Valid tokens to create expression are { '(', IdentifierNode, Literal }
            iter.MoveNext();
            Expression = NodeFactory.CreateBinaryExpressionTree(iter);

            // CreateExpressionTree will consume the semi-colon
            //iter.MoveNext();
            if (iter.Current.TokenType != TokenType.EndOfLine) throw new UnexpectedTokenException(iter.Current, TokenType.EndOfLine);
        }

        public IdentifierNode Identifier { get; set; }
        public DatatypeNode Datatype { get; set; }
        public AssignmentNode? Assignment { get; set; }
        public Node? Expression { get; set; }

        public override string ToString() => $"{Datatype} {Identifier} = {Expression}";
    }

    public class OperatorNode : Node
    {
        public OperatorNode(IEnumerator<Token> iter)
        {
        }
    }

    public class AssignmentStatement : Statement
    {
        public AssignmentStatement(IEnumerator<Token> iter)
        {
            Assert.TokenType(iter.Current, TokenType.Identifier);
            Target = new IdentifierNode(iter.Current);

            iter.MoveNext();
            Assert.TokenType(iter.Current, TokenType.Assignment);

            iter.MoveNext();
            Expression = NodeFactory.CreateBinaryExpressionTree(iter);

        }

        public IdentifierNode Target { get; set; }
        public Node Expression { get; set; }
    }

    public class AssignmentNode : Node
    {
        public AssignmentNode(IEnumerator<Token> iter)
        {
            if (iter.Current.Source != "=") throw new UnexpectedTokenException(iter.Current, TokenType.Assignment);
        }

        public override string ToString() => $"=";
    }

    public class FunctionDeclarationNode : Node
    {
        public FunctionDeclarationNode(IEnumerator<Token> iter)
        {
            Assert.TokenType(iter.Current, TokenType.FunctionDeclaration);

            iter.MoveNext();
            Identifier = new IdentifierNode(iter);

            iter.MoveNext();
            Params = new ParamDeclarationNode(iter);

            iter.MoveNext();
            Body = new StatementBlockNode(iter);

            Parser.FunctionDefinitions.Add(this);
        }

        public IdentifierNode Identifier { get; set; }
        public StatementBlockNode Body { get; set; }
        public ParamDeclarationNode Params { get; set; }

        public override string ToString() => $"{Identifier}";
    }


    // Identifier = ;
    public class VariableAssignment : Statement
    {
        public VariableAssignment(IEnumerator<Token> iter)
        {
            Identifier = new(iter);
        }

        public IdentifierNode Identifier;

        public override string ToString() => $"{Identifier}";
    }

    //public class BinaryExpressionNode : Node
    //{

    //}


    public class IdentifierNode : Node
    {
        public IdentifierNode(IEnumerator<Token> iter)
        {
            if (iter.Current.TokenType != TokenType.Identifier) throw new UnexpectedTokenException(iter.Current, TokenType.Identifier);
            Value = iter.Current.Source;
        }

        public IdentifierNode(Token token)
        {
            if (token.TokenType != TokenType.Identifier) throw new UnexpectedTokenException(token, TokenType.Identifier);
            Value = token.Source;
        }

        public string Value { get; set; } = string.Empty;

        public override string ToString() => Value;
    }

    public class DatatypeNode : Node
    {
        public DatatypeNode(IEnumerator<Token> iter)
        {
            if (iter.Current.TokenGroup != TokenGroup.Datatype) throw new UnexpectedTokenException(iter.Current, TokenGroup.Datatype);
            Datatype = iter.Current.Source switch
            {
                "int32" => Datatype.Int32,
                "Int64" => Datatype.Int64,
                "float32" => Datatype.Float32,
                "float64" => Datatype.Float64,
                _ => throw new UnexpectedTokenException(iter.Current, TokenGroup.Datatype)
            };
        }

        public Datatype Datatype { get; set; }

        public override string ToString() => Datatype.ToString();
    }

    public class RefVariableNode : Node
    {
        public IdentifierNode Identifier;

        public RefVariableNode(IEnumerator<Token> iter)
        {
            Assert.TokenType(iter.Current, TokenType.Identifier);

            Identifier = new IdentifierNode(iter.Current);
            iter.MoveNext();
        }
        public RefVariableNode(Token token)
        {
            Identifier = new IdentifierNode(token);
        }

        public override string ToString()
        {
            return Identifier.ToString();
        }
    }

    public class FunctionCallStatement : Statement
    {
        public IdentifierNode Identifier;
        public ParamCallNode Params { get; set; }

        public FunctionCallStatement(List<Token> tokens)
        {
            // Must start and end with '(' and ')' and have comma deliminated expressions in-between
            Identifier = new IdentifierNode(tokens.First());

            var iter = tokens.GetEnumerator();
            iter.MoveNext();
            iter.MoveNext();
            Params = new ParamCallNode(iter);

            //tokens.MoveNext();

        }
    }

    public class ReturnStatement : Statement
    {
        public Node Return;

        public ReturnStatement(IEnumerator<Token> iter)
        {
            iter.MoveNext();
            Return = NodeFactory.CreateBinaryExpressionTree(iter);
        }
    }

    public class LiteralNode : Node
    {
        public LiteralNode(IEnumerator<Token> iter) { }
        public LiteralNode() { }
    }

    public class StringLiteralNode : LiteralNode
    {
        public StringLiteralNode(Token token)
        {
            Value = token.Source.Replace("\"", "");
        }

        public string Value { get; set; } = string.Empty;

        public override string ToString() => Value;
    }

    public class IntLiteralNode : LiteralNode
    {
        public IntLiteralNode(Token token)
        {
            Value = int.Parse(token.Source);
        }

        public int Value { get; set; } = 0;
        public override string ToString() => Value.ToString();
    }

    public class FloatLiteralNode : LiteralNode
    {
        public FloatLiteralNode(Token token)
        {
            Value = double.Parse(token.Source);
        }

        public double Value { get; set; } = 0;
        public override string ToString() => Value.ToString();
    }

    public class StatementBlockNode : Node
    {
        public StatementBlockNode(IEnumerator<Token> iter)
        {
            Assert.TokenType(iter.Current, TokenType.CurlyBracketOpen);

            iter.MoveNext();
            // Statements can begin only with identifiers, keywords, and variable declarations

            do
            {
                Statements.Add(NodeFactory.CreateStatment(iter));
                if (!iter.MoveNext()) throw new UnexpectedTokenException(iter.Current, TokenType.CurlyBracketClose);
            }
            while (iter.Current.TokenType != TokenType.CurlyBracketClose);

        }

        public List<Statement> Statements { get; set; } = new();
    }

    public class ParamDeclarationNode : Node
    {
        public ParamDeclarationNode(IEnumerator<Token> iter)
        {
            if (iter.Current.TokenType != TokenType.BracketOpen) throw new UnexpectedTokenException(iter.Current, TokenType.BracketOpen);

            iter.MoveNext();
            if (iter.Current.TokenType == TokenType.BracketClose) return;

            do
            {
                ParamList.Add(new VariableDeclarationStatement(iter, TokenGroup.ParamListSeparator));
                if ((iter.Current.TokenType != TokenType.BracketClose) && !iter.MoveNext()) throw new UnexpectedTokenException(iter.Current, TokenType.BracketClose);
            }
            while (iter.Current.TokenType != TokenType.BracketClose);

        }

        public List<VariableDeclarationStatement> ParamList { get; set; } = new();
    }

    public class ParamCallNode : Node
    {
        public ParamCallNode(IEnumerator<Token> iter)
        {
            Assert.TokenType(iter.Current, TokenType.BracketOpen);
            iter.MoveNext();
            if (iter.Current.TokenType == TokenType.BracketClose) return;

            do
            {
                ParamList.Add(NodeFactory.CreateBinaryExpressionTree(iter, TokenGroup.ParamListSeparator));
                if ((iter.Current.TokenType != TokenType.BracketClose) && !iter.MoveNext()) throw new UnexpectedTokenException(iter.Current, TokenType.BracketClose);
            }
            while (iter.Current.TokenType != TokenType.BracketClose);

        }
        public List<Node> ParamList { get; set; } = new();
    }
}
