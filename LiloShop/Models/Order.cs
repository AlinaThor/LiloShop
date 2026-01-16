using Microsoft.EntityFrameworkCore.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiloShop.Models
{
    public class Order
    {
        public int Id { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalPrice { get; set; }
        public OrderStatus Status { get; set; }

        public int CustomerId { get; set; }
        public Customer Customer { get; set; }
        public string ShippingMethod { get; set; }
        public decimal ShippingCost { get; set; }
        public string PaymentMethod { get; set; }

        public List<OrderItem> Items { get; set; } = new();
    }
}
