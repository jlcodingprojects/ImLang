//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace ImLang
//{
//    public interface INode { }

//    // Program made up of nodes
//    // Nodes: Declarations and Statements
//    // Declarations: Variables and Functions
//    // Statements: variable assignment, function calls, if statements

//    // DECLARATIONS
//    // VariableDeclaration: Format is always "vartype (int|str|decimal) Identifier = Expression;"
//    // FunctionDeclaration: Format is always "fn vartype (int|str|decimal|void) Identifier(arg0, arg1, ...) { Nodes[]; return Expression }"

//    // STATEMENTS
//    // VariableAssignment: Format is always "Identifier = Expression;"
//    // FunctionCall: Format is  "Identifier(Expression);" - Note: a FunctionCall is both a statement and an expression
//    // IfStatement: Format is "If (Expression) { Nodes[]; }"

//    // Expression: Evaluated, either a Literal, an Identifier, a BinaryExpression, or a FunctionCall
//    // Binary expression is simply a function call with two params, in the format LEFT OP RIGHT
//    // FunctionCall is in the format FN Param1, Param2, ...

//    public class Statement : INode
//    {

//    }

//    public class Declaration : INode
//    {
//        public Identifier Id { get; set; } = new();
//    }

//    public class Expression
//    {
//        public VarType ReturnType { get; set; } = VarType.Int;
//    }

//    public enum VarType
//    {
//        Void,
//        Int,
//        Decimal,
//        Str,
//    }

//    public class VariableDeclaration : Declaration
//    {
//        public Statement Init { get; set; } = new();
//    }

//    public class FunctionDeclaration : Declaration
//    {
//        public List<Statement> Body{ get; set; } = new();
//        public List<Identifier> Args { get; set; } = new();
//        public VarType ReturnType { get; set; }
//    }

//    // Identifier = ;
//    public class VariableAssignment : Statement
//    {
//        public Identifier Id { get; set; } = new();
//    }

//    public class BinaryExpression : Statement
//    {

//    }


//    public class Identifier
//    {
//        public string Value { get; set; } = string.Empty;
//    }

//    public class CallExpression : Expression
//    {
//        public Identifier Id = new();
//        public List<INode> Args = new();
//    }

//    public class Literal : INode
//    {

//    }

//    public class StringLiteral : Literal
//    {
//        public string Value { get; set; } = string.Empty;
//    }

//    public class NumericLiteral : Literal
//    {
//        public int Value { get; set; } = 0;
//    }

//    public enum ReturnType
//    {
//        Void,
//        Int,
//        String,
//    }


//    public class StatementBlock : INode
//    {
//        public List<Statement> Statements { get; set; } = new();
//    }
//}
