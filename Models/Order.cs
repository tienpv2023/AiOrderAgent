namespace AiOrderAgent.Models;

public class Order
{
    public int Id { get; set; }

    public string CustomerName { get; set; } = "";

    public string Status { get; set; } = "";

    public decimal TotalAmount { get; set; }

    public DateTime OrderDate { get; set; }
}