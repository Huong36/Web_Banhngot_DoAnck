using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Web_Banhngot_DoAnck.Models
{
    public class CartItem
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string ImageUrl { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public decimal Total { get { return Price * Quantity; } }
    }
}