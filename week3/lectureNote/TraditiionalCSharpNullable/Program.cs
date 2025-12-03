
namespace TraditionalCSharpNullable
{
    class A
    {
        public int x;
    }

    class Program
    {
        static void Main(string[] args)
        {
            A a1 = new A { x = 10 };
            Console.WriteLine($"a1 == {a1}");

            a1 = null;
            Console.WriteLine($"a1 == {a1}");

            /*/
            Console.WriteLine($"a1.x == {a1.x}"); // '.' -> check at runtime
            /*/
            string s = a1.ToString();
            Console.WriteLine("will print s:");
            Console.WriteLine(s);
            /**/
            
        }
    }
}