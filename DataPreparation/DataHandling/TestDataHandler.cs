using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataPreparation.Data;

namespace DataPreparation.Testing
{
    internal static class  TestDataHandler
    {
        internal static void DataUp(List<IDataPreparation> testData)
        {
            foreach (var data in testData)
            {
                data.TestUpData();
            }
        }

        internal static void DataDown(List<IDataPreparation> testData)
        {
            foreach (var data in testData)
            {
                data.TestDownData();
            }
        }

    }
}
