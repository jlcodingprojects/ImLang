using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ImLang
{
    public class Token
    {
        public Token() { }
        public Token(TokenType tokenType, string source, int startOffset, int endOffset)
        {
            TokenType = tokenType;
            Source = source;
            StartOffset = startOffset;
            EndOffset = endOffset;
        }

        public TokenType TokenType { get; set; }
        public string Source { get; set; } = String.Empty;
        public int StartOffset { get; set; }
        public int EndOffset { get; set; }

        public Token Clone()
        {
            return new Token(TokenType, Source, StartOffset, EndOffset);
        }

        public override string ToString() => $"{Source}  ---  {TokenType.ToString()}";
        //public abstract IEnumerable<TokenType> ValidNextTokens();
    }

    public enum TokenType
    {
        Empty,
        FunctionDeclaration,
        VariableDeclaration,
        Keyword,
        VariableDatatype,
        ReturnDatatype,
        Identifier ,
        LiteralInt,
        LiteralFloat,
        LiteralString,
        BinaryExpression,
        Operator,
        EndOfLine,
        BracketOpen,
        BracketClose,
        CurlyBracketOpen,
        CurlyBracketClose,
        Comma,
    }
}
