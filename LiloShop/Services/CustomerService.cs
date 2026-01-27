using LiloShop.Models;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Numerics;

namespace LiloShop.Services
{
    public class CustomerService
    {
        private readonly Database _database;

        public CustomerService(Database database)
        {
            _database = database;
        }

        public List<Customer> GetAllCustomers()
        {
            return _database.Customers.ToList();
        }
        public Customer Login(string email, string password)
        {

            var hashedPassword = SecurityService.HashPassword(password);
            return _database.Customers
                .Include(c => c.Orders)
                .FirstOrDefault(c => c.Email == email && c.Password == hashedPassword);
        }
        public void CreateCustomer(string name, string email, string address, string city, string phone, string userPassword)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Kunden måste ha ett namn");
            }
            if (string.IsNullOrWhiteSpace(email))
            {
                throw new ArgumentException("Kunden måste ha en emailadress");
            }

            if (!email.Contains("@"))
            {
                throw new ArgumentException("Giltigt epost saknas, måste innehålla ett @");
            }

            if (string.IsNullOrWhiteSpace(phone))
            {
                throw new ArgumentException("Kunden måste ha ett telefonnummer");
            }

            if (string.IsNullOrWhiteSpace(userPassword))
            {
                throw new ArgumentException("Kunden måste ha ett lösenord");
            }

            var hashedPassword = SecurityService.HashPassword(userPassword);

            _database.Add(new Customer
            {
                Name = name,
                Email = email,
                Address = address,
                City = city,
                Phonenumber = phone,
                Password = hashedPassword
            });
            _database.SaveChanges();
        }
    }
}
