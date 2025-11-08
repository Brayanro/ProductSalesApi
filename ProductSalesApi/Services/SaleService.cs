using Microsoft.EntityFrameworkCore;
using ProductSalesApi.Data;
using ProductSalesApi.DTOs;
using ProductSalesApi.Entities;

namespace ProductSalesApi.Services;

public class SaleService
{
    private readonly AppDbContext _context;

    public SaleService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Sale> RegisterSaleAsync(SaleCreateDto dto)
    {
        if (dto.Items == null || dto.Items.Count == 0)
            throw new ArgumentException("Sale must contain at least one item.");

        // Validar que los productos existan
        var productIds = dto.Items.Select(i => i.ProductId).ToList();
        var products = await _context.Products
            .Where(p => productIds.Contains(p.Id))
            .ToListAsync();

        if (products.Count != productIds.Count)
            throw new ArgumentException("One or more ProductIds are invalid.");

        var sale = new Sale
        {
            Date = dto.Date, // 👈 ya es DateOnly
            Items = new List<SaleItem>()
        };

        decimal total = 0;

        foreach (var item in dto.Items)
        {
            var product = products.First(p => p.Id == item.ProductId);

            // Validar stock disponible
            if (product.Stock < item.Quantity)
                throw new ArgumentException($"Not enough stock for product '{product.Name}'.");

            // Descontar stock
            product.Stock -= item.Quantity;

            var saleItem = new SaleItem
            {
                ProductId = product.Id,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice
            };

            sale.Items.Add(saleItem);
            total += item.Quantity * item.UnitPrice;
        }

        sale.Total = total;

        _context.Sales.Add(sale);
        await _context.SaveChangesAsync();

        return sale;
    }

    // ✅ Adaptamos el método de reporte para DateOnly
    public async Task<List<Sale>> GetSalesByDateRangeAsync(DateOnly start, DateOnly end)
    {
        return await _context.Sales
            .Include(s => s.Items)
            .ThenInclude(i => i.Product)
            .Where(s => s.Date >= start && s.Date <= end)
            .ToListAsync();
    }
}
