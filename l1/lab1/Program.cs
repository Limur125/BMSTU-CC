using System.Diagnostics;
using System.Security.AccessControl;
using System.Text;
using System.Text.RegularExpressions;

namespace lab1
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.Write("Regex: ");
            string? input = Console.ReadLine();
            if (input == null)
                return;

            string regex = "(" + input + ")#";
            var parser = new RegexParser();
            var resTree = parser.parseExpression(regex);
            var dfa = resTree.CreateDFA();
            DFA mindfa = new MinDFA(dfa);

            printDFA(dfa, input);

            Console.Write("String: ");
            string? inputString = Console.ReadLine();
            while (!string.IsNullOrWhiteSpace(inputString))
            {
                mindfa.Accept(inputString);
                Console.Write("String: ");
                inputString = Console.ReadLine();
            }

        }

        static void printDFA(DFA dfa, string input)
        {
            var nodes = dfa.Dtran.Keys.ToList();
            nodes.AddRange(dfa.finishStates);
            using StreamWriter file = new StreamWriter("graph.dot");
            file.WriteLine("digraph dfa {");
            file.WriteLine($"999999 [label=\"{input}\" peripheries=0 shape=\"box\"];");
            file.WriteLine($"999999 -> {nodes.IndexOf(dfa.startState)}");
            int i = 1;
            foreach (var finNode in dfa.finishStates)
            {
                file.WriteLine($"{999999 + i} [style=invis];");
                file.WriteLine($"{nodes.IndexOf(finNode)} -> {999999 + i++}");
            }

            foreach (var keyValuePair in dfa.Dtran)
                foreach (var finKeyValuePair in keyValuePair.Value)
                    file.WriteLine($"{nodes.IndexOf(keyValuePair.Key)} -> {nodes.IndexOf(finKeyValuePair.Value)} [label=\"{finKeyValuePair.Key}\"];");
            file.WriteLine("}");
        }
    }
}
