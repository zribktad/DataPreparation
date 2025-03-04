using DataPreparation.Data.Setup;
using OrderService.DTO;

namespace OrderService.Boa.ShowCases.Factories;

public class DtoFactoryBase: IDataFactoryBase
{

    [DataFactoryCreate(typeof(OrderItemDTO))]
    public OrderItemDTO Create()
    {
        return null;
    }
    [DataFactoryDelete(typeof(OrderItemDTO))]
    public bool Delete()
    {
        throw new NotImplementedException();
    }

    public object Create(long id, IDataParams? args)
    {
        throw new NotImplementedException();
    }

    public bool Delete(long id, object data, IDataParams? args)
    {
        throw new NotImplementedException();
    }
}