using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace ImLang
{
    class Program
    {
        
        public static void Main(string[] args)
        {
            string filename = "imlang.wasm";
            //Compiler compiler = new Compiler("print123;end(hello);foobar123;varasdf;1+1=3", filename);
            //var binary = Compiler.BuildBinaryFromSource("var num = 0;end(hello);foobar123;1+1=3");
            //var htmlFile = binary.GetExecutableHtml();

            //var base64Bin = binary.GetBase64Binary();
            //var executableHtml = binary.GetExecutableHtml();
            //File.WriteAllText("imlang.htm", executableHtml);
            //compiler.BuildWasm();
            string sourceTest = @"
fn add(int32 left, int32 right)
{
  int32 result = left + right;
  return  result;
}

fn subtract(int32 left, int32 right)
{
  int32 result = left - add(right, 0);
  return  result;
}
";

            List<Token> tokens = Tokeniser.GetTokenArray(sourceTest);

            var body = Parser.Parse(tokens);

            // Parse with recursive node structure

            Console.WriteLine("Compiling...");
            Console.Write("Built: " + filename);
            Console.ReadKey(true);
        }
    }
}