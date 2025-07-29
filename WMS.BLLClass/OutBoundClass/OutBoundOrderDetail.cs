using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WMS.BLLClass
{
    public class OutBoundOrderDetailSearch : BaseSearch
    {
        public int OutBoundOrderId { get; set; }
        public string WhCode { get; set; }

        public string CustomerPoNumber { get; set; }
        public string AltItemNumber { get; set; }

    }

    public class OutBoundOrderDetailResult
    {
        public int Id { get; set; }
        public string ClientCode { get; set; }
        public string SoNumber { get; set; }
        public string CustomerPoNumber { get; set; }
        public string AltItemNumber { get; set; }
        public int? ItemId { get; set; }
        public string Style1 { get; set; }
        public string Style2 { get; set; }
        public string Style3 { get; set; }
        public string UnitName { get; set; }
        public int Qty { get; set; }

        public int? Sequence { get; set; }
        public string LotNumber1 { get; set; }
        public string LotNumber2 { get; set; }
        public DateTime? LotDate { get; set; }

        public decimal? TotalCbm { get; set; }

        public string StowPosition { get; set; }
    }


    public class OutBoundOrderDetailInsert
    {
        public string WhCode { get; set; }
        public int OutBoundOrderId { get; set; }
        public int DSFlag { get; set; }
        public int? ClientId { get; set; }

        public List<OutBoundOrderDetailModel> OutBoundOrderDetailModel;
    }

    public class OutBoundOrderDetailModel
    {
        public int JsonId { get; set; }
        public string SoNumber { get; set; }
        public string CustomerPoNumber { get; set; }
        public string AltItemNumber { get; set; }
        public string Style1 { get; set; }
        public string Style2 { get; set; }
        public string Style3 { get; set; }
        public string LotNumber1 { get; set; }
        public string LotNumber2 { get; set; }
        public DateTime? LotDate { get; set; }
        public int UnitId { get; set; }
        public string UnitName { get; set; }
        public int Qty { get; set; }
        public string CreateUser { get; set; }
    }


    public class OutBoundOrderExtendInsert
    {
        public string WhCode { get; set; }
        public string ClientCode { get; set; }
        public string SoNumber { get; set; }
        public int Qty { get; set; }
        public string PlanQty { get; set; }
        public string ShippingOrigin { get; set; }
        public string CreateUser { get; set; }

        public List<OutBoundOrderDetailExtendModel> OutBoundOrderDetailExtendModel;
    }

    public class OutBoundOrderDetailExtendModel
    {
        public string SNNumber { get; set; }
    }
}
