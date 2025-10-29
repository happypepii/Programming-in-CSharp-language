using System;
using System.IO;

namespace toSumOrNot
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 3)
            {
                Console.WriteLine("Argument Error");
                return;
            }

            string inputFile = args[0];
            string outputFile = args[1];
            string columnName = args[2];

            try
            {
                using (var reader = new StreamReader(inputFile))
                using (var writer = new StreamWriter(outputFile))
                {
                    ProcessInput(reader, writer, columnName);
                }
            }
            catch
            {
                Console.WriteLine("File Error");
            }
        }
        static void ProcessInput(TextReader reader, TextWriter writer, string columnName)
        {
            string? line = reader.ReadLine();

            if (line == null)
            {
                Console.WriteLine("Invalid File Format");
                return;
            }

            var header = SplitWords(line);
            int colCount = header.Length;

            if (colCount == 0)
            {
                Console.WriteLine("Invalid File Format");
                return;
            }

            int targetIndex = Array.IndexOf(header, columnName);
            if (targetIndex == -1)
            {
                Console.WriteLine("Non-existent Column Name");
                return;
            }

            long sum = 0;
            bool hasData = false;

            while ((line = reader.ReadLine()) != null)
            {
                hasData = true;
                var words = SplitWords(line);

                if (words.Length != colCount)
                {
                    Console.WriteLine("Invalid File Format");
                    return;
                }

                if (!int.TryParse(words[targetIndex], out int num))
                {
                    Console.WriteLine("Invalid Integer Value");
                    return;
                }

                sum += num;
            }

            if (!hasData)
                sum = 0;

            writer.WriteLine(columnName);
            writer.WriteLine(new string('-', columnName.Length));
            writer.WriteLine(sum.ToString());
        }

        static string[] SplitWords(string line)
        {
            return line.Split(new[] { ' ', '\t', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        }
    }
}
