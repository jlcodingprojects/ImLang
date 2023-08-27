using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using static ImLang.Enums;

namespace ImLang.Compilation
{
    public static class Compiler
    {
        private enum IdentifierType
        {
            Variable,
            Param
        }
        private static void addScopedVariable(string variableName)
        {
            nextAddress++;
            variableAddressMapping.Add($"{fnPrefix}-{variableName}", nextAddress);
        }

        private static int getScopedVariableAddress(string variableName)
        {
            return variableAddressMapping[$"{fnPrefix}-{variableName}"];
        }

        private static (int Addr, IdentifierType Idt) getAddressAndType(string variableName)
        {
            if (variableAddressMapping.ContainsKey($"{fnPrefix}-{variableName}"))
            {
                return (variableAddressMapping[$"{fnPrefix}-{variableName}"], IdentifierType.Variable);
            }

            if (paramIndexMapping.ContainsKey(variableName))
            {
                return (paramIndexMapping[variableName], IdentifierType.Param);
            }

            throw new Exception($"Variable not registered {variableName}");
        }

        private static Dictionary<string, int> variableAddressMapping = new Dictionary<string, int>();
        private static Dictionary<string, int> paramIndexMapping = new Dictionary<string, int>();
        private static Dictionary<string, int> functionIndexMapping = new Dictionary<string, int>();
        private static byte nextAddress = 0x00;
        private static string fnPrefix = "";
        public static Binary BuildBinaryFromAST(BodyNode root)
        {
            variableAddressMapping.Clear();
            paramIndexMapping.Clear();
            functionIndexMapping.Clear();
            Func.ResetGuid();
            nextAddress = 0x10;

            var binary = new Binary();

            //Func f = new Func("add");
            //f.initAddInt();
            //binary.AddFunction(f);
            //return binary;
            List<(Func Function, FunctionDeclarationNode Node)> functions = new();
            foreach (var node in root.Functions)
            {
                if (node is not FunctionDeclarationNode funcnode) throw new InvalidOperationException("Body contains non-function construct");

                Func func = new Func(funcnode.Identifier.Value);
                functions.Add((func, funcnode));

                functionIndexMapping.Add(funcnode.Identifier.Value, func.GetIndex());
            }

            foreach (var fn in functions)
            {
                fnPrefix = Guid.NewGuid().ToString();
                var opcodes = fn.Node.Params.ParamList
                    .Select(p => p.Datatype.Datatype)
                    .Select(d => d switch
                    {
                        Datatype.Int32 => Types.i32,
                        Datatype.Int64 => Types.i64,
                        Datatype.Float32 => Types.f32,
                        Datatype.Float64 => Types.f64,
                    });

                paramIndexMapping.Clear();
                for (int i = 0; i < fn.Node.Params.ParamList.Count; i++)
                {
                    paramIndexMapping.Add(fn.Node.Params.ParamList[i].Identifier.Value, (byte)i);
                }

                fn.Function.SetInputParameters(opcodes.ToArray());
                fn.Function.SetOutputParameters(Types.i32);
                //func.SetInputParameters(opcodes.ToArray());


                foreach (var statement in fn.Node.Body.Statements)
                {
                    var code = getCodeForStatement(statement);
                    var statementline = Encoder.HexString(code);
                    fn.Function.PushCode(code);
                    //func.PushCode(Op.get_local, Encoder.uLEB128(0x00), Op.i32_const, Encoder.uLEB128(0x04), Op.i32_add, Op.get_local, Encoder.uLEB128(0x01), Op.i32_store);
                }

                binary.AddFunction(fn.Function);
            }

            return binary;
        }

        private static List<byte> getCodeForStatement(Statement statement)
        {
            List<byte> code = new List<byte>();

            if (statement is VariableDeclarationStatement vardec)
            {
                addScopedVariable(vardec.Identifier.Value);
                code.AddRange(declareVariableCode(vardec));
                //code.AddRange(Op.set_local);
            }

            if (statement is AssignmentStatement assign)
            {
                var varAddr = getScopedVariableAddress(assign.Target.Value);
                code.AddRange(Encoder.Concatentate(Op.set_local));
            }

            if (statement is ReturnStatement ret)
            {
                code.AddRange(getCodeForReturnStatement(ret));
            }


            return code;
        }

        private static List<byte> getCodeForReturnStatement(ReturnStatement ret)
        {
            List<byte> code = new List<byte>();

            code.AddRange(binaryExpressionCode(ret.Return));

            return code;
        }

        private static List<byte> readVariableCode(RefVariableNode variable)
        {
            var addrAndType = getAddressAndType(variable.Identifier.Value);

            if (addrAndType.Idt == IdentifierType.Variable)
            {
                return Encoder.Concatentate(Op.i32_const, Encoder.uLEB128(addrAndType.Addr), Op.i32_load);
            }
            if (addrAndType.Idt == IdentifierType.Param)
            {
                return Encoder.Concatentate(Op.get_local, Encoder.uLEB128(addrAndType.Addr));
            }
            throw new NotImplementedException("Variable not declared");

        }

        private static List<byte> declareVariableCode(VariableDeclarationStatement vardec)
        {
            var variableAddress = getScopedVariableAddress(vardec.Identifier.Value);

            return Encoder.Concatentate(Op.i32_const, Encoder.uLEB128(variableAddress), binaryExpressionCode(vardec.Expression), Op.i32_store);
        }

        private static List<byte> functionCallCode(FunctionCallStatement func)
        {
            List<byte> code = new List<byte>();
            if (!functionIndexMapping.ContainsKey(func.Identifier.Value)) throw new NotImplementedException($"Function {func} doesnt exist");

            var funcIdx = functionIndexMapping[func.Identifier.Value];

            // each param must be a binary expression. Push all params to stack
            var paramOpcodes = func.Params.ParamList.Select(c => binaryExpressionCode(c)).SelectMany(b => b).ToList();

            var encoded = Encoder.Concatentate(paramOpcodes, Op.call, Encoder.uLEB128(funcIdx));

            string asdf = Encoder.HexString(encoded);

            return encoded;
        }

        private static List<byte> writeVariableCode(AssignmentStatement assign)
        {
            var variableAddress = getScopedVariableAddress(assign.Target.Value);

            if (assign.Expression is IntLiteralNode literal)
            {
                return Encoder.Concatentate(Op.i32_const, Encoder.uLEB128(variableAddress), Op.i32_const, Encoder.sLEB128(literal.Value), Op.i32_store);
            }
            // push variable address, push value, call store
            return Encoder.Concatentate(Op.i32_const, Encoder.uLEB128(variableAddress), Op.i32_const, Encoder.uLEB128(1337), Op.i32_load);
        }

        private static List<byte> binaryExpressionCode(Node assign)
        {
            // pop values onto stack for literal and variables

            if (assign is RefVariableNode refvar)
            {
                return readVariableCode(refvar);
            }

            if (assign is IntLiteralNode literal)
            {
                return Encoder.Concatentate(Op.i32_const, Encoder.sLEB128(literal.Value));
            }

            if (assign is FunctionCallStatement funcCall)
            {
                return functionCallCode(funcCall);
            }

            if (assign is BinaryExpressionNode op)
            {
                var opcode = op.Operator switch
                {
                    BinaryOperator.Add => Op.i32_add,
                    BinaryOperator.Subtract => Op.i32_sub,
                    BinaryOperator.Divide => Op.i32_div_s,
                    BinaryOperator.Multiply => Op.i32_mul,
                };
                return Encoder.Concatentate(binaryExpressionCode(op.Left), binaryExpressionCode(op.Right), opcode);
            }
            //var variableAddress = variableAddressMapping["1337"];
            // push variable address, push value, call store
            throw new InvalidOperationException("Invalid binary tree");
            //return Encoder.Concatentate(Op.i32_const, Encoder.uLEB128(variableAddress), Op.i32_const, Encoder.uLEB128(1337), Op.i32_load);
        }




        public static Binary BuildBinaryFromSource(string sourceText)
        {
            // Binary class will automatically sort functions based on index to ensure the export definition index matches the function code index
            // This means functions can be added in whatever order desired

            //string[] operators = { "+", "+", "/", "*" };
            //Random r = new Random();
            //string field = "";
            //for (int i = 0; i < 10; i++)
            //{
            //    field += (Math.Floor(r.NextDouble() * 100)).ToString();
            //    field += operators[(int)Math.Floor(r.NextDouble() * 4)];
            //}
            //field += (Math.Floor(r.NextDouble() * 100)).ToString();

            //Console.WriteLine(field);
            //List<Token> tokens = Tokeniser.GetTokenArray(@"return " + field + ";");
            List<Token> tokens = Tokeniser.GetTokenArray(sourceText);
            //Parser parser = new Parser(tokens);
            //parser.Parse();
            //parser.Dump();
            //List<Statement> statements = parser.GetStatements();

            var binary = new Binary();
            Func func = new Func("temp");

            func.SetInputParameters();
            func.SetOutputParameters(Types.i32);
            // Todo - work out what this did
            //func.setLocalCounts();
            //*
            Console.WriteLine("Printing pretty statements");
            //foreach (Statement s in statements)
            //{
            //    //s.Tree.PrintPretty("", true);
            //    func.PushCode(EncodeStatement(s));
            //}

            //List<byte> temp = EncodeStatement(statements[0]);
            //foreach (byte b in temp)
            //{
            //    if (b == 0x6a)
            //    {
            //        Console.Write("+");
            //    }
            //    else if (b == 0x6b)
            //    {
            //        Console.Write("-");
            //    }
            //    else if (b == 0x6c)
            //    {
            //        Console.Write("*");
            //    }
            //    else if (b == 0x6d)
            //    {
            //        Console.Write("/");
            //    }
            //}
            //*/


            binary.AddFunction(func);

            return binary;
        }

        private static List<byte> EncodeStatement(Statement s)
        {
            List<byte> code = new List<byte>();

            //if (s.Tree.Details.Source == "return")
            //{
            //	code.AddRange(EncodeBinaryExpression(s.Tree.Children[0])); //first expression must be binary expression
            //}

            return code;
        }

        private static List<byte> EncodeBinaryExpression(Node n)
        {
            List<byte> code = new List<byte>();

            //Console.WriteLine(n.Details.Value);

            //if(n.Details.Group == TokenCode.Number)
            //{
            //	code.AddRange(Op.i32_const);
            //	code.AddRange(Encoder.sLEB128(Int32.Parse(n.Details.Value))); //i32_const accepts signed ints, not unsigned
            //} else if(n.Details.Group == TokenCode.Expression) {
            //	code.AddRange(EncodeBinaryExpression(n.Children[0]));
            //	code.AddRange(EncodeBinaryExpression(n.Children[1]));

            //	if(n.Details.Value == "+") {
            //		code.AddRange(Op.i32_add);
            //	} else if(n.Details.Value == "-") {
            //		code.AddRange(Op.i32_sub);
            //	} else if(n.Details.Value == "*") {
            //		code.AddRange(Op.i32_mul);
            //	} else if(n.Details.Value == "/") {
            //		code.AddRange(Op.i32_div_s);
            //	}
            //}


            return code;
        }

        public static Binary MakeLinkedListBinary()
        {
            Binary bin = new Binary();

            Func allocate = new Func("allocate", true);

            Func nodeNew = new Func("newNode", false);
            Func nodeGetNext = new Func("nodeGetNext", false);
            Func nodeGetPrev = new Func("nodeGetPrev", false);
            Func nodeGetData = new Func("nodeGetData", false);
            Func nodeSetNext = new Func("nodeSetNext", false);
            Func nodeSetPrev = new Func("nodeSetPrev", false);
            Func nodeSetData = new Func("nodeSetData", false);

            Func listGetHead = new Func("listGetHead", false);
            Func listGetTail = new Func("listGetTail", false);
            Func listGetTemp = new Func("listGetTemp", false);
            Func listGetTempRef = new Func("listGetTempRef", false);
            Func listSetHead = new Func("listSetHead", false);
            Func listSetTail = new Func("listSetTail", false);
            Func listSetTemp = new Func("listSetTemp", false);

            Func listNew = new Func("listNew", true);
            Func listAdd = new Func("listAdd", true);
            Func listSum = new Func("listSum", true);

            allocate.SetInputParameters(Types.i32);
            allocate.SetOutputParameters(Types.i32);

            nodeNew.SetInputParameters(Types.i32);
            nodeNew.SetOutputParameters(Types.i32);
            nodeGetNext.SetInputParameters(Types.i32);
            nodeGetNext.SetOutputParameters(Types.i32);
            nodeGetPrev.SetInputParameters(Types.i32);
            nodeGetPrev.SetOutputParameters(Types.i32);
            nodeGetData.SetInputParameters(Types.i32);
            nodeGetData.SetOutputParameters(Types.i32);

            nodeSetNext.SetInputParameters(Types.i32, Types.i32);
            nodeSetPrev.SetInputParameters(Types.i32, Types.i32);
            nodeSetData.SetInputParameters(Types.i32, Types.i32);

            listNew.SetInputParameters(Types.i32);
            listNew.SetOutputParameters(Types.i32);
            listGetTail.SetInputParameters(Types.i32);
            listGetTail.SetOutputParameters(Types.i32);
            listGetHead.SetInputParameters(Types.i32);
            listGetHead.SetOutputParameters(Types.i32);
            listGetTemp.SetInputParameters(Types.i32);
            listGetTemp.SetOutputParameters(Types.i32);
            listGetTempRef.SetInputParameters(Types.i32);
            listGetTempRef.SetOutputParameters(Types.i32);

            listSetHead.SetInputParameters(Types.i32, Types.i32);
            listSetTail.SetInputParameters(Types.i32, Types.i32);
            listSetTemp.SetInputParameters(Types.i32, Types.i32);

            listAdd.SetInputParameters(Types.i32, Types.i32);
            listSum.SetInputParameters(Types.i32);
            listSum.SetOutputParameters(Types.i32);

            allocate.PushCode(Op.i32_const, Encoder.uLEB128(0x00), Op.get_local, Encoder.uLEB128(0x00), Op.i32_const, Encoder.uLEB128(0x01), Op.i32_add, Op.i32_store);
            allocate.PushCode(Op.i32_const, Encoder.uLEB128(0x04), Op.i32_const, Encoder.uLEB128(0x20), Op.i32_store);
            allocate.PushCode(Op.i32_const, Encoder.uLEB128(0x08), Op.i32_const, Encoder.uLEB128(0x00), Op.i32_store);

            allocate.PushCode(Op.block, Op.loop);
            {
                allocate.PushCode(Op.i32_const, Encoder.uLEB128(0x04), Op.i32_load, Op.i32_load, Op.i32_const, Encoder.uLEB128(0x00), Op.i32_eq);

                allocate.PushCode(Op._if);

                allocate.PushCode(Op.i32_const, Encoder.uLEB128(0x08), Op.i32_const, Encoder.uLEB128(0x01), Op.i32_const, Encoder.uLEB128(0x08), Op.i32_load, Op.i32_add, Op.i32_store);
                allocate.PushCode(Op.i32_const, Encoder.uLEB128(0x04), Op.i32_const, Encoder.uLEB128(0x04), Op.i32_const, Encoder.uLEB128(0x04), Op.i32_load, Op.i32_add, Op.i32_store);

                allocate.PushCode(Op._else);

                allocate.PushCode(Op.i32_const, Encoder.uLEB128(0x08), Op.i32_const, Encoder.uLEB128(0x00), Op.i32_store);
                allocate.PushCode(Op.i32_const, Encoder.uLEB128(0x04), Op.i32_const, Encoder.uLEB128(0x04), Op.i32_load, Op.i32_const, Encoder.uLEB128(0x04), Op.i32_load, Op.i32_load);
                allocate.PushCode(Op.i32_const, Encoder.uLEB128(0x04), Op.i32_mul, Op.i32_add, Op.i32_const, Encoder.uLEB128(0x04), Op.i32_add, Op.i32_store);

                allocate.PushCode(Op.end);

                //compare found segment to the required length        		
                allocate.PushCode(Op.i32_const, Encoder.uLEB128(0x08), Op.i32_load, Op.i32_const, Encoder.uLEB128(0x00), Op.i32_load, Op.i32_eq);
                allocate.PushCode(Op.br_if, Encoder.uLEB128(0x01)); //if long enough ,break out of loop and allocate it
                allocate.PushCode(Op.br, Encoder.uLEB128(0x00)); //else keep searching
            }
            allocate.PushCode(Op.end, Op.end);

            allocate.PushCode(Op.i32_const, Encoder.uLEB128(0x04), Op.i32_const, Encoder.uLEB128(0x04), Op.i32_load, Op.i32_const, Encoder.uLEB128(0x08), Op.i32_load, Op.i32_const, Encoder.uLEB128(0x04), Op.i32_mul, Op.i32_sub, Op.i32_store);

            allocate.PushCode(Op.i32_const, Encoder.uLEB128(0x04), Op.i32_load, Op.i32_const, Encoder.uLEB128(0x00), Op.i32_load, Op.i32_const, Encoder.uLEB128(0x01), Op.i32_sub, Op.i32_store);

            //load new candidate address
            allocate.PushCode(Op.i32_const, Encoder.uLEB128(0x04), Op.i32_load);

            //*********************************************************************************************************************************************************

            nodeNew.PushCode(Op.i32_const, Encoder.uLEB128(0x0c), Op.i32_const, Encoder.uLEB128(0x03), Op.call, Encoder.uLEB128(allocate.GetIndex()), Op.i32_store); //call allocate function
            nodeNew.PushCode(Op.i32_const, Encoder.uLEB128(0x0c), Op.i32_load, Op.i32_const, Encoder.uLEB128(0x00), Op.call, Encoder.uLEB128(nodeSetNext.GetIndex()));
            nodeNew.PushCode(Op.i32_const, Encoder.uLEB128(0x0c), Op.i32_load, Op.i32_const, Encoder.uLEB128(0x00), Op.call, Encoder.uLEB128(nodeSetPrev.GetIndex()));
            nodeNew.PushCode(Op.i32_const, Encoder.uLEB128(0x0c), Op.i32_load, Op.get_local, Encoder.uLEB128(0x00), Op.call, Encoder.uLEB128(nodeSetData.GetIndex()));
            nodeNew.PushCode(Op.i32_const, Encoder.uLEB128(0x0c), Op.i32_load); //return pointer to this.

            nodeGetNext.PushCode(Op.get_local, Encoder.uLEB128(0x00), Op.i32_const, Encoder.uLEB128(0x04), Op.i32_add, Op.i32_load);
            nodeGetPrev.PushCode(Op.get_local, Encoder.uLEB128(0x00), Op.i32_const, Encoder.uLEB128(0x08), Op.i32_add, Op.i32_load);
            nodeGetData.PushCode(Op.get_local, Encoder.uLEB128(0x00), Op.i32_const, Encoder.uLEB128(0x0c), Op.i32_add, Op.i32_load);

            nodeSetNext.PushCode(Op.get_local, Encoder.uLEB128(0x00), Op.i32_const, Encoder.uLEB128(0x04), Op.i32_add, Op.get_local, Encoder.uLEB128(0x01), Op.i32_store);
            nodeSetPrev.PushCode(Op.get_local, Encoder.uLEB128(0x00), Op.i32_const, Encoder.uLEB128(0x08), Op.i32_add, Op.get_local, Encoder.uLEB128(0x01), Op.i32_store);
            nodeSetData.PushCode(Op.get_local, Encoder.uLEB128(0x00), Op.i32_const, Encoder.uLEB128(0x0c), Op.i32_add, Op.get_local, Encoder.uLEB128(0x01), Op.i32_store);


            listNew.PushCode(Op.i32_const, Encoder.uLEB128(0x10), Op.i32_const, Encoder.uLEB128(0x03), Op.call, Encoder.uLEB128(allocate.GetIndex()), Op.i32_store);

            listNew.PushCode(Op.i32_const, Encoder.uLEB128(0x10), Op.i32_load, Op.i32_const, Encoder.uLEB128(0x04), Op.i32_add);
            listNew.PushCode(Op.get_local, Encoder.uLEB128(0x00), Op.call, Encoder.uLEB128(nodeNew.GetIndex()), Op.i32_store);

            listNew.PushCode(Op.i32_const, Encoder.uLEB128(0x10), Op.i32_load, Op.i32_const, Encoder.uLEB128(0x08), Op.i32_add, Op.i32_const, Encoder.uLEB128(0x10), Op.i32_load, Op.i32_const, Encoder.uLEB128(0x04), Op.i32_add, Op.i32_load, Op.i32_store);
            listNew.PushCode(Op.i32_const, Encoder.uLEB128(0x10), Op.i32_load, Op.i32_const, Encoder.uLEB128(0x0c), Op.i32_add, Op.i32_const, Encoder.uLEB128(0x10), Op.i32_load, Op.i32_const, Encoder.uLEB128(0x04), Op.i32_add, Op.i32_load, Op.i32_store);
            listNew.PushCode(Op.i32_const, Encoder.uLEB128(0x10), Op.i32_load); //return pointer to this


            listGetHead.PushCode(Op.get_local, Encoder.uLEB128(0x00), Op.i32_const, Encoder.uLEB128(0x04), Op.i32_add, Op.i32_load);
            listGetTail.PushCode(Op.get_local, Encoder.uLEB128(0x00), Op.i32_const, Encoder.uLEB128(0x08), Op.i32_add, Op.i32_load);
            listGetTemp.PushCode(Op.get_local, Encoder.uLEB128(0x00), Op.i32_const, Encoder.uLEB128(0x0c), Op.i32_add, Op.i32_load);
            listGetTempRef.PushCode(Op.get_local, Encoder.uLEB128(0x00), Op.i32_const, Encoder.uLEB128(0x0c), Op.i32_add);

            listSetHead.PushCode(Op.get_local, Encoder.uLEB128(0x00), Op.i32_const, Encoder.uLEB128(0x04), Op.i32_add, Op.get_local, Encoder.uLEB128(0x01), Op.i32_store);
            listSetTail.PushCode(Op.get_local, Encoder.uLEB128(0x00), Op.i32_const, Encoder.uLEB128(0x08), Op.i32_add, Op.get_local, Encoder.uLEB128(0x01), Op.i32_store);
            listSetTemp.PushCode(Op.get_local, Encoder.uLEB128(0x00), Op.i32_const, Encoder.uLEB128(0x0c), Op.i32_add, Op.get_local, Encoder.uLEB128(0x01), Op.i32_store);

            listAdd.PushCode(Op.get_local, Encoder.uLEB128(0x00), Op.get_local, Encoder.uLEB128(0x01), Op.call, Encoder.uLEB128(nodeNew.GetIndex()), Op.call, Encoder.uLEB128(listSetTemp.GetIndex()));
            listAdd.PushCode(Op.get_local, Encoder.uLEB128(0x00), Op.call, Encoder.uLEB128(listGetTemp.GetIndex()), Op.get_local, Encoder.uLEB128(0x00), Op.call, Encoder.uLEB128(listGetTail.GetIndex()), Op.call, Encoder.uLEB128(nodeSetPrev.GetIndex()));
            listAdd.PushCode(Op.get_local, Encoder.uLEB128(0x00), Op.call, Encoder.uLEB128(listGetTail.GetIndex()), Op.get_local, Encoder.uLEB128(0x00), Op.call, Encoder.uLEB128(listGetTemp.GetIndex()), Op.call, Encoder.uLEB128(nodeSetNext.GetIndex()));
            listAdd.PushCode(Op.get_local, Encoder.uLEB128(0x00), Op.get_local, Encoder.uLEB128(0x00), Op.call, Encoder.uLEB128(listGetTemp.GetIndex()), Op.call, Encoder.uLEB128(listSetTail.GetIndex()));

            listSum.PushCode(Op.i32_const, Encoder.uLEB128(0x00), Op.i32_const, Encoder.uLEB128(0x00), Op.i32_store);
            listSum.PushCode(Op.get_local, Encoder.uLEB128(0x00), Op.get_local, Encoder.uLEB128(0x00), Op.call, Encoder.uLEB128(listGetHead.GetIndex()), Op.call, Encoder.uLEB128(listSetTemp.GetIndex()));

            listSum.PushCode(Op.block, Op.loop);
            {
                listSum.PushCode(Op.i32_const, Encoder.uLEB128(0x00));
                {
                    listSum.PushCode(Op.get_local, Encoder.uLEB128(0x00), Op.call, Encoder.uLEB128(listGetTemp.GetIndex()), Op.call, Encoder.uLEB128(nodeGetData.GetIndex()), Op.i32_const, Encoder.uLEB128(0x00), Op.i32_load, Op.i32_add);
                }
                listSum.PushCode(Op.i32_store);

                listSum.PushCode(Op.get_local, Encoder.uLEB128(0x00), Op.get_local, Encoder.uLEB128(0x00));
                listSum.PushCode(Op.call, Encoder.uLEB128(listGetTemp.GetIndex()), Op.call, Encoder.uLEB128(nodeGetNext.GetIndex()), Op.call, Encoder.uLEB128(listSetTemp.GetIndex()));

                listSum.PushCode(Op.get_local, Encoder.uLEB128(0x00), Op.call, Encoder.uLEB128(listGetTemp.GetIndex()), Op.i32_const, Encoder.uLEB128(0x00), Op.i32_eq);

                listSum.PushCode(Op.br_if, Encoder.uLEB128(0x01)); //if long enough ,break out of loop and allocate it
                listSum.PushCode(Op.br, Encoder.uLEB128(0x00)); //else keep searching
            }
            listSum.PushCode(Op.end, Op.end);

            listSum.PushCode(Op.i32_const, Encoder.uLEB128(0x00), Op.i32_load);

            bin.AddFunction(allocate);

            bin.AddFunction(nodeNew);
            bin.AddFunction(nodeGetNext);
            bin.AddFunction(nodeGetPrev);
            bin.AddFunction(nodeGetData);
            bin.AddFunction(nodeSetNext);
            bin.AddFunction(nodeSetPrev);
            bin.AddFunction(nodeSetData);

            bin.AddFunction(listNew);
            bin.AddFunction(listGetHead);
            bin.AddFunction(listGetTail);
            bin.AddFunction(listGetTemp);
            bin.AddFunction(listGetTempRef);
            bin.AddFunction(listSetHead);
            bin.AddFunction(listSetTail);
            bin.AddFunction(listSetTemp);

            bin.AddFunction(listAdd);
            bin.AddFunction(listSum);

            return bin;
        }

        public static void LoadSource(string src)
        {
        }

        public static void GenerateAst()
        {
        }
    }
}