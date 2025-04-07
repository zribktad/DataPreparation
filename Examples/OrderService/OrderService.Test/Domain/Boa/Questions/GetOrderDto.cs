using Boa.Constrictor.Screenplay;
using DataPreparation.Data.Setup;
using OrderService.BoaTest.ShowCases.Factories;
using OrderService.DTO;
using OrderService.Models;
using OrderService.Test.Domain.Boa.Abilities;
using OrderService.Test.Domain.Factories.AsyncMock;
using Shouldly;
using CustomerFactoryAsync = OrderService.BoaTest.Factories.SQLite.CustomerFactoryAsync;

namespace OrderService.Test.Domain.Boa.Questions;

public class GetOrderDto : IQuestion<OrderDTO>
{
    
    public static GetOrderDto One()
    {
         return new GetOrderDto();
    }

    public OrderDTO RequestAs(IActor actor)
    {
        var ability = actor.Using<UseSourceFactory>();
        return  ability.Factory.Get<OrderDTO, OrderDtoFactory>();
    }
}