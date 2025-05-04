using System.Reflection;

using Hjson;


namespace ExternalLocalizer.Hjson;

internal static partial class HjsonCustom
{
    private const string _hjsonValue = "Hjson.HjsonValue";
    private static readonly Func<string, JsonValue> _parse = GetDelegateOfMethod<Func<string, JsonValue>>(_hjsonValue, "Parse", [typeof(string)]);

    private static T GetDelegateOfMethod<T>(string type, string methodName, Type[] types) where T : Delegate
    {
        var methodType = typeof(T);
        var args = methodType.GetGenericArguments();
        var paramTypes = args.Take(args.Length - 1).ToArray();

        return typeof(HjsonValue)
            .Assembly
            .GetType(type)!
            .GetMethod(methodName, BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, types)!
            .CreateDelegate<T>();
    }
}