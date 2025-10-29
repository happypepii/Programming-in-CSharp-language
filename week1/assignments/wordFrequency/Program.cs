namespace wordFrequency;

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
        // "var" key word is used for type inference
        // is equivalent to Dictionary<string, int> frequencies 
        var frequencies = new Dictionary<string, int>();
        try
        {
            // "using" key word ensures the file is closed after usage
            using (var reader = new StreamReader(fileName))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    var words = line.Split(new[] { ' ', '\t', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var word in words)
                    {
                        // if word alreay in the dictionary, add count
                        if (frequencies.ContainsKey(word))
                        {
                            frequencies[word]++;
                        }
                        // add word to dictionary with count = 1
                        else
                        {
                            frequencies[word] = 1;
                        }

                    }
                }

            }

        }
        catch
        {
            // In case of problems encountered when opening
            Console.WriteLine("File Error");
            return;
        }

        foreach(var pair in frequencies.OrderBy(p => p.Key))
        {
            Console.WriteLine($"{pair.Key}: {pair.Value}");
        }
    }
}
