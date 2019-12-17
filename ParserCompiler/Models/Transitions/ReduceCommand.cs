﻿using ParserCompiler.Models.Symbols;

namespace ParserCompiler.Models.Transitions
{
    public class ReduceCommand : Transition<int, Terminal, int> 
    {
        public ReduceCommand(int from, Terminal symbol, int to) : base(from, symbol, to)
        {
        }

        public override string ToString() => $"{base.From},{base.Symbol} -> R{base.To}";
    }
}