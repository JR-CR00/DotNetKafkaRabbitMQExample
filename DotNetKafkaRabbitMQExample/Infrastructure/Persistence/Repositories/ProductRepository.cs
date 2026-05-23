using DotNetKafkaRabbitMQExample.Infrastructure.Persistence;
using System;
using Microsoft.EntityFrameworkCore;
using DotNetKafkaRabbitMQExample.Domain.Entities;
using DotNetKafkaRabbitMQExample.Application.Interfaces;

namespace DotNetKafkaRabbitMQExample.Infrastructure.Persistence.Repositories;

public class ProductRepository : IProductRepository
{

    private readonly ApplicationDbContext _db;

    public ProductRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    public bool BuyProduct(string name, int quantity)
    {
        if (string.IsNullOrWhiteSpace(name) || quantity <= 0)
            return false;

        var product = _db.Products.FirstOrDefault(p => p.Name.Trim().ToLower() == name.Trim().ToLower());

        if (product == null || product.Stock < quantity)
            return false;

        product.Stock -= quantity;
        product.UpdatedAt = DateTime.Now;
        _db.Products.Update(product);
        return Save();
    }

    public bool CreateProduct(Product product)
    {

        if (product is null)
            return false;

        product.CreatedAt = DateTime.Now;
        _db.Products.Add(product);
        return Save();
    }

    public bool DeleteProduct(Product product)
    {
        if (product is null)
            return false;

        _db.Products.Remove(product);
        return Save();
    }

    public Product? GetProduct(int productId)
    {
        if (productId <= 0)
            return null;

        return _db.Products.AsNoTracking().Include(p => p.Category).FirstOrDefault(p => p.Id == productId);
    }

    public ICollection<Product> GetProductPaginated(int pageNumber, int pageSize)
    {
        return _db.Products.OrderBy(p => p.Id)
        .Skip((pageNumber - 1) * pageSize)
        .Take(pageSize)
        .ToList();
    }

    public ICollection<Product> GetProducts()
    {
        return _db.Products
        .AsNoTracking()
        .Include(p => p.Category)
        .OrderBy(p => p.Id)
        .ToList();
    }

    public ICollection<Product> GetProductsForCategory(int categoryId)
    {
        if (categoryId <= 0)
            return new List<Product>();

        return _db.Products.AsNoTracking().Include(p => p.Category).Where(p => p.CategoryId == categoryId).OrderBy(p => p.Id).ToList();
    }

    public int GetTotalProductsCount()
    {
        return _db.Products.Count();
    }

    public bool ProductExists(int productId)
    {
        if (productId <= 0)
            return false;

        return _db.Products.Any(p => p.Id == productId);
    }

    public bool ProductExists(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return false;

        return _db.Products.Any(p => p.Name.Trim().ToLower() == name.Trim().ToLower());
    }

    public bool Save()
    {
        return _db.SaveChanges() >= 0;
    }

    public ICollection<Product> SearchProduct(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return new List<Product>();

        return _db.Products.AsNoTracking().Include(p => p.Category).Where(p => p.Name.Trim().ToLower().Contains(searchTerm.Trim().ToLower())).OrderBy(p => p.Id).ToList();
    }

    public bool UpdateProduct(Product product)
    {
        if (product is null)
            return false;

        product.UpdatedAt = DateTime.Now;
        _db.Products.Update(product);
        return Save();
    }
}


