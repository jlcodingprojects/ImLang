using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ImLang;

namespace ImLang
{
    public class Parser
    {
        private List<Token> _tokens;
        private List<Statement> statements;

        int _currentToken = 0;

        public Parser(List<Token> tokens)
        {
            _tokens = new List<Token>();
            statements = new List<Statement>();

            foreach (Token t in tokens)
            {
                //_tokens.Add(t.Clone());
            }
        }

        public void Dump()
        {
            Console.WriteLine("Statement count: {0}", statements.Count);
            foreach (Statement s in statements)
            {
                Console.WriteLine(s.ToString());
            }
        }

        public List<Statement> GetStatements()
        {
            return statements;
        }

        private void eat()
        {
            _currentToken++;
        }

        public void Parse()
        {
            //foreach (Token token in _tokens)
            //{

            //    if (token.Group != TokenCode.Whitespace)
            //    {
            //        Console.WriteLine("Group: {0}, {2}, Value: |{1}|", token.GroupName, token.Value, token.Group);
            //        //Console.Write(t.Value);
            //    }
            //}

            //while (_currentToken < _tokens.Count)
            //{
            //    Console.WriteLine(_currentToken);
            //    parseStatement();
            //}
        }

        private void parseStatement()
        {
            ////if|else|for|while|object|var|return|new|int|float|long|double)
            //if (_tokens[_currentToken].Group == TokenCode.Keyword)
            //{
            //    Statement s = new Statement(_tokens[_currentToken]);
            //    eat(); //keyword
            //    parseExpressionRaw(s);

            //    eat(); //;

            //    statements.Add(s);
            //}
        }

        private void parseExpression(Statement s)
        {
            //while (_tokens[_currentToken].Value != ";")
            //{
            //    if (_tokens[_currentToken].Value == "(")
            //    {
            //        s.PushChild(_tokens[_currentToken]); //temporary placeholder, will be updated to binaryExpression next step
            //        s.Follow();
            //        eat();
            //    }
            //    if (_tokens[_currentToken].Group == TokenCode.Number)
            //    {
            //        s.PushChild(_tokens[_currentToken]); //temporary placeholder, will be updated to binaryExpression next step
            //        eat();
            //    }
            //    if (_tokens[_currentToken].Value == ")")
            //    {
            //        Console.WriteLine("Stepping up... : " + _tokens[_currentToken].Value);
            //        s.StepUp(); //back up a step
            //        eat();
            //    }
            //    if (_tokens[_currentToken].Group == TokenCode.Expression)
            //    {
            //        Console.WriteLine("SYMBOL: " + _tokens[_currentToken].Value);
            //        //s.StepUp();
            //        s.PushCurrent(_tokens[_currentToken]); //now update to binaryExpression
            //        eat();
            //    }
            //    //Console.WriteLine();
            //    //eat(); //parameter
            //}
        }
        private void parseExpressionRaw(Statement s)
        {
        //    while (_tokens[_currentToken].Value != ";")
        //    {
        //        if (_tokens[_currentToken].Group == TokenCode.Number)
        //        {
        //            if (s.Current.Details.Group == TokenCode.Keyword) //initial number
        //            {
        //                s.PushChild(_tokens[_currentToken]);
        //                s.Follow();
        //                eat();
        //            }
        //            else
        //            {
        //                s.PushChild(_tokens[_currentToken]);
        //                s.Follow();
        //                eat();
        //            }
        //        }

        //        if (_tokens[_currentToken].Group == TokenCode.Expression)
        //        {
        //            if (s.Current.Parent.Details.Group != TokenCode.Expression) //only 1 number in tree
        //            {
        //                s.PushChild(_tokens[_currentToken]); //now update to binaryExpression
        //                s.Current.RotateLeft();
        //                eat();
        //                //} else if (Tokeniser.OperatorPrecedence(tokens[currentToken], s.Current.Parent.Details)) {
        //                //s.PushChild(tokens[currentToken]); //right side precedence
        //                //s.Current.RotateLeft();
        //                //eat();
        //            }
        //            else
        //            { //left side precedence
        //                s.StepUp();
        //                s.InsertParent(_tokens[_currentToken]);
        //                s.StepUp();
        //                //s.PushChild(); //right side precedence
        //                //s.Current.RotateLeft();
        //                eat();
        //            }

        //        }
        //        //Console.WriteLine();
        //        //eat(); //parameter
        //    }
        }
    }
}
