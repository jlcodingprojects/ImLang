# Overview

1. Break source into tokens

2. Parse into AST

3. Emit binary

## Tokeniser

This is relatively simple. It takes a source file as a string and splits it into each viable token. Valid tokens are as follows:

- Whitespace - Space, CR

- Keywords - 

- Dividers, 