using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using DotNetKafkaRabbitMQExample.Models;
using DotNetKafkaRabbitMQExample.Repository;
using Xunit;

namespace DotNetKafkaRabbitMQExample.Tests
{
    public class ProductRepositoryTests
    {
        private ApplicationDbContext GetDbContext()
        {
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.Test.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            var connectionString = configuration.GetConnectionString("DefaultConnection");

            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();


            if (!string.IsNullOrEmpty(connectionString))
            {
                optionsBuilder.UseNpgsql(connectionString);
            }
            else
            {
                throw new InvalidOperationException("No connection string provided.");
            }
            
            var databaseContext = new ApplicationDbContext(optionsBuilder.Options);
            databaseContext.Database.EnsureCreated();
            return databaseContext;
        }

        [Fact]
        public void CreateProduct_AddsProductToDb()
        {
            // Arrange
            var dbContext = GetDbContext();
            var repository = new ProductRepository(dbContext);
            var productName = "Test Product " + Guid.NewGuid().ToString();
            var product = new Product 
            { 
                Name = productName, 
                Price = 100, 
                SKU = "SKU-" + Guid.NewGuid().ToString().Substring(0,8),
                CategoryId = 1,
                Category = dbContext.Categories.FirstOrDefault(c => c.Id == 1) ?? new Category { Id = 1, Name = "Test Category" }
            };

            // Act
            var result = repository.CreateProduct(product);

            // Assert
            Assert.True(result);
            Assert.Contains(dbContext.Products, p => p.Name == productName);
        }

        [Fact]
        public void BuyProduct_DecreasesStock()
        {
            // Arrange
            var dbContext = GetDbContext();
            var productName = "Stock Product " + Guid.NewGuid().ToString();
            var product = new Product 
            { 
                Name = productName, 
                Stock = 10,
                Price = 100, 
                SKU = "SKU-" + Guid.NewGuid().ToString().Substring(0,8),
                CategoryId = 1,
                Category = dbContext.Categories.FirstOrDefault(c => c.Id == 1) ?? new Category { Id = 1, Name = "Test Category" }
            };
            dbContext.Products.Add(product);
            dbContext.SaveChanges();
            var repository = new ProductRepository(dbContext);

            // Act
            var result = repository.BuyProduct(productName, 3);

            // Assert
            Assert.True(result);
            var updatedProduct = dbContext.Products.First(p => p.Name == productName);
            Assert.Equal(7, updatedProduct.Stock);
        }

        [Fact]
        public void GetProductPaginated_ReturnsCorrectPage()
        {
            // Arrange
            var dbContext = GetDbContext();
            var category = dbContext.Categories.FirstOrDefault(c => c.Id == 1) ?? new Category { Id = 1, Name = "Test Category" };
            
            // Aseguramos que haya al menos 10 productos para la prueba de paginación
            if (dbContext.Products.Count() < 10)
            {
                for (int i = 1; i <= 10; i++)
                {
                    dbContext.Products.Add(new Product 
                    { 
                        Name = $"Pagination Product {i} " + Guid.NewGuid().ToString().Substring(0,5),
                        Price = 10,
                        SKU = $"SKU-PAG-{i}-" + Guid.NewGuid().ToString().Substring(0,5),
                        CategoryId = 1,
                        Category = category
                    });
                }
                dbContext.SaveChanges();
            }
            
            var repository = new ProductRepository(dbContext);

            // Act
            var result = repository.GetProductPaginated(1, 3); // Page 1, Size 3

            // Assert
            Assert.Equal(3, result.Count);
        }
    }
}
