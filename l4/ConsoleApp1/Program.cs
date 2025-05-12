namespace ConsoleApp1
{
    internal class Program
    {
        static void Main(string[] args)
        {
            StreamReader reader = new StreamReader("src.txt");
            string program = reader.ReadToEnd();

            Lexer lexer = new Lexer();
            lexer.Lex(program);
            Analyzer analyzer = new Analyzer(lexer);
            analyzer.Build();
            Console.WriteLine(analyzer.ToString().Trim());
            analyzer.PrintTree();
        }
    }
}
