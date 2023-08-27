using System;
using System.Collections.Generic;
using System.Linq;

namespace ImLang
{
    public class Func : IComparable<Func>
    {
        public static void ResetGuid() => GUID = 0x02;
        private List<byte> inParam;
        private List<byte> outParam;
        private List<byte> code;
        private List<byte> local;

        private string label;
        private byte index;
        private bool export;

        // Start at 1 because we are importing one function
        private static byte GUID = 0x02;

        public Func(string label, bool export = true)
        {
            inParam = new List<byte>();
            outParam = new List<byte>();
            code = new List<byte>();
            local = new List<byte>();

            this.label = label;
            this.index = GUID++;
            this.export = export;

            //TODO remove
            this.export = true;

            local.Add(0x00);
        }

        public void initAddInt()
        {
            //label = "add";
            inParam.Clear();
            outParam.Clear();
            code.Clear();

            inParam.Add(Types.i32);
            inParam.Add(Types.i32);
            outParam.Add(Types.i32);

            code.AddRange(Op.get_local); code.AddRange(Encoder.uLEB128(0x00));
            code.AddRange(Op.get_local); code.AddRange(Encoder.uLEB128(0x01));
            code.AddRange(Op.i32_add);
        }

        public void pushCodeSingle(params byte[] opcode)
        {
            for (int i = 0; i < opcode.Length; i++)
            {
                code.Add(opcode[i]);
            }
        }
        public void PushCode(params IEnumerable<byte>[] opcode)
        {
            for (int i = 0; i < opcode.Length; i++)
            {
                code.AddRange(opcode[i]);
            }
        }

        public void ClearCode()
        {
            code.Clear();
        }

        public List<byte> GetInputParameters()
        {
            return inParam;
        }
        public void SetInputParameters(params byte[] opcode)
        {
            inParam.Clear();

            for (int i = 0; i < opcode.Length; i++)
            {
                inParam.Add(opcode[i]);
            }
        }

        public List<byte> GetOutputParameters()
        {
            return outParam;
        }
        //ONLY 1 OUTPUT PARAMETER CURRENTLY ALLOWED. CAN EASILY EXTEND IF Wasm IS UPDATED
        public void SetOutputParameters(byte opcode)
        {
            outParam.Clear();
            outParam.Add(opcode);
        }

        public void SetCode(List<byte> newCode)
        {
            code = newCode;
        }

        public List<byte> GetCode()
        {
            return code;
        }

        public List<byte> GetLocal()
        {
            return local;
        }

        public List<byte> GetEncodedBody()
        {
            List<byte> temp = new List<byte>();
            temp = Encoder.Concatentate(GetLocal(), GetCode());
            temp.AddRange(Op.end);
            temp = Encoder.Wrap(temp);

            return temp;
        }

        public List<byte> GetTypeDefinition()
        {
            var temp = new List<byte>();

            temp.Add(Types.FUNC);
            temp.AddRange(Encoder.Wrap(inParam));
            temp.AddRange(Encoder.Wrap(outParam));

            return temp;
        }

        //LABEL
        public List<byte> GetEncodedLabel()
        {
            return Encoder.EncodeString(label);
        }
        public string getLabel()
        {
            return label;
        }
        public void setLabel(string newLabel)
        {
            label = newLabel;
        }

        public byte GetIndex()
        {
            return index;
        }
        public void SetIndex(byte newIndex)
        {
            index = newIndex;
        }

        public void SetExport(bool newExport)
        {
            this.export = newExport;
        }
        public bool GetExport()
        {
            return this.export;
        }
        public List<byte> GetExportDefinition()
        {
            List<byte> exportSection = new List<byte>();

            exportSection.AddRange(GetEncodedLabel());
            exportSection.Add(ExportType.FUNC);
            exportSection.Add(GetIndex()); //must be unique

            return exportSection;
        }

        //compare on index
        public int CompareTo(Func f)
        {
            return index.CompareTo(f.GetIndex());
        }
    }
}
