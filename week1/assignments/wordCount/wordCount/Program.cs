namespace wordCount;

class Program
{
    static void Main(string[] args)
    {
        // In case of invalid number of command-line arguments 
        if (args.Length != 1)
        {
            Console.WriteLine("Argument Error");
            return;
        }
        string fileName = args[0];
        int count = 0; try
        {
            // "using" key word ensures the file is closed after usage 
            using (var reader = new StreamReader(fileName))
            {
                string line; while ((line = reader.ReadLine()) != null)
                {
                    var words = line.Split(new[] { ' ', '\t', '\n' }, StringSplitOptions.RemoveEmptyEntries); count += words.Length;
                }
            }
        }
        catch
        {
            // In case of problems encountered when opening 
            Console.WriteLine("File Error");
            return;
        }
        Console.WriteLine(count);
    }
}