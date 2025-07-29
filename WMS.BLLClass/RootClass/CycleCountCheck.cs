using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WMS.BLLClass
{
    public class CycleCountCheckSearch : BaseSearch
    {
        public string WhCode { get; set; }
        public string TaskNumber { get; set; }

        public string LocationId { get; set; }
    }
    public class CycleCountCheckResult
    {
        public string WhCode { get; set; }
        public string TaskNumber { get; set; }
        public string LocationId { get; set; }
        public string Inv_HuId { get; set; }
        public string Inv_CustomerPoNumber { get; set; }
        public string Inv_AltItemNumber { get; set; }
        public string Inv_LotNumber1 { get; set; }
        public string Inv_LotNumber2  { get; set; }
        public int? Inv_Qty { get; set; }
        public string Che_HuId { get; set; }
        public string Che_CustomerPoNumber { get; set; }
        public string Che_AltItemNumber { get; set; }
        public int? Che_Qty { get; set; }
    }
}
