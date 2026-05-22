using System;
using DotNetKafkaRabbitMQExample.Models;

namespace DotNetKafkaRabbitMQExample.Repository.IRepository;

public interface IProductRepository
{
    ICollection<Product> GetProducts();
    ICollection<Product> GetProductPaginated(int pageNumber, int pageSize);
    int GetTotalProductsCount();
    ICollection<Product> GetProductsForCategory(int categoryId);
    ICollection<Product> SearchProduct(string searchTerm);
    Product? GetProduct(int productId);
    bool BuyProduct(string name, int quantity);
    bool ProductExists(int productId);
    bool ProductExists(string name);
    bool CreateProduct(Product product);
    bool UpdateProduct(Product product);
    bool DeleteProduct(Product product);
    bool Save();
}
