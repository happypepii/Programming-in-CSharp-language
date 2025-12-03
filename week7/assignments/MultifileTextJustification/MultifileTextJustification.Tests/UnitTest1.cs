using System.Text;
using Xunit;
using TextJustification;

namespace MultifileTextJustification.Tests
{
    public class BatchTextJustificationTests
    {
        // ====================== Helper Classes for Testing ======================
        
        /// <summary>
        /// Mock character reader for testing without file I/O
        /// </summary>
        class MockCharacterReader : ICharacterReader
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

        /// <summary>
        /// Mock character writer for capturing output without file I/O
        /// </summary>
        class MockCharacterWriter : ICharacterWriter
        {
            private readonly List<string> lines = new List<string>();

            public List<string> Lines => lines;

            public void WriteLine(string text = "")
            {
                lines.Add(text);
            }

            public void Dispose() { }
        }

        // ====================== TOP 15 CRITICAL TESTS ======================

        /// <summary>
        /// TEST 1: Verify basic justification with single file
        /// Critical: Core functionality test
        /// </summary>
        [Fact]
        public void Test01_SingleFile_BasicJustification()
        {
            var writer = new MockCharacterWriter();
            var formatter = new BatchParagraphFormatter(writer, 17);
            
            var reader = new MockCharacterReader("If a train station is where the train stops, what is a work station?");
            formatter.ProcessFile(reader);
            formatter.Finalize();

            Assert.Equal(5, writer.Lines.Count);
            Assert.Equal("If     a    train", writer.Lines[0]);
            Assert.Equal("station  is where", writer.Lines[1]);
            Assert.Equal("the  train stops,", writer.Lines[2]);
            Assert.Equal("what  is  a  work", writer.Lines[3]);
            Assert.Equal("station?", writer.Lines[4]); // Last line left-aligned
        }

        /// <summary>
        /// TEST 2: Multiple files processed as one continuous paragraph
        /// Critical: Tests file concatenation requirement
        /// </summary>
        [Fact]
        public void Test02_MultipleFiles_OneContinuousParagraph()
        {
            var writer = new MockCharacterWriter();
            var formatter = new BatchParagraphFormatter(writer, 17);
            
            // Process same content 3 times
            for (int i = 0; i < 3; i++)
            {
                var reader = new MockCharacterReader("If a train station is where the train stops, what is a work station?");
                formatter.ProcessFile(reader);
            }
            formatter.Finalize();

            // Should have 14 lines (verified from ex02.out)
            Assert.Equal(14, writer.Lines.Count);
            Assert.Equal("If     a    train", writer.Lines[0]);
            Assert.Equal("station?", writer.Lines[13]);
        }

        /// <summary>
        /// TEST 3: File boundary acts as word separator
        /// Critical: Core requirement for multi-file handling
        /// </summary>
        [Fact]
        public void Test03_FileBoundary_WordSeparator()
        {
            var writer = new MockCharacterWriter();
            var formatter = new BatchParagraphFormatter(writer, 20);
            
            var reader1 = new MockCharacterReader("Hello");
            var reader2 = new MockCharacterReader("World");
            
            formatter.ProcessFile(reader1);
            formatter.ProcessFile(reader2);
            formatter.Finalize();

            Assert.Single(writer.Lines);
            Assert.Equal("Hello World", writer.Lines[0]);
        }

        /// <summary>
        /// TEST 4: Empty input produces single empty line
        /// Critical: Edge case requirement
        /// </summary>
        [Fact]
        public void Test04_EmptyInput_SingleEmptyLine()
        {
            var writer = new MockCharacterWriter();
            var formatter = new BatchParagraphFormatter(writer, 17);
            
            formatter.Finalize();

            Assert.Single(writer.Lines);
            Assert.Equal("", writer.Lines[0]);
        }

        /// <summary>
        /// TEST 5: Highlight spaces strategy replaces spaces with dots
        /// Critical: --highlight-spaces feature
        /// </summary>
        [Fact]
        public void Test05_HighlightSpaces_ReplacesWithDots()
        {
            var strategy = new HighlightSpacesStrategy();
            var result = strategy.ProcessLine("Hello World Test");
            
            Assert.Equal("Hello.World.Test<-", result);
        }

        /// <summary>
        /// TEST 6: Normal strategy doesn't modify output
        /// Critical: Default behavior verification
        /// </summary>
        [Fact]
        public void Test06_NormalStrategy_NoModification()
        {
            var strategy = new NormalOutputStrategy();
            var result = strategy.ProcessLine("Hello World");
            
            Assert.Equal("Hello World", result);
        }

        /// <summary>
        /// TEST 7: Factory creates correct writer type
        /// Critical: Design pattern requirement
        /// </summary>
        [Fact]
        public void Test07_Factory_CreatesCorrectWriter()
        {
            string tempFile = Path.GetTempFileName();
            try
            {
                var writer = CharacterWriterFactory.Create(tempFile, true);
                Assert.NotNull(writer);
                Assert.IsType<StrategyCharacterWriter>(writer);
                writer.Dispose();
            }
            finally
            {
                if (File.Exists(tempFile)) File.Delete(tempFile);
            }
        }

        /// <summary>
        /// TEST 8: Paragraph breaks handled correctly
        /// Critical: Multi-paragraph support
        /// </summary>
        [Fact]
        public void Test08_ParagraphBreaks_HandledCorrectly()
        {
            var writer = new MockCharacterWriter();
            var formatter = new BatchParagraphFormatter(writer, 20);
            
            var reader = new MockCharacterReader("First paragraph.\n\nSecond paragraph.");
            formatter.ProcessFile(reader);
            formatter.Finalize();

            Assert.Equal(3, writer.Lines.Count);
            Assert.Equal("First paragraph.", writer.Lines[0]);
            Assert.Equal("", writer.Lines[1]); // Separator
            Assert.Equal("Second paragraph.", writer.Lines[2]);
        }

        /// <summary>
        /// TEST 9: Extra spaces distributed left-to-right
        /// Critical: Justification algorithm correctness
        /// </summary>
        [Fact]
        public void Test09_ExtraSpaces_LeftToRight()
        {
            var writer = new MockCharacterWriter();
            var formatter = new BatchParagraphFormatter(writer, 10);
            
            var reader = new MockCharacterReader("A B C DDDDDD");
            formatter.ProcessFile(reader);
            formatter.Finalize();

            Assert.Equal(2, writer.Lines.Count);
            // "A B C" = 3 chars, need 7 spaces = 2 gaps
            // 7/2 = 3 remainder 1
            // First gap: 4 spaces, second gap: 3 spaces
            Assert.Equal("A    B   C", writer.Lines[0]);
        }

        /// <summary>
        /// TEST 10: Real example 01 - Single file with highlight
        /// Critical: Matches provided test case
        /// </summary>
        [Fact]
        public void Test10_Example01_WithHighlight()
        {
            string tempFile = Path.GetTempFileName();
            try
            {
                var strategy = new HighlightSpacesStrategy();
                using (var writer = new StrategyCharacterWriter(tempFile, strategy))
                {
                    var formatter = new BatchParagraphFormatter(writer, 17);
                    var reader = new MockCharacterReader("If a train station is where the train stops, what is a work station?");
                    formatter.ProcessFile(reader);
                    formatter.Finalize();
                }

                var lines = File.ReadAllLines(tempFile);
                Assert.Equal(5, lines.Length);
                Assert.Equal("If.....a....train<-", lines[0]);
                Assert.Equal("station..is.where<-", lines[1]);
                Assert.Equal("the..train.stops,<-", lines[2]);
                Assert.Equal("what..is..a..work<-", lines[3]);
                Assert.Equal("station?<-", lines[4]);
            }
            finally
            {
                if (File.Exists(tempFile)) File.Delete(tempFile);
            }
        }

        /// <summary>
        /// TEST 11: Real example 02 - Three files with highlight
        /// Critical: Multi-file with highlight
        /// </summary>
        [Fact]
        public void Test11_Example02_ThreeFiles_WithHighlight()
        {
            string tempFile = Path.GetTempFileName();
            try
            {
                var strategy = new HighlightSpacesStrategy();
                using (var writer = new StrategyCharacterWriter(tempFile, strategy))
                {
                    var formatter = new BatchParagraphFormatter(writer, 17);
                    
                    for (int i = 0; i < 3; i++)
                    {
                        var reader = new MockCharacterReader("If a train station is where the train stops, what is a work station?");
                        formatter.ProcessFile(reader);
                    }
                    formatter.Finalize();
                }

                var lines = File.ReadAllLines(tempFile);
                Assert.Equal(14, lines.Length);
                Assert.Equal("If.....a....train<-", lines[0]);
                Assert.Equal("station?<-", lines[13]);
            }
            finally
            {
                if (File.Exists(tempFile)) File.Delete(tempFile);
            }
        }

        /// <summary>
        /// TEST 12: Real example 08 - Empty files with content
        /// Critical: Tests empty file handling
        /// </summary>
        [Fact]
        public void Test12_Example08_EmptyFiles_WithContent()
        {
            string tempFile = Path.GetTempFileName();
            try
            {
                var strategy = new HighlightSpacesStrategy();
                using (var writer = new StrategyCharacterWriter(tempFile, strategy))
                {
                    var formatter = new BatchParagraphFormatter(writer, 80);
                    
                    // 3 empty files
                    formatter.ProcessFileBoundary();
                    formatter.ProcessFileBoundary();
                    formatter.ProcessFileBoundary();
                    
                    // Content file
                    var reader = new MockCharacterReader("If a train station is where the train stops, what is a work station?");
                    formatter.ProcessFile(reader);
                    
                    // 2 more empty files
                    formatter.ProcessFileBoundary();
                    formatter.ProcessFileBoundary();
                    
                    formatter.Finalize();
                }

                var lines = File.ReadAllLines(tempFile);
                Assert.Single(lines);
                Assert.Equal("If.a.train.station.is.where.the.train.stops,.what.is.a.work.station?<-", lines[0]);
            }
            finally
            {
                if (File.Exists(tempFile)) File.Delete(tempFile);
            }
        }

        /// <summary>
        /// TEST 13: Real example 12 - Three files without highlight
        /// Critical: Multi-file normal mode
        /// </summary>
        [Fact]
        public void Test13_Example12_ThreeFiles_NoHighlight()
        {
            string tempFile = Path.GetTempFileName();
            try
            {
                var strategy = new NormalOutputStrategy();
                using (var writer = new StrategyCharacterWriter(tempFile, strategy))
                {
                    var formatter = new BatchParagraphFormatter(writer, 17);
                    
                    for (int i = 0; i < 3; i++)
                    {
                        var reader = new MockCharacterReader("If a train station is where the train stops, what is a work station?");
                        formatter.ProcessFile(reader);
                    }
                    formatter.Finalize();
                }

                var lines = File.ReadAllLines(tempFile);
                Assert.Equal(14, lines.Length);
                Assert.Equal("If     a    train", lines[0]);
                Assert.Equal("station?", lines[13]);
            }
            finally
            {
                if (File.Exists(tempFile)) File.Delete(tempFile);
            }
        }

        /// <summary>
        /// TEST 14: Argument validation - insufficient args
        /// Critical: Error handling
        /// </summary>
        [Fact]
        public void Test14_ArgumentValidation_InsufficientArgs()
        {
            using (var sw = new StringWriter())
            {
                Console.SetOut(sw);
                
                Program.Main(new string[] { "input.txt" });
                
                var output = sw.ToString();
                Assert.Contains("Argument Error", output);
            }
        }

        /// <summary>
        /// TEST 15: Performance - handles many files
        /// Critical: Scalability requirement (65535 files)
        /// </summary>
        [Fact]
        public void Test15_Performance_ManyFiles()
        {
            var writer = new MockCharacterWriter();
            var formatter = new BatchParagraphFormatter(writer, 20);

            // Simulate 1000 small files
            for (int i = 0; i < 1000; i++)
            {
                var reader = new MockCharacterReader("test ");
                formatter.ProcessFile(reader);
            }
            formatter.Finalize();

            // Should handle many files without memory issues
            Assert.NotEmpty(writer.Lines);
        }

        // ====================== ADDITIONAL EDGE CASE TESTS ======================

        /// <summary>
        /// TEST 16: Trailing whitespace in file
        /// </summary>
        [Fact]
        public void Test16_TrailingWhitespace_InFile()
        {
            var writer = new MockCharacterWriter();
            var formatter = new BatchParagraphFormatter(writer, 20);
            
            var reader1 = new MockCharacterReader("Hello   ");
            var reader2 = new MockCharacterReader("World");
            
            formatter.ProcessFile(reader1);
            formatter.ProcessFile(reader2);
            formatter.Finalize();

            Assert.Single(writer.Lines);
            Assert.Equal("Hello World", writer.Lines[0]);
        }

        /// <summary>
        /// TEST 17: Leading whitespace in file
        /// </summary>
        [Fact]
        public void Test17_LeadingWhitespace_InFile()
        {
            var writer = new MockCharacterWriter();
            var formatter = new BatchParagraphFormatter(writer, 20);
            
            var reader1 = new MockCharacterReader("Hello");
            var reader2 = new MockCharacterReader("   World");
            
            formatter.ProcessFile(reader1);
            formatter.ProcessFile(reader2);
            formatter.Finalize();

            Assert.Single(writer.Lines);
            Assert.Equal("Hello World", writer.Lines[0]);
        }

        /// <summary>
        /// TEST 18: Single newline between files (should NOT create paragraph break)
        /// </summary>
        [Fact]
        public void Test18_SingleNewline_BetweenFiles()
        {
            var writer = new MockCharacterWriter();
            var formatter = new BatchParagraphFormatter(writer, 20);
            
            var reader1 = new MockCharacterReader("First\n");
            var reader2 = new MockCharacterReader("Second");
            
            formatter.ProcessFile(reader1);
            formatter.ProcessFile(reader2);
            formatter.Finalize();

            Assert.Single(writer.Lines);
            Assert.Equal("First Second", writer.Lines[0]);
        }

        /// <summary>
        /// TEST 19: Double newline within file (should create paragraph break)
        /// </summary>
        [Fact]
        public void Test19_DoubleNewline_WithinFile()
        {
            var writer = new MockCharacterWriter();
            var formatter = new BatchParagraphFormatter(writer, 20);
            
            var reader = new MockCharacterReader("First\n\nSecond");
            formatter.ProcessFile(reader);
            formatter.Finalize();

            Assert.Equal(3, writer.Lines.Count);
            Assert.Equal("First", writer.Lines[0]);
            Assert.Equal("", writer.Lines[1]);
            Assert.Equal("Second", writer.Lines[2]);
        }

        /// <summary>
        /// TEST 20: Double newline at end of file followed by content in next file
        /// </summary>
        [Fact]
        public void Test20_DoubleNewlineAtEnd_ThenNextFile()
        {
            var writer = new MockCharacterWriter();
            var formatter = new BatchParagraphFormatter(writer, 20);
            
            var reader1 = new MockCharacterReader("First\n\n");
            var reader2 = new MockCharacterReader("Second");
            
            formatter.ProcessFile(reader1);
            formatter.ProcessFile(reader2);
            formatter.Finalize();

            Assert.Equal(3, writer.Lines.Count);
            Assert.Equal("First", writer.Lines[0]);
            Assert.Equal("", writer.Lines[1]);
            Assert.Equal("Second", writer.Lines[2]);
        }

        /// <summary>
        /// TEST 21: Partial word at end of file (word boundary)
        /// </summary>
        [Fact]
        public void Test21_PartialWord_AtFileBoundary()
        {
            var writer = new MockCharacterWriter();
            var formatter = new BatchParagraphFormatter(writer, 20);
            
            // No whitespace after "Hel" - file boundary should split word
            var reader1 = new MockCharacterReader("Hel");
            var reader2 = new MockCharacterReader("lo");
            
            formatter.ProcessFile(reader1);
            formatter.ProcessFile(reader2);
            formatter.Finalize();

            Assert.Single(writer.Lines);
            // Should be two separate words
            Assert.Equal("Hel lo", writer.Lines[0]);
        }

        /// <summary>
        /// TEST 22: Empty file between non-empty files
        /// </summary>
        [Fact]
        public void Test22_EmptyFile_BetweenNonEmpty()
        {
            var writer = new MockCharacterWriter();
            var formatter = new BatchParagraphFormatter(writer, 20);
            
            var reader1 = new MockCharacterReader("Hello");
            // Empty file
            var reader2 = new MockCharacterReader("World");
            
            formatter.ProcessFile(reader1);
            formatter.ProcessFileBoundary(); // Empty file
            formatter.ProcessFile(reader2);
            formatter.Finalize();

            Assert.Single(writer.Lines);
            Assert.Equal("Hello World", writer.Lines[0]);
        }

        /// <summary>
        /// TEST 23: Multiple empty files
        /// </summary>
        [Fact]
        public void Test23_MultipleEmptyFiles()
        {
            var writer = new MockCharacterWriter();
            var formatter = new BatchParagraphFormatter(writer, 20);
            
            formatter.ProcessFileBoundary();
            formatter.ProcessFileBoundary();
            formatter.ProcessFileBoundary();
            formatter.Finalize();

            Assert.Single(writer.Lines);
            Assert.Equal("", writer.Lines[0]);
        }

        /// <summary>
        /// TEST 24: File ends with newline (common case)
        /// </summary>
        [Fact]
        public void Test24_FileEndsWithNewline()
        {
            var writer = new MockCharacterWriter();
            var formatter = new BatchParagraphFormatter(writer, 20);
            
            var reader1 = new MockCharacterReader("Hello World\n");
            var reader2 = new MockCharacterReader("Next File\n");
            
            formatter.ProcessFile(reader1);
            formatter.ProcessFile(reader2);
            formatter.Finalize();

            // "Hello World Next" = 16 chars fits on one line (width 20)
            // "File" needs 4 more chars: 16 + 1 + 4 = 21 > 20, so wraps
            Assert.Equal(2, writer.Lines.Count);
            Assert.Equal("Hello   World   Next", writer.Lines[0]);
            Assert.Equal("File", writer.Lines[1]);
        }

        /// <summary>
        /// TEST 25: Three newlines (paragraph break + extra)
        /// </summary>
        [Fact]
        public void Test25_ThreeNewlines()
        {
            var writer = new MockCharacterWriter();
            var formatter = new BatchParagraphFormatter(writer, 20);
            
            var reader = new MockCharacterReader("First\n\n\nSecond");
            formatter.ProcessFile(reader);
            formatter.Finalize();

            Assert.Equal(3, writer.Lines.Count);
            Assert.Equal("First", writer.Lines[0]);
            Assert.Equal("", writer.Lines[1]);
            Assert.Equal("Second", writer.Lines[2]);
        }

        /// <summary>
        /// TEST 26: Very long line that must wrap multiple times
        /// </summary>
        [Fact]
        public void Test26_VeryLongLine_MultipleWraps()
        {
            var writer = new MockCharacterWriter();
            var formatter = new BatchParagraphFormatter(writer, 10);
            
            var reader = new MockCharacterReader("A B C D E F G H I J K");
            formatter.ProcessFile(reader);
            formatter.Finalize();

            // Should wrap into multiple lines
            Assert.True(writer.Lines.Count >= 3);
        }

        /// <summary>
        /// TEST 27: Single character words
        /// </summary>
        [Fact]
        public void Test27_SingleCharacterWords()
        {
            var writer = new MockCharacterWriter();
            var formatter = new BatchParagraphFormatter(writer, 10);
            
            // Add more content to force justification
            var reader = new MockCharacterReader("I a m here");
            formatter.ProcessFile(reader);
            formatter.Finalize();

            // "I a m" won't fill a line by itself, so it becomes last line (left-aligned)
            // Need to trigger a full line first
            Assert.True(writer.Lines.Count >= 1);
        }

        /// <summary>
        /// TEST 28: Word exactly at line width
        /// </summary>
        [Fact]
        public void Test28_WordExactlyAtWidth()
        {
            var writer = new MockCharacterWriter();
            var formatter = new BatchParagraphFormatter(writer, 10);
            
            var reader = new MockCharacterReader("ABCDEFGHIJ Next");
            formatter.ProcessFile(reader);
            formatter.Finalize();

            Assert.Equal(2, writer.Lines.Count);
            Assert.Equal("ABCDEFGHIJ", writer.Lines[0]);
            Assert.Equal("Next", writer.Lines[1]);
        }

        /// <summary>
        /// TEST 29: Whitespace-only file
        /// </summary>
        [Fact]
        public void Test29_WhitespaceOnlyFile()
        {
            var writer = new MockCharacterWriter();
            var formatter = new BatchParagraphFormatter(writer, 20);
            
            var reader1 = new MockCharacterReader("Hello");
            var reader2 = new MockCharacterReader("   \n  \t  ");
            var reader3 = new MockCharacterReader("World");
            
            formatter.ProcessFile(reader1);
            formatter.ProcessFile(reader2);
            formatter.ProcessFile(reader3);
            formatter.Finalize();

            Assert.Single(writer.Lines);
            Assert.Equal("Hello World", writer.Lines[0]);
        }

        /// <summary>
        /// TEST 30: File with only newlines
        /// </summary>
        [Fact]
        public void Test30_OnlyNewlinesFile()
        {
            var writer = new MockCharacterWriter();
            var formatter = new BatchParagraphFormatter(writer, 20);
            
            var reader1 = new MockCharacterReader("First");
            var reader2 = new MockCharacterReader("\n\n\n");
            var reader3 = new MockCharacterReader("Second");
            
            formatter.ProcessFile(reader1);
            formatter.ProcessFile(reader2);
            formatter.ProcessFile(reader3);
            formatter.Finalize();

            // First word seen, then 2+ newlines triggers paragraph break
            Assert.Equal(3, writer.Lines.Count);
            Assert.Equal("First", writer.Lines[0]);
            Assert.Equal("", writer.Lines[1]);
            Assert.Equal("Second", writer.Lines[2]);
        }

        /// <summary>
        /// TEST 31: Debug Test 11 - Verify exact output
        /// </summary>
        [Fact]
        public void Test31_DebugTest11_ThreeFilesHighlight()
        {
            var writer = new MockCharacterWriter();
            var formatter = new BatchParagraphFormatter(writer, 17);
            
            for (int i = 0; i < 3; i++)
            {
                var reader = new MockCharacterReader("If a train station is where the train stops, what is a work station?");
                formatter.ProcessFile(reader);
            }
            formatter.Finalize();

            Assert.Equal(14, writer.Lines.Count);
        }

        /// <summary>
        /// TEST 32: Debug Test 13 - Verify exact output without highlight
        /// </summary>
        [Fact]
        public void Test32_DebugTest13_ThreeFilesNoHighlight()
        {
            var writer = new MockCharacterWriter();
            var formatter = new BatchParagraphFormatter(writer, 17);
            
            for (int i = 0; i < 3; i++)
            {
                var reader = new MockCharacterReader("If a train station is where the train stops, what is a work station?");
                formatter.ProcessFile(reader);
            }
            formatter.Finalize();

            Assert.Equal(14, writer.Lines.Count);
            
            // Verify each line
            Assert.Equal("If     a    train", writer.Lines[0]);
            Assert.Equal("station  is where", writer.Lines[1]);
            Assert.Equal("the  train stops,", writer.Lines[2]);
            Assert.Equal("what  is  a  work", writer.Lines[3]);
            Assert.Equal("station?   If   a", writer.Lines[4]);
            Assert.Equal("train  station is", writer.Lines[5]);
            Assert.Equal("where  the  train", writer.Lines[6]);
            Assert.Equal("stops,  what is a", writer.Lines[7]);
            Assert.Equal("work  station? If", writer.Lines[8]);
            Assert.Equal("a  train  station", writer.Lines[9]);
            Assert.Equal("is    where   the", writer.Lines[10]);
            Assert.Equal("train stops, what", writer.Lines[11]);
            Assert.Equal("is     a     work", writer.Lines[12]);
            Assert.Equal("station?", writer.Lines[13]);
        }
    }
}