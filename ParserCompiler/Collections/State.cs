﻿using System;
using System.Collections.Generic;
using System.Linq;
using ParserCompiler.Models;
using ParserCompiler.Models.Rules;

namespace ParserCompiler.Collections
{
    public class State : IEquatable<State>
    {
        private Set<Rule> Rules { get; }
        private Set<FirstSet> FirstSets { get; }
        public Set<StateElement> Elements { get; }

        public Set<Progression> Core { get; }

        public State(IEnumerable<Rule> rules, Set<FirstSet> firstSets, Set<FollowSet> followSets)
        {
            List<Rule> rulesList = new List<Rule>(rules);
            this.Rules = rulesList.AsSet();
            this.FirstSets = firstSets;
            this.Elements = rulesList.Select(rule => rule.ToProgression().ToStateElement(followSets)).AsSet();
            this.Core = this.Elements.Select(element => element.Progression).AsSet();
        }

        private State(State copy, IEnumerable<StateElement> elements)
        {
            this.Rules = copy.Rules;
            this.FirstSets = copy.FirstSets;
            this.Elements = elements.AsSet();
            this.Core = this.Elements.Select(element => element.Progression).AsSet();
        }

        public IEnumerable<Transition> Advance() =>
            this.Elements
                .SelectMany(element => element.Advance())
                .GroupBy(move => move.consumed, move => move.rest)
                .Select(group => new Transition(this, group.Key, new State(this, group)))
                .Select(transition => transition.Closure());

        private State Expand() =>
            this.Elements
                .SelectMany(element => element.PeekNonTerminal(this.FirstSets))
                .SelectMany(tuple => this.Rules.Where(rule => rule.Head.Equals(tuple.next)).Select(rule => (rule: rule, follow: tuple.follow)))
                .Select(tuple => new StateElement(tuple.rule.ToProgression(), tuple.follow))
                .Aggregate(this, (state, element) => state.Append(element));

        private State Append(StateElement element) =>
            new State(this, this.Elements.Union(element));

        public State Union(State other) =>
            new State(this, this.Elements.Union(other.Elements));

        public State Closure()
        {
            State result = this;
            while (result.Expand() is State next && !next.Equals(result))
            {
                result = next;
            }
            return result;
        }

        public override string ToString() => Formatting.ToString(this);

        public override bool Equals(object obj) =>
            this.Equals(obj as State);

        public bool Equals(State other) =>
            !(other is null) &&
            other.Elements.Equals(this.Elements);

        public override int GetHashCode() => 
            this.Elements.GetHashCode();
    }
}
