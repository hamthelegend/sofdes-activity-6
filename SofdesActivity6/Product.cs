using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SofdesActivity6;

public class DuplicateIdException : Exception
{
    public DuplicateIdException(): base("Duplicate ID"){}
}

public class IdDoesNotExistException : Exception
{
    public IdDoesNotExistException() : base("ID does not exist") { }
}

public class Product
{
    public Product(string id,
        string name,
        string description,
        int quantity,
        DateTimeOffset dateCreated,
        DateTimeOffset dateUpdated)
    {
        Id = id;
        Name = name;
        Description = description;
        Quantity = quantity;
        DateCreated = dateCreated;
        DateUpdated = dateUpdated;
    }

    public ProductEntity ToProductEntity()
    {
        return new ProductEntity()
        {
            Id = Id,
            Name = Name,
            Description = Description,
            Quantity = Quantity,
            DateCreated = DateCreated.Date,
            DateUpdated = DateUpdated.Date
        };
    }

    public string Id { get; }
    public string Name { get; }
    public string Description { get; }
    public int Quantity { get; }
    public DateTimeOffset DateCreated { get; }
    public DateTimeOffset DateUpdated { get; }

}

[Table("Products")]
public class ProductEntity
{
    [Key]
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public int Quantity { get; set; }
    public DateTimeOffset DateCreated { get; set; }
    public DateTimeOffset DateUpdated { get; set; }

    public Product ToProduct()
    {
        return new Product(Id, Name, Description, Quantity, DateCreated, DateUpdated);
    }

}

public class ProductContext : DbContext
{
    public DbSet<ProductEntity> ProductEntities { get; set; }

    public string DbPath { get; }

    public ProductContext()
    {
        const Environment.SpecialFolder folder = Environment.SpecialFolder.LocalApplicationData;
        var path = Environment.GetFolderPath(folder);
        DbPath = System.IO.Path.Join(path, "products.db");
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options) => options.UseSqlite($"Data Source={DbPath}");

}

public static class ProductsDb
{
    public static Product Get(string id)
    {
        using var context = new ProductContext();
        var productEntity = context.ProductEntities.FirstOrDefault(product => product.Id == id);
        if (productEntity == null) throw new IdDoesNotExistException();
        return productEntity.ToProduct();
    }
    public static List<Product> GetAll(string searchQuery = "")
    {
        using var context = new ProductContext();
        var productEntities = context.ProductEntities.Where(product => product.Name.ToLower().Contains(searchQuery.ToLower()));
        return productEntities.Select(productEntity => productEntity.ToProduct()).ToList();
    }

    public static void Insert(Product product)
    {
        using var context = new ProductContext();
        var productEntityOnDb = context.ProductEntities.FirstOrDefault(productEntity => productEntity.Id == product.Id);
        if (productEntityOnDb != null) throw new DuplicateIdException();
        var productEntity = product.ToProductEntity();
        context.Add(productEntity);
        context.SaveChanges();
    }

    public static void Update(Product product)
    {
        using var context = new ProductContext();
        var productEntityOnDb = context.ProductEntities.FirstOrDefault(productEntity => productEntity.Id == product.Id);
        if (productEntityOnDb == null) throw new IdDoesNotExistException();
        productEntityOnDb.Id = product.Id;
        productEntityOnDb.Name = product.Name;
        productEntityOnDb.Description = product.Description;
        productEntityOnDb.Quantity = product.Quantity;
        productEntityOnDb.DateUpdated = product.DateUpdated;
        context.SaveChanges();
    }

    public static bool Delete(string id)
    {
        using var context = new ProductContext();
        var productEntityOnDb = context.ProductEntities.FirstOrDefault(productEntity => productEntity.Id == id);
        if (productEntityOnDb == null) throw new IdDoesNotExistException();
        context.Remove(productEntityOnDb);
        context.SaveChanges();
        return true;
    }

}