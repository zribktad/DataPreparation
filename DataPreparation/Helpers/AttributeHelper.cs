using System.Reflection;
using NUnit.Framework;

namespace DataPreparation.Helpers;

internal static class AttributeHelper
{
    internal static IList<Attribute> GetAttributes(MethodInfo methodInfo,params Type[] attributeTypes)
    {
        List<Attribute> attributes = new ();
        foreach(var attributeType in attributeTypes)
        {
            attributes.AddRange(methodInfo.GetCustomAttributes(attributeType)); 
        }
        return attributes;
    }
    
}