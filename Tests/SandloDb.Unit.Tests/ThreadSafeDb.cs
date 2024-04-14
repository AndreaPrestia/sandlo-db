using System.Collections.Concurrent;

namespace SandloDb.Unit.Tests;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Xunit;

public class Product
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public decimal Price { get; set; }
}

public class ThreadSafeInMemoryDbContext
{
    private Dictionary<Guid, Product> _products;
    private readonly object _lock = new();

    public ThreadSafeInMemoryDbContext()
    {
        _products = new Dictionary<Guid, Product>();
    }

    public void AddProduct(Product product)
    {
        if (product == null)
            throw new ArgumentNullException(nameof(product));

        lock (_lock)
        {
            if (_products.ContainsKey(product.Id))
                throw new ArgumentException("Product with the same Id already exists");

            _products.Add(product.Id, product);
        }
    }

    public Product GetProduct(Guid id)
    {
        lock (_lock)
        {
            if (_products.ContainsKey(id))
                return _products[id];
            else
                return null!;
        }
    }

    public IEnumerable<Product> GetAllProducts()
    {
        lock (_lock)
        {
            return _products.Values.ToList();
        }
    }

    public void RemoveProduct(Guid id)
    {
        lock (_lock)
        {
            if (_products.ContainsKey(id))
                _products.Remove(id);
        }
    }

    public void AddProducts(IEnumerable<Product> products)
    {
        if (products == null)
            throw new ArgumentNullException(nameof(products));

        lock (_lock)
        {
            foreach (var product in products)
            {
                if (_products.ContainsKey(product.Id))
                    throw new ArgumentException($"Product with Id {product.Id} already exists");

                _products.Add(product.Id, product);
            }
        }
    }

    public void UpdateProducts(IEnumerable<Product> products)
    {
        if (products == null)
            throw new ArgumentNullException(nameof(products));

        lock (_lock)
        {
            foreach (var product in products)
            {
                if (!_products.ContainsKey(product.Id))
                    throw new KeyNotFoundException($"Product with Id {product.Id} does not exist");

                _products[product.Id] = product;
            }
        }
    }

    public void RemoveProducts(IEnumerable<Guid> productIds)
    {
        if (productIds == null)
            throw new ArgumentNullException(nameof(productIds));

        lock (_lock)
        {
            foreach (var productId in productIds)
            {
                if (_products.ContainsKey(productId))
                    _products.Remove(productId);
            }
        }
    }
}

public class ThreadSafeInMemoryDbContextTests
{
    [Fact]
    public void AddProduct_NullProduct_ThrowsException()
    {
        // Arrange
        var dbContext = new ThreadSafeInMemoryDbContext();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => dbContext.AddProduct(null!));
    }

    [Fact]
    public void AddProduct_DuplicateId_ThrowsException()
    {
        // Arrange
        var dbContext = new ThreadSafeInMemoryDbContext();
        var product = new Product { Id = Guid.NewGuid(), Name = "Test Product", Price = 10.0m };

        // Act
        dbContext.AddProduct(product);

        // Assert
        Assert.Throws<ArgumentException>(() => dbContext.AddProduct(product));
    }

    [Fact]
    public void GetProduct_ProductExists_ReturnsProduct()
    {
        // Arrange
        var dbContext = new ThreadSafeInMemoryDbContext();
        var product = new Product { Id = Guid.NewGuid(), Name = "Test Product", Price = 10.0m };
        dbContext.AddProduct(product);

        // Act
        var retrievedProduct = dbContext.GetProduct(product.Id);

        // Assert
        Assert.NotNull(retrievedProduct);
        Assert.Equal(product.Id, retrievedProduct.Id);
        Assert.Equal(product.Name, retrievedProduct.Name);
        Assert.Equal(product.Price, retrievedProduct.Price);
    }

    [Fact]
    public void GetProduct_ProductDoesNotExist_ReturnsNull()
    {
        // Arrange
        var dbContext = new ThreadSafeInMemoryDbContext();
        var productId = Guid.NewGuid();

        // Act
        var retrievedProduct = dbContext.GetProduct(productId);

        // Assert
        Assert.Null(retrievedProduct);
    }

    [Fact]
    public void GetAllProducts_NoProducts_ReturnsEmptyList()
    {
        // Arrange
        var dbContext = new ThreadSafeInMemoryDbContext();

        // Act
        var allProducts = dbContext.GetAllProducts();

        // Assert
        Assert.Empty(allProducts);
    }

    [Fact]
    public void GetAllProducts_MultipleProducts_ReturnsAllProducts()
    {
        // Arrange
        var dbContext = new ThreadSafeInMemoryDbContext();
        var products = new List<Product>();
        for (int i = 0; i < 5; i++)
        {
            var product = new Product { Id = Guid.NewGuid(), Name = $"Product {i + 1}", Price = (i + 1) * 10.0m };
            dbContext.AddProduct(product);
            products.Add(product);
        }

        // Act
        var allProducts = dbContext.GetAllProducts().ToList();

        // Assert
        Assert.Equal(products.Count, allProducts.Count);
        foreach (var product in products)
        {
            Assert.Contains(product, allProducts);
        }
    }

    [Fact]
    public void RemoveProduct_ProductExists_RemovesProduct()
    {
        // Arrange
        var dbContext = new ThreadSafeInMemoryDbContext();
        var product = new Product { Id = Guid.NewGuid(), Name = "Test Product", Price = 10.0m };
        dbContext.AddProduct(product);

        // Act
        dbContext.RemoveProduct(product.Id);
        var retrievedProduct = dbContext.GetProduct(product.Id);

        // Assert
        Assert.Null(retrievedProduct);
    }

    [Fact]
    public void RemoveProduct_ProductDoesNotExist_NoException()
    {
        // Arrange
        var dbContext = new ThreadSafeInMemoryDbContext();
        var productId = Guid.NewGuid();

        // Act & Assert
        dbContext.RemoveProduct(productId); // Ensure no exception is thrown
    }

    [Fact]
    public void ConcurrentAccess_AddProduct_NoCollisions()
    {
        // Arrange
        var dbContext = new ThreadSafeInMemoryDbContext();
        int numThreads = 10;
        int numIterations = 100;
        var threads = new List<Thread>();

        // Act
        for (int i = 0; i < numThreads; i++)
        {
            var thread = new Thread(() =>
            {
                for (int j = 0; j < numIterations; j++)
                {
                    var productId = Guid.NewGuid();
                    var product = new Product { Id = productId, Name = $"Product {productId}", Price = 10.0m };
                    dbContext.AddProduct(product);
                }
            });
            threads.Add(thread);
            thread.Start();
        }

        // Assert
        foreach (var thread in threads)
        {
            thread.Join();
        }

        // Ensure all products are added without collisions
        var allProducts = dbContext.GetAllProducts();
        Assert.Equal(numThreads * numIterations, allProducts.Count());
    }

    [Fact]
    public void ConcurrentAccess_RemoveProduct_NoCollisions()
    {
        // Arrange
        var dbContext = new ThreadSafeInMemoryDbContext();
        int numThreads = 10;
        int numIterations = 100;
        var threads = new List<Thread>();
        var products = new List<Product>();

        // Add products
        for (int i = 0; i < numThreads * numIterations; i++)
        {
            var product = new Product { Id = Guid.NewGuid(), Name = $"Product {i}", Price = 10.0m };
            dbContext.AddProduct(product);
            products.Add(product);
        }

        // Act
        for (int i = 0; i < numThreads; i++)
        {
            var thread = new Thread(() =>
            {
                for (int j = 0; j < numIterations; j++)
                {
                    Product productToRemove;
                    lock (products)
                    {
                        if (products.Count > 0)
                        {
                            productToRemove = products[0];
                            products.RemoveAt(0);
                        }
                        else
                        {
                            return;
                        }
                    }

                    dbContext.RemoveProduct(productToRemove.Id);
                }
            });
            threads.Add(thread);
            thread.Start();
        }

        // Assert
        foreach (var thread in threads)
        {
            thread.Join();
        }

        // Ensure all products are removed without collisions
        var allProducts = dbContext.GetAllProducts();
        Assert.Empty(allProducts);
    }

    [Fact]
    public void AddProducts_ValidProducts_AddsProducts()
    {
        // Arrange
        var dbContext = new ThreadSafeInMemoryDbContext();
        var products = new List<Product>
        {
            new Product { Id = Guid.NewGuid(), Name = "Product 1", Price = 10.0m },
            new Product { Id = Guid.NewGuid(), Name = "Product 2", Price = 20.0m },
            new Product { Id = Guid.NewGuid(), Name = "Product 3", Price = 30.0m }
        };

        // Act
        dbContext.AddProducts(products);

        // Assert
        var allProducts = dbContext.GetAllProducts().ToList();
        Assert.Equal(products.Count, allProducts.Count);
        foreach (var product in products)
        {
            Assert.Contains(product, allProducts);
        }
    }

    [Fact]
    public void UpdateProducts_ValidProducts_UpdatesProducts()
    {
        // Arrange
        var dbContext = new ThreadSafeInMemoryDbContext();
        var product = new Product { Id = Guid.NewGuid(), Name = "Product 1", Price = 10.0m };
        dbContext.AddProduct(product);
        var updatedProducts = new List<Product>
        {
            new Product { Id = product.Id, Name = "Updated Product 1", Price = 20.0m }
        };

        // Act
        dbContext.UpdateProducts(updatedProducts);

        // Assert
        var updatedProduct = dbContext.GetProduct(product.Id);
        Assert.NotNull(updatedProduct);
        Assert.Equal(updatedProducts[0].Name, updatedProduct.Name);
        Assert.Equal(updatedProducts[0].Price, updatedProduct.Price);
    }

    [Fact]
    public void RemoveProducts_ValidProductIds_RemovesProducts()
    {
        // Arrange
        var dbContext = new ThreadSafeInMemoryDbContext();
        var products = new List<Product>
        {
            new Product { Id = Guid.NewGuid(), Name = "Product 1", Price = 10.0m },
            new Product { Id = Guid.NewGuid(), Name = "Product 2", Price = 20.0m },
            new Product { Id = Guid.NewGuid(), Name = "Product 3", Price = 30.0m }
        };
        dbContext.AddProducts(products);
        var productIdsToRemove = products.Select(p => p.Id).ToList();

        // Act
        dbContext.RemoveProducts(productIdsToRemove);

        // Assert
        var remainingProducts = dbContext.GetAllProducts().ToList();
        Assert.Empty(remainingProducts);
    }

    [Fact]
    public void AddProducts_ConcurrentAccess_NoCollisions()
    {
        // Arrange
        var dbContext = new ThreadSafeInMemoryDbContext();
        int numThreads = 10;
        int numIterations = 100;
        var threads = new List<Thread>();
        var products = new ConcurrentBag<Product>(); // Using ConcurrentBag for thread-safe collection
        var generatedIds = new HashSet<Guid>();

        // Act
        for (int i = 0; i < numThreads; i++)
        {
            var thread = new Thread(() =>
            {
                for (int j = 0; j < numIterations; j++)
                {
                    Guid productId;
                    lock (generatedIds)
                    {
                        do
                        {
                            productId = Guid.NewGuid();
                        }
                        while (!generatedIds.Add(productId)); // Ensure unique ID
                    }
                    var product = new Product { Id = productId, Name = $"Product {productId}", Price = 10.0m };
                    products.Add(product);
                }
            });
            threads.Add(thread);
            thread.Start();
        }

        // Wait for all threads to complete
        foreach (var thread in threads)
        {
            thread.Join();
        }

        // Act
        dbContext.AddProducts(products);

        // Assert
        var allProducts = dbContext.GetAllProducts().ToList();
        Assert.Equal(numThreads * numIterations, allProducts.Count);

        // Ensure each product is added exactly once
        var uniqueProductIds = new HashSet<Guid>();
        foreach (var product in allProducts)
        {
            Assert.True(uniqueProductIds.Add(product.Id)); // Add returns false if ID already exists
        }
    }

    [Fact]
    public void UpdateProducts_ConcurrentAccess_NoCollisions()
    {
        // Arrange
        var dbContext = new ThreadSafeInMemoryDbContext();
        int numThreads = 10;
        int numIterations = 100;
        var threads = new List<Thread>();
        var products = new List<Product>();

        // Add products
        for (int i = 0; i < numThreads * numIterations; i++)
        {
            var productId = Guid.NewGuid();
            var product = new Product { Id = productId, Name = $"Product {productId}", Price = 10.0m };
            dbContext.AddProduct(product);
            products.Add(product);
        }

        // Act
        for (int i = 0; i < numThreads; i++)
        {
            var thread = new Thread(() =>
            {
                for (int j = 0; j < numIterations; j++)
                {
                    Product productToUpdate;
                    lock (products)
                    {
                        if (products.Count > 0)
                        {
                            productToUpdate = products[0];
                            products.RemoveAt(0);
                        }
                        else
                        {
                            return;
                        }
                    }
                    productToUpdate.Price += 1.0m; // Increment price
                }
            });
            threads.Add(thread);
            thread.Start();
        }

        // Wait for all threads to complete
        foreach (var thread in threads)
        {
            thread.Join();
        }

        // Act
        dbContext.UpdateProducts(products);

        // Assert
        var allProducts = dbContext.GetAllProducts().ToList();
        foreach (var product in allProducts)
        {
            Assert.Equal(11.0m, product.Price); // Price should be incremented by 1.0
        }
    }

    [Fact]
    public void RemoveProducts_ConcurrentAccess_NoCollisions()
    {
        // Arrange
        var dbContext = new ThreadSafeInMemoryDbContext();
        int numThreads = 10;
        int numIterations = 100;
        var threads = new List<Thread>();
        var products = new List<Product>();

        // Add products
        for (int i = 0; i < numThreads * numIterations; i++)
        {
            var productId = Guid.NewGuid();
            var product = new Product { Id = productId, Name = $"Product {productId}", Price = 10.0m };
            dbContext.AddProduct(product);
            products.Add(product);
        }

        // Act
        for (int i = 0; i < numThreads; i++)
        {
            var thread = new Thread(() =>
            {
                for (int j = 0; j < numIterations; j++)
                {
                    Product productToRemove;
                    lock (products)
                    {
                        if (products.Count > 0)
                        {
                            productToRemove = products[0];
                            products.RemoveAt(0);
                        }
                        else
                        {
                            return;
                        }
                    }
                    dbContext.RemoveProduct(productToRemove.Id);
                }
            });
            threads.Add(thread);
            thread.Start();
        }

        // Wait for all threads to complete
        foreach (var thread in threads)
        {
            thread.Join();
        }

        // Assert
        var allProducts = dbContext.GetAllProducts().ToList();
        Assert.Empty(allProducts);
    }

}