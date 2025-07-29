using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WMS.BLLClass
{
    public class InterceptOrderRecResult
    {
        public string AltItemNumber { get; set; }
        public int ItemId { get; set; }
        public string UnitName { get; set; }
        public int UnitId { get; set; }
        public int Qty { get; set; }
    }
}
