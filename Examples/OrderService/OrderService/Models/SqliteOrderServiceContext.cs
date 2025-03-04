using Microsoft.EntityFrameworkCore;

namespace OrderService.Models;

public class SqliteOrderServiceContext : OrderServiceContext
{
    public SqliteOrderServiceContext(DbContextOptions<OrderServiceContext> options) : base(options)
    {
    }
}