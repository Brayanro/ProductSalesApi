namespace ProductSalesApi.Entities;

public class Sale
{
    public int Id { get; set; }
    public DateOnly Date { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);
    public decimal Total { get; set; }  
    public List<SaleItem> Items { get; set; } = new();
}