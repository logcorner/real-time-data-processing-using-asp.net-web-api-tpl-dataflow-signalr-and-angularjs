using System;
using System.Collections.Generic;

namespace DataflowFileService
{
    public class SalesOrderDetailEntity
    {
        public string SalesPersonName { get; set; }

        public Guid SalesOrderDetailID { get; set; }

        public string CarrierTrackingNumber { get; set; }

        public int OrderQty { get; set; }

        public double PriceDiscount { get; set; }
        public double LineTotal { get; set; }

        public DateTime Date { get; set; }

        public IEnumerable<Product> Products { get; internal set; }
    }
}