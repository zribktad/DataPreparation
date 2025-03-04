using Microsoft.EntityFrameworkCore;

namespace OrderService.Models
{
    public class PrepDB
    {
        public static void prepPopulation(IApplicationBuilder app)
        {

            using (var servicesScope = app.ApplicationServices.CreateScope())
            {
                SeedData(servicesScope.ServiceProvider.GetRequiredService<OrderServiceContext>());
            }
        }
        public static void SeedData(OrderServiceContext context)
        {
            if (!context.Database.IsRelational()) return;
            try
            {
                context.Database.Migrate();
                Console.WriteLine("Migrations applied successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error applying migrations: " + ex.Message);
            }

            if (context.Orders.Any())
            {
                Console.WriteLine("Tables already populated.");
                return;
            }

            var orderItem = new OrderItem { ItemId = 1, Quantity = 2 };
            var orderStatus = new OrderStatus { Status = Status.CREATED, StatusDate = DateTime.Now.ToUniversalTime() };
            var complaint = new Complaint { Reason = "Item was damaged", Status = "Open", Created = DateTime.Now.ToUniversalTime() };
            var rating = new Rating { NumOfStars = 5, Reason = "Great service"};
            var order = new Order
            {
                OrderItems = new List<OrderItem> { orderItem },
                OrderStatuses = new List<OrderStatus> { orderStatus },
                OrderDate = DateTime.Now.ToUniversalTime(),
                Complaint = complaint,
                Rating = rating,
            };
            complaint.Order = order;
            rating.Order = order;

            var customers = new List<Customer>
            {
                new Customer
                {
                    Name = "John Doe",
                    Email = "johndoe@shop.com",
                    Phone = "1234567890",
                    Address = new Address
                    {
                        Street = "123 Main St",
                        City = "Springfield",
                        PostalCode = "62701"
                    }
                },
                new Customer
                {
                    Name = "Jane Doe",
                    Email = "jane@doe.com",
                    Phone = "0987654321",
                    Address = new Address
                    {
                        Street = "456 Elm St",
                        City = "Springfield",
                        PostalCode = "62702"
                    },
                    Orders = new List<Order> { order }
                }
            };

            try
            {
                context.Ratings.Add(rating);
                context.Complaints.Add(complaint);
                context.Orders.Add(order);
                context.Customers.AddRange(customers);
                context.SaveChanges();

                Console.WriteLine("Initial data inserted successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error inserting initial data: " + ex.Message);
            }
        }
    }
}
