﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using EasyParse.LexicalAnalysis.Tokens;
using EasyParse.Parsing;

namespace EasyParse.CalculatorDemo
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            Parser parser = Parser.FromXmlResource(Assembly.GetExecutingAssembly(), "EasyParse.CalculatorDemo.ParserDefinition.xml");
            Parser addingParser = Parser.FromXmlResource(Assembly.GetExecutingAssembly(), "EasyParse.CalculatorDemo.AdditionParserDefinition.xml");

            Console.WriteLine("Enter expressions to evaluate (blank line to exit):");
            foreach (string line in Console.In.ReadLinesUntil(string.Empty))
            {
                ProcessAddition(addingParser, line);
                Process(parser, line);
            }
        }

        private static void ProcessAddition(Parser parser, string line)
        {
            ParsingResult result = parser.Parse(line);
            if (result.IsSuccess)
                Console.WriteLine(result);
            else
                Console.WriteLine($"Not an additive expression: {result.ErrorMessage}");
        }

        private static void Process(Parser parser, string line)
        {
            List<Token> tokens = parser.Lexer.Tokenize(line).ToList();
            string tokensReport = string.Join(" ", tokens.Select(x => $"{x}"));
            
            Console.WriteLine();
            Console.WriteLine($"Tokens: {tokensReport}");
            
            ParsingResult result = parser.Parse(line);
            Console.WriteLine();
            Console.WriteLine($"Syntax tree:{Environment.NewLine}{result}");

            Console.WriteLine();
            Console.WriteLine($"{line} = {(result.IsSuccess ? result.Compile(Calculator) : result.ErrorMessage)}");
            Console.WriteLine(new string('-', 50));
        }

        private static ICompiler Calculator => new Calculator();
    }
}
