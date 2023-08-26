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
    public abstract class Node
    {
        public Node() { }
        public Node(IEnumerator<Token> iter)
        {
            //iter.MoveNext();
            //Iter = iter;
        }
    }

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
                functions.Add(iter.Current switch
                {
                    //{ TokenType: TokenType.VariableDeclaration } => new VariableDeclarationNode(iter),
                    { TokenType: TokenType.FunctionDeclaration } => new FunctionDeclarationNode(iter),
                    _ => throw new UnexpectedTokenException(iter.Current, TokenType.FunctionDeclaration)
                });
            }
            while (iter.MoveNext());
        }

        private List<Node> functions { get; set; } = new List<Node>();
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

    public class Statement : Node
    {
        public static Statement CreateStatment(IEnumerator<Token> iter)
        {
            if (!validTokens.Any(t => t == iter.Current.TokenGroup)) throw new UnexpectedTokenException(iter.Current, TokenGroup.Function);

            return iter.Current switch
            {
                { TokenGroup: TokenGroup.Datatype } => new VariableDeclarationNode(iter),
                //{ TokenType: TokenType.VariableDeclaration } => new VariableDeclarationNode(iter),
                { TokenType: TokenType.Identifier } => CallExpressionNode.CreateIdentifierStatement(iter),
                { Source: "return" } => new ReturnNode(iter),
                { TokenType: TokenType.Keyword } => CallExpressionNode.CreateIdentifierStatement(iter),
                _ => throw new UnexpectedTokenException(iter.Current, TokenGroup.Function)
            };
        }

        private static readonly List<TokenGroup> validTokens = new List<TokenGroup>()
        {
            TokenGroup.Datatype,
            TokenGroup.Function,
            TokenGroup.Identifier,
        };
    }


    /// <summary>
    /// Parse valid expressions. This is either a literal or a CallExpression
    /// An expression consists of a function and parameters
    /// Valid function are LITERAL, OPERATOR, and IDENTIFIER
    /// </summary>
    public class BinaryExpressionNode : Node
    {
        // Parsing an expression tree is complex so do in a factory method
        public static Node CreateBinaryExpressionTree(IEnumerator<Token> iter, TokenGroup terminator = TokenGroup.LineSeparator)
        {
            List<Token> tokens = new List<Token>();

            // First gather all tokens to be used in the expression
            while (iter.Current.TokenGroup != terminator)
            {
                tokens.Add(iter.Current);
                iter.MoveNext();
            }

            return createBinaryExpressionTree(tokens);
        }
        private static Node createBinaryExpressionTree(List<Token> tokens, int startIndex = 0)
        {
            BinaryExpressionNode? rootNode = null;

            for (int i = startIndex; i < tokens.Count; i++)
            {
                // If binary operator, recursively add left and right sides as nodes
                // Left side must be literal as if it were a function it would have already been parsed
                // Right side may be anything however, so feed in iterator to current node
                if (tokens[i].TokenType == TokenType.BinaryOperator)
                {
                    Node leftSide = tokens[i - 1] switch
                    {
                        { TokenGroup: TokenGroup.Literal } => LiteralNode.CreateLiteralNode(tokens[i - 1]),
                        { TokenType: TokenType.Identifier } => new IdentifierNode(tokens[i - 1]),
                        _ => throw new UnexpectedTokenException(tokens[i - 1], TokenType.Identifier)
                    };


                    var newNode = new BinaryExpressionNode(tokens[i],
                        leftSide,
                        createBinaryExpressionTree(tokens.Skip(i + 1).ToList()));

                    if (rootNode == null)
                    {
                        rootNode = newNode;
                    }
                    else
                    {
                        //var temp = rootNode;
                        //newNode.Right = temp;
                        rootNode.Right = newNode;
                    }
                }
            }


            // No binary operators were found
            if (rootNode is null)
            {
                if (tokens.First().TokenGroup == TokenGroup.Literal)
                {
                    Assert.TokenGroup(tokens.First(), TokenGroup.Literal);
                    return LiteralNode.CreateLiteralNode(tokens.First());
                }

                // Variable, since function calls are not supported inside binary expressions
                if (tokens.First().TokenType == TokenType.Identifier)
                {
                    return CallExpressionNode.CreateIdentifierExpression(tokens.ToList());
                }
            }

            return rootNode;
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

    public class VariableDeclarationNode : Statement
    {
        /// <summary>
        /// Expect Identifier Operator Expression
        /// </summary>
        /// <param name="iter"></param>
        public VariableDeclarationNode(IEnumerator<Token> iter, TokenGroup terminator = TokenGroup.LineSeparator)
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
            Expression = BinaryExpressionNode.CreateBinaryExpressionTree(iter);

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

    public class IdentifierAsStatementNode : Statement
    {
        public IdentifierNode Identifier;

        public IdentifierAsStatementNode(Token token)
        {
            Identifier = new IdentifierNode(token);
        }
    }

    public class CallExpressionNode : Statement
    {
        public IdentifierNode Identifier;
        public ParamCallNode Params { get; set; }

        public static Statement CreateIdentifierStatement(IEnumerator<Token> iter)
        {
            Stack<TokenType> bracketStack = new Stack<TokenType>();
            List<Token> tokens = new List<Token>();

            Assert.TokenType(iter.Current, TokenType.Identifier);
            iter.MoveNext();
            Assert.TokenType(iter.Current, TokenType.BracketOpen);

            bracketStack.Push(TokenType.BracketOpen);
            int bracketCount = 1;
            while (bracketCount > 0)
            {
                tokens.Add(iter.Current);
                if (iter.Current.TokenType == TokenType.BracketOpen)
                {
                    bracketStack.Push(TokenType.BracketOpen);
                    bracketCount++;
                }
                if (iter.Current.TokenType == TokenType.BracketClose)
                {
                    if (bracketStack.Pop() != TokenType.BracketClose) throw new InvalidOperationException($"Invalid brackets at {iter.Current.Source} at {iter.Current.StartOffset}");
                    bracketStack.Push(TokenType.BracketClose);
                    bracketCount--;
                }
            }

            return CreateIdentifierExpression(tokens);
        }
        public static Statement CreateIdentifierExpression(List<Token> tokens)
        {
            Assert.TokenType(tokens.First(), TokenType.Identifier);

            var type = Parser.GetIdentifierType(tokens.First().Source);

            return type switch
            {
                IdentifierType.Variable => new IdentifierAsStatementNode(tokens.First()),
                IdentifierType.Function => new CallExpressionNode(tokens),
            };
        }

        public CallExpressionNode(List<Token> tokens)
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

    public class CallFunctionNode : Statement
    {
        public CallFunctionNode(IEnumerator<Token> iter)
        {

        }
    }

    public class ReturnNode : Statement
    {
        public Node Return;

        public ReturnNode(IEnumerator<Token> iter)
        {
            iter.MoveNext();
            Return = BinaryExpressionNode.CreateBinaryExpressionTree(iter);
        }
    }

    public class LiteralNode : Node
    {
        public static LiteralNode CreateLiteralNode(Token token)
        {
            if (token.TokenGroup != TokenGroup.Literal) throw new UnexpectedTokenException(token, TokenGroup.Literal);
            return token switch
            {
                { TokenType: TokenType.LiteralString } => new StringLiteralNode(token),
                { TokenType: TokenType.LiteralInt } => new IntLiteralNode(token),
                { TokenType: TokenType.LiteralFloat } => new FloatLiteralNode(token),
                _ => throw new UnexpectedTokenException(token, TokenType.LiteralInt)
            };
        }

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
            Value = long.Parse(token.Source);
        }

        public long Value { get; set; } = 0;
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
                Statements.Add(Statement.CreateStatment(iter));
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
            // Statements can begin only with identifiers, keywords, and variable declarations

            do
            {
                ParamList.Add(new VariableDeclarationNode(iter, TokenGroup.ParamListSeparator));
                if ((iter.Current.TokenType != TokenType.BracketClose) && !iter.MoveNext()) throw new UnexpectedTokenException(iter.Current, TokenType.BracketClose);
            }
            while (iter.Current.TokenType != TokenType.BracketClose);

        }

        public List<VariableDeclarationNode> ParamList { get; set; } = new();
    }

    public class ParamCallNode : Node
    {
        public ParamCallNode(IEnumerator<Token> iter)
        {
            if (iter.Current.TokenType != TokenType.BracketOpen) throw new UnexpectedTokenException(iter.Current, TokenType.BracketOpen);

            iter.MoveNext();
            // Statements can begin only with identifiers, keywords, and variable declarations

            do
            {
                ParamList.Add(BinaryExpressionNode.CreateBinaryExpressionTree(iter, TokenGroup.ParamListSeparator));
                if ((iter.Current.TokenType != TokenType.BracketClose) && !iter.MoveNext()) throw new UnexpectedTokenException(iter.Current, TokenType.BracketClose);
            }
            while (iter.Current.TokenType != TokenType.BracketClose);

        }
        public List<Node> ParamList { get; set; } = new();
    }
}
