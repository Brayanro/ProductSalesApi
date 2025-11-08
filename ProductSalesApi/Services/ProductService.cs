using ProductSalesApi.Data;
using ProductSalesApi.DTOs;
using ProductSalesApi.Entities;
using Microsoft.EntityFrameworkCore;

namespace ProductSalesApi.Services;

public class ProductService
{
    private readonly AppDbContext _context;

    public ProductService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<ProductResponseDto>> GetAllAsync()
    {
        return await _context.Products
            .Select(p => new ProductResponseDto
            {
                Id = p.Id,
                Name = p.Name,
                Price = p.Price,
                Stock = p.Stock,
                ImageUrl = p.ImageUrl
            })
            .ToListAsync();
    }

    public async Task<ProductResponseDto?> GetByIdAsync(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null) return null;
        
        return new ProductResponseDto
        {
            Id = product.Id,
            Name = product.Name,
            Price = product.Price,
            Stock = product.Stock,
            ImageUrl = product.ImageUrl
        };
    }

    public async Task<ProductResponseDto> CreateAsync(ProductCreateDto productDto)
    {
        var product = new Product
        {
            Name = productDto.Name,
            Price = productDto.Price,
            Stock = productDto.Stock,
            ImageUrl = productDto.ImageUrl
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        return new ProductResponseDto
        {
            Id = product.Id,
            Name = product.Name,
            Price = product.Price,
            Stock = product.Stock,
            ImageUrl = product.ImageUrl
        };
    }

    public async Task<bool> UpdateAsync(int id, ProductCreateDto productDto)
    {
        var existingProduct = await _context.Products.FindAsync(id);
        if (existingProduct == null) return false;

        existingProduct.Name = productDto.Name;
        existingProduct.Price = productDto.Price;
        existingProduct.Stock = productDto.Stock;
        existingProduct.ImageUrl = productDto.ImageUrl;

        _context.Products.Update(existingProduct);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product is null) return false;
        
        _context.Products.Remove(product);
        return await _context.SaveChangesAsync() > 0;
    }
}