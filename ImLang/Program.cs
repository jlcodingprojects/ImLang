using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using ImLang.Compilation;

namespace ImLang
{
    class Program
    {

        public static void Main(string[] args)
        {
            string fileName = (args.Length == 0)
                ? "example.iml"
                : args[0];

            string source = File.ReadAllText(fileName);

            List<Token> tokens = Tokeniser.GetTokenArray(source);
            var body = Parser.Parse(tokens);


            var binary = Compiler.BuildBinaryFromAST((BodyNode)body);
            var htm = binary.GetExecutableHtml();
            File.WriteAllText($"{fileName.Split(".").First()}.htm", htm);

            Console.WriteLine("Compiling...");
            Console.Write("Built: " + $"{fileName.Split(".").First()}.htm");
        }
    }
}