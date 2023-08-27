using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;

namespace ImLang
{
    /// <summary>
    /// Split a string into tokens
    /// </summary>
    public static class Tokeniser
    {
        public static List<Token> GetTokenArray(string fullSource)
        {
            // Cut out all whitespace, keeping everything that is in quotes within a single line
            var sourceMatches = TokenRegex.BigSplit.Matches(fullSource);

            // Match corresponding tokens with precedence (such as matching keywords before identifiers)
            var foundTokens = sourceMatches.Select(match =>
                {
                    if (string.IsNullOrEmpty(match.Value) || string.IsNullOrWhiteSpace(match.Value)) return new Token(TokenType.Empty, TokenGroup.Declaration, match);

                    if (TokenRegex.FunctionDeclaration.IsMatch(match.Value)) return new Token(TokenType.FunctionDeclaration, TokenGroup.Declaration, match);
                    if (TokenRegex.VariableDeclaration.IsMatch(match.Value)) return new Token(TokenType.VariableDeclaration, TokenGroup.Declaration, match);
                    if (TokenRegex.Keyword.IsMatch(match.Value)) return new Token(TokenType.Keyword, TokenGroup.Function, match);
                    if (TokenRegex.VariableDatatype.IsMatch(match.Value)) return new Token(TokenType.VariableDatatype, TokenGroup.Datatype, match);

                    if (TokenRegex.Identifier.IsMatch(match.Value)) return new Token(TokenType.Identifier, TokenGroup.Identifier, match);
                    if (TokenRegex.LiteralInt.IsMatch(match.Value)) return new Token(TokenType.LiteralInt, TokenGroup.Literal, match);
                    if (TokenRegex.LiteralFloat.IsMatch(match.Value)) return new Token(TokenType.LiteralFloat, TokenGroup.Literal, match);
                    if (TokenRegex.LiteralString.IsMatch(match.Value)) return new Token(TokenType.LiteralString, TokenGroup.Literal, match);

                    if (TokenRegex.Assignment.IsMatch(match.Value)) return new Token(TokenType.Assignment, TokenGroup.Function, match);
                    if (TokenRegex.BinaryOperator.IsMatch(match.Value)) return new Token(TokenType.BinaryOperator, TokenGroup.Function, match);
                    if (TokenRegex.EqualityOperator.IsMatch(match.Value)) return new Token(TokenType.EqualityOperator, TokenGroup.Function, match);

                    if (TokenRegex.EndOfLine.IsMatch(match.Value)) return new Token(TokenType.EndOfLine, TokenGroup.LineSeparator, match);
                    if (TokenRegex.BracketOpen.IsMatch(match.Value)) return new Token(TokenType.BracketOpen, TokenGroup.Divider, match);
                    if (TokenRegex.BracketClose.IsMatch(match.Value)) return new Token(TokenType.BracketClose, TokenGroup.ParamListSeparator, match);
                    if (TokenRegex.CurlyBracketOpen.IsMatch(match.Value)) return new Token(TokenType.CurlyBracketOpen, TokenGroup.Divider, match);
                    if (TokenRegex.CurlyBracketClose.IsMatch(match.Value)) return new Token(TokenType.CurlyBracketClose, TokenGroup.Divider, match);
                    if (TokenRegex.Comma.IsMatch(match.Value)) return new Token(TokenType.Comma, TokenGroup.ParamListSeparator, match);

                    throw new Exception($"Invalid token {match}");
                }
             ).ToList();

            return foundTokens;
        }
    }

    public static class TokenRegex
    {
        // Break first by newline, then keep anything in quotes, then break everything else by any non-alphanumeric character
        public static readonly Regex Divider = new Regex(@"([^\s""]+)|(""[^""\n\r]*"")");
        public static readonly Regex NonAlphanumeric = new Regex(@"([^a-zA-Z\d\.""])|(""[^""]*"")");

        public static readonly Regex BigSplit = new Regex(@"([a-zA-Z\d\.]+)|(""[^""\n\r]*"")|([^a-zA-b\d\s])");
        //public static readonly Regex NonAlphanumeric = new Regex(@"");

        // Match a contiguous whitespace section
        public static readonly Regex Whitespace = new Regex(@"\s+");

        // Function or variable declaration
        public static readonly Regex FunctionDeclaration = new Regex(@"^(fn)$");
        public static readonly Regex VariableDeclaration = new Regex(@"^(var)$");

        // Reserved keywords - match entire token
        public static readonly Regex Keyword = new Regex(@"^(if|while|return|print|setpixel)$");

        // Match valid datatype for declaring variables and returning from functions
        public static readonly Regex VariableDatatype = new Regex(@"^(int32)");
        //public static readonly Regex VariableDatatype = new Regex(@"^(int32|int64|float32|float64)");
        //public static readonly Regex ReturnDatatype = new Regex(@"^(int32|int64|float32|float64|void)");

        // a valid identifier - must be declared before using
        public static readonly Regex Identifier = new Regex(@"^[a-zA-Z][a-zA-Z0-9]+");

        // must match entire token for these
        public static readonly Regex LiteralInt = new Regex("(^[0-9]+$)");
        public static readonly Regex LiteralFloat = new Regex("(^[0-9]*\\.[0-9]*$)");
        public static readonly Regex LiteralString = new Regex("(^\".*\"$)");

        // Expressions and operators
        public static readonly Regex Assignment = new Regex(@"(^=$)");
        public static readonly Regex BinaryOperator = new Regex(@"(\+|\-|\/|\*)");
        public static readonly Regex EqualityOperator = new Regex(@"(^==$)");

        public static readonly Regex EndOfLine = new Regex(@"(;)");
        public static readonly Regex BracketOpen = new Regex(@"(\()");
        public static readonly Regex BracketClose = new Regex(@"(\))");
        public static readonly Regex CurlyBracketOpen = new Regex(@"({)");
        public static readonly Regex CurlyBracketClose = new Regex(@"(})");
        public static readonly Regex Comma = new Regex(@"(,)");
    }
}
