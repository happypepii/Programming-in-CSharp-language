using System.Text;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("MultifileTextJustification.Tests")]

namespace TextJustification
{
    public class Program
    {
        public static void Main(string[] args)
        {
            if (args.Length < 3)
            {
                Console.WriteLine("Argument Error");
                return;
            }

            bool highlightSpaces = false;
            int startIndex = 0;

            // Check for --highlight-spaces flag
            if (args[0] == "--highlight-spaces")
            {
                highlightSpaces = true;
                startIndex = 1;

                if (args.Length < 4) // Need at least: flag, input, output, width
                {
                    Console.WriteLine("Argument Error");
                    return;
                }
            }

            // Last argument is width
            string widthArg = args[args.Length - 1];
            if (!int.TryParse(widthArg, out int width) || width <= 0)
            {
                Console.WriteLine("Argument Error");
                return;
            }

            // Second to last is output file
            string outputPath = args[args.Length - 2];

            try
            {
                var justifier = new BatchTextJustifier(width, highlightSpaces);
                // Process files on-demand without storing all paths in memory
                justifier.ProcessFiles(args, startIndex, args.Length - 2, outputPath);
            }
            catch
            {
                Console.WriteLine("File Error");
            }
        }
    }

    class BatchTextJustifier
    {
        private readonly int lineWidth;
        private readonly bool highlightSpaces;

        public BatchTextJustifier(int width, bool highlightSpaces)
        {
            lineWidth = width;
            this.highlightSpaces = highlightSpaces;
        }

        public void ProcessFiles(string[] args, int startIndex, int endIndex, string outputPath)
        {
            // Create writer with appropriate strategy
            ICharacterWriter writer = CharacterWriterFactory.Create(outputPath, highlightSpaces);
            
            using (writer)
            {
                var formatter = new BatchParagraphFormatter(writer, lineWidth);

                // Process files on-demand without loading all paths into memory
                for (int i = startIndex; i < endIndex; i++)
                {
                    string inputPath = args[i];
                    
                    try
                    {
                        if (File.Exists(inputPath))
                        {
                            using var reader = new CharacterReader(inputPath);
                            formatter.ProcessFile(reader);
                        }
                        else
                        {
                            // Treat non-existent file as empty - file boundary serves as word separator
                            formatter.ProcessFileBoundary();
                        }
                    }
                    catch
                    {
                        // Treat error as empty file - file boundary serves as word separator
                        formatter.ProcessFileBoundary();
                    }
                }

                // Finish processing
                formatter.Finalize();
            }
        }
    }

    internal class BatchParagraphFormatter
    {
        private readonly ICharacterWriter writer;
        private readonly int lineWidth;

        private StringBuilder currentWord = new StringBuilder();
        private List<string> currentLine = new List<string>();
        private bool hasSeenAnyContent = false;  // Have we seen any content at all?
        private bool hasSeenWordInParagraph = false;  // Have we seen a word in current paragraph?
        private int consecutiveNewlines = 0;
        private bool needsParagraphSeparator = false;

        public BatchParagraphFormatter(ICharacterWriter writer, int lineWidth)
        {
            this.writer = writer;
            this.lineWidth = lineWidth;
        }

        public void ProcessFile(ICharacterReader reader)
        {
            while (reader.ReadNext(out char ch))
            {
                ProcessCharacter(ch);
            }

            // File boundary acts as word separator
            ProcessFileBoundary();
        }

        public void ProcessFileBoundary()
        {
            // Complete any current word (file boundary is a word separator)
            if (currentWord.Length > 0)
            {
                CompleteCurrentWord();
            }
        }

        public void Finalize()
        {
            // Complete any partial word
            if (currentWord.Length > 0)
            {
                CompleteCurrentWord();
            }

            // Output final line if we have content
            if (currentLine.Count > 0)
            {
                // Last line of document is left-aligned
                string leftAligned = string.Join(" ", currentLine);
                writer.WriteLine(leftAligned);
            }
            else if (!hasSeenAnyContent)
            {
                // No content at all - output empty line
                writer.WriteLine("");
            }
        }

        private void ProcessCharacter(char ch)
        {
            if (char.IsWhiteSpace(ch))
            {
                // If we have a complete word, process it
                if (currentWord.Length > 0)
                {
                    CompleteCurrentWord();
                }

                // Track newlines for paragraph breaks
                if (ch == '\n')
                {
                    consecutiveNewlines++;

                    // Paragraph break: 2+ consecutive newlines after seeing content
                    if (consecutiveNewlines >= 2 && hasSeenWordInParagraph)
                    {
                        // Output last line of current paragraph (left-aligned)
                        if (currentLine.Count > 0)
                        {
                            string leftAligned = string.Join(" ", currentLine);
                            writer.WriteLine(leftAligned);
                            currentLine.Clear();
                        }

                        // Mark that we need a separator before next paragraph
                        needsParagraphSeparator = true;
                        hasSeenWordInParagraph = false;
                    }
                }
            }
            else
            {
                // Non-whitespace character
                currentWord.Append(ch);
                consecutiveNewlines = 0;
            }
        }

        private void CompleteCurrentWord()
        {
            string word = currentWord.ToString();
            hasSeenAnyContent = true;
            hasSeenWordInParagraph = true;

            // If we need a paragraph separator, output it now (before first word of new paragraph)
            if (needsParagraphSeparator)
            {
                writer.WriteLine("");
                needsParagraphSeparator = false;
            }

            // Check if adding this word would exceed line width
            if (currentLine.Count > 0)
            {
                int currentWordsLength = currentLine.Sum(w => w.Length);
                int minLength = currentWordsLength + currentLine.Count + word.Length;

                if (minLength > lineWidth)
                {
                    // Current line is full, justify and write it immediately
                    string justifiedLine = JustifyLine(currentLine, lineWidth);
                    writer.WriteLine(justifiedLine);
                    currentLine.Clear();
                }
            }

            currentLine.Add(word);
            currentWord.Clear();
            consecutiveNewlines = 0;
        }

        private string JustifyLine(List<string> words, int lineWidth)
        {
            if (words.Count == 1)
            {
                return words[0];
            }

            int totalWordLength = words.Sum(w => w.Length);
            int totalSpaces = lineWidth - totalWordLength;
            int gaps = words.Count - 1;
            int spacesPerGap = totalSpaces / gaps;
            int extraSpaces = totalSpaces % gaps;

            StringBuilder line = new StringBuilder();

            for (int i = 0; i < words.Count; i++)
            {
                line.Append(words[i]);

                if (i < gaps)
                {
                    int spacesToAdd = spacesPerGap + (i < extraSpaces ? 1 : 0);
                    line.Append(' ', spacesToAdd);
                }
            }

            return line.ToString();
        }
    }

    // ====================== Strategy Pattern ======================
    
    /// <summary>
    /// Strategy interface for writing output
    /// </summary>
    interface IOutputStrategy
    {
        string ProcessLine(string line);
    }

    /// <summary>
    /// Normal output strategy - no modifications
    /// </summary>
    class NormalOutputStrategy : IOutputStrategy
    {
        public string ProcessLine(string line)
        {
            return line;
        }
    }

    /// <summary>
    /// Highlight spaces strategy - replaces spaces with dots and adds line markers
    /// </summary>
    class HighlightSpacesStrategy : IOutputStrategy
    {
        public string ProcessLine(string line)
        {
            // Replace all spaces with dots
            string processed = line.Replace(' ', '.');
            // Add line ending marker
            return processed + "<-";
        }
    }

    // ====================== Factory Pattern ======================
    
    /// <summary>
    /// Factory for creating character writers with appropriate strategy
    /// </summary>
    static class CharacterWriterFactory
    {
        public static ICharacterWriter Create(string filePath, bool highlightSpaces)
        {
            IOutputStrategy strategy = highlightSpaces 
                ? new HighlightSpacesStrategy() 
                : new NormalOutputStrategy();
            
            return new StrategyCharacterWriter(filePath, strategy);
        }
    }

    // ====================== Writers ======================

    /// <summary>
    /// Character writer that uses a strategy for output formatting
    /// </summary>
    class StrategyCharacterWriter : ICharacterWriter
    {
        private readonly StreamWriter writer;
        private readonly IOutputStrategy strategy;

        public StrategyCharacterWriter(string filePath, IOutputStrategy strategy)
        {
            writer = new StreamWriter(filePath);
            this.strategy = strategy;
        }

        public void WriteLine(string text = "")
        {
            string processedText = strategy.ProcessLine(text);
            writer.WriteLine(processedText);
            writer.Flush();
        }

        public void Dispose() => writer.Dispose();
    }

    class CharacterReader : ICharacterReader
    {
        private readonly StreamReader reader;

        public CharacterReader(string filePath)
        {
            reader = new StreamReader(filePath);
        }

        public bool ReadNext(out char ch)
        {
            int value = reader.Read();
            if (value == -1)
            {
                ch = '\0';
                return false;
            }
            ch = (char)value;
            return true;
        }

        public void Dispose() => reader.Dispose();
    }

    // Keep original CharacterWriter for backward compatibility
    class CharacterWriter : ICharacterWriter
    {
        private readonly StreamWriter writer;

        public CharacterWriter(string filePath)
        {
            writer = new StreamWriter(filePath);
        }

        public void WriteLine(string text = "")
        {
            writer.WriteLine(text);
            writer.Flush();
        }

        public void Dispose() => writer.Dispose();
    }

    // ====================== Interfaces ======================

    /// <summary>
    /// Defines a character-level reader abstraction for streaming text input.
    /// </summary>
    interface ICharacterReader : IDisposable
    {
        bool ReadNext(out char ch);
    }

    /// <summary>
    /// Defines a character-level writer abstraction for streaming text output.
    /// </summary>
    interface ICharacterWriter : IDisposable
    {
        void WriteLine(string text = "");
    }
}