using System.Reflection;
using DataPreparation.Data;
using DataPreparation.Models;

namespace DataPreparation.Testing
{
    /// <summary>
    /// A store for managing data preparation instances associated with specific methods.
    /// </summary>
    internal static class TestDataPreparationStore
    {
        private static readonly Dictionary<MethodInfo, List<PreparedData>> DataPreparationTestStore = new();

        //maybe TODO check if data are in the store
        /// <summary>
        /// Adds a single data preparation instance to the store for the specified method.
        /// </summary>
        /// <param name="methodInfo">The method information to associate with the data preparation instance.</param>
        /// <param name="data">The data preparation instance to add.</param>
        internal static void AddDataPreparation(MethodInfo methodInfo, PreparedData data)
        {
            if (!DataPreparationTestStore.TryGetValue(methodInfo, out var preparations))
            {
                preparations = new List<PreparedData>();
                DataPreparationTestStore[methodInfo] = preparations;
            }

            preparations.Add(data);
        }

        /// <summary>
        /// Adds a list of data preparation instances to the store for the specified method.
        /// </summary>
        /// <param name="methodInfo">The method information to associate with the data preparation instances.</param>
        /// <param name="data">The list of data preparation instances to add.</param>
        private static void AddDataPreparation(MethodInfo methodInfo, List<PreparedData> data)
        {
            if (!DataPreparationTestStore.TryGetValue(methodInfo, out var preparations))
            {
                preparations = new List<PreparedData>();
                DataPreparationTestStore[methodInfo] = preparations;
            }

            preparations.AddRange(data);
        }


        /// <summary>
        /// Retrieves the list of data preparation instances associated with the specified method.
        /// </summary>
        /// <param name="methodInfo">The method information to look up.</param>
        /// <returns>A list of data preparation instances, or null if none are found.</returns>
        internal static List<PreparedData>? GetPreparedData(MethodInfo methodInfo)
        {
            return DataPreparationTestStore.GetValueOrDefault(methodInfo);
        }
        
        internal static bool HasPreparedData(MethodInfo methodInfo)
        {
            return DataPreparationTestStore.GetValueOrDefault(methodInfo) != null;
        }

        internal static void AddDataPreparation(MethodInfo methodMethodInfo, object preparedMethodData, object[] upData, object[] downData)
        {
            AddDataPreparation(methodMethodInfo, new PreparedData(preparedMethodData, upData, downData));
        }
     
        internal static void AddDataPreparation(MethodInfo methodMethodInfo, List<object> preparedDataList)
        {
            AddDataPreparation(methodMethodInfo, preparedDataList.Select(data => new PreparedData(data,[],[])).ToList());
        }
     
     
    }
}