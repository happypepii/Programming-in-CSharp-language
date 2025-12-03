using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Xunit;
using TextJustification;

namespace TextJustification.Tests
{
    public class TextJustificationTests
    {
        // Helper class to simulate file operations in memory
        private class MockCharacterReader : ICharacterReader
        {
            private readonly string content;
            private int position = 0;

            public MockCharacterReader(string content)
            {
                this.content = content;
            }

            public bool ReadNext(out char ch)
            {
                if (position < content.Length)
                {
                    ch = content[position++];
                    return true;
                }
                ch = '\0';
                return false;
            }

            public void Dispose() { }
        }

        private class MockCharacterWriter : ICharacterWriter
        {
            private readonly StringBuilder output = new StringBuilder();

            public void WriteLine(string text = "")
            {
                output.AppendLine(text);
            }

            public string GetOutput() => output.ToString();

            public void Dispose() { }
        }

        private string ProcessText(string input, int width)
        {
            var reader = new MockCharacterReader(input);
            var writer = new MockCharacterWriter();
            var formatter = new ParagraphFormatter(reader, writer, width);

            formatter.FormatAndWriteParagraphs();

            return writer.GetOutput();
        }

        [Fact]
        public void Test_Example1_FromProblemStatement()
        {
            // Example from problem statement
            string input = "If a train station is where the train stops, what is a work station?";
            int width = 17;

            string expected =
                "If     a    train\n" +
                "station  is where\n" +
                "the  train stops,\n" +
                "what  is  a  work\n" +
                "station?\n";

            string actual = ProcessText(input, width);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Test_SingleWord()
        {
            string input = "Hello";
            int width = 10;

            string expected = "Hello\n";

            string actual = ProcessText(input, width);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Test_WordLongerThanWidth()
        {
            // Word longer than width should be on its own line
            string input = "Supercalifragilisticexpialidocious";
            int width = 10;

            string expected = "Supercalifragilisticexpialidocious\n";

            string actual = ProcessText(input, width);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Test_MultipleParagraphs_WithMultipleEmptyLines()
        {
            // Multiple empty lines should still result in single empty line separator
            string input = "First paragraph.\n\n\n\nSecond paragraph.";
            int width = 20;

            string expected =
                "First paragraph.\n" +
                "\n" +
                "Second paragraph.\n";

            string actual = ProcessText(input, width);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Test_EmptyLinesAtStart()
        {
            // Empty lines at start should be ignored
            string input = "\n\n\nFirst paragraph.";
            int width = 20;

            string expected = "First paragraph.\n";

            string actual = ProcessText(input, width);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Test_OnlyWhitespaceLines_BetweenParagraphs()
        {
            // Lines with only spaces/tabs should act as paragraph separators
            string input = "First paragraph.\n   \n  \t  \nSecond paragraph.";
            int width = 20;

            string expected =
                "First paragraph.\n" +
                "\n" +
                "Second paragraph.\n";

            string actual = ProcessText(input, width);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Test_TabsAndSpaces_AsWhitespace()
        {
            string input = "Word1\t\tWord2   Word3";
            int width = 20;

            string expected = "Word1 Word2 Word3\n";

            string actual = ProcessText(input, width);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Test_LastLineLeftAligned()
        {
            string input = "This is a test. This is only a test.";
            int width = 15;

            string actual = ProcessText(input, width);

            // Last line should be left-aligned (single spaces)
            string[] lines = actual.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            string lastLine = lines[lines.Length - 1];

            // Check last line doesn't have multiple consecutive spaces
            Assert.DoesNotContain("  ", lastLine);
        }


        [Fact]
        public void Test_ThreeParagraphs()
        {
            string input = "Para1 word.\n\nPara2 word.\n\nPara3 word.";
            int width = 20;

            string expected =
                "Para1 word.\n" +
                "\n" +
                "Para2 word.\n" +
                "\n" +
                "Para3 word.\n";

            string actual = ProcessText(input, width);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Test_NewlineWithinParagraph_NotSeparator()
        {
            // Single newline within paragraph should not separate paragraphs
            string input = "First line of paragraph\nsecond line of paragraph";
            int width = 30;

            // Should be treated as one paragraph
            string actual = ProcessText(input, width);

            // Should not have empty line in middle
            Assert.DoesNotContain("\n\n", actual.Substring(0, actual.Length - 1));
        }

        [Fact]
        public void Test_EdgeCase_ExactWidthFit()
        {
            string input = "abc def ghi";
            int width = 11; // Exactly: "abc def ghi"

            string expected = "abc def ghi\n";

            string actual = ProcessText(input, width);

            Assert.Equal(expected, actual);
        }


        [Fact]
        public void Test_EmptyInput()
        {
            string input = "";
            int width = 10;

            string expected = "";

            string actual = ProcessText(input, width);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Test_OnlyWhitespace()
        {
            string input = "   \n\n\t\t  \n  ";
            int width = 10;

            string expected = "";

            string actual = ProcessText(input, width);

            Assert.Equal(expected, actual);
        }

        // Integration test with actual files
        [Fact]
        public void Integration_Test_WithTempFiles()
        {
            string input = "If a train station is where the train stops, what is a work station?";
            int width = 17;

            string inputFile = Path.GetTempFileName();
            string outputFile = Path.GetTempFileName();

            try
            {
                File.WriteAllText(inputFile, input);

                var justifier = new TextJustifier(width);
                justifier.ProcessFile(inputFile, outputFile);

                string output = File.ReadAllText(outputFile);

                string expected =
                    "If     a    train\n" +
                    "station  is where\n" +
                    "the  train stops,\n" +
                    "what  is  a  work\n" +
                    "station?\n";

                Assert.Equal(expected, output);
            }
            finally
            {
                if (File.Exists(inputFile)) File.Delete(inputFile);
                if (File.Exists(outputFile)) File.Delete(outputFile);
            }
        }
    }
}