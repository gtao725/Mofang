using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WMS.BLLClass
{

    public class RecSkuDataCe
    {
        public int ItemId { get; set; }       
        public string WhCode { get; set; }
        public string AltItemNumber { get; set; }
        public string EAN { get; set; }
        public string Style1 { get; set; }
        public string Style2 { get; set; }
        public string Style3 { get; set; }
        public string LotNumber1 { get; set; }
        public string LotNumber2 { get; set; }
        public DateTime? LotDate { get; set; }
        public int RecQty { get; set; }
        public int RegQty { get; set; }

        public decimal CBM { get; set; }

        public decimal Weight { get; set; }

    }
}
