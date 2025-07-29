using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WMS.BLLClass
{
    public class SortTaskDetailResult
    {
        public string LoadId { get; set; }
        public string WhCode { get; set; }
        public string OutPoNumber { get; set; }
        public string AltItemNumber { get; set; }
        public string EAN { get; set; }
        public int? ItemId { get; set; }
        public int? HandFlag { get; set; }
        public int? ScanFlag { get; set; }
        public string ScanRule { get; set; }
        public int? PlanQty { get; set; }
        public int? Qty { get; set; }
        public int? PackQty { get; set; }
        public int? GroupId { get; set; }

        public string GroupNumber { get; set; }

        public string Status { get; set; }
        public int? HoldQty { get; set; }
    }

    public class SortTaskDetailSearch : BaseSearch
    {
        public string LoadId { get; set; }
        public string AltItemNumber { get; set; }
        public string SortGroupNumber { get; set; }
        public string WhCode { get; set; }
        public DateTime? CreateDateBegin { get; set; }
        public DateTime? CreateDateEnd { get; set; }
    }


    public class SortTaskDetailSelectResult
    {
        public string LoadId { get; set; }
        public string OutPoNumber { get; set; }
        public string AltItemNumber { get; set; }
        public string EAN { get; set; }
        public int? PlanQty { get; set; }
        public int? Qty { get; set; }
        public int? GroupId { get; set; }
        public string GroupNumber { get; set; }
        public string Status { get; set; }
        public DateTime? CreateDate { get; set; }
    }

    public class LoadProcedureResult
    {
        public string LoadId { get; set; }
        public string ClientCode { get; set; }
        public string AltCustomerOutPoNumber { get; set; }
        public string CustomerOutPoNumber { get; set; }
        public string StatusName { get; set; }
        public string AltItemNumber { get; set; }
        public string EAN { get; set; }
        public int? PlanQty { get; set; }
        public int? PickQty { get; set; }
        public int? Qty { get; set; }
        public int? PackQty { get; set; }
        public int? TransferQty { get; set; }
        public string SortGroupNumber { get; set; }
        public string UpdateUser { get; set; }
        public DateTime? UpdateDate { get; set; }
        public string CreateUser { get; set; }
        public DateTime? CreateDate { get; set; }
        public DateTime? BeginPickDate { get; set; }
        public DateTime? EndPickDate { get; set; }
        public DateTime? BeginPackDate { get; set; }
        public DateTime? EndPackDate { get; set; }
        public DateTime? BeginSortDate { get; set; }
        public DateTime? EndSortDate { get; set; }
        public DateTime? ShipDate { get; set; }

        public int? LoadSumQty { get; set; }

        public string ExpressNumber { get; set; }
        public string ExpressMessage { get; set; }

        public DateTime? OrderDate { get; set; }
        public string OrderType { get; set; }
        public string OrderSource { get; set; }
        public string ProcessName { get; set; }

        public string ItemName { get; set; }

    }

    public class LoadProcedureSearch : BaseSearch
    {
        public string WhCode { get; set; }
        public string LoadId { get; set; }
        public string ClientCode { get; set; }
        public string AltCustomerOutPoNumber { get; set; }
        public string CustomerOutPoNumber { get; set; }
        public string AltItemNumber { get; set; }
        public string EAN { get; set; }
        public DateTime? BeginCreateDate { get; set; }
        public DateTime? EndCreateDate { get; set; }
        public string StatusName { get; set; }

        public DateTime? BeginOrderDate { get; set; }
        public DateTime? EndOrderDate { get; set; }
    }
}
