using System.Reflection;
using NUnit.Framework;

namespace DataPreparation.Helpers;

public class AttributeHelper
{
    public static IList<Attribute> GetAttributes(MethodInfo methodInfo,params Type[] attributeTypes)
    {
        List<Attribute> attributes = new ();
        foreach(var attributeType in attributeTypes)
        {
            attributes.AddRange(methodInfo.GetCustomAttributes(attributeType)); 
        }
        return attributes;
    }
    
}