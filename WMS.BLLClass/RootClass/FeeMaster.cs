using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WMS.BLLClass
{
    public class FeeMasterSearch : BaseSearch
    {
        public string FeeNumber { get; set; }
        public string WhCode { get; set; }
        public string SoNumber { get; set; }
        public string Type { get; set; }
        public string Status { get; set; }
        public string ClientCode { get; set; }
        public string CreateUser { get; set; }
        public string InvoiceNumberOrderBy { get; set; }
        public DateTime? BeginCreateDate { get; set; }
        public DateTime? EndCreateDate { get; set; }
        public DateTime? BeginFeeCreateDate { get; set; }
        public DateTime? EndFeeCreateDate { get; set; }
    }

    public class FeeDetailSearch : BaseSearch
    {
        public string FeeNumber { get; set; }
        public string WhCode { get; set; }
        public string SoNumber { get; set; }
        public string PoNumber { get; set; }
        public string HuId { get; set; }
        public string LocationId { get; set; }
        public string ClientCode { get; set; }
        public string ReceiptId { get; set; } 
    }

    public class FeeDetailResult
    {
        public string FeeNumber { get; set; }
        public string WhCode { get; set; }
        public decimal? TotalPrice { get; set; }
    }

    public class FeeDetailHuDetailListResult
    {
        public string Action { get; set; }
        public string ClientCode { get; set; } 
        public string ReceiptId { get; set; }
        public string HuId { get; set; }
        public string LocationId { get; set; }
        public string SoNumber { get; set; }
        public string CustomerPoNumber { get; set; }
        public string AltItemNumber { get; set; }
        public string UnitName { get; set; }
        public int? Qty { get; set; }
        public string HoldReason { get; set; }
    }

    public class FeeDetailResult1
    {
        public string TCRProcessMode { get; set; }     
        public decimal? Price { get; set; }
        public int Qty { get; set; }
        public int OperationQty { get; set; }
        public decimal? OperationHours { get; set; }

        public decimal? OperationQtyFee { get; set; }
        public decimal? ChangDiFee { get; set; }
        public decimal? PeopleNormalFee { get; set; }
        public decimal? PeopleFujiaFee { get; set; }
        public decimal? EquipmentUseFee { get; set; }
        public decimal? PeopleNormalHours { get; set; }
        public decimal? PeopleNormalNightHours { get; set; }
        public decimal? PeopleWeekendHours { get; set; }
        public decimal? PeopleStatutoryHolidayHours { get; set; }
        public decimal? TruckFee { get; set; }
        public decimal? DaDanFee { get; set; }
        public decimal? OtherFee { get; set; }
        public decimal? TotalPrice { get; set; }
        public decimal? HSTotalPrice { get; set; }
    }
}
