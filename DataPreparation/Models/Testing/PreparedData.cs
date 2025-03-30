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
        _logger = logger.CreateLogger<PreparedData>();

        if (_preparedDataInstance == null)
        {
            _logger.LogError("Instance of prepared data is null");
            throw new ArgumentNullException("Instance of prepared data is null");
        }
       
        switch (_preparedDataInstance)
        {
            case IBeforePreparation:
             
                _runUpMethod = typeof(IBeforePreparation).GetMethod(nameof(IBeforePreparation.UpData));
                _runDownMethod = typeof(IBeforePreparation).GetMethod(nameof(IBeforePreparation.DownData));
                break;
            case IBeforePreparationTask:
                _runUpMethod = typeof(IBeforePreparationTask).GetMethod(nameof(IBeforePreparationTask.UpData));
                _runDownMethod = typeof(IBeforePreparationTask).GetMethod(nameof(IBeforePreparationTask.DownData));
               
                break;
            default:
                _logger.LogTrace("Checking of {preparedDataInstance} for UpData and DownData methods and parameters", _preparedDataInstance.GetType().Name);
                var methods = _preparedDataInstance.GetType().GetMethods();
                foreach (var method in methods)
                {
                    if(method.GetCustomAttribute<UpDataAttribute>() != null)
                    {
                        _runUpMethod = method;
                    }
                    else if(method.GetCustomAttribute<DownDataAttribute>() != null)
                    {
                        _runDownMethod = method;
                    }
            
                }
                _logger.LogDebug($"Prepared data of type {preparedDataInstance.GetType()} has been checked for UpData and DownData methods and parameters");
                break;
        }
        
        _paramsUpData = CheckParams(_runUpMethod, paramsUpData,_logger);
        _paramsDownData =CheckParams(_runDownMethod, paramsDownData,_logger);
        
    }
    //Check types and make conversion from string to int, long, etc.
   private static object[] CheckParams(MethodInfo? method, object[]? paramsData, ILogger logger)
    {
        if(method == null) return Array.Empty<object>();
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
    public bool IsRunUpASync() => _runUpMethod?.ReturnType == typeof(Task);
    
    public bool IsRunDownASync() => _runDownMethod?.ReturnType == typeof(Task);
   
    public object? RunUp()
    {
        _logger.LogTrace($"Start UpData for {_preparedDataInstance.GetType().Name}");
        if (_runUpMethod != null)
        {
            _logger.LogTrace($"Running UpData for {_preparedDataInstance.GetType().Name}");
            return _runUpMethod.Invoke(_preparedDataInstance, _paramsUpData);
        }

        _logger.LogWarning($"No UpData method found in {_preparedDataInstance.GetType().Name}");
        return null;
    }

    public object? RunDown()
    {
        _logger.LogTrace($"Start DownData for {_preparedDataInstance.GetType().Name}");
        if (_runDownMethod != null)
        {
            _logger.LogTrace($"Running DownData for {_preparedDataInstance.GetType().Name}");
            return _runDownMethod.Invoke(_preparedDataInstance, _paramsDownData);
        }

        _logger.LogWarning($"No DownData method found in {_preparedDataInstance.GetType().Name}");

        return null;
    }

    public Task RunUpAsync()
    {
        var result = RunUp();
        if (result is Task t) return t;
        return Task.CompletedTask;
    }
    public Task RunDownAsync()
    {
        var result = RunDown();
        if(result is Task t) return t;
        return Task.CompletedTask;

      
    }

  
}