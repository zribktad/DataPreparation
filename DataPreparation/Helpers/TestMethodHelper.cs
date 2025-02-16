using System.Diagnostics;
using System.Reflection;
using NUnit.Framework;

namespace DataPreparation.Helpers;

internal static class TestMethodHelper
{
    public static MethodBase? GetLatestTestMethod()
    {
        StackTrace stackTrace = new StackTrace();
        MethodBase? methodInfo = null;

        foreach (var stackFrame in stackTrace.GetFrames())
        {
            MethodBase? method = stackFrame.GetMethod();
            var testAttribute = method?.GetCustomAttribute<TestAttribute>();

            if (testAttribute != null)
            {
                methodInfo = method;
                break;
            }
        }
        return methodInfo;
    }
}