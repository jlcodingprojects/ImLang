using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ImLang.Enums;

namespace ImLang.Nodes
{
    public static class NodeFactory
    {
        public static Statement CreateStatment(IEnumerator<Token> iter)
        {
            if (!validStatementTokens.Any(t => t == iter.Current.TokenGroup)) throw new UnexpectedTokenException(iter.Current, TokenGroup.Function);

            var statement = iter.Current switch
            {
                { TokenGroup: TokenGroup.Datatype } => new VariableDeclarationStatement(iter),
                { TokenType: TokenType.Identifier } => CreateIdentifierStatement(iter),
                { TokenType: TokenType.ReturnKeyword } => new ReturnStatement(iter),
                { TokenType: TokenType.IfKeyword } => new IfStatement(iter),
                { TokenType: TokenType.WhileKeyword } => new WhileStatement(iter),
                { TokenType: TokenType.LogKeyword} => new LogStatement(iter),
                { TokenType: TokenType.DrawKeyword } => new DrawStatement(iter),
                { TokenType: TokenType.Keyword } => CreateIdentifierStatement(iter),
                _ => throw new UnexpectedTokenException(iter.Current, TokenGroup.Function)
            };

            return statement;
        }

        private static readonly List<TokenGroup> validStatementTokens = new List<TokenGroup>()
        {
            TokenGroup.Datatype,
            TokenGroup.Function,
            TokenGroup.Identifier,
        };


        public static LiteralNode CreateLiteral(Token token)
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

        public static Statement CreateFunctionCall(IEnumerator<Token> iter)
        {
            Assert.TokenType(iter.Current, TokenType.Identifier);
            var identifierToken = iter.Current;
            iter.MoveNext();

            var tokens = getBoundedByBrackets(iter);
            // Todo: consolidate how bracket consumption works when creating statements so this is never necesssary
            if (iter.Current.TokenType == TokenType.BracketClose) iter.MoveNext();

            tokens.Insert(0, identifierToken);

            return new FunctionCallStatement(tokens);
        }

        private static List<Token> getBoundedByBrackets(IEnumerator<Token> iter)
        {
            //Stack<TokenType> bracketStack = new Stack<TokenType>();
            List<Token> tokens = new List<Token>();

            //Assert.TokenType(iter.Current, TokenType.Identifier);
            //tokens.Add(iter.Current);
            //iter.MoveNext();
            Assert.TokenType(iter.Current, TokenType.BracketOpen);

            //bracketStack.Push(TokenType.BracketOpen);
            //tokens.Add(iter.Current);
            
            int bracketCount = 0;
            do
            {
                //iter.MoveNext();
                tokens.Add(iter.Current);
                if (iter.Current.TokenType == TokenType.BracketOpen)
                {
                    //bracketStack.Push(TokenType.BracketOpen);
                    bracketCount++;
                }
                if (iter.Current.TokenType == TokenType.BracketClose)
                {
                    //if (bracketStack.Pop() != TokenType.BracketOpen) throw new InvalidOperationException($"Invalid brackets at {iter.Current.Source} at {iter.Current.StartOffset}");
                    //bracketStack.Push(TokenType.BracketClose);
                    bracketCount--;
                }

                if (bracketCount == 0) break;

                iter.MoveNext();
            } while (bracketCount > 0);

            return tokens;
        }

        public static Node CreateIdentifierReference(List<Token> tokens)
        {
            var iter = tokens.GetEnumerator();
            iter.MoveNext();
            return CreateIdentifierReference(iter);
        }

        /// <summary>
        /// Either a function call statement a refernce to a variable
        /// </summary>
        /// <param name="iter"></param>
        /// <returns></returns>
        public static Node CreateIdentifierReference(IEnumerator<Token> iter)
        {
            Assert.TokenType(iter.Current, TokenType.Identifier);
            var type = Parser.GetIdentifierType(iter.Current.Source);

            return type switch
            {
                IdentifierType.Variable => new RefVariableNode(iter),
                IdentifierType.Function => CreateFunctionCall(iter),
            };
        }

        /// <summary>
        /// Either a function call statement or assignment statement
        /// </summary>
        /// <param name="iter"></param>
        /// <returns></returns>
        public static Statement CreateIdentifierStatement(IEnumerator<Token> iter)
        {
            Assert.TokenType(iter.Current, TokenType.Identifier);
            var type = Parser.GetIdentifierType(iter.Current.Source);

            return type switch
            {
                IdentifierType.Variable => new AssignmentStatement(iter),
                IdentifierType.Function => CreateFunctionCall(iter),
            };
        }

        /// <summary>
        /// Create a binary expression
        /// </summary>
        /// <param name="iter"></param>
        /// <param name="terminator">The type of token which will terminate the expression (usually ';', ',', ')')</param>
        /// <returns></returns>
        public static Node CreateBinaryExpressionTree(IEnumerator<Token> iter, TokenGroup terminator = TokenGroup.LineSeparator)
        {
            List<Token> tokens = new List<Token>();

            // First gather all tokens to be used in the expression
            while (iter.Current != null && iter.Current.TokenGroup != terminator)
            {
                // If anything is bounded by brackets, go close bracket found
                if (iter.Current.TokenType == TokenType.BracketOpen)
                {
                    tokens.AddRange(getBoundedByBrackets(iter));
                    iter.MoveNext();
                }

                if (iter.Current == null || iter.Current.TokenGroup == terminator) break;
                tokens.Add(iter.Current);
                iter.MoveNext();
            }

            return createBinaryExpressionTree(tokens);
        }
        private static Node createBinaryExpressionTree(List<Token> tokens, int startIndex = 0)
        {
            BinaryExpressionNode? rootNode = null;

            Node prevNode = null;

            IEnumerator<Token> iter = tokens.GetEnumerator();
            while(iter.MoveNext())
            {
                if(iter.Current.TokenType != TokenType.BinaryOperator)
                {
                    prevNode = iter.Current switch
                    {
                        { TokenGroup: TokenGroup.Literal } => CreateLiteralNode(iter),
                        { TokenType: TokenType.Identifier } => CreateIdentifierReference(iter),
                        _ => throw new UnexpectedTokenException(iter.Current, TokenType.Identifier)
                    };
                }

                if (iter.Current == null) break;

                if (iter.Current.TokenType == TokenType.BinaryOperator)
                {

                    var newNode = new BinaryExpressionNode(iter.Current);
                    iter.MoveNext();
                    var right = CreateBinaryExpressionTree(iter);
                    newNode.Left = prevNode;
                    newNode.Right = right;
                    
                    // Check if it needs rotating
                    if(right is BinaryExpressionNode rightBinary)
                    {
                        if(newNode.Operator > rightBinary.Operator)
                        {
                            var temp = rightBinary.Left;
                            rightBinary.Left = newNode;
                            newNode.Right = temp;
                            newNode = rightBinary;
                        }
                    }


                    if (rootNode == null)
                    {
                        rootNode = newNode;
                    }
                    else
                    {
                        var temp = rootNode.Right;
                        //newNode.Right = temp;
                        rootNode.Right = newNode;
                        newNode.Left = temp;
                    }
                }
            }

            if (rootNode == null) return prevNode;
            return rootNode;


            //int leftSideStartIndex = 0;
            //for (int i = startIndex; i < tokens.Count; i++)
            //{
            //    // If binary operator, recursively add left and right sides as nodes
            //    // Left side must be literal as if it were a function it would have already been parsed
            //    // Right side may be anything however, so feed in iterator to current node
            //    if (tokens[i].TokenType == TokenType.BinaryOperator)
            //    {
            //        var leftSideTokens = tokens.Skip(leftSideStartIndex).Take(i - leftSideStartIndex).ToList();
            //        leftSideStartIndex = i + 1;

            //        Node leftSide = tokens[i - 1] switch
            //        {
            //            { TokenGroup: TokenGroup.Literal } => CreateLiteralNode(tokens[i - 1]),
            //            { TokenType: TokenType.Identifier } => CreateIdentifierReference(leftSideTokens),
            //            _ => throw new UnexpectedTokenException(tokens[i - 1], TokenType.Identifier)
            //        };


                    
            //    }
            //}


            //// No binary operators were found
            //if (rootNode is null)
            //{
            //    if (tokens.First().TokenGroup == TokenGroup.Literal)
            //    {
            //        Assert.TokenGroup(tokens.First(), TokenGroup.Literal);
            //        return CreateLiteralNode(tokens.First());
            //    }

            //    if (tokens.First().TokenType == TokenType.Identifier)
            //    {
            //        return CreateIdentifierReference(tokens.ToList());
            //    }
            //}

            //return rootNode;
        }

        public static LiteralNode CreateLiteralNode(IEnumerator<Token> iter)
        {
            var node = CreateLiteralNode(iter.Current);
            iter.MoveNext();
            return node;
        }
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

        //public static Node CreateIdentifierStatement(List<Token> tokens)
        //{
        //    Assert.TokenType(tokens.First(), TokenType.Identifier);

        //    var type = Parser.GetIdentifierType(tokens.First().Source);

        //    return type switch
        //    {
        //        IdentifierType.Variable => new RefVariableNode(tokens.First()),
        //        IdentifierType.Function => new CallFunctionNode(tokens),
        //    };
        //}
    }
}
