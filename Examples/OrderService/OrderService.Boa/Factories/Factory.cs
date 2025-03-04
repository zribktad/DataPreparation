using Microsoft.Extensions.DependencyInjection;

namespace OrderService.Boa.ShowCases.Factories;

public class DataFactoryDeleteAttribute : Attribute
{
    public DataFactoryDeleteAttribute(Type type)
    {
        throw new NotImplementedException();
    }
}

public class DataFactoryCreateAttribute : Attribute
{
    public DataFactoryCreateAttribute(Type orderItemDto)
    {
        throw new NotImplementedException();
    }
}