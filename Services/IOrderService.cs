using AiOrderAgent.Models;

namespace AiOrderAgent.Services;

public interface IOrderService
{
    Task<Order?> GetOrderStatusAsync(int orderId);
}