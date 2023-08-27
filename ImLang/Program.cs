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
            string filename = "imlang.wasm";
            //Compiler compiler = new Compiler("print123;end(hello);foobar123;varasdf;1+1=3", filename);
            //var binary = Compiler.BuildBinaryFromSource("var num = 0;end(hello);foobar123;1+1=3");
            //var htmlFile = binary.GetExecutableHtml();

            //var base64Bin = binary.GetBase64Binary();
            //var executableHtml = binary.GetExecutableHtml();
            //File.WriteAllText("imlang.htm", executableHtml);
            //compiler.BuildWasm();
            string sourceTest = @"
fn fourtytwo()
{
  return 42;
}

fn subtract(int32 left, int32 right)
{
  int32 result = left - right;
  return result;
}

fn mul(int32 left, int32 right)
{
  int32 result = left * right;
  return result;
}

fn add(int32 left, int32 right)
{
  int32 result = left + fourtytwo ();
  return result;
}

fn bling(int32 left, int32 right, int32 nextRight)
{
  int32 result = left + mul(right + nextRight, fourtytwo());
  return result;
}
";
            /*fn subtract(int32 left, int32 right) {
              int32 result = left - add(right, 0)
            ;
              return  result;
            }*/

            List<Token> tokens = Tokeniser.GetTokenArray(sourceTest);
            var body = Parser.Parse(tokens);


            var binary = Compiler.BuildBinaryFromAST((BodyNode)body);
            var binar2y = Compiler.BuildBinaryFromAST((BodyNode)body);
            //var linkedlist = Compiler.MakeLinkedListBinary().GetExecutableHtml();

            var htm = binary.GetExecutableHtml();
            //var bin = binar2y.GetBinary();


            //File.WriteAllText($"{Environment.GetFolderPath(Environment.SpecialFolder.Desktop)}/LIST.htm", linkedlist);
            //File.WriteAllBytes($"{Environment.GetFolderPath(Environment.SpecialFolder.Desktop)}/BIN.wasm", bin);
            File.WriteAllText($"{Environment.GetFolderPath(Environment.SpecialFolder.Desktop)}/BIN.htm", htm);

            // Parse with recursive node structure

            Console.WriteLine("Compiling...");
            Console.Write("Built: " + filename);
        }
    }
}