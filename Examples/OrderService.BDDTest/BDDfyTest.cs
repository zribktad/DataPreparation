using TestStack.BDDfy;
using NUnit.Framework;
using System;

namespace BDDfyExample
{
    public class ShoppingCartTests
    {
        private ShoppingCart _cart;
        private Order _order;

        [SetUp]
        public void Setup()
        {
            _cart = new ShoppingCart();
        }

        // Čistenie po teste
        [TearDown]
        public void TearDown()
        {
            _cart.Clear();
        }

        [Test]
        public void Should_Place_Order_With_Discount()
        {
            this.Given(_ => GivenIHaveAnEmptyCart())
                .And(_ => GivenIAddAProductToCart("Laptop", 1200))
                .And(_ => GivenIApplyDiscountCode("BLACKFRIDAY"))
                .When(_ => WhenIProceedToCheckout())
                .Then(_ => ThenTheTotalPriceShouldBe(1080))
                .And(_ => ThenTheOrderShouldBeConfirmed())
                .BDDfy();
        }

        private void GivenIHaveAnEmptyCart()
        {
            _cart.Clear();
        }

        private void GivenIAddAProductToCart(string productName, decimal price)
        {
            _cart.AddProduct(new Product(productName, price));
        }

        private void GivenIApplyDiscountCode(string code)
        {
            _cart.ApplyDiscount(code);
        }

        
        private void WhenIProceedToCheckout()
        {
            _order = _cart.Checkout();
        }

        private void ThenTheTotalPriceShouldBe(decimal expected)
        {
            Assert.AreEqual(expected, _order.TotalPrice);
        }

        private void ThenTheOrderShouldBeConfirmed()
        {
            Assert.IsTrue(_order.IsConfirmed);
        }
    }

    public class ShoppingCart
    {
        private decimal _totalPrice;
        public Order Checkout() => new Order { TotalPrice = _totalPrice, IsConfirmed = true };
        public void AddProduct(Product product) => _totalPrice += product.Price;
        public void ApplyDiscount(string code) => _totalPrice *= 0.9m; // 10% zľava
        public void Clear() => _totalPrice = 0;
    }

    public class Order
    {
        public decimal TotalPrice { get; set; }
        public bool IsConfirmed { get; set; }
    }

    public class Product
    {
        public string Name { get; }
        public decimal Price { get; }
        public Product(string name, decimal price) => (Name, Price) = (name, price);
    }
}
