using AiOrderAgent.Services;
using System.Text.Json;

namespace AiOrderAgent.Tools;

public class GetOrderStatusTool
{
    private readonly IOrderService _orderService;

    public GetOrderStatusTool(IOrderService orderService)
    {
        _orderService = orderService;
    }

    public async Task<string> ExecuteAsync(string arguments)
    {
        using var json = JsonDocument.Parse(arguments);

        if (!json.RootElement.TryGetProperty("orderId", out var orderIdElement))
        {
            return JsonSerializer.Serialize(new
            {
                error = "orderId is required"
            });
        }

        var orderId = orderIdElement.GetInt32();

        var order = await _orderService.GetOrderStatusAsync(orderId);

        if (order == null)
        {
            return JsonSerializer.Serialize(new
            {
                found = false,
                message = $"Order {orderId} not found"
            });
        }

        return JsonSerializer.Serialize(new
        {
            found = true,
            order.Id,
            order.CustomerName,
            order.Status,
            order.TotalAmount,
            order.OrderDate
        });
    }
}