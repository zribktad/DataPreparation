using Boa.Constrictor.Screenplay;
using DataPreparation.Testing.Factory;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrderService.BoaTest.OrderService.Abilities;
using OrderService.BoaTest.OrderService.Questions;
using OrderService.BoaTest.OrderService.Tasks;
using OrderService.BoaTest.TestFakeModels;
using OrderService.DTO;
using OrderService.Models;
using OrderService.Repository;
using OrderService.Services;
using OrderService.Test.Domain;
using OrderService.Test.Domain.BddSteps;
using OrderService.Test.Domain.Boa.Abilities;
using OrderService.Test.Domain.Boa.Questions;
using Shouldly;
using Steeltoe.Discovery;
using TestStack.BDDfy;

namespace DataPreparation.Testing;

[DataPreparationFixture]
public class OrderServiceBdd : SQLiteFixture
{
    readonly OrderServiceSteps _serviceSteps = new();
    
    [DataPreparationTest]
    public void CreateOrder_FullOrderDTO_ReturnsOrder()
    {
       
        this.Given(_ => _serviceSteps.GivenIHaveActor())
            .And(_ => _serviceSteps.GivenActorCanUseSourceFactory())
            .And(_ => _serviceSteps.GivenActorCanUseOrderService())
            .When(_ => _serviceSteps.WhenICreatesOrderdto())
            .Then(_ => _serviceSteps.ThanICreatesOrder())
            .When(_ => _serviceSteps.WhenILookAtOrder())
            .Then(_ => _serviceSteps.ThenOrderShouldBeCreated())
            .BDDfy();
    }
    [DataPreparationTest]
    public void CancelOrder_ValidOrder_ChangesOrderStatus()
    {
        this.Given(_ => _serviceSteps.GivenIHaveActor())
            .And(_ => _serviceSteps.GivenActorCanUseSourceFactory())
            .And(_ => _serviceSteps.GivenActorCanUseOrderService())
            .And(_ => _serviceSteps.GivenActorCanUseOrderStatusService())
            .When(_ => _serviceSteps.WhenICreatesOrder())
            .Then(_ => _serviceSteps.ThenICancelCreatedOrder())
            .When(_ => _serviceSteps.WhenILookAtOrder())
            .Then(_ => _serviceSteps.ThenOrderShouldBeCanceled())
            .BDDfy();
    }

    [DataPreparationTest]
    [UsePreparedDataFor(typeof(UpdateOrderStatusTask))]
    public void CompleteOrderWorkflow_FromCreateToShipped()
    {
        
        this.Given(_ => _serviceSteps.GivenIHaveActor())
            .And(_ => _serviceSteps.GivenActorCanUseSourceFactory())
            .And(_ => _serviceSteps.GivenActorCanUseOrderService())
            .And(_ => _serviceSteps.GivenActorCanUseOrderStatusService())
            .And(_ => _serviceSteps.GivenActorUseOrderManagementService())
            .When(_ => _serviceSteps.WhenICreatesOrder())
            .Then(_ => _serviceSteps.ThenIChangeStatusOfOrderTo(Status.PROCESSING))
            .Then(_ => _serviceSteps.ThenIChangeStatusOfOrderTo(Status.SENT))
            .Then(_ => _serviceSteps.ThenIChangeStatusOfOrderTo(Status.DELIVERING))
            .Then(_ => _serviceSteps.ThenIChangeStatusOfOrderTo(Status.DELIVERED))
            .When(_ => _serviceSteps.WhenILookAtOrder())
            .Then(_ => _serviceSteps.ThenOrderStatusShouldBe(Status.DELIVERED))
            .BDDfy();
    }






}