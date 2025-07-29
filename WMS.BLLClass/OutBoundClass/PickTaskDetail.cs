using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WMS.BLLClass
{
    public class PickTaskDetailResult
    {
        public int Id { get; set; }
        public string WhCode { get; set; }
        public string LoadId { get; set; }
        public Nullable<int> OutBoundOrderId { get; set; }
        public string CustomerOutPoNumber { get; set; }
        public string AltItemNumber { get; set; }
        public Nullable<int> PickQty { get; set; }

        public List<PackScanNumberInsert> PackScanNumber;

    }



    public class PickTaskDetailWince
    {
        public int Id { get; set; }
        public string WhCode { get; set; }
        public string LoadId { get; set; }
        public Nullable<int> OutBoundOrderId { get; set; }
        public string HuId { get; set; }
        public string Location { get; set; }
        public string SoNumber { get; set; }
        public string CustomerPoNumber { get; set; }
        public string AltItemNumber { get; set; }
        public int ItemId { get; set; }
        public int Qty { get; set; }
        public Nullable<int> PickQty { get; set; }
        public Nullable<int> Sequence { get; set; }
        public Nullable<int> HandFlag { get; set; }
        public Nullable<int> ScanFlag { get; set; }
        public string ScanRule { get; set; }
        public string EAN { get; set; }

    }

    public class PickTaskDetailSumQtyResult
    {
        public string LoadId { get; set; }

        public int? PlanQty { get; set; }

        public int? PickQty { get; set; }

    }

    public class PickTaskDetailResult1
    {
        public string WhCode { get; set; }
        public string LoadId { get; set; }
        public int? Qty { get; set; }
        public decimal? CBM { get; set; }
        public string UnitName { get; set; }
        public int? Days { get; set; }



    }


    public class PickTaskDetailSearch : BaseSearch
    {
        public string WhCode { get; set; }
        public string LoadId { get; set; }
        public string HuId { get; set; }

    }

    public class PickTaskDetailResult2
    {
        public int? OutBoundOrderId { get; set; }
        public string Action1 { get; set; }
        public string Action2 { get; set; }
        public string Action3 { get; set; }
        public string LoadId { get; set; }
        public string HuId { get; set; }
        public string WhCode { get; set; }
        public string ClientCode { get; set; }
        public string SoNumber { get; set; }
        public string CustomerPoNumber { get; set; }
        public string AltItemNumber { get; set; }
        public string UnitName { get; set; }
        public int? UnitId { get; set; }
        public int ItemId { get; set; }
        public int? Qty { get; set; }
        public string Status { get; set; }
        public string Status1 { get; set; }
        public string LotNumber1 { get; set; }
        public string LotNumber2 { get; set; }

        public string CreateUser { get; set; }
    }

}
