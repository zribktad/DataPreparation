using DataPreparation.Testing;
using DataPreparation.Testing.Factory;
using OrderService.BoaTest.OrderService.Tasks;
using OrderService.Models;
using OrderService.Test.Domain;
using OrderService.Test.Domain.BddSteps;
using TestStack.BDDfy;

namespace OrderService.Test.BDDTest.SQLite;

[DataPreparationFixture]
[Story(
    AsA = "data preparer",
    IWant = "to retrieve all orders",
    SoThat = "I can verify that multiple orders are processed correctly")]
public class OrderRetrievalBddTest : SqLiteDataPreparationFixture
{
    private readonly OrderServiceSteps _steps = new();

    [DataPreparationTest]
    [UsePreparedDataFor(typeof(UpdateOrderStatusTask))]
    public void GetAllOrders_MultipleOrders_ReturnsAllOrders()
    {
        this.Given(_ => _steps.GivenIHaveActor())
            .And(_ => _steps.GivenActorCanUseSourceFactory())
            .And(_ => _steps.GivenActorCanUseOrderService())
            .And(_ => _steps.GivenActorCanUseOrderStatusService())
            .And(_ => _steps.GivenActorUseOrderManagementService())
            .When(_ => _steps.WhenICreatesOrder())
            .Then(_ => _steps.ThenIChangeStatusOfOrderTo(Status.PROCESSING))
            .Then(_ => _steps.ThenIChangeStatusOfOrderTo(Status.SENT))
            .Then(_ => _steps.ThenIChangeStatusOfOrderTo(Status.DELIVERING))
            .Then(_ => _steps.ThenIChangeStatusOfOrderTo(Status.DELIVERED))
            .When(_ => _steps.WhenILookAtOrder())
            .Then(_ => _steps.ThenOrderStatusShouldBe(Status.DELIVERED))
            .BDDfy();
    }
}