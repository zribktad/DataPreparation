using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using DataPreparation.Data;

namespace DataPreparation.Testing
{
    internal static class TestAttributeStore
    {

        private static readonly Dictionary<Type, List<ITestAction>?> UseAttributesForClass = new();
        private static readonly Dictionary<Type, List<IDataPreparation>> UseAttributesForClass = new();
        private static readonly Dictionary<Type, long> UseAttributesCount = new();
    
        internal static void AddAttributes<T>(Type type) where T : Attribute, ITestAction
        {
            var attributes = type.GetCustomAttributes<T>();
            if (attributes.Any()) // Check if there are any attributes of the specified type
            {
                if (!UseAttributesForClass.ContainsKey(type))
                {
                    UseAttributesForClass[type] = new List<ITestAction>();
                }
                UseAttributesForClass[type].AddRange(attributes);
            }
        }
        internal static List<ITestAction>? GetAttributes(Type type)
        {
            return UseAttributesForClass.GetValueOrDefault(type);

        }
        internal static void AddAttributeCount(Type type)
        {
            UseAttributesCount.TryAdd(type, 0);
            UseAttributesCount[type]++;
        }
        internal static void RemoveAttributeCount(Type type)
        {
            if (UseAttributesCount.ContainsKey(type))
            {
                UseAttributesCount[type]--;
            }
        }
        internal static void CheckIfAllDataReady(Type type)
        {
            UseAttributesCount.TryAdd(type, 0);
            UseAttributesCount[type]++;
        }
        internal static void RemoveAttributeCount(Type type)
        {
            if (UseAttributesCount.ContainsKey(type))
            {
                UseAttributesCount[type]--;
            }
        }
    }
}
