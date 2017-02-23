using System;

namespace DataflowFileService
{
    public class Product
    {
        public Guid ProductID { get; set; }

        public double Price { get; set; }
        public int Qty { get; set; }
    }
}