﻿using Boa.Constrictor.Screenplay;
using OrderService.BoaTest.OrderService.Abilities;
using OrderService.BoaTest.OrderService.Questions;
using OrderService.BoaTest.OrderService.Tasks;
using OrderService.DTO;
using OrderService.Models;
using OrderService.Test.Domain.Boa.Abilities;
using OrderService.Test.Domain.Boa.Questions;
using Shouldly;

namespace OrderService.Test.Domain.BddSteps;

public class OrderServiceStepsMock
{
    
    private IActor _actor;
    private Order _order;
    private Order _createdOrder;
    private OrderDTO _orderDto;
    public void GivenIHaveActor()
    {
        _actor = new Actor("OrderTester", new ConsoleLogger());
    }

    public void GivenActorCanUseSourceFactory()
    {
        _actor.Can(UseOrderService.FromMockFactory());
    }

    public void GivenActorCanUseOrderService()
    {
        _actor.Can( UseSourceFactory.FromDataPreparation());
    }

    public void WhenICreatesOrderdto()
    {
         _orderDto = _actor.AsksFor(GetOrderDto.One());
    }

    public void ThanICreatesOrder()
    {
        var createTask = CreateOrderTask.For(_orderDto); 
        _actor.AttemptsTo(createTask);
        _createdOrder = createTask.CreatedOrder;
    }

    public void WhenILookAtOrder()
    {
        _order = _actor.AsksFor(OrderById.WithId(_createdOrder.Id));
    }

    public void ThenOrderShouldBeCreated()
    {
        _order.ShouldNotBeNull();
        _order.CustomerId.ShouldBe(_orderDto.CustomerId);
        _order.OrderItems.ShouldNotBeNull();
        _order.OrderItems.Count().ShouldBe(_orderDto.OrderItems.Count);
    }
}