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
                ParagraphProcessor pc = new ParagraphProcessor();
                pc.Process(reader);
            }
            catch
            {
                Console.WriteLine("File Error");
            }
        }
    }

    class ParagraphProcessor
    {
        public void Process(TextReader reader)
        {
            string line;
            int wordCount = 0;
            bool inParagraph = false;

            while ((line = reader.ReadLine()) != null)
            {
                if (IsParagraphSeparator(line))
                {
                    if (inParagraph)
                    {
                        OutputParagraph(wordCount);
                        wordCount = 0;
                        inParagraph = false;
                    }
                    continue;
                }

                wordCount += CountWords(line);
                inParagraph = true;
            }
            if (inParagraph)
            {
                OutputParagraph(wordCount);
            }
        }

        private bool IsParagraphSeparator(string line)
        {
            return string.IsNullOrWhiteSpace(line);
        }

        private int CountWords(string line)
        {
            var words = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            return words.Length;
        }

        private void OutputParagraph(int count)
        {
            Console.WriteLine(count);
        }
        
    }
}
