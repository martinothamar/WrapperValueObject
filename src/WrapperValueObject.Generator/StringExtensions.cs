namespace WrapperValueObject.Generator
{
    public static class StringExtensions
    {
        public static string FirstCharToLower(this string str)
        {
            return char.ToLowerInvariant(str[0]) + str.Substring(1);
        }
    }
}
