# Overview

ImLang is a simple programming with just a few constructs.
It is top level statements with variables, function calls, and if/while for control 

Available constructs are as follows:
Declarations: Variables and Functions
Statements: variable assignment, function calls, if/while statements

# Parser, AST, Emitter

# Syntax is as follows:

## Declarations

VariableDeclaration
- var vartype Identifier = Expression;
FunctionDeclaration
- fn vartype Identifier(arg0, arg1, ...) { Nodes[]; return Expression; }

## Statements

VariableAssignment
- Identifier = Expression;
FunctionCall
- Identifier(Expression);
- - Both statement or expression
IfStatement
- If (Expression) { Nodes[]; }

## Expressions

Identifier

Literal

FunctionCall (param0, param1, ...)

BinaryExpression
- Expression OPERATOR Expression

Expressions are stored as an ExpressionTree which contains Nodes (functions/operators) and Leafs (Literals)


# Valid Vartypes

- int32
- int64
- float32
- float64
#- String # not yet implemented
- Void

# Valid Operators

- +
- -
- /
- *
