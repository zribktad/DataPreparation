using System.Collections.Concurrent;
using System.Reflection;
using DataPreparation.Data;
using DataPreparation.Models;
using Microsoft.Extensions.Logging;

namespace DataPreparation.Testing
{
    /// <summary>
    /// A store for managing data preparation instances associated with specific methods.
    /// </summary>
    internal class DataPreparationTestStores(ILoggerFactory loggerFactory)
    {
        private readonly List<PreparedData> _preparation = new();
        private readonly Stack<PreparedData>  _processed = new();
        private readonly ILogger _logger = loggerFactory.CreateLogger<DataPreparationTestStores>();
        
        internal void PushProcessed( PreparedData data)
        {
            _processed.Push(data);
        }
        
        internal bool TryPopProcessed(out PreparedData? data)
        {
            return _processed.TryPop(out data);
        }
        
        internal List<PreparedData> GetPreparation()
        {
            return _preparation;
        }
        
        internal void AddDataPreparation( object preparedMethodData, object[] upData, object[] downData)
        {
            _logger.LogTrace($"Adding data preparation instance with type {preparedMethodData.GetType().Name}.");
            _preparation.Add( new PreparedData(preparedMethodData, upData, downData,loggerFactory));
        }
     
        internal void AddDataPreparation( List<object?> preparedDataList)
        {
            _preparation.AddRange( preparedDataList.Where(data => data != null).Select(data =>
            {
                _logger.LogTrace($"Adding data preparation instance with type {data.GetType().Name}.");
                return new PreparedData(data, [], [], loggerFactory);
            }).ToList());
        }

        internal bool IsEmpty() => _processed.Count == 0 && _preparation.Count == 0;

        internal void AddDataPreparationList(List<object?> preparedData, object[]?[] upData, object[]?[] downData)
        {
            for (int i = 0; i < preparedData.Count; i++)
            {
                var data = preparedData[i];
                if (data == null) continue;
                var up = upData[i];
                var down = downData[i];
              
                _logger.LogTrace($"Adding data preparation instance with type {data.GetType().Name}.");
                _preparation.Add(new PreparedData(data, up, down, loggerFactory));
            }
         
        }
    }
}