- python is a dynamic typed language
- to use duck typing in static languages we use a contract
- contract 
	- one party implement the contract
	- another use the contract
	- **they can use the subset of the contract but not more than it**
	- contract is a **interface**, and it's a reference type
	- there are methods, properties in an interface but field is forbiddened
	- needs to provide explicit `public`, `private`
	- i can create any instance that implements I1
		- class A: I1{}
		- I1 i1 = new A();
		- even if constructor in A() returns a B:A it will work (B is type of A)
		- 


```C#
interface I1 {
	public int m1(int a, int b);
}

class A: I1 {
	public int m1(int a, int b){
		return a+b;
	}
}

class B1 {
	public int m1(int a, int b){
		return a+b;
	}
}

class B2: B1, I1{}

static void Main(){
	I1 i1 = new I1(); // will not work
	
	I1 i1 = new A();
	// create I1 i1 on stack
	// create instance A on GC heap and since there's no field. the object is 0 bytes with some overhead
	// stack -----> gc heap -----> type describing existence of type A
	Console.WriteLine($"i1.m1(10, 20): {i1.m1(10, 20)}");
	
	i1 = new B1(); // will not work
	
	i1 = new B2();
	// since it inheritate from B1 and B1 has no field, the object is 0 bytes with some overhead
	// remove I1 i1 pointer on stack which pointed to A and point it to B2 object
	// stack -----> gc heap B2 -----> type describing existence of type B2 -------> type describing existence of type B1
	Console.WriteLine($"i1.m1(10, 20): {i1.m1(10, 20)}");
	
	
}
```
![[Screenshot 2025-11-02 at 11.57.16.png]]

## Interface
- in the above example there should be an interface type too
- there should be an interface table
- if type A implement interface I1, it gets an interface table and this table contains
	- the table points to the method not the implementation
	- and the method points to the implementation (machine code)
	- when calling I1.m1 ---> getType() ---> I1 table`[0]` ?????idk
![[Screenshot 2025-11-02 at 12.23.00.png]]![[Screenshot 2025-11-02 at 12.26.43.png]]

## Harder Example explanation
```C#

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
```