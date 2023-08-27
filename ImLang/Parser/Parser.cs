using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using ImLang;
using static ImLang.Enums;

namespace ImLang
{
    public static class Parser
    {
        public static List<VariableDeclarationStatement> VariableDefinitions { get; set; } = new List<VariableDeclarationStatement>();
        public static List<string> FunctionDefinitions { get; set; } = new List<string>();
        public static Node Parse(List<Token> tokens)
        {
            VariableDefinitions = new List<VariableDeclarationStatement>();
            FunctionDefinitions = new List<string>();

            return new BodyNode(tokens.GetEnumerator());
        }

        public static bool IdentifierExists(IdentifierNode identifier)
        {
            return VariableDefinitions.Any(v => v.Identifier.Value == identifier.Value) || FunctionDefinitions.Any(f => f == identifier.Value);
        }

        public static IdentifierType GetIdentifierType(string identifier)
        {
            if(FunctionDefinitions.Any(f => f == identifier))  return IdentifierType.Function;
            if(VariableDefinitions.Any(v => v.Identifier.Value == identifier))  return IdentifierType.Variable;

            throw new InvalidOperationException($"Identifier not declared {identifier}");
        }
    }
}
