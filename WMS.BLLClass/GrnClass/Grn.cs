using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WMS.BLLClass
{
    public class GrnHeadSearch : BaseSearch
    {
        public string WhCode { get; set; }
        public string ClientCode { get; set; }
        public string So { get; set; }
        public string[] SoL { get; set; }
        public string recstatus { get; set; }
        public string sendstatus { get; set; }
        public string receiptid { get; set; }
        public DateTime? BeginCreateDate { get; set; }
        public DateTime? EndCreateDate { get; set; }

    }

    public class GrnHeadResult
    {
        public int Id { get; set; }
        public string WhCode { get; set; }
        public string ClientCode { get; set; }
        public string SoNumber { get; set; }

        public string SendType { get; set; }
        public string Status { get; set; }
        public DateTime? SendTime { get; set; }
        public int? GWI_Qty { get; set; }
        public Double? GWI_Cbm { get; set; }
        public Double? GWI_Kgs { get; set; }
        public int? WmsQty { get; set; }
        public Double? WmsCbm { get; set; }
        //public Double? WMS_Kgs { get; set; }
        public int? GRN_Qty { get; set; }
        public Double? GRN_Cbm { get; set; }
        public Double? GRN_Kgs { get; set; }
        public DateTime? CreateDate { get; set; }

    }

    public class GrnSoUpdateSearch
    {
        public string ClientCode { get; set; }
        public string SoNumber { get; set; }
        public string WhCode { get; set; }

    }
}
