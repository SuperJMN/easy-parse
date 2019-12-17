﻿using System.Collections.Generic;
using System.Linq;
using ParserCompiler.Models.Rules;
using ParserCompiler.Models.Transitions;

namespace ParserCompiler.Collections
{
    public class ParsingTable
    {
        private IDictionary<Set<Progression>, int> CoreToIndex { get; }
        private IDictionary<Rule, int> RuleToIndex { get; }
        public ShiftTable Shift { get; }
        public GotoTable Goto { get; }
        public ReduceTable Reduce { get; }

        public ParsingTable(List<State> parserStates, List<Rule> rules)
        {
            this.CoreToIndex = parserStates
                .Select((state, index) => (state, index))
                .ToDictionary(tuple => tuple.state.Core, tuple => tuple.index);

            this.RuleToIndex = rules
                .Select((rule, index) => (rule, index))
                .ToDictionary(tuple => tuple.rule, tuple => tuple.index);

            this.Shift = new ShiftTable();
            this.Goto = new GotoTable();
            this.Reduce = new ReduceTable();
        }

        private ParsingTable(ParsingTable table, ShiftTable shift, GotoTable @goto, ReduceTable reduce)
        {
            this.CoreToIndex = table.CoreToIndex;
            this.RuleToIndex = table.RuleToIndex;
            this.Shift = shift;
            this.Goto = @goto;
            this.Reduce = reduce;
        }

        public ParsingTable Add(CoreTransition transition) =>
            new ParsingTable(this, this.Shift.TryAdd(transition, this.CoreToIndex), this.Goto.TryAdd(transition, this.CoreToIndex), this.Reduce);

        public ParsingTable AddRange(IEnumerable<CoreReduction> reductions) =>
            this.AddRange(reductions.Select(reduction => new ReduceCommand(this.CoreToIndex[reduction.From], reduction.Symbol, this.RuleToIndex[reduction.To])));

        private ParsingTable AddRange(IEnumerable<ReduceCommand> reductions) =>
            new ParsingTable(this, this.Shift, this.Goto, this.Reduce.AddRange(reductions));

        public override string ToString() => Formatting.ToString(this);
    }

}
