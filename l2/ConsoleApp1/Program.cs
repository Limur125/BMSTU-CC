using System.Data;

namespace ConsoleApp1
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var grammar = ReadGrammar("input.txt");
            var leftRecGram = grammar.DeleteLeftRecursion();
            PrintGrammar(leftRecGram);
            var chainGram = grammar.DeleteChainRules();
            PrintGrammar(chainGram);
        }

        static Grammar ReadGrammar(string fileName)
        {
            using var sr = new StreamReader(fileName);
            var nonTermLine = sr.ReadLine() ?? throw new Exception("NonTermRead");
            var nonTerms = nonTermLine.Split(" ", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            var termLine = sr.ReadLine() ?? throw new Exception("TermRead");
            var terms = termLine.Split(" ", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            var firstSymbol = string.Empty;
            Dictionary<string, List<List<string>>> grammarRules = [];
            for (var ruleLine = sr.ReadLine(); ruleLine != null; ruleLine = sr.ReadLine())
            {
                if (!ruleLine.Contains("->"))
                {
                    firstSymbol = ruleLine.Trim(' ');
                    break;
                }
                var inRules = ruleLine.Split("|", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
                var rule = inRules[0].Trim(' ').Split(" ", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
                var leftSymb = rule[0];

                if (!grammarRules.ContainsKey(leftSymb))
                {
                    grammarRules[leftSymb] = [[.. rule.Skip(2)]];
                }
                else
                {
                    grammarRules[leftSymb].Add([.. rule.Skip(2)]);
                }

                foreach (var rRules in inRules.Skip(1))
                {
                    rule = rRules.Trim(' ').Split(" ", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
                    grammarRules[leftSymb].Add([.. rule]);
                }
            }
            return new([.. nonTerms], [.. terms], firstSymbol, grammarRules);
        }
        static void PrintGrammar(Grammar g)
        {

            foreach (var pair in g.Rules)
            {
                Console.Write($"{pair.Key} ->");
                for (var i = 0; i < pair.Value.Count; i++)
                {
                    foreach (var pI in pair.Value[i])
                    {
                        Console.Write($"{pI} ");
                    }
                    if (i < pair.Value.Count - 1)
                    {
                        Console.Write("| ");
                    }
                }
                Console.WriteLine();
            }
            Console.WriteLine();
        }
    }
}
