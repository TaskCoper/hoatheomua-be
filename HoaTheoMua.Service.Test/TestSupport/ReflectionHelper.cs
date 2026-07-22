using System;
using System.Linq;
using System.Reflection;

namespace HoaTheoMua.Service.Test.TestSupport;

// Reflection helpers shared by the entity-seed builders. Several entities expose
// init-only / private-set properties, so tests populate them by name. Consolidated
// here so the same body is not copy-pasted into every seed builder.
public static class ReflectionHelper
{
    // Sets a property by name (case-insensitive), coercing the value to the property's
    // type (enum / Guid / DateTimeOffset / assignable / Convert.ChangeType). No-ops when
    // the property is absent or read-only, so a builder can set a superset of fields.
    public static void SetPropertyIfExists(object obj, string propertyName, object? value)
    {
        var property = obj.GetType().GetProperty(
            propertyName,
            BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

        if (property == null || !property.CanWrite)
        {
            return;
        }

        if (value == null)
        {
            property.SetValue(obj, null);
            return;
        }

        var targetType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;

        if (targetType.IsEnum)
        {
            var enumValue = Enum.Parse(targetType, value.ToString()!, ignoreCase: true);
            property.SetValue(obj, enumValue);
            return;
        }

        if (targetType == typeof(Guid))
        {
            property.SetValue(obj, Guid.Parse(value.ToString()!));
            return;
        }

        if (targetType == typeof(DateTimeOffset))
        {
            if (value is DateTimeOffset dateTimeOffset)
            {
                property.SetValue(obj, dateTimeOffset);
            }
            else
            {
                property.SetValue(obj, DateTimeOffset.Parse(value.ToString()!));
            }

            return;
        }

        if (targetType.IsAssignableFrom(value.GetType()))
        {
            property.SetValue(obj, value);
            return;
        }

        property.SetValue(obj, Convert.ChangeType(value, targetType));
    }

    // Resolves an enum type by simple name across loaded assemblies and parses the value
    // (case-insensitive). Returns the raw string when no such enum exists, so a seed
    // builder can pass an enum value without a compile-time reference to its type.
    public static object? ParseEnumIfExists(string enumName, string value)
    {
        var enumType = AppDomain.CurrentDomain
            .GetAssemblies()
            .SelectMany(x =>
            {
                try
                {
                    return x.GetTypes();
                }
                catch
                {
                    return Array.Empty<Type>();
                }
            })
            .FirstOrDefault(x => x.IsEnum && x.Name == enumName);

        if (enumType == null)
        {
            return value;
        }

        return Enum.Parse(enumType, value, ignoreCase: true);
    }
}
