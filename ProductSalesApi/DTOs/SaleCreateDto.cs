namespace ProductSalesApi.DTOs;

public class SaleCreateDto
{
    public DateOnly Date { get; set; } 
    public List<SaleItemCreateDto> Items { get; set; } = new();
}