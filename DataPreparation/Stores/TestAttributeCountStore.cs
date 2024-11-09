using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using DataPreparation.Data;
using NUnit.Framework.Interfaces;

namespace DataPreparation.Testing
{
    /// <summary>
    /// Provides a store for counting the use of attributes associated with test cases.
    /// </summary>
    internal static class TestAttributeCountStore
    {
        //Store use data preparation attributes for each test case
        private static readonly Dictionary<MethodInfo, List<ITestAction>?> AttributesForTestStore = new();
        //count the use of attributes data up
        private static readonly Dictionary<MethodInfo, long> UseAttributesTestUpCount = new();
        //count the use of attributes data down
        private static readonly Dictionary<MethodInfo, long> UseAttributesTestDownCount = new();

        
        internal static void AddAttributes(MethodInfo methodInfo)
        {
            AddAttributes<UsePreparedDataAttribute>(methodInfo);
            AddAttributes<UsePreparedDataForAttribute>(methodInfo);
            AddAttributes<UsePreparedDataParamsAttribute>(methodInfo);
        }

        /// <summary>
        /// Adds the attributes of the data preperation use type.
        /// Use for counting the attributes
        /// </summary>
        /// <param name="methodInfo"></param>
        /// <typeparam name="T"></typeparam>
        private static void AddAttributes<T>(MethodInfo methodInfo) where T : Attribute, ITestAction
        {
            var attributes = methodInfo.GetCustomAttributes<T>();
            if (attributes.Any()) // Check if there are any attributes of the specified type
            {
                if (!AttributesForTestStore.ContainsKey(methodInfo))
                {
                    AttributesForTestStore[methodInfo] = new List<ITestAction>();
                }
                AttributesForTestStore[methodInfo].AddRange(attributes);
            }
        }
     
       
        private static List<ITestAction> GetAttributes(MethodInfo methodInfo)
        {
            return AttributesForTestStore.GetValueOrDefault(methodInfo, []);
        }
        private static int GetAttributesCount(MethodInfo methodInfo)
        {
            return GetAttributes(methodInfo).Count;
        }
        
        internal static void AddAttributeCount(MethodInfo methodInfo)
        {
            UseAttributesTestUpCount.TryAdd(methodInfo, 0);
            UseAttributesTestUpCount[methodInfo]++;
        }
        internal static void RemoveAttributeCount(MethodInfo methodInfo)
        {
            UseAttributesTestDownCount.TryAdd(methodInfo, 0);
            UseAttributesTestDownCount[methodInfo]++;
        }
        internal static bool AreAllTestAttributesUp(MethodInfo methodInfo)
        {
            return CheckTestAttributeCount(methodInfo, GetAttributesCount(methodInfo), UseAttributesTestUpCount);
        }

        internal static bool AreAllTestAttributesDown(MethodInfo methodInfo)
        {
            return CheckTestAttributeCount(methodInfo, GetAttributesCount(methodInfo), UseAttributesTestDownCount);
        }

        private static bool CheckTestAttributeCount(MethodInfo methodInfo, long expectedCount , Dictionary<MethodInfo, long> counter)
        {
            if (counter.TryGetValue(methodInfo, out long actualCount))
            {
                return actualCount == expectedCount;
            }

            return expectedCount == 0; // If the type is not found, consider it as having zero attributes.
        }
    }
}
