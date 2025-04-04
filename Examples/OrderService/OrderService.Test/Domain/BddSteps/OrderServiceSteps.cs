using Boa.Constrictor.Screenplay;
using OrderService.BoaTest.Boa.Abilities;
using OrderService.BoaTest.OrderService.Abilities;
using OrderService.BoaTest.OrderService.Questions;
using OrderService.BoaTest.OrderService.Tasks;
using OrderService.BoaTest.OrderStatusService.Abilities;
using OrderService.DTO;
using OrderService.Models;
using OrderService.Test.Domain.Boa.Abilities;
using OrderService.Test.Domain.Boa.Questions;
using Shouldly;

namespace OrderService.Test.Domain.BddSteps;

public class OrderServiceSteps
{
    private IActor _actor;
    private Order _order;
    private Order _createdOrder;
    private OrderDTO _orderDto;

    #region Given Steps

    public void GivenIHaveActor()
    {
        _actor = new Actor("OrderTester", new ConsoleLogger());
    }

    public void GivenActorCanUseSourceFactory()
    {
        _actor.Can(UseSourceFactory.FromDataPreparation());
    }

    public void GivenActorCanUseOrderService()
    {
        _actor.Can(UseOrderService.FromDataPreparationProvider());
    }
    public void GivenActorCanUseOrderStatusService()
    {
        _actor.Can(UseOrderStatusService.FromDataPreparationProvider());
    }
    
    public void GivenActorUseOrderManagementService()
    {
        _actor.Can(UseOrderManagementService.FromDataPreparationProvider());
    }
    #endregion
 
    #region When Steps
    public async Task WhenICreatesOrderdto()
    {
        _orderDto = await _actor.AsksForAsync(NewOrderDtoAsync.WithNoArgs());
    }
    
    public void WhenILookAtOrder()
    {
        _order = _actor.AsksFor(new OrderById(_createdOrder.Id));
    }
    public async Task WhenICreatesOrder()
    {
        await WhenICreatesOrderdto();
        ThanICreatesOrder();
        WhenILookAtOrder();
        ThenOrderShouldBeCreated();

    }
    #endregion
    
    #region Then Steps
    public void ThanICreatesOrder()
    {
        var createTask = CreateOrderAndRegisterTask.For(_orderDto);
        _actor.AttemptsTo(createTask);
        _createdOrder = createTask.CreatedOrder;
        _createdOrder.ShouldNotBeNull();
    }
    
    
    public void ThenOrderShouldBeCreated()
    {
        
        _order.ShouldNotBeNull();
        _order.CustomerId.ShouldBe(_orderDto.CustomerId);
        _order.OrderItems.ShouldNotBeNull();
        _order.OrderItems.Count().ShouldBe(_orderDto.OrderItems.Count());
    }
    
    public void ThenICancelCreatedOrder()
    {
        _actor.AttemptsTo(CancelOrderTask.For(_createdOrder.Id));
    }

    public void ThenOrderShouldBeCanceled()
    {
        _order.ShouldNotBeNull();
        _order.CustomerId.ShouldBe(_orderDto.CustomerId);
        _order.OrderItems.ShouldNotBeNull();
        _order.OrderItems.Count().ShouldBe(_orderDto.OrderItems.Count());
    }
    #endregion


    public void ThenIChangeStatusOfOrderTo(Status status)
    {
        _actor.AttemptsTo(UpdateOrderStatusTask.For(_order.Id, status));
    }

    public void ThenOrderStatusShouldBe(Status status)
    {
        _order.OrderStatuses.LastOrDefault()!.Status.ShouldBe(status);
    }
}