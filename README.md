# ImmortalLang
My goal is to create my own programming language, parser, and compiler. I'm starting out following this tutorial: http://blog.scottlogic.com/2019/05/17/webassembly-compiler.html

## Usage

Compile and run. It will compile the file example.iml by default. Supply filename in parameter

- ImLang.exe filename.iml

It will create filename.htm which is a template with the .wasm file embedded. Open to see a basic UI for running functions

## Example ImLang code:

fn add(int32 left, int32 right)
{
  int32 result = left + right;
  if(result == fortytwo())
  {
    result = 1337;
  }

  return result;
}

fn fortytwo()
{
  return 42;
}

fn drawit(int32 x, int32 y)
{
  draw(x, y, 150);
}

## Current Features:

- Binary expressions for function calls, variables, and literals

- i32 datatype. No other datatypes are currently supported

## Shortcomings

- All variables are static global. This will be changed in a future update so they are declared in scope and cleared out after
- - Unfortunately this means recursion isnt currently possible


## Eventual Features:
- Basic var types (int, float, double, long, array, pointer)
- Simple classes (properties and methods)
- Control flow (if/else if/else)
- Loops (for, while)
- Some arithmetic functions (bitwise operators, exponentiation etc)

Ideally I will implement all the features required to build the ImmortalLang compiler, then rewrite it in itself (I mean, all the best languages are written in themselves so for ImmortalLang to truely reach that level of immortality it is basically a requirement).

## Next steps:
`[DONE]` Figure out how local variables work (instead of allocating part of the memory to each function variable).

`[DONE]` Begin parsing simple .imlang code and figure out how to translate that into wasm.

`[DONE]` Binary expression parsing

`[Todo]` Control flow statements

`[Todo]` Add variables

`[Todo]` Functions/classes
