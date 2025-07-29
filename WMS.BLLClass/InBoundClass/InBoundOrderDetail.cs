using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WMS.BLLClass
{
    public class InBoundOrderDetailResult
    {
        public int? Id { get; set; }
        public string WhCode { get; set; }
        public string SoNumber { get; set; }
        public string CustomerPoNumber { get; set; }
        public string AltItemNumber { get; set; }
        public string Style1 { get; set; }
        public string Style2 { get; set; }
        public string Style3 { get; set; }
        public int? Qty { get; set; }
        public int? RegQty { get; set; }
        public Decimal? Weight { get; set; }
        public Decimal? CBM { get; set; }
        public int PoId { get; set; }
        public int ItemId { get; set; }
        public int? ClientId { get; set; }
        public string ClientCode { get; set; }
        public int? ProcessId { get; set; }
        public string ProcessName { get; set; }

        public int? UnitId { get; set; }
        public string UnitName { get; set; }

    }
    public class InBoundOrderDetailSearch : BaseSearch
    {
        public string WhCode { get; set; }
        public int? ClientId { get; set; }
        public string SoNumber { get; set; }
        public string CustomerPoNumber { get; set; }
        public string AltItemNumber { get; set; }
        public string Style1 { get; set; }
        public string Style2 { get; set; }
        public string Style3 { get; set; }
        public string OrderType { get; set; }
        public int? ProcessId { get; set; }
    }
}
