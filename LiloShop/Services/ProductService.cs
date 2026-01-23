using LiloShop.Models;
using Microsoft.EntityFrameworkCore;
using System.Drawing;

namespace LiloShop.Services
{
    public class ProductService
    {
        private readonly Database _database;

        public ProductService(Database database)
        {
            _database = database;
        }

        public List<Product> GetAllProducts()
        {
            return _database.Products
                .Include(p => p.Category)
                .ToList();
        }

        public List<Product> GetSpecialOfferProducts(int take = 3)
        {
            return _database.Products
                .Include(p => p.Category)
                .Where(x => x.IsSpecialOffer)
                .Take(take)
                .ToList();
        }



        public void UpdateProduct(int productId, string name, string description, string priceInput, string colorInput, string sizeInput, string offerInput)
        {
            var product = _database.Products.FirstOrDefault(x => x.Id == productId);
            if (product == null) 
            {
                throw new ArgumentException($"Produkten hittades inte i databasen med produktId {productId}");
            }
            if (!string.IsNullOrWhiteSpace(name))
            {
                product.Name = name;
            }

            if (!string.IsNullOrWhiteSpace(description))
            {
                product.Description = description;
            }

            // Pris
            if (!string.IsNullOrWhiteSpace(priceInput))
            {
                if (decimal.TryParse(priceInput, out var price) && price > 0)
                {
                    product.Price = price;
                }
                else
                {
                    throw new ArgumentException("Felaktigt pris, nuvarande pris behålls. ");
                }
            }

            //Färg
            if (!string.IsNullOrWhiteSpace(colorInput))
            {
                if (int.TryParse(colorInput, out var colorValue) && Enum.IsDefined(typeof(Models.Color), colorValue))
                {
                    product.Color = (Models.Color)colorValue;
                }
                else
                {
                    throw new ArgumentException("Felaktigt färg, nuvarande färg behålls. ");
                }
            }

            // Storlek
            if (!string.IsNullOrWhiteSpace(sizeInput))
            {
                if (int.TryParse(sizeInput, out var size))
                {
                    product.Size = size;
                }
                else
                {
                    throw new ArgumentException("Felktig storlek, nuvarande storlek behålls");
                }

            }
            // Specialerbjudande
            if (!string.IsNullOrWhiteSpace(offerInput))
            {
                if (offerInput.ToLower() == "j")
                {
                    product.IsSpecialOffer = true;
                }
                else if (offerInput.ToLower() == "n")
                {
                    product.IsSpecialOffer = false;
                }

            }

            _database.SaveChanges();
        }

        public void DeleteProduct(Product product)
        {
            _database.Products.Remove(product);
            _database.SaveChanges();
        }

        public void CreateProduct(Product product)
        {
            //validera
            if (string.IsNullOrWhiteSpace(product.Name))
            {
                throw new ArgumentException("Produktnamn saknas");
            }
            _database.Products.Add(product);
            _database.SaveChanges();
        }
    }
}
