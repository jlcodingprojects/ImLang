using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImLang
{
    public class Enums
    {
        public enum Datatype
        {
            Int32,
            Int64,
            Float32,
            Float64,
            Void,
        }

        public enum IdentifierType
        {
            Variable,
            Function,
        }

        public enum BinaryOperator
        {
            Add,
            Subtract,
            Divide,
            Multiply,
        }

        public enum ExpressionOperator
        {
            Literal,
            BinaryExpression,
            FunctionCall,
        }

        public enum ReturnType
        {
            Void,
            Int,
            String,
        }
    }
}
