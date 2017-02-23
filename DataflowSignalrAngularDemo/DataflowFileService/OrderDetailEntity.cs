using System;
using System.Collections.Generic;

namespace DataflowFileService
{
    public class OrderDetailEntity
    {
        public Guid OrderID { get; set; }

        public List<Product> Products { get; set; }

        public Account Account { get; set; }

        public SalesPerson SalesPerson { get; set; }

        public DateTime OrderDate;
        public string Origin { get; set; }
        public double PriceDiscount { get; set; }
        public Processor Processor { get; set; }
    }
}