using System;
using System.Collections.Generic;
using System.Text;

namespace Models
{
    public class Order
    {
        public string Id { get; set; }

        public DateTime TimeStamp { get; set; }

        public IEnumerable<Product> Products { get; set; }
    }
}
