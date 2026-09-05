using System.Reflection;
using MassTransit;

namespace HoaTheoMua.Service.DependencyInjection.Extensions;

internal static class NameFormatterExtensions
{
    public static string ToKebabCaseString(this MemberInfo member)
        => KebabCaseEndpointNameFormatter.Instance.SanitizeName(member.Name);
}

/// <summary>
/// Names RabbitMQ exchanges in kebab-case. SetKebabCaseEndpointNameFormatter only covers queue
/// names; without this, exchanges keep their namespace-qualified CLR names
/// ("HoaTheoMua.Service.Fulfillment:OrderPaid").
/// </summary>
internal class KebabCaseEntityNameFormatter : IEntityNameFormatter
{
    public string FormatEntityName<T>() => Format(typeof(T));

    // Generic types have to be unwrapped explicitly. Type.Name for Fault<DispatchDelivery> is the
    // literal "Fault`1", so naively sanitizing it collapses Fault<DispatchDelivery>,
    // Fault<CreatePosOrder>, Fault<RevalidateProductCache> and Fault<CancelDelivery> onto ONE
    // exchange - every fault would then fan out to consumers bound for a different fault type.
    private static string Format(Type type)
    {
        if (!type.IsGenericType)
        {
            return type.ToKebabCaseString();
        }

        var tickIndex = type.Name.IndexOf('`');
        var baseName = tickIndex < 0 ? type.Name : type.Name[..tickIndex];
        var arguments = string.Join("--", type.GetGenericArguments().Select(Format));

        return $"{KebabCaseEndpointNameFormatter.Instance.SanitizeName(baseName)}--{arguments}--";
    }
}
