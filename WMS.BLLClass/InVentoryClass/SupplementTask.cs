using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WMS.BLLClass
{
    public class SupplementTaskCe
    {
        public string SupplementNumber { get; set; }
        public string UpdateUser { get; set; }
    }

    public class SupplementTaskDetailCe
    {
        public string SupplementNumber { get; set; }
        public string WhCode { get; set; }

        public int? HuDetailId { get; set; }
        public string HuId { get; set; }
        public string LocationId { get; set; }
        public string PutLocationId { get; set; }
        public string GroupNumber { get; set; }

        public int? ItemId { get; set; }
        public string AltItemNumber { get; set; }
        public string EAN { get; set; }

        public int Qty { get; set; }

        public string LotNumber1 { get; set; }

        public string LotNumber2 { get; set; }

        public DateTime? LotDate { get; set; }
        public string Status { get; set; }
    }

}
