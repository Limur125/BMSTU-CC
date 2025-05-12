using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;


namespace ConsoleApp1
{
    using Token = Dictionary<string, string>;
    public class Lexer
    {
        public List<Token> Tokens { get; set; } = [];
        public int Num { get; set; } = 0;

        private readonly HashSet<string> operators = ["{", "}", ";", "=", "!", "&", "~"];
        private readonly HashSet<string> blockOpenBrackets = ["{"];
        private readonly HashSet<string> blockCloseBrackets = ["}"];
        private readonly HashSet<string> operatorSep = [";"];
        private readonly HashSet<string> operatorAssignment = ["="];
        private readonly HashSet<string> operatorOr = ["!"];
        private readonly HashSet<string> operatorAnd = ["&"];
        private readonly HashSet<string> operatorNot = ["~"];
        private readonly HashSet<string> keywords = ["true", "false"];

        public Token? Next()
        {
            Token? res = null;
            if (Num < Tokens.Count)
                res = Tokens[Num];
            Num++;

            return res;
        }

        public Token? Prev()
        {
            Token? res = null;
            if (Num < Tokens.Count && Num > 0)
                res = Tokens[Num];
            Num--;
            return res;
        }

        public List<Token> Lex(string source)
        {
            Num = 0;
            List<char> chars = [.. source];
            Tokens = [];

            while (chars.Count != 0)
            {
                string ch = chars[0].ToString();

                if (ch == "\n")
                {
                    chars.RemoveAt(0);
                    continue;
                }
                if (IsOperator(ch))
                {
                    string type_ = "OP";
                    string operatorString = ExtractOperator(chars);
                    if (operatorOr.Contains(operatorString))
                        type_ += "_or";
                    else if (operatorAnd.Contains(operatorString))
                        type_ += "_and";
                    else if (operatorNot.Contains(operatorString))
                        type_ += "_not";
                    else if (blockOpenBrackets.Contains(operatorString))
                        type_ += "_blockopenbrackets";
                    else if (blockCloseBrackets.Contains(operatorString))
                        type_ += "_blockclosebrackets";
                    else if (operatorAssignment.Contains(operatorString))
                        type_ += "_assignment";
                    else if (operatorSep.Contains(operatorString))
                        type_ += "_sep";


                    Tokens.Add(new() { { "type", type_ }, { "value", operatorString } });
                    continue;
                }
                if (IsLetter(ch))
                {
                    var word = ExtractWord(chars);

                    if (IsKeyword(word))
                    {
                        Tokens.Add(new() { { "type", "KEYWORD" }, { "value", word } });
                        continue;
                    }
                    Tokens.Add(new() { { "type", "NAME"}, { "value", word } });
                    continue;
                }
                chars.RemoveAt(0);
            }
            return Tokens;
        }

        private bool IsOperator(string ch)
        {
            return operators.Contains(ch);
        }

        private bool IsLetter(string ch)
        {
            return Regex.IsMatch(ch, "[a-zA-Z]|_");
        }

        private bool IsKeyword(string word)
        {
            return keywords.Contains(word);
        }

        private string ExtractOperator(List<char> chars)
        {
            string op = "";
            foreach (var letter in chars)
            {
                if (!IsOperator(op + letter))
                    break;

                op += letter;
            }
            chars.RemoveRange(0, op.Length);
            return op;
        }

        private string ExtractWord(List<char> chars)
        {
            string word = "";
            foreach(var letter in chars)
            {
                if (!(IsLetter(letter.ToString()) || Regex.IsMatch(letter.ToString(), "([0-9]|_)")))
                    break;

                word += letter;
            }
            chars.RemoveRange(0, word.Length);
            return word;
        }
    }
}
