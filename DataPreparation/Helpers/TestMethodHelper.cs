using System.Diagnostics;
using System.Reflection;
using DataPreparation.Testing;
using DataPreparation.Testing.Factory;
using NUnit.Framework;

namespace DataPreparation.Helpers;

internal static class TestMethodHelper
{
    internal static MethodBase GetLatestTestMethod()
    {
        StackTrace stackTrace = new StackTrace();
        foreach (var stackFrame in stackTrace.GetFrames())
        {
            MethodBase? method = stackFrame.GetMethod();
            if (method?.GetCustomAttribute<TestAttribute>() != null)
            {
                if(method.DeclaringType?.GetCustomAttribute<DataPreparationFixtureAttribute>() == null)
                {
                    throw new InvalidOperationException($"This method should be called from a test method in Fixture with [{nameof(DataPreparationFixtureAttribute)}].");
                }
                return method;
            }
        }
        throw new InvalidOperationException("This method should be called from a test method.");
    }
}