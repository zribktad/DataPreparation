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
    public class DataPreparationTestStores(ILoggerFactory loggerFactory)
    {
        private readonly List<PreparedData> _preparation = new();
        private readonly Stack<PreparedData>  _processed = new();
        private readonly ILogger _logger = loggerFactory.CreateLogger<DataPreparationTestStores>();
        
        public void PushProcessed( PreparedData data)
        {
            _processed.Push(data);
        }
        public bool TryPopProcessed(out PreparedData? data)
        {
            return _processed.TryPop(out data);
        }
        internal  List<PreparedData> GetPreparation()
        {
            return _preparation;
        }
        internal void AddDataPreparation( object preparedMethodData, object[] upData, object[] downData)
        {
            _logger.LogTrace($"Adding data preparation instance with type {preparedMethodData.GetType().Name}.");
            _preparation.Add( new PreparedData(preparedMethodData, upData, downData,loggerFactory));
        }
     
        internal void AddDataPreparation( List<object> preparedDataList)
        {
            _preparation.AddRange( preparedDataList.Select(data =>
            {
                _logger.LogTrace($"Adding data preparation instance with type {data.GetType().Name}.");
                return new PreparedData(data, [], [], loggerFactory);
            }).ToList());
        }
     
     
    }
}