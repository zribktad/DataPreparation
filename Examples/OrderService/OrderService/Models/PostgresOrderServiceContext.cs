using Microsoft.EntityFrameworkCore;

namespace OrderService.Models;

public class PostgresOrderServiceContext: OrderServiceContext
{
    public PostgresOrderServiceContext(DbContextOptions<OrderServiceContext> options) : base(options)
    {
    }
}