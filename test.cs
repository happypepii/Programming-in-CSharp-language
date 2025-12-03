using System;

namespace BugFindingTale
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var line = Console.ReadLine();
                if (line == null)
                {
                    Console.WriteLine("Error!");
                    return;
                }

                // what if the line doesn't contain a number? or it's a float/double
                // or a number contains ,
                // => throws format exception
                var a = int.Parse(line); 
                if (a < 0)
                {
                    Console.WriteLine("Error!");
                    return;
                }

                line = Console.ReadLine();
                if (line == null)
                {
                    Console.WriteLine("Error!");
                    return;
                }
                var b = int.Parse(line);
                if (b < 0)
                {
                    Console.WriteLine("Error!");
                    return;
                }

                var result = a - b;

                Console.WriteLine("Result: {0}", result);
            }
            catch (FormatException)
            {
                Console.WriteLine("Error.");
            }
        }
    }
}