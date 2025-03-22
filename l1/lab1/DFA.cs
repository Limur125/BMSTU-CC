using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace lab1
{
    public class DFA
    {
        public int startState = 0;
        public HashSet<int> finishStates = [];
        public List<DFAState> Dstates = [];
        public Dictionary<int, Dictionary<string, int>> Dtran = [];
        public HashSet<string> Alphabet = [];

        public DFA() { }
        public DFA(Node tree, Dictionary<int, string> indexedStates, Dictionary<int, HashSet<int>> treeFollowpos)
        {
            foreach (var s in indexedStates)
            {
                Alphabet.Add(s.Value);
            }
            int i = 0;
            Dstates.Add(new (tree.firstpos, i++));
            while (Dstates.Any(s => s.mark == false))
            {
                var state = Dstates.First(s => s.mark == false);
                state.mark = true;
                foreach (var symbol in Alphabet)
                {
                    var fins = indexedStates.Where(ins => ins.Value == symbol).Select(ins => ins.Key).ToHashSet();
                    HashSet<int> U = [];
                    var flag = false;
                    foreach (var p in state.states)
                    {
                        if (!fins.Contains(p))
                            continue;
                        if (symbol == "#")
                        {
                            finishStates.Add(state.index);
                            break;
                        }
                        var ps = treeFollowpos[p];
                        U = [.. U, .. ps];
                    }
                    if (flag) continue;
                    DFAState newState = new(U, i);
                    if (U.Count == 0)
                        continue;
                    var oldState = Dstates.FirstOrDefault(s => s == newState);
                    if (oldState is null)
                    {
                        Dstates.Add(newState);
                        i++;
                    }
                    else
                    {
                        newState = oldState;
                    }

                    if (!Dtran.ContainsKey(state.index))
                        Dtran[state.index] = [];
                    Dtran[state.index][symbol] = newState.index;

                }
            }
        }

        public bool Accept(string data)
        {
            var curState = startState;
            for (int i = 0; i < data.Length; i++)
            {
                Console.Write("|-{0}", data[i..]);
                int toState = Dtran.GetValueOrDefault(curState, []).GetValueOrDefault(data[i].ToString(), -1);
                if (toState == -1)
                {
                    Console.WriteLine("|-нет");
                    return false;
                }
                curState = toState;
            }
            if (!finishStates.Contains(curState))
            {
                Console.WriteLine("|-нет");
                return false;
            }
            Console.WriteLine("|-да");
            return true;
        }
    }

    public class DFAState(HashSet<int> states, int index) 
    {
        public int index = index;
        public HashSet<int> states = states;
        public bool mark = false;

        public static bool operator ==(DFAState a, DFAState b)
        {
            if (a.states.Count != b.states.Count) 
                return false;
            foreach(var s in a.states)
                if (!b.states.Contains(s))
                    return false;
            return true;
        }

        public static bool operator !=(DFAState a, DFAState b)
        {
            if (a.states.Count == b.states.Count)
                return false;
            foreach (var s in a.states)
                if (!b.states.Contains(s))
                    return true;
            return false;
        }
    }
}
