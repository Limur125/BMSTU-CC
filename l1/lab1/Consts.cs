using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lab1
{
    internal class Consts
    {
        public static string closeSymbol = ")";
        public static string openSymbol = "(";
        public static string starSymbol = "*";
        public static string orSymbol = "|";
        public static string andSymbol = "•";
        public static string epsSymbol = "eps";
        public static string plusSymbol = "+";

        public static List<string> allControlSymbols = 
            [closeSymbol,
            openSymbol,
            starSymbol,
            orSymbol,
            andSymbol,
            plusSymbol];

        public static List<string> allOperators = 
            [starSymbol,
            plusSymbol,
            andSymbol,
            orSymbol];

        public static List<string> priorityOperators =
            [starSymbol,
            plusSymbol,
            andSymbol,
            orSymbol];

    }
}
