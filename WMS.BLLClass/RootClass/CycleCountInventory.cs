using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WMS.BLLClass
{
    public class CycleCountInventorySearch : BaseSearch
    {
        public string AltItemNumber { get; set; }
        public string WhCode { get; set; }
        public string LocationId { get; set; }
        public string Location { get; set; }
        public string LocationColumn { get; set; }
        public int? LocationRowBegin { get; set; }
        public int? LocationRowEnd { get; set; }
    }

    public class CycleCountInventoryResult
    {
        public int? Id { get; set; }
        public string WhCode { get; set; }
        public string TaskNumber { get; set; }
        public string LocationId { get; set; }
        public string ClientCode { get; set; }
        public string HuId { get; set; }
        public string SoNumber { get; set; }
        public string CustomerPoNumber { get; set; }
        public string AltItemNumber { get; set; }
        public string LotNumber1 { get; set; }
        public string LotNumber2 { get; set; }
        public int? Qty { get; set; }
        public string Location { get; set; }
        public string LocationColumn { get; set; }
        public int? LocationRow { get; set; }
        public int? HoldId { get; set; }
        public string HoldReason { get; set; }
        public string Status { get; set; }
    }
}
