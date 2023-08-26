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
        public Token(TokenType tokenType, TokenGroup tokenGroup, Match match)
        {
            TokenType = tokenType;
            TokenGroup = tokenGroup;
            Source = match.Value;
            StartOffset = match.Index;
            EndOffset = match.Index + match.Value.Length;
        }

        public Token(TokenType tokenType, TokenGroup tokenGroup, string source, int index)
        {
            TokenType = tokenType;
            TokenGroup = tokenGroup;
            Source = source;
            StartOffset = index;
            EndOffset = index + source.Length;
        }

        public TokenType TokenType { get; set; }
        public TokenGroup TokenGroup { get; set; }
        public string Source { get; set; } = String.Empty;
        public int StartOffset { get; set; }
        public int EndOffset { get; set; }

        public Token Clone()
        {
            return new Token(TokenType, TokenGroup, Source, StartOffset);
        }

        public override string ToString() => $"{Source}  ({TokenType})";
    }

    public enum TokenType
    {
        Empty,
        FunctionDeclaration,
        VariableDeclaration,
        Keyword,
        VariableDatatype,
        Identifier,
        LiteralInt,
        LiteralFloat,
        LiteralString,
        BinaryOperator,
        EqualityOperator,
        EndOfLine,
        BracketOpen,
        BracketClose,
        CurlyBracketOpen,
        CurlyBracketClose,
        Comma,
        Assignment,
    }

    public enum TokenGroup
    {
        Declaration,
        Identifier,
        Function,
        Literal,
        Datatype,
        Divider,
        LineSeparator,
        ParamListSeparator
    }
}
