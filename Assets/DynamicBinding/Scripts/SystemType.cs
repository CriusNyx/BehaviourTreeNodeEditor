using System;

namespace DynamicBinding
{
    [TypeEnum(typeof(ResolveSystemType), nameof(ResolveSystemType.ResolveType))]
    public enum SystemType
    {
        Int,
        String,
        Float,
    }

    public static class ResolveSystemType
    {
        public static Type ResolveType(Enum value)
        {
            if (value is SystemType systemType)
            {
                switch (systemType)
                {
                    case SystemType.Int:
                        return typeof(int);
                    case SystemType.Float:
                        return typeof(float);
                    case SystemType.String:
                        return typeof(string);
                    default:
                        return null;
                }
            }
            return null;
        }
    }
}