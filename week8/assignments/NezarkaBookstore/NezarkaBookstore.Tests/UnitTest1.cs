using System;
using System.IO;
using Xunit;

namespace NezarkaBookstore.Tests
{
    public class UnitTest1
    {
        [Fact]
        public void TestBooksListing()
        {
            string input =
        @"DATA-BEGIN
BOOK;1;Lord of the Rings;J. R. R. Tolkien;59
BOOK;2;Hobbit: There and Back Again;J. R. R. Tolkien;49
BOOK;3;Going Postal;Terry Pratchett;28
BOOK;4;The Colour of Magic;Terry Pratchett;159
BOOK;5;I Shall Wear Midnight;Terry Pratchett;31
CUSTOMER;1;Jan;Novak
CART-ITEM;1;1;1
CART-ITEM;1;2;1
DATA-END
GET 1 http://www.nezarka.net/Books";

            string path = Path.Combine(AppContext.BaseDirectory, "TestFiles", "02-Books.html");
            string expectedOutput = File.ReadAllText(path);
            expectedOutput += "====\n";

            var inputReader = new StringReader(input);
            var outputWriter = new StringWriter();
            Console.SetIn(inputReader);
            Console.SetOut(outputWriter);

            NezarkaBookstore.Program.Main(Array.Empty<string>());

            string actualOutput = outputWriter.ToString();

            // Normalize line endings
            expectedOutput = expectedOutput.Replace("\r\n", "\n");
            actualOutput = actualOutput.Replace("\r\n", "\n").Replace("\r", "\n");

            Assert.Equal(expectedOutput, actualOutput);
        }


        [Fact]
        public void TestShoppingCart()
        {
            string input =
        @"DATA-BEGIN
BOOK;1;Lord of the Rings;J. R. R. Tolkien;59
BOOK;2;Hobbit: There and Back Again;J. R. R. Tolkien;49
BOOK;3;Going Postal;Terry Pratchett;28
BOOK;4;The Colour of Magic;Terry Pratchett;159
BOOK;5;I Shall Wear Midnight;Terry Pratchett;31
CUSTOMER;1;Jan;Novak
CART-ITEM;1;1;3
CART-ITEM;1;5;1
CART-ITEM;1;3;1
DATA-END
GET 1 http://www.nezarka.net/ShoppingCart";

            string path = Path.Combine(AppContext.BaseDirectory, "TestFiles", "04-ShoppingCart.html");
            string expectedOutput = File.ReadAllText(path);
            expectedOutput += "====\n";

            var inputReader = new StringReader(input);
            var outputWriter = new StringWriter();
            Console.SetIn(inputReader);
            Console.SetOut(outputWriter);

            NezarkaBookstore.Program.Main(Array.Empty<string>());

            string actualOutput = outputWriter.ToString();

            // Normalize line endings
            expectedOutput = expectedOutput.Replace("\r\n", "\n");
            actualOutput = actualOutput.Replace("\r\n", "\n").Replace("\r", "\n");

            Assert.Equal(expectedOutput, actualOutput);
        }

        [Fact]
        public void TestBooksDetail()
        {
            string input =
        @"DATA-BEGIN
BOOK;1;Lord of the Rings;J. R. R. Tolkien;59
BOOK;2;Hobbit: There and Back Again;J. R. R. Tolkien;49
BOOK;3;Going Postal;Terry Pratchett;28
BOOK;4;The Colour of Magic;Terry Pratchett;159
BOOK;5;I Shall Wear Midnight;Terry Pratchett;31
CUSTOMER;1;Jan;Novak
CART-ITEM;1;1;1
CART-ITEM;1;2;1
DATA-END
GET 1 http://www.nezarka.net/Books/Detail/3";

            string path = Path.Combine(AppContext.BaseDirectory, "TestFiles", "03-BooksDetail.html");
            string expectedOutput = File.ReadAllText(path);
            expectedOutput += "====\n";

            var inputReader = new StringReader(input);
            var outputWriter = new StringWriter();
            Console.SetIn(inputReader);
            Console.SetOut(outputWriter);

            NezarkaBookstore.Program.Main(Array.Empty<string>());

            string actualOutput = outputWriter.ToString();

            // Normalize line endings
            expectedOutput = expectedOutput.Replace("\r\n", "\n");
            actualOutput = actualOutput.Replace("\r\n", "\n").Replace("\r", "\n");

            Assert.Equal(expectedOutput, actualOutput);
        }

        [Fact]
        public void TestShoppingCartEmpty()
        {
            string input =
        @"DATA-BEGIN
BOOK;1;Lord of the Rings;J. R. R. Tolkien;59
BOOK;2;Hobbit: There and Back Again;J. R. R. Tolkien;49
BOOK;3;Going Postal;Terry Pratchett;28
BOOK;4;The Colour of Magic;Terry Pratchett;159
BOOK;5;I Shall Wear Midnight;Terry Pratchett;31
CUSTOMER;1;Jan;Novak
CUSTOMER;2;Pavel;Novak
DATA-END
GET 2 http://www.nezarka.net/ShoppingCart";

            string path = Path.Combine(AppContext.BaseDirectory, "TestFiles", "05-ShoppingCart-Empty.html");
            string expectedOutput = File.ReadAllText(path);
            expectedOutput += "====\n";

            var inputReader = new StringReader(input);
            var outputWriter = new StringWriter();
            Console.SetIn(inputReader);
            Console.SetOut(outputWriter);

            NezarkaBookstore.Program.Main(Array.Empty<string>());

            string actualOutput = outputWriter.ToString();

            // Normalize line endings
            expectedOutput = expectedOutput.Replace("\r\n", "\n");
            actualOutput = actualOutput.Replace("\r\n", "\n").Replace("\r", "\n");

            Assert.Equal(expectedOutput, actualOutput);
        }


        [Fact]
        public void TestInvalidRequest()
        {
            string input = @"DATA-BEGIN
CUSTOMER;1;Jan;Novak
DATA-END
GET 1 http://www.nezarka.net/InvalidPath";

            var inputReader = new StringReader(input);
            var outputWriter = new StringWriter();

            var originalIn = Console.In;
            var originalOut = Console.Out;

            try
            {
                Console.SetIn(inputReader);
                Console.SetOut(outputWriter);

                NezarkaBookstore.Program.Main(new string[] { });

                string actualOutput = outputWriter.ToString();

                Assert.Contains("Invalid request.", actualOutput);
                Assert.Contains("====", actualOutput);
            }
            finally
            {
                Console.SetIn(originalIn);
                Console.SetOut(originalOut);
            }
        }

        [Fact]
        public void TestDataError()
        {
            string input = @"DATA-BEGIN
BOOK;invalid;Title;Author;100
DATA-END";

            var inputReader = new StringReader(input);
            var outputWriter = new StringWriter();

            var originalIn = Console.In;
            var originalOut = Console.Out;

            try
            {
                Console.SetIn(inputReader);
                Console.SetOut(outputWriter);

                NezarkaBookstore.Program.Main(new string[] { });

                string actualOutput = outputWriter.ToString();

                Assert.Equal("Data error.\n", actualOutput);
            }
            finally
            {
                Console.SetIn(originalIn);
                Console.SetOut(originalOut);
            }
        }


        
    }
}