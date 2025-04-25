using System.ComponentModel.DataAnnotations;
using Boa.Constrictor.Screenplay;
using DataPreparation.Factory.Testing;
using DataPreparation.Provider;
using DataPreparation.Testing;
using DataPreparation.Testing.Factory;
using Microsoft.Extensions.DependencyInjection;
using OrderService.BoaTest.Boa.Abilities;
using OrderService.BoaTest.CustomerService.Abilities;
using OrderService.BoaTest.CustomerService.Questions;
using OrderService.BoaTest.CustomerService.Tasks;
using OrderService.BoaTest.Factories.SQLite;
using OrderService.BoaTest.OrderService.Abilities;
using OrderService.BoaTest.OrderService.Questions;
using OrderService.BoaTest.OrderService.Tasks;
using OrderService.BoaTest.OrderStatusService.Abilities;
using OrderService.BoaTest.PreparedData;
using OrderService.DTO;
using OrderService.Models;
using OrderService.Services;
using OrderService.Test.Domain;
using OrderService.Test.Domain.Boa.Abilities;
using OrderService.Test.Domain.Boa.Questions;
using OrderService.Test.Domain.Factories.SQLite;
using Shouldly;

namespace OrderService.BoaTest;

[DataPreparationFixture]
public class SqLiteDataPreparationOrderServiceBoaTests : SqLiteDataPreparationFixture, IBeforeTest
{
    public void BeforeTest(IServiceProvider testProvider)
    {
        // code to run before each test
    }


    [DataPreparationTest]
    public async Task CreateOrder_FullOrderDTO_ReturnsOrder()
    {
        var actor = new Actor("OrderTester", new ConsoleLogger());
        actor.Can(UseOrderService.With(PreparationContext.GetProvider().GetRequiredService<IOrderService>()));


        var factory = PreparationContext.GetFactory();

        #region Other Variant

        var orderItems = await factory.GetAsync<OrderItem, OrderItemFactoryAsync>(2);
        var customer = await factory.GetAsync<Customer, CustomerFactoryAsync>();
        var orderDto = new OrderDTO { CustomerId = customer.Id, OrderItems = orderItems };

        #endregion
        

        orderDto = await factory.GetAsync<OrderDTO, OrderDtoFactoryAsync>();
        //FactoryObjects the order service

        // Act
        var createTask = CreateOrderTask.For(orderDto);
        actor.AttemptsTo(createTask);
        factory.Register<Order, OrderRegisterAsync>(createTask.CreatedOrder, out _); //this is for data dependency

        createTask.CreatedOrder.ShouldNotBeNull();
        ;
        var result = actor.AsksFor(new OrderById(createTask.CreatedOrder.Id));
        // Assert

        result.ShouldNotBeNull();
        result.CustomerId.ShouldBe(orderDto.CustomerId);
        result.OrderItems.ShouldNotBeNull();
        result.OrderItems.Count().ShouldBe(orderDto.OrderItems.Count());
    }

    [DataPreparationTest]
    public async Task CreateOrder_FullOrderDTO_ReturnsOrder_UseBOA()
    {
        var actor = new Actor("OrderTester", new ConsoleLogger());
        actor.Can(UseSourceFactory.FromDataPreparation());
        actor.Can(UseOrderService.FromDataPreparationProvider());

        // Act
        var orderDto = await actor.AsksForAsync(NewOrderDtoAsync.WithNoArgs());
        var createTask = CreateOrderAndRegisterTask.For(orderDto);
        actor.AttemptsTo(createTask);

        // Assert
        createTask.CreatedOrder.ShouldNotBeNull();
        ;
        var result = actor.AsksFor(OrderById.WithId(createTask.CreatedOrder.Id));

        result.ShouldNotBeNull();
        result.CustomerId.ShouldBe(orderDto.CustomerId);
        result.OrderItems.ShouldNotBeNull();
        result.OrderItems.Count().ShouldBe(orderDto.OrderItems.Count());
    }


    [DataPreparationTest]
    public async Task CreateCustomer_CreateOrder_Factory()
    {
        // *********** Create Customer ***********
        // Arrange customer
        var factory = PreparationContext.GetFactory();
        var customerDto = factory.Get<CustomerDTO, CustomerDtoFactory>();

        var actor = new Actor("OrderTester", new ConsoleLogger());
        actor.Can(UseOrderService.With(PreparationContext.GetProvider().GetRequiredService<IOrderService>()));
        actor.Can(UseCustomerService.With(PreparationContext.GetProvider().GetRequiredService<ICustomerService>()));

        // Act
        var createCustomerTask = CreateCustomerTask.For(customerDto);
        actor.AttemptsTo(createCustomerTask);
        PreparationContext.GetFactory()
            .Register<Customer,
                CustomerFactoryAsync>(createCustomerTask
                .CreatedCustomer); //this is for data dependency using CustomerFactoryAsync


        // Assert
        var createdCustomer = actor.AsksFor(new CustomerById(createCustomerTask.CreatedCustomer.Id));
        createdCustomer.ShouldNotBeNull();
        createdCustomer.ShouldBeOfType<Customer>();
        createdCustomer.Name.ShouldBe(customerDto.Name);
        createdCustomer.Email.ShouldBe(customerDto.Email);

        // *********** Create Order with created customer ***********
        //Aranage Order
        var orderDto =
            await PreparationContext.GetFactory()
                .NewAsync<OrderDTO,
                    OrderDtoFactoryAsync>(); //(ListParams.Use(createdCustomer)); //add customer to order
        var createTask = CreateOrderTask.For(orderDto);
        actor.AttemptsTo(createTask);
        factory.Register<Order, OrderRegisterAsync>(createTask.CreatedOrder,
            out _); //this is for data dependency using new register

        createTask.CreatedOrder.ShouldNotBeNull();
        var result = actor.AsksFor(new OrderById(createTask.CreatedOrder.Id));
        // Assert
        result.ShouldNotBeNull();
        result.CustomerId.ShouldBe(orderDto.CustomerId);
        result.OrderItems.ShouldNotBeNull();
        result.OrderItems.Count().ShouldBe(orderDto.OrderItems.Count());
    }

    [DataPreparationTest]
    public async Task CancelOrder_ValidOrder_ChangesOrderStatus()
    {
        var actor = new Actor("OrderTester", new ConsoleLogger());
        actor.Can(UseSourceFactory.FromDataPreparation());
        actor.Can(UseOrderService.FromDataPreparationProvider());
        actor.Can(UseOrderStatusService.FromDataPreparationProvider());

        var orderDto = await actor.AsksForAsync(NewOrderDtoAsync.WithNoArgs());
        var createTask = CreateOrderAndRegisterTask.For(orderDto);
        actor.AttemptsTo(createTask);

        actor.AttemptsTo(CancelOrderTask.For(createTask.CreatedOrder.Id));

        var canceledOrder = actor.AsksFor(new OrderById(createTask.CreatedOrder.Id));

        canceledOrder.ShouldNotBeNull();
        canceledOrder.OrderStatuses.ShouldContain(item => item.Status == Status.CANCELED);
        canceledOrder.OrderStatuses.ShouldContain(item => item.Status == Status.CANCELED);
    }

    [DataPreparationTest]
    public async Task GetOrdersByCustomer_MultipleOrders_ReturnsOrdersForCustomer()
    {
        var factory = PreparationContext.GetFactory();
        var customer = await factory.GetAsync<Customer, CustomerFactoryAsync>();

        var actor = new Actor("OrderTester", new ConsoleLogger());
        actor.Can(UseOrderService.With(PreparationContext.GetProvider().GetRequiredService<IOrderService>()));

        var orderCount = 3;
        for (var i = 0; i < orderCount; i++)
        {
            var orderDto = await factory.NewAsync<OrderDTO, OrderDtoFactoryAsync>(ListParams.Use(customer));
            var createTask = CreateOrderTask.For(orderDto);
            actor.AttemptsTo(createTask);
            factory.Register<Order, OrderRegisterAsync>(createTask.CreatedOrder, out _);
        }

        var customerOrders = actor.AsksFor(OrdersByCustomer.WithId(customer.Id));

        // Assert
        customerOrders.ShouldNotBeNull();
        customerOrders.Count().ShouldBe(orderCount);
        customerOrders.ShouldAllBe(o => o.CustomerId == customer.Id);
    }

    [DataPreparationTest]
    public async Task CreateOrder_EmptyItems_ThrowsValidationException()
    {
        var factory = PreparationContext.GetFactory();
        var customer = await factory.GetAsync<Customer, CustomerFactoryAsync>();

        var orderDto = new OrderDTO { CustomerId = customer.Id, OrderItems = new List<OrderItem>() };

        var actor = new Actor("OrderTester", new ConsoleLogger());
        actor.Can(UseOrderService.With(PreparationContext.GetProvider().GetRequiredService<IOrderService>()));

        Should.Throw<ValidationException>(() => actor.AttemptsTo(CreateOrderTask.For(orderDto)));
    }


    [DataPreparationTest]
    public async Task UpdateOrderStatus_ValidStatus_ChangesStatus()
    {
        var factory = PreparationContext.GetFactory();
        var orderDto = await factory.GetAsync<OrderDTO, OrderDtoFactoryAsync>();

        var actor = new Actor("OrderTester", new ConsoleLogger());
        actor.Can(UseOrderService.With(PreparationContext.GetProvider().GetRequiredService<IOrderService>()));
        actor.Can(
            UseOrderStatusService.With(PreparationContext.GetProvider().GetRequiredService<IOrderStatusService>()));

        var createTask = CreateOrderTask.For(orderDto);
        actor.AttemptsTo(createTask);
        factory.Register<Order, OrderRegisterAsync>(createTask.CreatedOrder, out _);

        var updateStatusTask = UpdateOrderStatusTask.For(createTask.CreatedOrder.Id, Status.PROCESSING);
        actor.AttemptsTo(updateStatusTask);

        var updatedOrder = actor.AsksFor(new OrderById(createTask.CreatedOrder.Id));

        // Assert
        updatedOrder.ShouldNotBeNull();
        updatedOrder.OrderStatuses.LastOrDefault().Status.ShouldBe(Status.PROCESSING);
    }

    [DataPreparationTest]
    [UsePreparedDataFor(typeof(UpdateOrderStatusTask))]  //alternative  [UsePreparedData(typeof(UpdateOrderStatusTaskData))]
    public async Task CompleteOrderWorkflow_FromCreateToShipped()
    {
        var actor = new Actor("OrderProcessor", new ConsoleLogger());
        actor.Can(UseSourceFactory.FromDataPreparation());
        actor.Can(UseOrderService.FromDataPreparationProvider());
        actor.Can(UseOrderStatusService.FromDataPreparationProvider());
        actor.Can(UseOrderManagementService.FromDataPreparationProvider());

        var orderDto = await actor.AsksForAsync(NewOrderDtoAsync.WithNoArgs());
        var createTask = CreateOrderAndRegisterTask.For(orderDto);
        actor.AttemptsTo(createTask);


        var initialOrder = actor.AsksFor(OrderById.WithId(createTask.CreatedOrder.Id));
        initialOrder.OrderStatuses.LastOrDefault()!.Status.ShouldBe(Status.CREATED);

        actor.AttemptsTo(UpdateOrderStatusTask.For(createTask.CreatedOrder.Id, Status.PROCESSING));
        actor.AttemptsTo(UpdateOrderStatusTask.For(createTask.CreatedOrder.Id, Status.SENT));
        actor.AttemptsTo(UpdateOrderStatusTask.For(createTask.CreatedOrder.Id, Status.DELIVERING));
        actor.AttemptsTo(UpdateOrderStatusTask.For(createTask.CreatedOrder.Id, Status.DELIVERED));

        var finalOrder = actor.AsksFor(new OrderById(createTask.CreatedOrder.Id));
        finalOrder.OrderStatuses.LastOrDefault()!.Status.ShouldBe(Status.DELIVERED);
    }
    
    [DataPreparationTest]
    [UsePreparedDataFor(typeof(UpdateOrderStatusTask))]  //alternative  [UsePreparedData(typeof(UpdateOrderStatusTaskData))]
    public async Task CompleteOrderStatusWorkflow_FromCreateToShipped()
    {
        var actor = new Actor("OrderProcessor", new ConsoleLogger());
        actor.Can(UseSourceFactory.FromDataPreparation());
        actor.Can(UseOrderService.FromDataPreparationProvider());
        actor.Can(UseOrderStatusService.FromDataPreparationProvider());
        actor.Can(UseOrderManagementService.FromDataPreparationProvider());
        var order = await actor.AsksForAsync(GetOrder.FromFactory());
        order.OrderStatuses.LastOrDefault()!.Status.ShouldBe(Status.CREATED);
        

        actor.AttemptsTo(UpdateOrderStatusTask.For(order.Id, Status.PROCESSING));
        actor.AttemptsTo(UpdateOrderStatusTask.For(order.Id, Status.SENT));
        actor.AttemptsTo(UpdateOrderStatusTask.For(order.Id, Status.DELIVERING));
        actor.AttemptsTo(UpdateOrderStatusTask.For(order.Id, Status.DELIVERED));

        var deliveredOrder = actor.AsksFor(new OrderById(order.Id));
    
        deliveredOrder.OrderStatuses.LastOrDefault()!.Status.ShouldBe(Status.DELIVERED);
    }

    [DataPreparationTest]
    public async Task GetAllOrders_MultipleOrders_ReturnsAllOrders()
    {
        var actor = new Actor("OrderAdmin", new ConsoleLogger());
        actor.Can(UseOrderService.FromDataPreparationProvider());
        actor.Can(UseSourceFactory.FromDataPreparation());

        var initialOrders = actor.AsksFor(AllOrders.FromService());
        var initialCount = initialOrders.Count();

        var orderCount = 3;
        for (var i = 0; i < orderCount; i++)
        {
            var orderDto = await actor.AsksForAsync(NewOrderDtoAsync.WithNoArgs());
            var createTask = CreateOrderAndRegisterTask.For(orderDto);
            actor.AttemptsTo(createTask);
        }

        var allOrders = actor.AsksFor(AllOrders.FromService()).ToList();

        // Assert
        allOrders.ShouldNotBeNull();
        allOrders.Count().ShouldBe(initialCount + orderCount);
    }

    [DataPreparationTest]
    public async Task GetOrdersByStatus_FilterByStatus_ReturnsFilteredOrders()
    {
        var factory = PreparationContext.GetFactory();

        var actor = new Actor("OrderAnalyst", new ConsoleLogger());
        actor.Can(UseOrderService.With(PreparationContext.GetProvider().GetRequiredService<IOrderService>()));
        actor.Can(
            UseOrderStatusService.With(PreparationContext.GetProvider().GetRequiredService<IOrderStatusService>()));


        var orderDto1 = await factory.GetAsync<OrderDTO, OrderDtoFactoryAsync>();
        var createTask1 = CreateOrderTask.For(orderDto1);
        actor.AttemptsTo(createTask1);
        factory.Register<Order, OrderRegisterAsync>(createTask1.CreatedOrder, out _);

        var orderDto2 = await factory.GetAsync<OrderDTO, OrderDtoFactoryAsync>();
        var createTask2 = CreateOrderTask.For(orderDto2);
        actor.AttemptsTo(createTask2);
        factory.Register<Order, OrderRegisterAsync>(createTask2.CreatedOrder, out _);

        actor.AttemptsTo(UpdateOrderStatusTask.For(createTask2.CreatedOrder.Id, Status.PROCESSING));

        var createdOrders = actor.AsksFor(OrdersByStatus.WithStatus(Status.CREATED));
        var processingOrders = actor.AsksFor(OrdersByStatus.WithStatus(Status.PROCESSING));

        // Assert
        createdOrders.ShouldContain(o => o.Id == createTask1.CreatedOrder.Id);
        processingOrders.ShouldContain(o => o.Id == createTask2.CreatedOrder.Id);
    }
}