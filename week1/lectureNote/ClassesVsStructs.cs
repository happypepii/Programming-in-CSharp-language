using System.Runtime.Intrinsics.Arm;

namespace ClassesVsStructs
{
    class C
    {
        public int x;
    }

    struct S
    {
        public int y;
    }

    class Program
    {
        static void Main(string[] args)
        {
            C c1 = new C();
            c1.x = 10;

            S s1 = new S();
            s1.y = 100;

            Console.WriteLine("Main before f call: c1.x == {0}, s1.y =={1}", c1.x, s1.y);
            f(c1, s1);
            Console.WriteLine("Main after f call: c1.x == {0}, s1.y =={1}", c1.x, s1.y);

            Console.WriteLine();
            Console.WriteLine(c1);
            Console.WriteLine(s1);
        }

        static void f(C c2, S s2)
        {
            Console.WriteLine("f starts:       c2.x == {0}, s2.y =={1}", c2.x, s2.y);
            c2.x = 20;
            s2.y = 200;
            Console.WriteLine("f before g calls:       c2.x == {0}, s2.y =={1}", c2.x, s2.y);
            g(c2, s2);

            Console.WriteLine("f ends:       c2.x == {0}, s2.y =={1}", c2.x, s2.y);

        }
        static void g(C c3, S s3)
        {
            Console.WriteLine("g starts:       c3.x == {0}, s3.y =={1}", c3.x, s3.y);
            c3.x = 30;
            s3.y = 300;
            Console.WriteLine("g ends:       c3.x == {0}, s3.y == {1}", c3.x, s3.y);

        }
        
    }
}