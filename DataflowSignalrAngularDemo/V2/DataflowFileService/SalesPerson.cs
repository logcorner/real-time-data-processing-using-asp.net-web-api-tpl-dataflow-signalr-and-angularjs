using System;

namespace DataflowFileService
{
    public class SalesPerson
    {
        public string Name { get; internal set; }
        public Guid SalesPersonId { get; internal set; }
    }
}