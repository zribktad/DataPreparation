using DataPreparation.Testing;
using DataPreparation.Testing.Factory;
using OrderService.BoaTest.OrderService.Tasks;
using OrderService.Test.Domain;
using OrderService.Test.Domain.BddSteps;
using TestStack.BDDfy;

namespace OrderService.Test.BDDTest.SQLite;

[DataPreparationFixture]
[Story(
    AsA = "data preparer",
    IWant = "to create an order",
    SoThat = "I can verify that an order is created correctly")]
public class OrderCreationBddTest : SqLiteDataPreparationFixture
{
    private readonly OrderServiceSteps _steps = new();

    [DataPreparationTest]
    [UsePreparedDataFor(typeof(UpdateOrderStatusTask))]
    public void CreateOrder_FullOrderDTO_ReturnsOrder()
    {
        this.Given(_ => _steps.GivenIHaveActor())
            .And(_ => _steps.GivenActorCanUseSourceFactory())
            .And(_ => _steps.GivenActorCanUseOrderService())
            .When(_ => _steps.WhenICreatesOrderdto())
            .Then(_ => _steps.ThanICreatesOrder())
            .When(_ => _steps.WhenILookAtOrder())
            .Then(_ => _steps.ThenOrderShouldBeCreated())
            .BDDfy();
    }
}