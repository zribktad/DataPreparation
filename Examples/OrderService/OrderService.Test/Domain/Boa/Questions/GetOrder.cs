using Boa.Constrictor.Screenplay;
using DataPreparation.Data.Setup;
using OrderService.BoaTest.ShowCases.Factories;
using OrderService.DTO;
using OrderService.Models;
using OrderService.Test.Domain.Boa.Abilities;
using OrderService.Test.Domain.Factories.AsyncMock;
using OrderService.Test.Domain.Factories.SQLite;
using Shouldly;
using CustomerFactoryAsync = OrderService.Test.Domain.Factories.SQLite.CustomerFactoryAsync;

namespace OrderService.Test.Domain.Boa.Questions;

public class GetOrder : IQuestionAsync<Order>
{
    
    public static GetOrder FromFactory()
    {
         return new GetOrder();
    }

    public Task<Order> RequestAsAsync(IActor actor)
    {
        var ability = actor.Using<UseSourceFactory>();
        return  ability.SFactory.GetAsync<Order, OrderFactoryAsync>();
    }
}