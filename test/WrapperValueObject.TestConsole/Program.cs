using System;

namespace WrapperValueObject.TestConsole
{
    [WrapperValueObject(typeof(float))]
    public readonly partial struct SomeValue
    {
        public override string ToString() => _value.ToString("0.0");
    }

    [WrapperValueObject(typeof(float), typeof(float))]
    public readonly partial struct SomeCompoundValue
    {
    }

    public static class Program
    {
        public static string ToString(this in SomeValue value, string? format) => "asdsa";

        static void Main(string[] args)
        {
            SomeValue value1 = 1f;
            SomeValue value2 = 2f;

            Console.WriteLine($"value1: {value1}, value2: {value2}");
            Console.WriteLine($"Equals: {value1.Equals(value2)}");
            Console.WriteLine($"value1 + value2 = {value1 + value2}");
            Console.WriteLine($"ToString(): {value1.ToString()}");
            Console.WriteLine($"ToString(format): {value1.ToString("0.00")}");

            Console.WriteLine();

            var value3 = new SomeCompoundValue(1f, 2f);
            var value4 = new SomeCompoundValue(1f, 2f);

            Console.WriteLine($"value3: {value3}, value4: {value4}");
            Console.WriteLine($"Equals: {value3.Equals(value4)}");
            Console.WriteLine($"ToString(): {value3}");
        }
    }
}
