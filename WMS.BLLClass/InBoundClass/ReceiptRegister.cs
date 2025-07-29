using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WMS.BLLClass
{
    public class ReceiptRegisterInsert
    {
        public int InBoundOrderDetailId { get; set; }
        public string ReceiptId { get; set; }
        public string WhCode { get; set; }
        public string CustomerPoNumber { get; set; }
        public string AltItemNumber { get; set; }
        public int PoId { get; set; }
        public int ItemId { get; set; }
        public int RegQty { get; set; }
        public string CreateUser { get; set; }
        public DateTime CreateDate { get; set; }
        public int UnitId { get; set; }
        public string UnitName { get; set; }

        public int ProcessId { get; set; }
        public string ProcessName { get; set; }
    }

    public class ReceiptRegisterResult
    {
        public string Action { get; set; }
        public string Action1 { get; set; }
        public string Action2 { get; set; }
        public int Id { get; set; }
        public string WhCode { get; set; }
        public string ReceiptId { get; set; }
        public string ReceiptType { get; set; }
        public int ClientId { get; set; }
        public string ClientCode { get; set; }
        public DateTime? RegisterDate { get; set; }
        public string Status { get; set; }
        public string ProcessName { get; set; }
        public string TransportType { get; set; }
        public string TruckNumber { get; set; }
        public string PhoneNumber { get; set; }
        public DateTime? ArriveDate { get; set; }
        public string LocationId { get; set; }
        public string CreateUser { get; set; }
        public DateTime? CreateDate { get; set; }
        public string DSFlag { get; set; }
        public string QueueUpFLag { get; set; }
        public int? SumQty { get; set; }
        public decimal? SumCBM { get; set; }
        public string BkDate { get; set; }
        public int? TruckStatus { get; set; }
        public string TruckStatusShow { get; set; }
        public string QueueUpUser { get; set; }
        public string QueueUpRemark { get; set; }
        public string BaoAnRemark { get; set; }
        public DateTime? BkDateBegin { get; set; }
        public DateTime? BkDateEnd { get; set; }
        public string ParkingUser { get; set; }
        public DateTime? ParkingDate { get; set; }
        public DateTime? StorageDate { get; set; }
        public DateTime? DepartureDate { get; set; }
        public string AgentCode { get; set; }
        public int Sequence { get; set; }
        public double? BkHours { get; set; }
        public string OneTruckMoreNumber { get; set; }

        public int? GreenPassFlag { get; set; }
        public string GreenPassFlagShow { get; set; }

        public int? TruckCountShow { get; set; }
        public int? DNCountShow { get; set; }
        public int? FaxInCountShow { get; set; }
        public int? FaxOutCountShow { get; set; }
        public int? BillCountShow { get; set; }
        public int? ResetCountShow { get; set; }
        public string GoodType { get; set; }

    }
    public class ReceiptRegisterSearch : BaseSearch
    {
        public string WhCode { get; set; }
        public int? ClientId { get; set; }
        public string ReceiptId { get; set; }
        public string ReceiptType { get; set; }
        public string TruckNumber { get; set; }
        public string PhoneNumber { get; set; }
        public DateTime? BeginCreateDate { get; set; }
        public DateTime? EndCreateDate { get; set; }
        public string Status { get; set; }
        public string TruckStatus { get; set; }
        public string LocationId { get; set; }
        public string ClientCode { get; set; }
        public string AgentCode { get; set; }

        public string CreateUser { get; set; }
    }

    public class DSReceiptRegisterResult
    {
        public int OutBoundOrderId { get; set; }
        public int LoadMasterId { get; set; }
        public int InBoundOrderDetailId { get; set; }
        public int ClientId { get; set; }
        public string ClientCode { get; set; }
        public int Qty { get; set; }
    }

    public class EditInBoundResult
    {
        public string Action { get; set; }
        public string ReceiptId { get; set; }
        public int ClientId { get; set; }
        public string ClientCode { get; set; }
        public string SoNumber { get; set; }
        public string CustomerPoNumber { get; set; }
        public string AltItemNumber { get; set; }
        public string Style1 { get; set; }
        public string Style2 { get; set; }
        public string Style3 { get; set; }
        public DateTime? RegisterDate { get; set; }
        public DateTime? ReceiptDate { get; set; }
        public string Status { get; set; }
        public int? RegQty { get; set; }
        public int? RecQty { get; set; }
        public string WhCode { get; set; }
        public string UserCode { get; set; }
        public string UnitName { get; set; }
        public string HuId { get; set; }
    }

    public class EditInBoundDetailResult
    {
        public int RegDetailId { get; set; }
        public string ReceiptId { get; set; }
        public string ClientCode { get; set; }
        public string SoNumber { get; set; }
        public int SoId { get; set; }
        public string CustomerPoNumber { get; set; }
        public int PoId { get; set; }
        public string AltItemNumber { get; set; }
        public string Style1 { get; set; }
        public string Style2 { get; set; }
        public string Style3 { get; set; }
        public int ItemId { get; set; }
        public string UnitName { get; set; }
        public int UnitId { get; set; }
        public int? Qty { get; set; }
        public int RegQty { get; set; }
        public string OrderType { get; set; }

        public int InOrderDetailId { get; set; }
        public int? ProcessId { get; set; }
        public string ProcessName { get; set; }
    }

    public class EditInBoundSearch : BaseSearch
    {
        public string WhCode { get; set; }
        public string ReceiptId { get; set; }
        public int ClientId { get; set; }
        public string ClientCode { get; set; }
        public string SoNumber { get; set; }
        public DateTime? RegisterDate { get; set; }
        public string Status { get; set; }
        public string CreateUser { get; set; }
        public DateTime? CreateDate { get; set; }
        public string HuId { get; set; }
        public string CustomerPoNumber { get; set; }
        public string AltItemNumber { get; set; }
    }
}
