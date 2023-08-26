//namespace ImLang
//{
//    public static class TokenFactory
//    {
//        public static Token GetToken(string source, Token previousToken)
//        {
//            int endOffset = previousToken.StartOffset + previousToken.EndOffset;
//            List<TokenType> tokens = new List<TokenType>();

//            var validNextTokens = previousToken.ValidNextTokens().Where(t =>
//                t switch
//                {
//                    TokenType.FunctionDeclaration => TokenRegex.DeclarationFn.IsMatch(source),
//                    TokenType.Datatype => TokenRegex.ReturnDatatype.IsMatch(source),
//                    TokenType.Keyword => TokenRegex.Keyword.IsMatch(source),
//                    TokenType.BinaryExpression => TokenRegex.BinaryExpression.IsMatch(source),
//                    TokenType.Literal => TokenRegex.Literal.IsMatch(source),
//                    TokenType.Identifier => TokenRegex.Identifier.IsMatch(source),
//                    _ => throw new Exception($"Unknown token at {endOffset} immediately before {source.Substring(0, 10)}...")
//                }
//            ).ToList();

//            if(validNextTokens.Count != 1)
//                throw new Exception($"Unknown token at {previousToken.StartOffset + previousToken.EndOffset} immediately before {source.Substring(0, 10)}...");

//            return validNextTokens.First() switch
//            {
//                TokenType.FunctionDeclaration => new FunctionDeclarationToken(source, endOffset),
//                TokenType.Datatype => new FunctionDeclarationToken(source, endOffset),
//                TokenType.Keyword => new FunctionDeclarationToken(source, endOffset),
//                TokenType.BinaryExpression => new FunctionDeclarationToken(source, endOffset),
//                TokenType.Literal => new FunctionDeclarationToken(source, endOffset),
//                TokenType.Identifier => new FunctionDeclarationToken(source, endOffset),
//                _ => throw new Exception($"Unknown token at {endOffset} immediately before {source.Substring(0, 10)}...")
//            };
//        }
//    }
//}
