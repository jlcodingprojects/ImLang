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

            List<Token> tokens = Tokeniser.GetTokenArray(@"
var num = 0;
fn add(int32 left, int32 right)
{
  var int32 result = left + right;
  return  result;
}

var float32 = 1337.42;

""this is in quotes"";

add(12, 32);
foobar123;
1+  1=   3");
            //foreach ( Token t in tokens )
            //{
            	
            //	if(t.Group != TokenCode.Whitespace)
            //	{
            //		Console.WriteLine("Group: {0}, {2}, Value: |{1}|", t.GroupName, t.Value, t.Group);
            //	}
            //}
            Console.WriteLine("\nprint123;end(hello);foobar123;varasdf;1+1=3");
            

            Console.WriteLine("Compiling...");
            Console.Write("Built: " + filename);
            Console.ReadKey(true);
        }
    }
}