using System;

namespace WrapperValueObject
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false)]
    public class WrapperValueObjectAttribute : Attribute
    {
        private readonly Type _type;

        public WrapperValueObjectAttribute(Type type)
        {
            _type = type;
        }
    }
}
