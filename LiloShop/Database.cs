using LiloShop.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace LiloShop
{
    public class Database : DbContext
    {

        //Local
        //public const string CONNECTION_STRING = "Server=.\\SQLExpress;Database=LilosNewShop;Trusted_Connection=True; TrustServerCertificate=True;";

        //Azure
        public const string CONNECTION_STRING = "Server=tcp:alinasdb.database.windows.net,1433;Initial Catalog=LiloShop;Persist Security Info=False;User ID=alina;Password=LiloCodivine3060;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";


        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Admin> Admins { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (string.IsNullOrWhiteSpace(CONNECTION_STRING))
            {
                throw new Exception("Kommentera fram korrekt connectionstirng i Database.cs");
            }
            optionsBuilder.UseSqlServer(CONNECTION_STRING);

        }

    }
}
