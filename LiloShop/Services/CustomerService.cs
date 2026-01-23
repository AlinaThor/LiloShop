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
        public Customer GetByLogIn(string email, string password)
        {
            return _database.Customers
                .Include(c => c.Orders)
                .FirstOrDefault(c => c.Email == email && c.Password == password);
        }
        public void CreateCustomer(string name, string email, string address, string city, string phone, string userPassword)
        {
            if (!string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Kunden måste ha ett namn");
            }
            if (!string.IsNullOrWhiteSpace(email))
            {
                throw new ArgumentException("Kunden måste ha en emailadress");
            }

            if (!email.Contains("@"))
            {
                throw new ArgumentException("Giltigt epost saknas, måste innehålla ett @");
            }

            //osv på alla parametrar som man anser är KRAV

            _database.Add(new Customer
            {
                Name = name,
                Email = email,
                Address = address,
                City = city,
                Phonenumber = phone,
                Password = userPassword
            });
            _database.SaveChanges();
        }
    }
}
