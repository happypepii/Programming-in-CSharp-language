namespace HuffmanCodingI.Tests;

public class UnitTest1
{
    [Fact]
    public void InputTest1()
    {
        // Arrange
        string inputFile = Path.Combine(
            AppContext.BaseDirectory,
                "Tests",
                "binary.in"
        );

        string expectedOutput = File.ReadAllText(
            Path.Combine(AppContext.BaseDirectory, "Tests", "binary.out")
        );


        var sw = new StringWriter();
        Console.SetOut(sw);

        // Act
        Program.Main(new[] { inputFile });

        // Assert
        string actualOutput = sw.ToString();

        Assert.Equal(expectedOutput, actualOutput);
    }



    [Fact]
    public void InputTest2()
    {
        // Arrange
        string inputFile = Path.Combine(
            AppContext.BaseDirectory,
                "Tests",
                "simple.in"
        );

        string expectedOutput = File.ReadAllText(
            Path.Combine(AppContext.BaseDirectory, "Tests", "simple.out")
        );


        var sw = new StringWriter();
        Console.SetOut(sw);

        // Act
        Program.Main(new[] { inputFile });

        // Assert
        string actualOutput = sw.ToString();

        Assert.Equal(expectedOutput, actualOutput);
    }

    [Fact]
    public void InputTest3()
    {
        // Arrange
        string inputFile = Path.Combine(
            AppContext.BaseDirectory,
                "Tests",
                "simple2.in"
        );

        string expectedOutput = File.ReadAllText(
            Path.Combine(AppContext.BaseDirectory, "Tests", "simple2.out")
        );


        var sw = new StringWriter();
        Console.SetOut(sw);

        // Act
        Program.Main(new[] { inputFile });

        // Assert
        string actualOutput = sw.ToString();

        Assert.Equal(expectedOutput, actualOutput);
    }

    [Fact]
    public void InputTest4()
    {
        // Arrange
        string inputFile = Path.Combine(
            AppContext.BaseDirectory,
                "Tests",
                "simple3.in"
        );

        string expectedOutput = File.ReadAllText(
            Path.Combine(AppContext.BaseDirectory, "Tests", "simple3.out")
        );


        var sw = new StringWriter();
        Console.SetOut(sw);

        // Act
        Program.Main(new[] { inputFile });

        // Assert
        string actualOutput = sw.ToString();

        Assert.Equal(expectedOutput, actualOutput);
    }

    [Fact]
    public void InputTest5()
    {
        // Arrange
        string inputFile = Path.Combine(
            AppContext.BaseDirectory,
                "Tests",
                "simple4.in"
        );

        string expectedOutput = File.ReadAllText(
            Path.Combine(AppContext.BaseDirectory, "Tests", "simple4.out")
        );


        var sw = new StringWriter();
        Console.SetOut(sw);

        // Act
        Program.Main(new[] { inputFile });

        // Assert
        string actualOutput = sw.ToString();

        Assert.Equal(expectedOutput, actualOutput);
    }


    private string CreateTempFile(byte[] data)
    {
        string path = Path.GetTempFileName();
        File.WriteAllBytes(path, data);
        return path;
    }

    private string CaptureOutput(Action action)
    {
        var sw = new StringWriter();
        Console.SetOut(sw);
        action();
        return sw.ToString();
    }

    // ─────────────────────────────
    // Argument / File errors
    // ─────────────────────────────

    [Fact]
    public void NoArguments_ShouldPrintArgumentError()
    {
        string output = CaptureOutput(() =>
        {
            Program.Main(Array.Empty<string>());
        });

        Assert.Equal("Argument Error", output);
    }

    [Fact]
    public void TooManyArguments_ShouldPrintArgumentError()
    {
        string output = CaptureOutput(() =>
        {
            Program.Main(new[] { "a.in", "b.in" });
        });

        Assert.Equal("Argument Error", output);
    }

    [Fact]
    public void FileDoesNotExist_ShouldPrintFileError()
    {
        string output = CaptureOutput(() =>
        {
            Program.Main(new[] { "this_file_does_not_exist.bin" });
        });

        Assert.Equal("File Error", output);
    }

    // ─────────────────────────────
    // Content edge cases
    // ─────────────────────────────

    [Fact]
    public void EmptyFile_ShouldPrintNothing()
    {
        string path = CreateTempFile(Array.Empty<byte>());

        string output = CaptureOutput(() =>
        {
            Program.Main(new[] { path });
        });

        Assert.Equal(string.Empty, output);
    }

    [Fact]
    public void SingleCharacterOnly()
    {
        // 'A' = 65, repeated 6 times
        byte[] data = { 65, 65, 65, 65, 65, 65 };
        string path = CreateTempFile(data);

        string output = CaptureOutput(() =>
        {
            Program.Main(new[] { path });
        });

        Assert.Equal("*65:6", output);
    }

    [Fact]
    public void TwoCharacters_DifferentWeights()
    {
        // A=65 (1), B=66 (3)
        byte[] data = { 66, 66, 66, 65 };
        string path = CreateTempFile(data);

        string output = CaptureOutput(() =>
        {
            Program.Main(new[] { path });
        });

        // lighter (A) must be left
        Assert.Equal("4 *65:1 *66:3", output);
    }

    [Fact]
    public void TwoCharacters_SameWeight_CharacterPriority()
    {
        // A=65, B=66 → same weight
        byte[] data = { 66, 65 };
        string path = CreateTempFile(data);

        string output = CaptureOutput(() =>
        {
            Program.Main(new[] { path });
        });

        // smaller character code first
        Assert.Equal("2 *65:1 *66:1", output);
    }

    // ─────────────────────────────
    // Tie-breaking tests
    // ─────────────────────────────

    [Fact]
    public void MultipleEqualWeights_LeafBeforeInner()
    {
        // A B C D all once
        byte[] data = { 65, 66, 67, 68 };
        string path = CreateTempFile(data);

        string output = CaptureOutput(() =>
        {
            Program.Main(new[] { path });
        });

        // Deterministic result based on rules
        Assert.Equal(
            "4 2 *65:1 *66:1 2 *67:1 *68:1",
            output
        );
    }

    [Fact]
    public void InnerNodeCreationOrderMatters()
    {
        // Force same-weight inner nodes
        byte[] data =
        {
            65,65,   // A x2
            66,66,   // B x2
            67,67,   // C x2
            68,68    // D x2
        };

        string path = CreateTempFile(data);

        string output = CaptureOutput(() =>
        {
            Program.Main(new[] { path });
        });

        Assert.Equal(
            "8 4 *65:2 *66:2 4 *67:2 *68:2",
            output
        );
    }

    [Fact]
    public void SameWeight_LeafBeforeInner()
    {
        byte[] data = { 65, 66, 67, 67 };
        string path = CreateTempFile(data);

        string output = CaptureOutput(() =>
        {
            Program.Main(new[] { path });
        });

        Assert.Equal("4 *67:2 2 *65:1 *66:1", output);
    }

    [Fact]
    public void ComplexTieBreaking()
    {
        byte[] data = { 65, 66, 67, 68, 68, 68 };
        string path = CreateTempFile(data);

        string output = CaptureOutput(() =>
        {
            Program.Main(new[] { path });
        });

        Assert.Equal("6 *68:3 3 *67:1 2 *65:1 *66:1", output);
    }

    // ─────────────────────────────
    // Binary / non-text bytes
    // ─────────────────────────────

    [Fact]
    public void BinaryBytes_ZeroAndMax()
    {
        // 0x00 twice, 0xFF once
        byte[] data = { 0, 0, 255 };
        string path = CreateTempFile(data);

        string output = CaptureOutput(() =>
        {
            Program.Main(new[] { path });
        });

        Assert.Equal(
            "3 *255:1 *0:2",
            output
        );
    }

    // ─────────────────────────────
    // "Huge file" simulation (O(1) memory test)
    // ─────────────────────────────

    [Fact]
    public void VeryLargeFile_Simulated()
    {
        // 100 million bytes, but only 1 byte type
        const int count = 100_000_000;
        byte[] data = new byte[count];
        Array.Fill(data, (byte)42);

        string path = CreateTempFile(data);

        string output = CaptureOutput(() =>
        {
            Program.Main(new[] { path });
        });

        Assert.Equal("*42:100000000", output);
    }

}