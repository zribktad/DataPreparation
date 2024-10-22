using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using DataPreparation.Data;
using NUnit.Framework.Internal;

namespace DataPreparation.Testing
{
    internal static class  TestDataHandler
    {
        internal static void DataUp(MethodInfo testMethodInfo)
        {

            TestAttributeStore.AddAttributeCount(testMethodInfo);
            if (!TestAttributeStore.AreAllTestAttributesUp(testMethodInfo)) return;

            var testData = TestDataPreparationStore.GetPreparedData(testMethodInfo);
            if (testData != null)
            {
                Console.WriteLine($"Data for test {testMethodInfo.Name} starting up");
                foreach (var data in testData)
                {
                    data.TestUpData();
                }
                Console.WriteLine($"Data for test {testMethodInfo.Name} are up");
            }

  
        }

        internal static void DataDown(MethodInfo testMethodInfo)
        {

            TestAttributeStore.RemoveAttributeCount(testMethodInfo);
            if(!TestAttributeStore.AreAllTestAttributesDown(testMethodInfo)) return;

            var testData = TestDataPreparationStore.GetPreparedData(testMethodInfo);
            
            if (testData != null)
            {
                Console.WriteLine($"Data for test {testMethodInfo.Name} starting down");
                foreach (var data in testData)
                {
                    data.TestDownData();
                }

                Console.WriteLine($"Data for test {testMethodInfo.Name} are down");
            }

        }

    }
}
