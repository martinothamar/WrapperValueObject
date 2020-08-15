# WrapperValueObject

A .NET source generator for creating simple value objects wrapping primitive types.

## Example

1. Use the attribute to specify the underlying type.
2. Declare the struct or class with the `partial` keyword.

```csharp
using System;

namespace WrapperValueObject.TestConsole
{
    [WrapperValueObject(typeof(float))]
    public readonly partial struct SomeValue
    {
    }

    public static class Program
    {
        static void Main(string[] args)
        {
            SomeValue value1 = 1f;
            SomeValue value2 = 2f;

            Console.WriteLine($"value1: {value1}, value2: {value2}");           // value1: 1, value2: 2
            Console.WriteLine($"Equals: {value1.Equals(value2)}");              // Equals: False
            Console.WriteLine($"==: {value1 == value2}");                       // ==: False
            Console.WriteLine($"value1 + value2 = {value1 + value2}");          // value1 + value2 = 3
            Console.WriteLine($"ToString(format): {value1.ToString("0.00")}");  // ToString(format): 1,00
        }
    }
}
```

### TODO

* Support more types
* Don't generate duplicate methods if a user specifies one
* Options to specify which types of methods to generate/not generate? I.e. math operator overloads, implicit/explicit casting
