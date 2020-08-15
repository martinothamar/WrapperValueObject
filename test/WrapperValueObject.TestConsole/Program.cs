using System;

namespace WrapperValueObject.TestConsole
{
    [WrapperValueObject(typeof(float))]
    public readonly partial struct SomeValue
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
            Console.WriteLine($"ToString(format): {value1.ToString("0.00")}");
        }
    }
}
