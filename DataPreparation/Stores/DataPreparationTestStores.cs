using System.Collections.Concurrent;
using System.Reflection;
using DataPreparation.Data;
using DataPreparation.Models;

namespace DataPreparation.Testing
{
    /// <summary>
    /// A store for managing data preparation instances associated with specific methods.
    /// </summary>
    public class DataPreparationTestStores
    {
        private readonly List<PreparedData> _dataPreparations = new();
        
    
        internal void AddDataPreparation( PreparedData data)
        {
            _dataPreparations.Add(data);
        }
        
        private void AddDataPreparation(List<PreparedData> data)
        {
            _dataPreparations.AddRange(data);
        }
        
        internal  List<PreparedData> GetAll()
        {
            return _dataPreparations;
        }
        internal  bool HasPreparedData()
        {
            return _dataPreparations.Count > 0;
        }

        internal void AddDataPreparation( object preparedMethodData, object[] upData, object[] downData)
        {
            AddDataPreparation( new PreparedData(preparedMethodData, upData, downData));
        }
     
        internal void AddDataPreparation( List<object> preparedDataList)
        {
            AddDataPreparation( preparedDataList.Select(data => new PreparedData(data,[],[])).ToList());
        }
     
     
    }
}