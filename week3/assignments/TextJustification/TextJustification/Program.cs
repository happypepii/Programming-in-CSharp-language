using System.Text;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;

[assembly: InternalsVisibleTo("TextJustification.Tests")]

namespace TextJustification
{
    public class Program
    {
        public static void Main(string[] args)
        {
            if (args.Length != 3 || !int.TryParse(args[2], out int width) || width <= 0)
            {
                Console.WriteLine("Argument Error");
                return;
            }

            try
            {
                var justifier = new TextJustifier(width);
                justifier.ProcessFile(args[0], args[1]);
            }
            catch
            {
                Console.WriteLine("File Error");
            }
        }
    }

    class TextJustifier
    {
        private readonly int lineWidth;

        public TextJustifier(int width)
        {
            lineWidth = width;
        }

        public void ProcessFile(string inputPath, string outputPath)
        {
            using var reader = new CharacterReader(inputPath);
            using var writer = new CharacterWriter(outputPath);

            var formatter = new ParagraphFormatter(reader, writer, lineWidth);
            formatter.FormatAndWriteParagraphs();
        }
    }

    internal class ParagraphFormatter
    {
        private readonly ICharacterReader reader;
        private readonly ICharacterWriter writer;
        private readonly int lineWidth;

        public ParagraphFormatter(ICharacterReader reader, ICharacterWriter writer, int lineWidth)
        {
            this.reader = reader;
            this.writer = writer;
            this.lineWidth = lineWidth;
        }

        public void FormatAndWriteParagraphs()
        {
            int paragraphCount = 0;

            while (true)
            {
                bool paragraphExists = ProcessOneParagraph(paragraphCount > 0);
                if (!paragraphExists)
                    break;
                paragraphCount++;
            }
        }

        private bool ProcessOneParagraph(bool addEmptyLineBefore)
        {
            // Store all complete lines of the paragraph (not the last line)
            List<string> completedLines = new List<string>();

            StringBuilder currentWord = new StringBuilder();
            List<string> currentLine = new List<string>();
            bool hasSeenWord = false;
            int consecutiveNewlines = 0;

            while (reader.ReadNext(out char ch))
            {
                if (char.IsWhiteSpace(ch))
                {
                    // If we have a complete word, process it
                    if (currentWord.Length > 0)
                    {
                        string word = currentWord.ToString();
                        hasSeenWord = true;

                        // Check if adding this word would exceed line width
                        // Calculate: current words length + spaces between them + space before new word + new word
                        if (currentLine.Count > 0)
                        {
                            int currentWordsLength = currentLine.Sum(w => w.Length);
                            // Need (currentLine.Count) spaces: (Count-1) between existing words + 1 before new word
                            int minLength = currentWordsLength + currentLine.Count + word.Length;

                            if (minLength > lineWidth)
                            {
                                // Current line is full, save it as a completed line
                                string justifiedLine = JustifyLine(currentLine, lineWidth);
                                completedLines.Add(justifiedLine);
                                currentLine.Clear();
                            }
                        }

                        currentLine.Add(word);
                        currentWord.Clear();
                        consecutiveNewlines = 0;
                    }

                    // Track newlines for paragraph breaks
                    if (ch == '\n')
                    {
                        consecutiveNewlines++;

                        // Paragraph break: 2+ consecutive newlines after seeing content
                        if (consecutiveNewlines >= 2 && hasSeenWord)
                        {
                            // Output this paragraph
                            OutputParagraph(completedLines, currentLine, addEmptyLineBefore);
                            return true;
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

            // EOF reached - check if we have content
            if (!hasSeenWord && currentWord.Length == 0 && currentLine.Count == 0)
            {
                return false; // No content
            }

            // Handle last word at EOF
            if (currentWord.Length > 0)
            {
                string word = currentWord.ToString();

                if (currentLine.Count > 0)
                {
                    int currentWordsLength = currentLine.Sum(w => w.Length);
                    int minLength = currentWordsLength + currentLine.Count + word.Length;

                    if (minLength > lineWidth)
                    {
                        // Save current line and start new one
                        string justifiedLine = JustifyLine(currentLine, lineWidth);
                        completedLines.Add(justifiedLine);
                        currentLine.Clear();
                    }
                }

                currentLine.Add(word);
            }

            // Output final paragraph
            OutputParagraph(completedLines, currentLine, addEmptyLineBefore);
            return true;
        }

        private void OutputParagraph(List<string> completedLines, List<string> lastLine, bool addEmptyLineBefore)
        {
            // Add separator empty line if needed
            if (addEmptyLineBefore && (completedLines.Count > 0 || lastLine.Count > 0))
            {
                writer.WriteLine("");
            }

            // Output all completed lines (justified)
            foreach (var line in completedLines)
            {
                writer.WriteLine(line);
            }

            // Output last line (left-aligned)
            if (lastLine.Count > 0)
            {
                string leftAligned = string.Join(" ", lastLine);
                writer.WriteLine(leftAligned);
            }
        }

        private string JustifyLine(List<string> words, int lineWidth)
        {
            if (words.Count == 1)
            {
                // Single word: left-align (no spaces to distribute)
                return words[0];
            }

            // Calculate spaces to distribute
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
                    // Add base spaces + 1 extra for the first 'extraSpaces' gaps (left-to-right)
                    int spacesToAdd = spacesPerGap + (i < extraSpaces ? 1 : 0);
                    line.Append(' ', spacesToAdd);
                }
            }

            return line.ToString();
        }
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