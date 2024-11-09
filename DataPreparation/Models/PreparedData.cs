using System.Reflection;
using DataPreparation.Data;

namespace DataPreparation.Models;

public class PreparedData
{
    private readonly object _preparedDataClass;
     private readonly object[] _paramsUpData;
     private readonly object[] _paramsDownData;
     private readonly MethodInfo? _runUpMethod;
     private readonly MethodInfo? _runDownMethod;
    
    public PreparedData(object preparedDataClass, object[] paramsUpData, object[] paramsDownData)
    {
        _preparedDataClass = preparedDataClass;
        _paramsUpData = paramsUpData;
        _paramsDownData = paramsDownData;

        if (_preparedDataClass == null)
        {
            throw new ArgumentNullException("PreparedData cannot be null");
        }
        if( _preparedDataClass is  IDataPreparation)
        {
            return;
        }
        
        var methods = _preparedDataClass.GetType().GetMethods();
        foreach (var method in methods)
        {
            if(method.GetCustomAttribute<UpDataAttribute>() != null)
            {
                CheckParams(method, _paramsUpData);
                _runUpMethod = method;
            }
            else if(method.GetCustomAttribute<DownDataAttribute>() != null)
            {
                CheckParams(method, _paramsDownData);
                _runDownMethod = method;
            }
            
        }
    }

    private void CheckParams(MethodInfo method, object[]? paramsData)
    {
        // Get expected parameter types directly from the method signature
        var expectedTypes = method.GetParameters().Select(p => p.ParameterType).ToArray();

        
        // Check parameter null
        if (paramsData == null)
        {
            if(expectedTypes.Length == 0)
            {
                return;
            }
            throw new ArgumentException(
                $"Incorrect number of parameters for '{method.Name}'. Expected: {expectedTypes.Length}, provided: 0.");
        }

      
        // Check parameter count
        if (expectedTypes.Length != paramsData.Length)
        {
            throw new ArgumentException(
                $"Incorrect number of parameters for '{method.Name}'. Expected: {expectedTypes.Length}, provided: {paramsData.Length}.");
        }

        // Check parameter types
        for (int i = 0; i < paramsData.Length; i++)
        {
            if (paramsData[i].GetType() != expectedTypes[i])
            {
                throw new ArgumentException(
                    $"Parameter at position {i + 1} in '{method.Name}' has incorrect type. " +
                    $"Expected: {expectedTypes[i].Name}, but got: {paramsData[i].GetType().Name}.");
            }
        }
    }



    public async Task RunUp()
    {
        if( _preparedDataClass is IDataPreparation dataPreparation)
        {
            dataPreparation.TestUpData();
            return;
        }
        
        if(_runUpMethod == null)   return; 
        
        var result= _runUpMethod.Invoke(_preparedDataClass, _paramsUpData);
        
        if (result is Task task)
        {
            await task;
        }
        
    }
    public async Task RunDown()
    {
        if( _preparedDataClass is IDataPreparation dataPreparation)
        {
            dataPreparation.TestDownData();
            return;
        }
        if(_runDownMethod == null)   return; 
        var result= _runDownMethod.Invoke(_preparedDataClass, _paramsDownData);
        
        if (result is Task task)
        {
            await task;
        }
    }
}