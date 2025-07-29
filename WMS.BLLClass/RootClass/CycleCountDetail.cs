using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WMS.BLLClass
{
    public class CycleCountDetailSearch : BaseSearch
    {
        public string WhCode { get; set; }
        public string TaskNumber { get; set; }
        public string LocationId { get; set; }
        public string Status { get; set; }
        public string Action { get; set; }
    }


    public class CycleCountDetailResult
    {
        public int Id { get; set; }
        public string WhCode { get; set; }
        public string LocationId { get; set; }
        public string TaskNumber { get; set; }
        public string HuId { get; set; }
        public int? Qty { get; set; }
        public string Status { get; set; }
        public string CheckUser { get; set; }
        public DateTime? CheckDate { get; set; }
        public string Action { get; set; }

        public string Action1 { get; set; }

        public string CustomerPoNumber { get; set; }

        public string AltItemNumber { get; set; }
    }
}
