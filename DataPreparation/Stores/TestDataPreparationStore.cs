using DataPreparation.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DataPreparation.Testing
{
    internal class TestDataPreparationStore
    {

        private static readonly Dictionary<MethodInfo, List<IDataPreparation>> DataPreparationTestStore = new();
        //maybe TODO check if data are in the store

        internal static void AddDataPreparation(MethodInfo methodInfo, IDataPreparation data)
        {
            if (!DataPreparationTestStore.ContainsKey(methodInfo))
            {
                DataPreparationTestStore[methodInfo] = new List<IDataPreparation>();
            }
            DataPreparationTestStore[methodInfo].Add(data);
        }

        internal static void AddDataPreparation(MethodInfo methodInfo, List<IDataPreparation> data)
        {
            if (!DataPreparationTestStore.ContainsKey(methodInfo))
            {
                DataPreparationTestStore[methodInfo] = new List<IDataPreparation>();
            }
            DataPreparationTestStore[methodInfo].AddRange(data);
        }

        public static List<IDataPreparation>? GetPreparedData(MethodInfo methodInfo)
        {
           return DataPreparationTestStore.GetValueOrDefault(methodInfo);
        }
    }
}
