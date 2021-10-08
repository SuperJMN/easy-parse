﻿using EasyParse.Parsing.Rules.Symbols;

namespace EasyParse.Parsing.Rules
{
    public abstract class Symbol
    {
        public abstract ParserGenerator.Models.Symbols.Symbol ToSymbolModel();

        public static implicit operator Symbol(string value) =>
            new LiteralSymbol(value);
    }
}
