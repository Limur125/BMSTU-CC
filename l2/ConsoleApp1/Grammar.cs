using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    public class Grammar(List<string> nonTerm, List<string> term, string firstSymbol, Dictionary<string, List<List<string>>> rules)
    {
        public List<string> NonTerm = nonTerm;
        public List<string> Term = term;
        public string FirstSymbol = firstSymbol;
        public Dictionary<string, List<List<string>>> Rules = rules;

        public Grammar DeleteLeftRecursion()
        {
            var rules = new Dictionary<string, List<List<string>>>(Rules);
            var newNonTerms = new List<string>(NonTerm);
            for (var i = 0; i < NonTerm.Count; i++)
            {
                var aI = NonTerm[i];

                for (var j = 0; j < i; j++)
                {
                    var aJ = NonTerm[j];

                    var aIArray = rules[aI];
                    var aJArray = rules[aJ];
                    List<List<string>> newProductionArray = [];
                    for (var k = 0; k < aIArray.Count; k++)
                    {
                        List<string> newProduction = [];

                        if (aIArray[k][0] == aJ)
                        {
                            foreach (var aJProduction in aJArray)
                            {
                                var aJProductionCopy = new List<string>(aJProduction);
                                if (aJProductionCopy[^1] == "eps")
                                {
                                    aJProductionCopy.RemoveAt(aJProductionCopy.Count - 1);
                                }
                                newProductionArray.Add([.. aJProductionCopy, .. aIArray[k].Skip(1)]);
                            }
                        }
                        else
                        {
                            newProduction.AddRange(aIArray[k]);
                        }
                        if (newProduction.Count > 0)
                        {
                            newProductionArray.Add(newProduction);
                        }
                    }
                    rules[aI] = newProductionArray;
                }
                Dictionary<string, List<List<string>>> newRules;
                List<string> newNonTerminal;
                (newRules, newNonTerminal) = deleteImmediateRecursion(new() { { aI, rules[aI] } });
                newNonTerms = [.. newNonTerms, .. newNonTerminal];

                foreach (var nt in newRules.Keys)
                    rules[nt] = newRules[nt];
            }
            return new(newNonTerms, Term, FirstSymbol, rules);
        }

        private (Dictionary<string, List<List<string>>>, List<string>) deleteImmediateRecursion(Dictionary<string, List<List<string>>>  rules)
        {
            var newRules = new Dictionary<string, List<List<string>>>(rules);
            var newNonTerm = new HashSet<string>();

            foreach (var pair in rules)
            {
                var left = pair.Key;
                var right = pair.Value;
                var addIndex = 1;
                List<List<string>> alpha = [];
                List<List<string>> betta = [];

                for (var i = 0; i < right.Count; i++)
                {
                    if (right[i][0] == left && right[i].Count >= 1)
                    {
                        alpha.Add([.. right[i].Skip(1)]);
                    }
                    else
                    {
                        betta.Add(right[i]);
                    }
                }
                if (betta.Count == 0)
                {
                    betta.Add([]);
                }
                if (alpha.Count > 0)
                {
                    var newSymbol = $"{left}{addIndex}";
                    newNonTerm.Add(newSymbol);
                    newRules[left] = [];
                    newRules[newSymbol] = [];
                    for (var i = 0; i < betta.Count; i++)
                    {
                        var symbol = betta[i];

                        if (betta[i].Count == 1 && betta[i][0] == "eps")
                        {
                            symbol = [];
                        }
                        newRules[left].Add([.. symbol, newSymbol]);
                    }
                    for (var i = 0; i < alpha.Count; i++)
                    {
                        newRules[newSymbol].Add([.. alpha[i], newSymbol]);
                    }
                    newRules[newSymbol].Add(["eps"]);
                }
            }
            return (newRules, newNonTerm.ToList());
        }
        
        public Grammar DeleteChainRules()
        {
            var rules = new Dictionary<string, List<List<string>>>(Rules);
            var nonTerm = new List<string>(NonTerm);

            List<(string, string)> chainPairs = [];

            foreach (var nt in nonTerm)
                chainPairs.Add((nt, nt));

            List<(string, string)> allChainRules = [];
            foreach (var key in rules.Keys)
            {
                foreach (var rule in rules[key])
                {
                    if (rule.Count == 1 && nonTerm.Contains(rule[0]))
                    {
                        allChainRules.Add((key, rule[0]));
                    } 
                }
            }
            foreach (var pair in allChainRules)
            {
                foreach (var cP in chainPairs.ToList())
                {
                    if (pair.Item1 == cP.Item2)
                    {
                        chainPairs.Add((cP.Item1, pair.Item2));
                    } 
                } 
            }

            var filteredPairs = chainPairs.Where(pair => pair.Item1 != pair.Item2).ToList();

            var newRules = new Dictionary<string, List<List<string>>>(Rules);

            foreach (var pair in filteredPairs)
            {
                foreach (var key in newRules.Keys)
                {
                    if (pair.Item1 == key)
                    {
                        foreach (var r in rules[pair.Item2])
                        {
                            newRules[key].Add(r);
                        } 
                    } 
                } 
            }

            foreach (var pair in filteredPairs)
            {
                foreach (var key in newRules.Keys)
                {
                    if (newRules[key].Any(r => r.Count == 1 && r.Contains(pair.Item2)))
                    {
                        newRules[key].RemoveAt(newRules[key].FindIndex(r => r.Count == 1 && r.Contains(pair.Item2)));
                    }
                }
            }

            return new(NonTerm, Term, FirstSymbol, newRules);
        }
    }
}
