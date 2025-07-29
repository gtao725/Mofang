using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WMS.BLLClass
{
    public class GwiDetailInsert
    {
        public string WhCode { get; set; }
        public string ClientCode { get; set; }

        public string SiteId { get; set; }
        public string CustomerId { get; set; }
        public string SoCode { get; set; }
        public string SoDigit { get; set; }

        public int LN { get; set; }
        public string SO { get; set; }
        public string PO { get; set; }
        public string SKU { get; set; }
        public string STYLE { get; set; }
        public int? GWI_Pcs { get; set; }
        public int? GWI_Qty { get; set; }
        public float? GWI_Cbm { get; set; }
        public float? GWI_Kgs { get; set; }
        public float? GWI_nwKgs { get; set; }
        public string CreateUser { get; set; }
        public DateTime CreateDate { get; set; }
    }
}
