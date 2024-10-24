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
    /// <summary>
    /// A store for managing data preparation instances associated with specific methods.
    /// </summary>
    internal class TestDataPreparationStore
    {

        private static readonly Dictionary<MethodInfo, List<IDataPreparation>> DataPreparationTestStore = new();
        //maybe TODO check if data are in the store
        /// <summary>
        /// Adds a single data preparation instance to the store for the specified method.
        /// </summary>
        /// <param name="methodInfo">The method information to associate with the data preparation instance.</param>
        /// <param name="data">The data preparation instance to add.</param>
        internal static void AddDataPreparation(MethodInfo methodInfo, IDataPreparation data)
        {
            if (!DataPreparationTestStore.ContainsKey(methodInfo))
            {
                DataPreparationTestStore[methodInfo] = new List<IDataPreparation>();
            }
            DataPreparationTestStore[methodInfo].Add(data);
        }
        /// <summary>
        /// Adds a list of data preparation instances to the store for the specified method.
        /// </summary>
        /// <param name="methodInfo">The method information to associate with the data preparation instances.</param>
        /// <param name="data">The list of data preparation instances to add.</param>
        internal static void AddDataPreparation(MethodInfo methodInfo, List<IDataPreparation> data)
        {
            if (!DataPreparationTestStore.ContainsKey(methodInfo))
            {
                DataPreparationTestStore[methodInfo] = new List<IDataPreparation>();
            }
            DataPreparationTestStore[methodInfo].AddRange(data);
        }

        /// <summary>
        /// Retrieves the list of data preparation instances associated with the specified method.
        /// </summary>
        /// <param name="methodInfo">The method information to look up.</param>
        /// <returns>A list of data preparation instances, or null if none are found.</returns>
        public static List<IDataPreparation>? GetPreparedData(MethodInfo methodInfo)
        {
           return DataPreparationTestStore.GetValueOrDefault(methodInfo);
        }
    }
}
