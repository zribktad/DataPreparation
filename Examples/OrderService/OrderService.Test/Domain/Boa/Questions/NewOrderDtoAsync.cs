using Boa.Constrictor.Screenplay;
using OrderService.BoaTest.ShowCases.Factories;
using OrderService.DTO;
using OrderService.Models;
using OrderService.Test.Domain.Boa.Abilities;
using OrderService.Test.Domain.Factories.AsyncMock;
using Shouldly;
using CustomerFactoryAsync = OrderService.BoaTest.Factories.SQLite.CustomerFactoryAsync;

namespace OrderService.Test.Domain.Boa.Questions;

public class NewOrderDtoAsync : IQuestionAsync<OrderDTO>
{

    public Task<OrderDTO> RequestAsAsync(IActor actor)
    {
        var ability = actor.Using<UseSourceFactory>();
        ability.Factory.ShouldNotBeNull();
      
        return ability.Factory.GetAsync<OrderDTO,OrderDtoFactoryAsync>();
    }

}