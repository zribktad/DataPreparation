using DataPreparation.Testing;
using DataPreparation.Testing.Factory;
using NUnit.Framework;
using OrderService.Test.Domain;
using OrderService.Test.Domain.BddSteps;
using TestStack.BDDfy;

namespace OrderService.Test.BDDTest.SQLite;

[DataPreparationFixture]
[Story(
    AsA = "data preparer",
    IWant = "to cancel an order",
    SoThat = "I can verify that the order status is updated correctly")]
public class OrderCancellationBddTest : SqLiteDataPreparationFixture
{
    readonly OrderServiceSteps _steps = new();

    [DataPreparationTest]
    public void CancelOrder_ValidOrder_ChangesOrderStatus()
    {
        this.Given(_ => _steps.GivenIHaveActor())
            .And(_ => _steps.GivenActorCanUseSourceFactory())
            .And(_ => _steps.GivenActorCanUseOrderService())
            .And(_ => _steps.GivenActorCanUseOrderStatusService())
            .When(_ => _steps.WhenICreatesOrder())
            .Then(_ => _steps.ThenICancelCreatedOrder())
            .When(_ => _steps.WhenILookAtOrder())
            .Then(_ => _steps.ThenOrderShouldBeCanceled())
            .BDDfy();
    }
}