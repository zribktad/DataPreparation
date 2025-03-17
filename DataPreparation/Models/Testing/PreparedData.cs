using System.Reflection;
using DataPreparation.Data;
using Microsoft.Extensions.Logging;

namespace DataPreparation.Models;

public class PreparedData
{
    private readonly object _preparedDataInstance;
     private readonly object[] _paramsUpData;
     private readonly object[] _paramsDownData;
     private readonly MethodInfo? _runUpMethod;
     private readonly MethodInfo? _runDownMethod;
     private readonly ILogger _logger;
    
    public PreparedData(object preparedDataInstance, object[] paramsUpData, object[] paramsDownData, ILoggerFactory logger)
    {
        _preparedDataInstance = preparedDataInstance;
        _paramsUpData = paramsUpData;
        _paramsDownData = paramsDownData;
        _logger = logger.CreateLogger<PreparedData>();

        if (_preparedDataInstance == null)
        {
            _logger.LogError("Instance of prepared data is null");
            throw new ArgumentNullException("Instance of prepared data is null");
        }
       
        switch (_preparedDataInstance)
        {
            case IDataPreparation:
                _runUpMethod = typeof(IDataPreparation).GetMethod(nameof(IDataPreparation.UpData));
                _runDownMethod = typeof(IDataPreparation).GetMethod(nameof(IDataPreparation.DownData));
                break;
            case IDataPreparationTask:
                _runUpMethod = typeof(IDataPreparationTask).GetMethod(nameof(IDataPreparationTask.UpData));
                _runDownMethod = typeof(IDataPreparationTask).GetMethod(nameof(IDataPreparationTask.DownData));
                break;
            default:
                _logger.LogTrace("Checking of {preparedDataInstance} for UpData and DownData methods and parameters", _preparedDataInstance.GetType().Name);
                var methods = _preparedDataInstance.GetType().GetMethods();
                foreach (var method in methods)
                {
                    if(method.GetCustomAttribute<UpDataAttribute>() != null)
                    {
                        CheckParams(method, _paramsUpData,_logger);
                        _runUpMethod = method;
                    }
                    else if(method.GetCustomAttribute<DownDataAttribute>() != null)
                    {
                        CheckParams(method, _paramsDownData,_logger);
                        _runDownMethod = method;
                    }
            
                }
                _logger.LogDebug($"Prepared data of type {preparedDataInstance.GetType()} has been checked for UpData and DownData methods and parameters");
                break;
        }
        

    }
    //Check types and make conversion from string to int, long, etc.
   private static object[] CheckParams(MethodInfo method, object[]? paramsData, ILogger logger)
    {
        var expectedTypes = method.GetParameters().Select(p => p.ParameterType).ToArray();
        var lenParams = paramsData?.Length ?? 0;
        if (expectedTypes.Length < lenParams)
        {
            var e =  new ArgumentException(
                $"Incorrect number of parameters for '{method.Name}'. Expected: {expectedTypes.Length}, provided: {paramsData?.Length}.");
            logger.LogError(e,"Error while checking parameters");
            throw e;
        }
        
        IList<object> newParams = new List<object>();

        for (int i = 0; i < lenParams; i++)
        {
            var expectedType = expectedTypes[i];
            var param = paramsData[i];

            if (expectedType.IsInstanceOfType(param))
            {
                newParams.Add(param);
                continue; // It's either exact match or convertible (e.g., int to long)
            }

            try
            {
                newParams.Add(Convert.ChangeType(param, expectedType));
                // Attempt conversion
            }
            catch
            {
                var e  = new ArgumentException(
                    $"Parameter at position {i + 1} in '{method.Name}' has incorrect type. " +
                    $"Expected: {expectedType.Name}, but got: {param.GetType().Name}.");
                logger.LogError(e,"Error while checking parameters");
                throw e;
            }
        }

        while (expectedTypes.Length > newParams.Count)
        {
            newParams.Add(default);
            logger.LogWarning("Argument {0} in method {1} is missing. Default value use", newParams.Count, method.Name);
        }
        return newParams.ToArray();
    }

    public  Task RunUp()
    {
        _logger.LogTrace($"Start UpData for {_preparedDataInstance.GetType().Name}");
        if (_runUpMethod != null)
        {
            _logger.LogTrace($"Running UpData for {_preparedDataInstance.GetType().Name}");
            var result = _runUpMethod.Invoke(_preparedDataInstance, _paramsUpData);
            if (result is Task t) return t;
        }
        else
        {
            _logger.LogWarning($"No UpData method found in {_preparedDataInstance.GetType().Name}");
        }
        
        return Task.CompletedTask;
    }
    public  Task RunDown()
    {
        _logger.LogTrace($"Start DownData for {_preparedDataInstance.GetType().Name}");
        if (_runDownMethod != null)
        {
            _logger.LogTrace($"Running DownData for {_preparedDataInstance.GetType().Name}");
             var result = _runDownMethod.Invoke(_preparedDataInstance, _paramsDownData);
             if(result is Task t) return t;
        }
        else
        {
            _logger.LogWarning($"No DownData method found in {_preparedDataInstance.GetType().Name}");
        }
        return Task.CompletedTask;

      
    }
}