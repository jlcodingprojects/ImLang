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
            List<Token> tokens = new List<Token>();

            // Cut out all whitespace, keeping things in quotes
            string[] sourceSplitByWhitespace = TokenRegex.Divider.Split(fullSource);
            // Now split by dividers
            // Split by end of lines
            var sourceTokens = sourceSplitByWhitespace
                .Select(s => TokenRegex.NonAlphanumeric.Split(s)).SelectMany(s => s)
                .Where(s => !string.IsNullOrWhiteSpace(s) && !string.IsNullOrEmpty(s))
                .ToList();

            var foundTokens = sourceTokens.Select(s =>
                {
                    if (string.IsNullOrEmpty(s) || string.IsNullOrWhiteSpace(s)) return new Token(TokenType.Empty, s, 0, 0);

                    if (TokenRegex.FunctionDeclaration.IsMatch(s)) return new Token(TokenType.FunctionDeclaration, s, 0, 0);
                    if (TokenRegex.VariableDeclaration.IsMatch(s)) return new Token(TokenType.VariableDeclaration, s, 0, 0);
                    if (TokenRegex.Keyword.IsMatch(s)) return new Token(TokenType.Keyword, s, 0, 0);
                    if (TokenRegex.VariableDatatype.IsMatch(s)) return new Token(TokenType.VariableDatatype, s, 0, 0);
                    if (TokenRegex.ReturnDatatype.IsMatch(s)) return new Token(TokenType.ReturnDatatype, s, 0, 0);

                    if (TokenRegex.Identifier.IsMatch(s)) return new Token(TokenType.Identifier, s, 0, 0);
                    if (TokenRegex.LiteralInt.IsMatch(s)) return new Token(TokenType.LiteralInt, s, 0, 0);
                    if (TokenRegex.LiteralFloat.IsMatch(s)) return new Token(TokenType.LiteralFloat, s, 0, 0);
                    if (TokenRegex.LiteralString.IsMatch(s)) return new Token(TokenType.LiteralString, s, 0, 0);
                    if (TokenRegex.BinaryExpression.IsMatch(s)) return new Token(TokenType.BinaryExpression, s, 0, 0);
                    if (TokenRegex.Operator.IsMatch(s)) return new Token(TokenType.Operator, s, 0, 0);

                    if (TokenRegex.EndOfLine.IsMatch(s)) return new Token(TokenType.EndOfLine, s, 0, 0);
                    if (TokenRegex.BracketOpen.IsMatch(s)) return new Token(TokenType.BracketOpen, s, 0, 0);
                    if (TokenRegex.BracketClose.IsMatch(s)) return new Token(TokenType.BracketClose, s, 0, 0);
                    if (TokenRegex.CurlyBracketOpen.IsMatch(s)) return new Token(TokenType.CurlyBracketOpen, s, 0, 0);
                    if (TokenRegex.CurlyBracketClose.IsMatch(s)) return new Token(TokenType.CurlyBracketClose, s, 0, 0);
                    if (TokenRegex.Comma.IsMatch(s)) return new Token(TokenType.Comma, s, 0, 0);

                    throw new Exception($"Invalid token {s}");
                }
             ).ToList();

            return tokens;
        }
    }

    public static class TokenRegex
    {
        // Break first by newline, then keep anything in quotes, then break everything else by any non-alphanumeric character
        public static readonly Regex Divider = new Regex(@"([^\s""]+)|(""[^""\n\r]*"")");
        public static readonly Regex NonAlphanumeric = new Regex(@"([^a-zA-Z\d\.""])|(""[^""]*"")");

        // Match a contiguous whitespace section
        public static readonly Regex Whitespace = new Regex(@"\s+");

        // Function or variable declaration
        public static readonly Regex FunctionDeclaration = new Regex(@"^(fn)");
        public static readonly Regex VariableDeclaration = new Regex(@"^(var)");

        // Reserved keywords - match entire token
        public static readonly Regex Keyword = new Regex(@"^(if|while|return|print|setpixel)$");

        // Match valid datatype for declaring variables and returning from functions
        public static readonly Regex VariableDatatype = new Regex(@"^(int32|int64|float32|float64)");
        public static readonly Regex ReturnDatatype = new Regex(@"^(int32|int64|float32|float64|void)");

        // a valid identifier - must be declared before using
        public static readonly Regex Identifier = new Regex(@"^[a-zA-Z][a-zA-Z0-9]+");

        // must match entire token for these
        public static readonly Regex LiteralInt = new Regex("(^[0-9]+$)");
        public static readonly Regex LiteralFloat = new Regex("(^[0-9]*\\.[0-9]*$)");
        public static readonly Regex LiteralString = new Regex("(^\".*\"$)");

        // Expressions and operators
        public static readonly Regex BinaryExpression = new Regex(@"(\+|\-|\/|\*)");
        public static readonly Regex Operator = new Regex(@"([=]|[==])");

        public static readonly Regex EndOfLine = new Regex(@"(;)");
        public static readonly Regex BracketOpen = new Regex(@"(\()");
        public static readonly Regex BracketClose = new Regex(@"(\))");
        public static readonly Regex CurlyBracketOpen = new Regex(@"({)");
        public static readonly Regex CurlyBracketClose = new Regex(@"(})");
        public static readonly Regex Comma = new Regex(@"(,)");
    }
}
