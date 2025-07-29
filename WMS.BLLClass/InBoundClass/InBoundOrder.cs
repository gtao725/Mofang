using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WMS.BLLClass
{
    public class InBoundOrderResult
    {
        public string Action { get; set; }
        public string Action1 { get; set; }
        public int Id { get; set; }
        public string WhCode { get; set; }
        public int ClientId { get; set; }
        public string ClientCode { get; set; }
        public string ForwarderCode { get; set; }
        public string SoNumber { get; set; }
        public string CustomerPoNumber { get; set; }
        public int? ProcessId { get; set; }
        public string ProcessName { get; set; }
        public string OrderType { get; set; }
        public int TotalQty { get; set; }
        public int TotalRegQty { get; set; }
        public string CreateUser { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime PlanInTime { get; set; }
        public string UpdateUser { get; set; }
        public DateTime UpdateDate { get; set; }
    }
    public class InBoundOrderSearch : BaseSearch
    {
        public string WhCode { get; set; }
        public string ClientCode { get; set; }
        public string ForwarderCode { get; set; }
        public string SoNumber { get; set; }
        public string CustomerPoNumber { get; set; }
        public DateTime? BeginCreateDate { get; set; }
        public DateTime? EndCreateDate { get; set; }
        public DateTime? BeginPlanInTime { get; set; }
        public DateTime? EndPlanInTime { get; set; }
    }

    public class ImportsInBoundOrderInsert
    {
            public string whCode  { get; set; }
            public string ClientCode  { get; set; }
            public string OrderType { get; set; }
            public string SiteId { get; set; }
            public string CustomerId { get; set; }
        public int? ProcessId  { get; set; }
            public string ProcessName   { get; set; }
            public string SoNumber   { get; set; }
            public string CustomerPoNumber  { get; set; }
            public string AltItemNumber   { get; set; }
            public string Style1  { get; set; }
            public string Style2  { get; set; }
            public string Style3   { get; set; }
            public int? Qty  { get; set; }
            public Decimal? CBM    { get; set; }
            public string UnitName { get; set; }
            public Decimal? Weight   { get; set; }
            public string CreateDate { get; set; }
            public string CreateUser  { get; set; }

    }

    public class InBoundOrderInsert
    {
        public string WhCode { get; set; }
        public int? ClientId { get; set; }
        public string ClientCode { get; set; }
        public string LocationId { get; set; }
        public string SoNumber { get; set; }
        public string OrderType { get; set; }
        public string OrderSource { get; set; }
        public int? ProcessId { get; set; }
        public string ProcessName { get; set; }
        public string CreateUser { get; set; }
        public string TruckNumber { get; set; }
        public string TruckLength { get; set; } //add by yangxin 2023-02-17
        public string BookOrigin { get; set; } //add by yangxin 2023-05-12 预约订单来源
        public int? TruckCount { get; set; }
        public int? DNCount { get; set; }
        public int? FaxInCount { get; set; }
        public int? FaxOutCount { get; set; }
        public int? BillCount { get; set; }
        public int? GreenPassFlag { get; set; }

        public string PhoneNumber { get; set; }
        public string BkDate { get; set; }

        public string GoodType { get; set; }

        public List<InBoundOrderDetailInsert> InBoundOrderDetailInsert;
    }

    public class InBoundOrderDetailInsert
    {
        public int JsonId { get; set; }
        public string AltCustomerPoNumber { get; set; }
        public string CustomerPoNumber { get; set; }
        public string AltItemNumber { get; set; }
        public string SSID { get; set; }
        public string Style1 { get; set; }
        public string Style2 { get; set; }
        public string Style3 { get; set; }
        public int UnitId { get; set; }
        public string UnitName { get; set; }

        public DateTime? CustomDate { get; set; }
        public string CustomNumber1 { get; set; }

        public int Qty { get; set; }
        public Decimal? Weight { get; set; }
        public Decimal? CBM { get; set; }
        public string CreateUser { get; set; }
        public int ItemId { get; set; }
        public string CheckAltItemNumberFlag { get; set; }

        public List<InBoundOrderDetailSNInsert> InBoundOrderDetailSNInsert;
    }

    public class InBoundOrderDetailSNInsert
    {
        public string SN { get; set; }
    }
}
