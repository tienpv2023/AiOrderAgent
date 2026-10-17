using AiOrderAgent.Data;
using AiOrderAgent.Models;
using Microsoft.EntityFrameworkCore;

namespace AiOrderAgent.Services;

public class OrderService : IOrderService
{
    private readonly AppDbContext _db;

    public OrderService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<Order?> GetOrderStatusAsync(int orderId)
    {
        return await _db.Orders
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == orderId);
    }
}