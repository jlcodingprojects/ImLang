using System;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace ImLang.Compilation
{
    public class Binary
    {
        List<byte> codeTotal = new List<byte>();

        List<byte> codeSection = new List<byte>();
        List<byte> typeSection = new List<byte>();
        List<byte> funcSection = new List<byte>();
        List<byte> memSection = new List<byte>();
        List<byte> exportSection = new List<byte>();
        List<byte> importSection = new List<byte>();

        List<byte> codeDef = new List<byte>();
        List<byte> typeDef = new List<byte>();
        List<byte> memDef = new List<byte>();
        List<byte> exportDef = new List<byte>();
        List<byte> importDef = new List<byte>();

        List<Func> functions = new List<Func>();

        public Binary()
        {
        }

        public void AddFunction(Func f)
        {
            functions.Add(f);
        }

        public string GetExecutableHtml()
        {
            var templateText = File.ReadAllText("html.template");
            var base64Code = GetBase64Binary();
            templateText = templateText.Replace("{base64code}", base64Code);

            return templateText;
        }
        public string GetBase64Binary()
        {
            byte[] bin = GetBinary();
            return Convert.ToBase64String(bin);
        }

        public byte[] GetBinary()
        {
            reset();
            //sort functions by index to make sure they are added in the correct order
            functions.Sort();

            //first add memory definition to export
            int exportCount = 1;

            exportDef.AddRange(Encoder.EncodeString("mem"));
            exportDef.Add(ExportType.MEM);
            exportDef.Add(0x00); //memory id 0

            for (int i = 0; i < functions.Count; i++)
            {
                funcSection.Add(functions[i].GetIndex()); //encode indicies of each function

                Console.WriteLine("{0}({4}): {5}\n\tIndex: {1}\n\tExport: {2}\n\tCode: {3}\n\t",
                                  functions[i].getLabel(),
                                  functions[i].GetIndex(),
                                  functions[i].GetExport(),
                                  Encoder.HexString(functions[i].GetCode()),
                                  Encoder.HexString(functions[i].GetInputParameters()),
                                  Encoder.HexString(functions[i].GetOutputParameters()));

                //then add each sucessive definition
                codeDef.AddRange(functions[i].GetEncodedBody());
                typeDef.AddRange(functions[i].GetTypeDefinition());
                if (functions[i].GetExport())
                {
                    exportDef.AddRange(functions[i].GetExportDefinition());
                    exportCount++;
                }
            }

            //each of the definitions is the Encoder.wrapList() called on all Func.def()
            //which means the first byte will be the function count
            codeDef.InsertRange(0, Encoder.uLEB128(functions.Count));
            typeDef.InsertRange(0, Encoder.uLEB128(functions.Count));
            exportDef.InsertRange(0, Encoder.uLEB128(exportCount));

            memDef.Add(0x00); memDef.Add(0x01); //flags, minimum size
            memDef.InsertRange(0, Encoder.uLEB128(1)); //1 memory object always

            // Only importing 1 function
            // Import section for console.logg
            //importDef.AddRange(Encoder.uLEB128(1));
            //importDef.AddRange(Encoder.EncodeString("console"));
            //importDef.AddRange(Encoder.EncodeString("log"));
            //importDef.Add(ExportType.FUNC);

            typeSection = Encoder.CreateSection(Section.TYPE, typeDef);

            funcSection = Encoder.Wrap(funcSection);
            funcSection = Encoder.CreateSection(Section.FUNC, funcSection);

            memSection = Encoder.CreateSection(Section.MEMORY, memDef);
            exportSection = Encoder.CreateSection(Section.EXPORT, exportDef);
            //importSection = Encoder.CreateSection(Section.IMPORT, importDef);
            codeSection = Encoder.CreateSection(Section.CODE, codeDef);

            Console.WriteLine("Type Section: {0}", Encoder.HexString(typeSection));
            Console.WriteLine("Func Section: {0}", Encoder.HexString(funcSection));
            Console.WriteLine("Mem Section: {0}", Encoder.HexString(memSection));
            Console.WriteLine("Export Section: {0}", Encoder.HexString(exportSection));
            Console.WriteLine("Code Section: {0}", Encoder.HexString(codeSection));

            codeTotal = Encoder.Concatentate(Headers.MagicModule, Headers.ModuleVersion, typeSection, importSection, funcSection, memSection, exportSection, codeSection);

            return codeTotal.ToArray();
        }

        private void reset()
        {
            codeTotal = new List<byte>();

            codeSection = new List<byte>();
            typeSection = new List<byte>();
            funcSection = new List<byte>();
            memSection = new List<byte>();
            exportSection = new List<byte>();
            importSection = new List<byte>();

            codeDef = new List<byte>();
            typeDef = new List<byte>();
            memDef = new List<byte>();
            exportDef = new List<byte>();
            importDef = new List<byte>();
        }

    }
}
