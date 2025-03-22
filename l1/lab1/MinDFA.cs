using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lab1
{
    public class MinDFA : DFA
    {
        public MinDFA(DFA dfa)
        {
            Dictionary<int, Dictionary<string, HashSet<int>>> reverseDtran = [];
            reverseDtran[-1] = [];
            foreach (var state in dfa.Dstates)
            {
                foreach (var symbol in dfa.Alphabet)
                {
                    if (dfa.Dtran.TryGetValue(state.index, out var value) && value.TryGetValue(symbol, out int fStateI))
                    {
                        if (!reverseDtran.ContainsKey(fStateI))
                            reverseDtran[fStateI] = [];
                        if (!reverseDtran[fStateI].ContainsKey(symbol))
                            reverseDtran[fStateI][symbol] = [];
                        reverseDtran[fStateI][symbol].Add(state.index);
                    }
                    else
                    {
                        if (!reverseDtran[-1].ContainsKey(symbol))
                            reverseDtran[-1][symbol] = [];
                        reverseDtran[-1][symbol].Add(state.index);
                    }
                }
            }
            foreach (var sym in dfa.Alphabet)
            {
                if (!reverseDtran[-1].ContainsKey(sym))
                    reverseDtran[-1][sym] = [];
                reverseDtran[-1][sym].Add(-1);
            }

            List<DFAState> states = [new([], -1), .. dfa.Dstates];
            Dictionary<int, Dictionary<int, bool>> marked = [];
            foreach (var statei in states)
            {
                marked[statei.index] = [];
                foreach (var statej in states)
                {
                    marked[statei.index][statej.index] = false;
                }
            }
            Queue<KeyValuePair<int, int>> queue = [];

            foreach (var stateI in states)
            {
                foreach (var stateJ in states)
                {
                    if (!marked[stateI.index][stateJ.index] && dfa.finishStates.Contains(stateI.index) != dfa.finishStates.Contains(stateJ.index))
                    {
                        marked[stateI.index][stateJ.index] = true;
                        marked[stateJ.index][stateI.index] = true;
                        queue.Enqueue(new (stateI.index, stateJ.index));
                    }
                }
            }

            while (queue.Count != 0)
            {
                var pair = queue.Dequeue();
                var u = pair.Key;
                var v = pair.Value;

                foreach (var c in dfa.Alphabet)
                {
                    if (reverseDtran.TryGetValue(u, out var trans1) && trans1.TryGetValue(c, out var rSet))
                    {
                        foreach (var r in rSet)
                        {
                            if (reverseDtran.TryGetValue(v, out var trans2) && trans2.TryGetValue(c, out var sSet))
                            {
                                foreach (var s in sSet)
                                {
                                    if (!marked[r][s])
                                    {
                                        marked[r][s] = true;
                                        marked[s][r] = true;
                                        queue.Enqueue(new(r, s));
                                    }
                                }
                            }
                        }
                    }
                }
            }


            int newIndex = 0;
            if (marked[-1].Values.Where(v => !v).Count() == 1)
                marked[-1][-1] = true;
            foreach (var keyI in marked.Keys)
            {
                HashSet<int> U = [];
                foreach (var keyJ in marked[keyI].Keys)
                {
                    if (!marked[keyI][keyJ])
                    {
                        U.Add(keyJ);
                    }
                }
                DFAState newState = new(U, newIndex);
                if (U.Count == 0)
                    continue;
                var oldState = Dstates.FirstOrDefault(s => s == newState);
                if (oldState is null)
                {
                    Dstates.Add(newState);
                    newIndex++;
                }
                else
                {
                    newState = oldState;
                }
            }

            foreach (var state in Dstates)
            {
                if (state.states.Contains(dfa.startState))
                    startState = state.index;
                foreach (var s in state.states) 
                {
                    if (dfa.finishStates.Contains(s))
                        finishStates.Add(state.index);
                }
            }

            foreach (var state in Dstates)
            {
                foreach (var oldState in state.states)
                {
                    if (reverseDtran.TryGetValue(oldState, out var tranSymbol))
                    {
                        foreach (var statesFrom in tranSymbol) {
                            foreach (var s in statesFrom.Value)
                            {
                                var newState = Dstates.FirstOrDefault(ns => ns.states.Contains(s));
                                if (newState is not null)
                                {
                                    if (!Dtran.ContainsKey(newState.index))
                                        Dtran[newState.index] = [];
                                    Dtran[newState.index][statesFrom.Key] = state.index;
                                }
                            }
                        }
                    }
                }
            }
        }
    }   
}

