using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WMS.BLLClass
{
   public class ReceiptDetailCompleteResult
    {
        public string Action { get; set; }
        public string ClientCode { get; set; }
        public string LocationId { get; set; }
        public string ReceiptId { get; set; }
        public string Status { get; set; }
        public string SoNumber { get; set; }
        public string PoNumber { get; set; }
        public string AltItemNumber { get; set; }
        public int? RegQty { get; set; }
        public int? RecQty { get; set; }
    }
}
