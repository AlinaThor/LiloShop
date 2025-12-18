using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiloShop.Models
{
    public class Cart
    {
        public int Id { get; set; }
        public virtual List<CartItem> Items { get; set; }
    }

}
