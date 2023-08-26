using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImLang.Nodes
{
    public class UnexpectedTokenException : Exception
    {
        public UnexpectedTokenException(Token token, TokenType expected)
            : base($"Unexpected {token.TokenType} at {token.StartOffset}, expected {expected} ({token.Source.Substring(0, Math.Min(10, token.Source.Length))}...)") { }
        public UnexpectedTokenException(Token token, TokenGroup expected)
            : base($"Unexpected {token.TokenType} at {token.StartOffset}, expected {expected} ({token.Source.Substring(0, Math.Min(10, token.Source.Length))}...)") { }
    }

    public static class Assert
    {
        /// <summary>
        /// Throws if token doesnt match type
        /// </summary>
        /// <param name="token"></param>
        /// <param name="tokenType"></param>
        /// <exception cref="UnexpectedTokenException"></exception>
        public static void TokenType(Token token, TokenType tokenType)
        {
            if (token.TokenType != tokenType) throw new UnexpectedTokenException(token, tokenType);
        }

        /// <summary>
        /// Throws if token doesnt match group
        /// </summary>
        /// <param name="token"></param>
        /// <param name="tokenGroup"></param>
        /// <exception cref="UnexpectedTokenException"></exception>
        public static void TokenGroup(Token token, TokenGroup tokenGroup)
        {
            if (token.TokenGroup != tokenGroup) throw new UnexpectedTokenException(token, tokenGroup);
        }
    }
}
