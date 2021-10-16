﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text.RegularExpressions;
using EasyParse.Parsing.Rules.Symbols;

namespace EasyParse.Parsing.Rules
{
    internal class IncompleteProductionBuilder : IPendingMapping
    {
        public IncompleteProductionBuilder(ImmutableList<Production> completedLines, NonTerminalName head)
            : this(completedLines, head, ImmutableList<Symbol>.Empty)
        {
        }

        private IncompleteProductionBuilder(
            ImmutableList<Production> completedLines, NonTerminalName head, ImmutableList<Symbol> body)
        {
            this.CompletedLines = completedLines;
            this.Head = head;
            this.Body = body;
        }

        protected ImmutableList<Production> CompletedLines { get; }
        private NonTerminalName Head { get; }
        private ImmutableList<Symbol> Body { get; }

        public IPendingMapping Match(Symbol first, params Symbol[] others) =>
            others.Aggregate(this.Append(first), (rule, symbol) => rule.Append(symbol));

        public IPendingMapping Literal(string value) =>
            this.Append(new LiteralSymbol(value));

        public IPendingMapping Regex<T>(string name, string pattern, Func<string, T> transform) =>
            this.Append(RegexSymbol.Create(name, new Regex(pattern), transform));

        public IPendingMapping Symbol(Func<IRule> factory) =>
            this.Append(new RecursiveNonTerminalSymbol(factory));

        private IncompleteProductionBuilder Append(Symbol symbol) =>
            new(this.CompletedLines, this.Head, this.Body.Add(symbol));

        public IRule To<T1, TResult>(Func<T1, TResult> transform) => this.To<TResult>(transform.DynamicInvoke, typeof(T1));
        public IRule To<T1, T2, TResult>(Func<T1, T2, TResult> transform) => this.To<TResult>(transform.DynamicInvoke, typeof(T1), typeof(T2));
        public IRule To<T1, T2, T3, TResult>(Func<T1, T2, T3, TResult> transform) => this.To<TResult>(transform.DynamicInvoke, typeof(T1), typeof(T2), typeof(T3));
        public IRule To<T1, T2, T3, T4, TResult>(Func<T1, T2, T3, T4, TResult> transform) => this.To<TResult>(transform.DynamicInvoke, typeof(T1), typeof(T2), typeof(T3), typeof(T4));
        public IRule To<T1, T2, T3, T4, T5, TResult>(Func<T1, T2, T3, T4, T5, TResult> transform) => this.To<TResult>(transform.DynamicInvoke, typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5));
        public IRule To<T1, T2, T3, T4, T5, T6, TResult>(Func<T1, T2, T3, T4, T5, T6, TResult> transform) => this.To<TResult>(transform.DynamicInvoke, typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6));
        public IRule To<T1, T2, T3, T4, T5, T6, T7, TResult>(Func<T1, T2, T3, T4, T5, T6, T7, TResult> transform) => this.To<TResult>(transform.DynamicInvoke, typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7));
        public IRule To<T1, T2, T3, T4, T5, T6, T7, T8, TResult>(Func<T1, T2, T3, T4, T5, T6, T7, T8, TResult> transform) => this.To<TResult>(transform.DynamicInvoke, typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8));
        public IRule To<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult> transform) => this.To<TResult>(transform.DynamicInvoke, typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9));
        public IRule To<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult> transform) => this.To<TResult>(transform.DynamicInvoke, typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9), typeof(T10));

        private IRule To<TResult>(Func<object[], object> transform, params Type[] argumentTypes) =>
            this.NonLiteralsCount != argumentTypes.Length ? this.ThrowIncorrectArgumentsCount<IRule>(argumentTypes.Length)
            : this.ContainsLiterals ? this.NonLiteralsTo<TResult>(transform, argumentTypes)
            : this.ToRule<TResult>(transform, argumentTypes);

        private IRule NonLiteralsTo<TResult>(Func<object[], object> transform, Type[] argumentTypes) =>
            this.NonLiteralsTo<TResult>(transform, this.InjectLiteralsTo(argumentTypes).ToArray(), this.NonLiteralIndices);

        private IRule NonLiteralsTo<TResult>(
            Func<object[], object> nonLiteralsTransform, Type[] argumentTypesWithLiterals, int[] argumentsMap)
        {
            object[] ArgumentsPurge(object[] arguments) => 
                argumentsMap.Select(index => arguments[index]).ToArray();

            object AugmentedTransform(object[] arguments) =>
                nonLiteralsTransform(ArgumentsPurge(arguments));

            return this.ToRule<TResult>(AugmentedTransform, argumentTypesWithLiterals);
        }

        private IEnumerable<Type> InjectLiteralsTo(IEnumerable<Type> argumentTypes)
        {
            using IEnumerator<Type> argumentType = argumentTypes.GetEnumerator();

            foreach (Symbol symbol in this.Body)
            {
                if (symbol is LiteralSymbol)
                {
                    yield return typeof(string);
                }
                else
                {
                    argumentType.MoveNext();
                    yield return argumentType.Current;
                }
            }
        }

        private int[] NonLiteralIndices =>
            this.Body
                .Select((symbol, index) => (symbol, index))
                .Where(tuple => tuple.symbol is not LiteralSymbol)
                .Select(tuple => tuple.index)
                .ToArray();

        private int NonLiteralsCount =>
            this.Body.Count(symbol => symbol is not LiteralSymbol);

        private bool ContainsLiterals =>
            this.Body.Any(symbol => symbol is LiteralSymbol);

        private IRule ToRule<TResult>(Func<object[], object> function, Type[] argumentTypes) =>
            new CompletedRule(this.Head, this.AllProductions<TResult>(function, argumentTypes));

        private ImmutableList<Production> AllProductions<TResult>(Func<object[], object> function, Type[] argumentTypes) =>
            this.CompletedLines.Add(this.ToProduction<TResult>(function, argumentTypes));

        private Production ToProduction<TResult>(Func<object[], object> function, Type[] argumentTypes) =>
            new Production(this.Head, this.Body, this.ToTransform<TResult>(function, argumentTypes));

        private Transform ToTransform<TResult>(Func<object[], object> function, Type[] argumentTypes) =>
            this.Body.Count == argumentTypes.Length ? new Transform(typeof(TResult), argumentTypes, function)
            : throw new ArgumentException(this.IncorrectArgumentCountMessage(argumentTypes.Length));

        private T ThrowIncorrectArgumentsCount<T>(int count) =>
            throw new ArgumentException(this.IncorrectArgumentCountMessage(count));

        private string IncorrectArgumentCountMessage(int count) =>
            $"Mapping function receives {count} arguments when expecting " + 
            $"{this.Body.Count()} in rule {this.Head}";
    }
}