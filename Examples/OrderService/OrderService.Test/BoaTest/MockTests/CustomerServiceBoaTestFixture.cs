using System;
using System.Collections.Generic;
using Boa.Constrictor.Screenplay;
using Shouldly;
using Moq;
using NUnit.Framework;
using OrderService.BoaTest.CustomerService.Abilities;
using OrderService.BoaTest.CustomerService.Questions;
using OrderService.BoaTest.CustomerService.Tasks;
using OrderService.DTO;
using OrderService.Models;
using OrderService.Repository;
using OrderService.Services;

namespace OrderService.BoaTest.CustomerService;

[TestFixture]
public class CustomerServiceBoaTestFixture
{
    private IActor _actor;
    private Services.CustomerService _customerService;
    private Mock<IRepository<Customer>> _mockRepository;

    [SetUp]
    public void SetUp()
    {
        _mockRepository = new Mock<IRepository<Customer>>();
        _customerService = new Services.CustomerService(_mockRepository.Object);
        _actor = new Actor("<NAME>");
        _actor.Can(UseCustomerService.With(_customerService));
    }

    [Test]
    public void GetAllCustomers_Returns_All_Customers()
    {
        var customers = new List<Customer> { new() { Id = 1 }, new() { Id = 2 } };
        _mockRepository.Setup(r => r.GetAll(It.IsAny<Func<IQueryable<Customer>, IQueryable<Customer>>>()))
            .Returns(customers);

        var result = _actor.AsksFor(new AllCustomers());

        result.ShouldNotBeNull();
        result.Count().ShouldBe(2);
    }

    [Test]
    public void GetCustomerById_Returns()
    {
        var customerId = 1;
        var customer = new Customer { Id = customerId };
        _mockRepository.Setup(r => r.GetById(customerId, It.IsAny<Func<IQueryable<Customer>, IQueryable<Customer>>>()))
            .Returns(customer);

        var result = _actor.AsksFor(new CustomerById(customerId));

        result.ShouldNotBeNull();
        result.ShouldBeOfType<Customer>();
        result.Id.ShouldBe(customerId);
    }

    [Test]
    public void GetCustomerById_Throws_WhenNotFound()
    {
        _mockRepository.Setup(r => r.GetById(0, It.IsAny<Func<IQueryable<Customer>, IQueryable<Customer>>>()))
            .Returns((Customer)null);

        Should.Throw<InvalidOperationException>(() => _actor.AsksFor(new CustomerById(0)));
    }

    [Test]
    public void CreateCustomer_Returns_New_Customer()
    {
        // Arrange
        var customerId = 1;
        var customerDto = new CustomerDTO { Name = "John Doe", Email = "john.doe@example.com" };
        _mockRepository.Setup(r => r.Insert(It.IsAny<Customer>()))
            .Callback<Customer>(c =>
            {
                c.Id = customerId;
                c.Name = customerDto.Name;
                c.Email = customerDto.Email;
            });
        _mockRepository.Setup(r => r.GetById(customerId, It.IsAny<Func<IQueryable<Customer>, IQueryable<Customer>>>()))
            .Returns(new Customer { Id = customerId, Name = customerDto.Name, Email = customerDto.Email });

        // Act
        _actor.AttemptsTo(CreateCustomerTask.For(customerDto));

        // Assert
        var createdCustomer = _actor.AsksFor(new CustomerById(1));
        createdCustomer.ShouldNotBeNull();
        createdCustomer.ShouldBeOfType<Customer>();
        createdCustomer.Id.ShouldBe(1);
        createdCustomer.Name.ShouldBe(customerDto.Name);
        createdCustomer.Email.ShouldBe(customerDto.Email);
    }

    [Test]
    public void UpdateCustomer_Returns_Updated_Customer()
    {
        // Arrange
        long customerId = 1;
        var originalCustomer = new Customer { Id = customerId, Name = "Original Name", Email = "original@example.com" };
        var updatedCustomer = new Customer { Name = "Updated Name", Email = "updated@example.com" };

        _mockRepository.Setup(r => r.GetById(customerId, It.IsAny<Func<IQueryable<Customer>, IQueryable<Customer>>>()))
            .Returns(originalCustomer);

        _mockRepository.Setup(r => r.Update(It.IsAny<Customer>()))
            .Callback<Customer>(c =>
            {
                originalCustomer.Name = c.Name;
                originalCustomer.Email = c.Email;
            });

        // Act
        _actor.AttemptsTo(UpdateCustomerTask.For(customerId, updatedCustomer));

        // Assert
        var retrievedCustomer = _actor.AsksFor(new CustomerById(customerId));
        retrievedCustomer.ShouldNotBeNull();
        retrievedCustomer.Name.ShouldBe(updatedCustomer.Name);
        retrievedCustomer.Email.ShouldBe(updatedCustomer.Email);
    }
}