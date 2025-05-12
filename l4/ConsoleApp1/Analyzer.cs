using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Xml.Linq;
using System.Diagnostics.Metrics;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace ConsoleApp1
{
    public class Node(List<Node>? nodes, string nodeName)
    {
        public List<Node>? Nodes { get; set; } = nodes;
        public string NodeName { get; set; } = nodeName;

        public override string ToString()
        {
            var match = Regex.Match(NodeName, @"(s|i) [0-9]+: (?<symbol>.*)");
            if (match.Success)
            {
                var value = match.Groups["symbol"].Value;
                if (value == ";" || value == "{" || value == "}")
                {
                    value += "\n";
                }
                return value;      
            }
            else
            {
                if (Nodes == null || Nodes.Count == 0)
                    return string.Empty;
                string res = string.Empty;
                foreach (var node in Nodes)
                    res += " " + node;
                return res.Trim();
            }
        }
    }

    public class Analyzer(Lexer lexer)
    {
        public Lexer Lexer = lexer;
        public Node Root { get; set; } = new(null, string.Empty);
        readonly private Dictionary<string, int> count = new() {
            {"p", 0}, {"b", 0}, {"ol", 0}, {"t", 0},
            {"i", 0}, {"o", 0}, {"e", 0}, {"le", 0},
            {"lm", 0}, {"sle", 0}, {"ple", 0}, {"ll", 0},
            {"lm1", 0}, {"le1", 0}, {"s", 0}};
        public void Build()
        {
            Program();
            if (lexer.Num < lexer.Tokens.Count)
                throw new Exception("Compile Error!");
        }
        private void Program()
        {
            var item = Block();
            Root = new([item], getName("p"));
        }

        Node Block() 
        {
            List<Node> blockNodes = [];
            var lex = Lexer.Next();
            if (lex is null)
                throw new Exception("Compile Error! Block");
            else if (lex["type"] == "OP_blockopenbrackets")
            {
                blockNodes.Add(new Node(null, getName("s", "{")));

                Node item = OperList();
                blockNodes.Add(item);

                lex = Lexer.Next();
                if (lex is null)
                    throw new Exception("Compile Error! Block");
                else if (lex["type"] == "OP_blockclosebrackets")
                {
                    blockNodes.Add(new Node(null, getName("s", "}")));
                    return new Node(blockNodes, getName("b"));
                }
            }
            throw new Exception("Compile Error! Block");
        }

        Node OperList()
        {
            List<Node> operListNodes = [];

            Node item = Oper();
            operListNodes.Add(item);

            item = Tail();
            operListNodes.Add(item);

            return new Node(operListNodes, getName("ol"));
        }

        Node Tail()
        {
            List<Node> tailNodes = [];
            var lex = Lexer.Next();
            if (lex is null)
                throw new Exception("Compile Error! Tail");
            else if (lex["type"] == "OP_sep")
            {
                tailNodes.Add(new Node(null, getName("s", ";")));
               
                Node item = Oper();
                tailNodes.Add(item);

                item = Tail();
                tailNodes.Add(item);

                return new Node(tailNodes, getName("t"));
            }

            Lexer.Prev();
            return new Node(null, getName("t"));            
        }

        Node Oper()
        {
            List<Node> operNodes = [];
            var lex = Lexer.Next();
            if (lex is null)
                throw new Exception("Compile Error! Oper");
            if (lex["type"] == "NAME")
            {
                Node item = new(null, getName("i", lex["value"]));
                operNodes.Add(item);

                lex = Lexer.Next();
                if (lex is null)
                    throw new Exception("Compile Error! Oper");
                if (lex["type"] == "OP_assignment")
                {
                    item = Expr();
                    operNodes.Add(item);
                    operNodes.Add(new(null, getName("s", "=")));
                    return new Node(operNodes, getName("o"));
                }
            }
            else if (lex["type"] == "OP_blockopenbrackets")
            {
                Lexer.Prev();
                Node item = Block();
                operNodes.Add(item);
                return new Node(operNodes, getName("o"));
            }
            throw new Exception("Compile Error! Oper");
        }

        Node Expr()
        {
            List<Node> exprNodes = [];
            Node? item = LogicExpr();
            exprNodes.Add(item);
            return new Node(exprNodes, getName("e"));
        }

        Node LogicExpr()
        {
            List<Node> logicExprNodes = [];
            Node item = LogicMono();
            logicExprNodes.Add(item);

            item = LogicExpr1();
            logicExprNodes.Add(item);

            return new Node(logicExprNodes, getName("le"));
        }

        Node LogicExpr1()
        {
            List<Node> logicExpr1Nodes = [];
            var lex = Lexer.Next();
            if (lex is null)
                throw new Exception("Compile Error! LogicExpr1");
            else if (lex["type"] == "OP_or")
            { 
                Node item = LogicMono();
                logicExpr1Nodes.Add(item);
                logicExpr1Nodes.Add(new(null, getName("s", "!")));
                item = LogicExpr1();
                logicExpr1Nodes.Add(item);

                return new Node(logicExpr1Nodes, getName("le1"));
            }
            else
            {
                Lexer.Prev();
                return new Node(null, getName("le1"));
            }
        }

        Node LogicMono()
        {
            List<Node> logicMonoNodes = [];
            Node item = SecondLogicExpr();
            logicMonoNodes.Add(item);

            item = LogicMono1();
            logicMonoNodes.Add(item);

            return new Node(logicMonoNodes, getName("lm"));
        }

        Node LogicMono1()
        {
            List<Node> logicMono1Nodes = [];
            var lex = Lexer.Next();
            if (lex is null)
                throw new Exception("Compile Error! LogicMono1");
            else if (lex["type"] == "OP_and")
            {
                Node item = SecondLogicExpr();
                logicMono1Nodes.Add(item);
                logicMono1Nodes.Add(new(null, getName("s", "&")));
                item = LogicMono1();
                logicMono1Nodes.Add(item);

                return new Node(logicMono1Nodes, getName("lm1"));
            }
            else
            {
                Lexer.Prev();
                return new Node(null, getName("lm1"));
            }
        }

        Node SecondLogicExpr()
        {
            List<Node> secondLogicNodes = [];
            var lex = Lexer.Next();
            Node? newNode = null;
            if (lex is null)
            {
                throw new Exception("Compile Error! SecondLogicExpr");
            }
            else if (lex["type"] == "OP_not")
            {
                newNode = new(null, getName("s", "~"));
            }
            else
            {
                Lexer.Prev();
            }

            Node item = PrimaryLogicExpr();
            secondLogicNodes.Add(item);
            if (newNode != null) 
                secondLogicNodes.Add(newNode);
            return new Node(secondLogicNodes, getName("sle"));
        }

        Node PrimaryLogicExpr()
        {
            List<Node> primaryLogicNodes = [];
            var lex = Lexer.Next();
            if (lex is null)
            {
                throw new Exception("Compile Error! PrimaryLogicExpr");
            }
            else if (lex["type"] == "NAME")
            {
                Node item = new(null, getName("i", lex["value"]));
                primaryLogicNodes.Add(item);
                return new Node(primaryLogicNodes, getName("ple")); 
            }
            else
            {
                Lexer.Prev();
                Node item = LogicLitera();
                primaryLogicNodes.Add(item);
                return new Node(primaryLogicNodes, getName("ple"));
            }
        }

        Node LogicLitera()
        {
            List<Node> logicLiteraNodes = [];
            Node item;
            var lex = Lexer.Next();
            if (lex is null)
            {
                throw new Exception("Compile Error! LogicLitera");
            }
            else if (lex["value"] == "true" || lex["value"] == "false")
            {
                item = new(null, getName("s", lex["value"]));
                logicLiteraNodes.Add(item);
                return new Node(logicLiteraNodes, getName("ll"));
            }
            throw new Exception("Compile Error! LogicLitera");
        }

        private string getName(string name, string value = "")
        {
            if (!string.IsNullOrEmpty(value))
                value = ": " + value;
            var newName = name + " " + count[name] + value;
            count[name]++;
            return newName;
        }

        public override string ToString()
        {
            return Root.ToString();
        }
        public void PrintTree()
        {
            using StreamWriter file = new StreamWriter("graph.dot");
            file.WriteLine("digraph ast {");
            PrintChildNodes(Root, file);
            file.WriteLine("}");
        }

        private void PrintChildNodes(Node parent, StreamWriter file)
        {
            if (parent.Nodes == null)
                return;
            foreach (var child in parent.Nodes)
            {
                file.WriteLine($"\"{parent.NodeName}\" -> \"{child.NodeName}\"");
                PrintChildNodes(child, file);
            }
        }
    }
}
