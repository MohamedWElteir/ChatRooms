using System.Reflection;

namespace ChatRooms.Domain.Tests.Helpers;

internal sealed class TestHelpers
{
    public static bool IsInitOnly(PropertyInfo prop)
    {
        var setter = prop.SetMethod;
        if (setter is null) return false;
        return setter.ReturnParameter
            .GetRequiredCustomModifiers()
            .Contains(typeof(System.Runtime.CompilerServices.IsExternalInit));
    }
}
