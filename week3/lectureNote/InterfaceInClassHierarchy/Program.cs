interface I1
{
    public int m1(int a, int b);
}

class A : I1
{
    public int m1(int a, int b)
    {
        return a + b;
    }
}

class B1
{
    public int m1(int a, int b)
    {
        return a + b + 1000;
    }
}

class B2 : B1, I1 { }

class C1 : I1
{
    public int m1(int a, int b)
    {
        return a + b + 2000;
    }
}

class C2 : C1
{
    public int m1(int a, int b)
    {
        return a / b;
    }
}

class C3 : C1, I1
{
    public int m1(int first, int second)
    {
        return first - second;
    }
}

class C4 : C2, I1{}

class Program
{
    static void Main()
    {
        // Error
        // I1 i = new I1(); 

        I1 i1 = new A();
        Console.WriteLine($"i1.m1(10,20): {i1.m1(10, 20)}");

        // Error
        // i1 = new B1(); 

        i1 = new B2();
        Console.WriteLine($"i1.m1(10,20): {i1.m1(10, 20)}");

        Console.WriteLine();
        Console.WriteLine("-----------");
        Console.WriteLine();


        C1 c1 = new C1();
        Console.WriteLine($"c1.m1(10,20): {c1.m1(10, 20)}");

        i1 = new C1();
        Console.WriteLine($"i1.m1(10,20): {i1.m1(10, 20)}");
        

        C2 c2 = new C2();
        Console.WriteLine($"c2.m1(10,20): {c2.m1(10, 20)}");

        Console.WriteLine();

        c1 = c2;
        Console.WriteLine($"c1.m1(10,20): {c1.m1(10, 20)}");

        Console.WriteLine();

        i1 = c2;
        Console.WriteLine($"i1.m1(10,20): {i1.m1(10, 20)}");

        i1 = new C3();
        Console.WriteLine($"i1.m1(10,20): {i1.m1(10, 20)}");

        Console.WriteLine();

        i1 = new C4();
        Console.WriteLine($"i1.m1(10,20): {i1.m1(10, 20)}");

    }
}