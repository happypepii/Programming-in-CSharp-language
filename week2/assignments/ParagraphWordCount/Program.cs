using System;
using System.IO;

namespace WordCount
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("Argument Error");
                return;
            }

            string fileName = args[0];

            try
            {
                using var reader = new StreamReader(fileName);
                ProcessFile(reader);
            }
            catch
            {
                Console.WriteLine("File Error");
            }
        }

        static void ProcessFile(TextReader reader)
        {
            string line;
            int wordCount = 0;
            bool inParagraph = false;

            while ((line = reader.ReadLine()) != null)
            {
                // Check if the line is empty or whitespace only
                if (string.IsNullOrWhiteSpace(line))
                {
                    // Paragraph delimiter
                    if (inParagraph)
                    {
                        Console.WriteLine(wordCount);
                        wordCount = 0;
                        inParagraph = false;
                    }
                    continue;
                }

                var words = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                wordCount += words.Length;
                inParagraph = true;
            }

            if (inParagraph)
            {
                Console.WriteLine(wordCount);
            }
        }
    }
}
