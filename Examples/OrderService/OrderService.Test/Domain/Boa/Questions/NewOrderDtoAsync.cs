﻿using Boa.Constrictor.Screenplay;
using DataPreparation.Data.Setup;
using OrderService.BoaTest.ShowCases.Factories;
using OrderService.DTO;
using OrderService.Models;
using OrderService.Test.Domain.Boa.Abilities;
using OrderService.Test.Domain.Factories.AsyncMock;
using Shouldly;
using CustomerFactoryAsync = OrderService.Test.Domain.Factories.SQLite.CustomerFactoryAsync;

namespace OrderService.Test.Domain.Boa.Questions;

public class NewOrderDtoAsync(IDataParams? dataParams) : IQuestionAsync<OrderDTO>
{
    public Task<OrderDTO> RequestAsAsync(IActor actor)
    {
        var ability = actor.Using<UseSourceFactory>();

        return ability.SFactory.NewAsync<OrderDTO, OrderService.Test.Domain.Factories.SQLite.OrderDtoFactoryAsync>(
            dataParams);
    }
    public static NewOrderDtoAsync WithNoArgs()
    {
        return new NewOrderDtoAsync(null);
    }
}