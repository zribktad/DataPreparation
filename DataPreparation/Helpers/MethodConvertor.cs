using System.Reflection;
using Mono.Cecil;

namespace DataPreparation.Helpers;

public static  class MethodConvertor
{
    public static MethodInfo? GetInfo(Type? classType, string methodName, TypeReference[] array)
    {
        if (classType == null) return null;
        // var method = classType.GetMethods()
        //     .Where(m => m.Name == methodName)
        //     .FirstOrDefault(m =>
        //         m.GetParameters().Select(p => p.ParameterType).SequenceEqual(parameterTypes) &&
        //         m.ReturnType.FullName == methodDef.ReturnType.FullName);

        return null;
    }
    public static MethodInfo? GetInfo(MethodDefinition? methodDef)
    {
        if (methodDef == null)
            return null;

        var type = Type.GetType($"{methodDef.DeclaringType.FullName}, {methodDef.DeclaringType.Module.Assembly.FullName}");
        if (type == null)
            return null;

        var parameterTypes = methodDef.Parameters.Select(p => p.ParameterType.FullName).ToArray();

        return type.GetMethods()
            .FirstOrDefault(m => m.Name == methodDef.Name &&
                                 m.GetParameters().Select(p => p.ParameterType.FullName).SequenceEqual(parameterTypes));
    }
}