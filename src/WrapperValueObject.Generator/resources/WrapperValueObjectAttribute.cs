using System;

namespace WrapperValueObject
{
    [Flags]
    public enum WrapperValueObjectJsonConverter
    {
        None = 0,
        NewtonsoftJson = 1,
        SystemTextJson = 2,
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false)]
    public class WrapperValueObjectAttribute : Attribute
    {
        private readonly (string Name, Type Type)[] _types;
        private readonly WrapperValueObjectJsonConverter _generateJsonConverter;

        public WrapperValueObjectAttribute(WrapperValueObjectJsonConverter generateJsonConverter = WrapperValueObjectJsonConverter.SystemTextJson)
            : this(typeof(Guid), generateJsonConverter)
        {
        }

        public WrapperValueObjectAttribute(Type type, WrapperValueObjectJsonConverter generateJsonConverter = WrapperValueObjectJsonConverter.SystemTextJson)
        {
            _types = new (string Name, Type Type)[] { ("Value", type) };
            _generateJsonConverter = generateJsonConverter;
        }

        public WrapperValueObjectAttribute(string name1, Type type1, WrapperValueObjectJsonConverter generateJsonConverter = WrapperValueObjectJsonConverter.SystemTextJson)
        {
            _types = new (string Name, Type Type)[]
            {
                (name1, type1),
            };
            _generateJsonConverter = generateJsonConverter;
        }

        public WrapperValueObjectAttribute(string name1, Type type1, string name2, Type type2, WrapperValueObjectJsonConverter generateJsonConverter = WrapperValueObjectJsonConverter.SystemTextJson)
        {
            _types = new (string Name, Type Type)[]
            {
                (name1, type1),
                (name2, type2),
            };
            _generateJsonConverter = generateJsonConverter;
        }

        public WrapperValueObjectAttribute(string name1, Type type1, string name2, Type type2, string name3, Type type3, WrapperValueObjectJsonConverter generateJsonConverter = WrapperValueObjectJsonConverter.SystemTextJson)
        {
            _types = new (string Name, Type Type)[]
            {
                (name1, type1),
                (name2, type2),
                (name3, type3),
            };
            _generateJsonConverter = generateJsonConverter;
        }

        public WrapperValueObjectAttribute(string name1, Type type1, string name2, Type type2, string name3, Type type3, string name4, Type type4, WrapperValueObjectJsonConverter generateJsonConverter = WrapperValueObjectJsonConverter.SystemTextJson)
        {
            _types = new (string Name, Type Type)[]
            {
                (name1, type1),
                (name2, type2),
                (name3, type3),
                (name4, type4),
            };
            _generateJsonConverter = generateJsonConverter;
        }
    }
}
