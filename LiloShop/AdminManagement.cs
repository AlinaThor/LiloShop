using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiloShop
{
    public class AdminManagement
    {
        private readonly Database _database;
        public AdminManagement()
        {
            _database = new Database();
        }

        public async Task RenderAdminAllOrders()
        {
            var sw = new Stopwatch();
            sw.Start();
            var orders = await _database.Orders.ToListAsync();


            //anropa metoden RenderOrders
            sw.Stop();
            //skriv ut tiden det tog att ladda och presentera alla ordrar
            Console.WriteLine($"Hela hämtningen tog {sw.ElapsedMilliseconds} ms (millisekunder)");
            //skrivut Klicka enter för att gå tillbaka

        }

       
    }
}
