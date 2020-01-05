﻿using System;
using System.Reflection;
using EasyParse.LexicalAnalysis;
using EasyParse.Parsing;

namespace EasyParse.Testing
{
    public abstract class ParserTestsBase
    {
        protected abstract Assembly XmlDefinitionAssembly { get; }
        protected abstract string XmlDefinitionResourceName { get; }
        protected virtual Func<Lexer, Lexer> LexicalRules { get; } = lexer => lexer;

        protected bool Recognized(string input) =>
            this.CreateParser().Parse(input).IsSuccess;

        protected bool Recognized(params string[] lines) =>
            this.CreateParser().Parse(lines).IsSuccess;

        protected object Compiled(ICompiler compiler, string input) =>
            this.CreateParser().Parse(input).Compile(compiler);

        protected object Compiled(ICompiler compiler, params string[] lines) =>
            this.CreateParser().Parse(lines).Compile(compiler);

        protected object CompiledLine(ICompiler compiler, string input) =>
            this.CreateParser().Parse(input).Compile(compiler);

        protected T Compiled<T>(ICompiler compiler, params string[] lines) where T : class =>
            (T) this.Compiled(compiler, lines);

        protected T Compiled<T>(ICompiler compiler, Action<object> orElse, params string[] lines) where T : class => 
            this.Compiled<T>(this.Compiled(compiler, lines), orElse);

        protected T CompiledLine<T>(ICompiler compiler, Action<object> orElse, string input) where T : class =>
            this.Compiled<T>(this.Compiled(compiler, input), orElse);

        private T Compiled<T>(object result, Action<object> orElse) where T : class
        {
            if (result is T success) return success;
            orElse(result);
            throw new InvalidOperationException($"Could not compile {result?.GetType().Name ?? "<null>"} into {typeof(T).Name}.");
        }

        private Parser CreateParser() =>
            Parser.FromXmlResource(this.XmlDefinitionAssembly, this.XmlDefinitionResourceName, this.LexicalRules);
    }
}
