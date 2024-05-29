using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustOrderNetstockService.models
{
    internal class OrderModel
    {
        public string ItemCode { get; set; }
        public string Location { get; set; }
        public string CustomerCode { get; set; }
        public string OrderNumber { get; set; }
        public decimal LineNumber { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal OrderQuantity { get; set; }
        public DateTime RequestDate { get; set; }
        public decimal OutstandingQuantity { get; set; }
    }
}
