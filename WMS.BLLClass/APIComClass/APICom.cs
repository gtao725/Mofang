using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WMS.BLLClass
{
    public class ReceiptData
    {
        public ReceiptHeadInfo receiptHeadInfo { get; set; }
        public List<ReceiptDetailInfo> receiptDetailInfoList { get; set; }
        public List<ReceiptWorkInfo> receiptWorkInfoList { get; set; }
    }
    public class ReceiptParam
    {
        public string ReceiptId { get; set; }
        public string TruckNumber { get; set; }
        public string WhCode { get; set; }
    }
    public class ReceiptHeadInfo
    {
        public string WhCode { get; set; }
        public string ReceiptId { get; set; }
        public string ClientCode { get; set; }
        public string RegisterDate { get; set; }
        public string ReceiptType { get; set; }
        public string TransportType { get; set; }
        public string TruckNumber { get; set; }
        public string TruckLength { get; set; }
        public string PhoneNumber { get; set; }
        public int? TruckStatus { get; set; }
        public int? GreenPassFLag { get; set; }
        public int? SumQty { get; set; }
        public decimal? SumCBM { get; set; }
        public int? RecSumQty { get; set; }
        public decimal? RecSumCBM { get; set; }
        public decimal? RecSumWeight { get; set; }
        public string BkDate { get; set; }
        public string BkDateBegin { get; set; }
        public string BkDateEnd { get; set; }
        public string ArriveDate { get; set; }
        public string ParkingUser { get; set; }
        public string ParkingDate { get; set; }
        public string BeginReceiptDate { get; set; }
        public string EndReceiptDate { get; set; }
        public int? ResetCount { get; set; }
        public int? TruckCount { get; set; }
        public int? BillCount { get; set; }
        public int? DNCount { get; set; }
        public string GoodType { get; set; }
        public string Remark { get; set; }
        public string CreateUser { get; set; }
        public string CreateDate { get; set; }
        public string UpdateUser { get; set; }
        public string UpdateDate { get; set; }
        public string BookOrigin { get; set; }
    }
    public class ReceiptDetailInfo
    {
        public string WhCode { get; set; }
        public string ReceiptId { get; set; }
        public string ClientCode { get; set; }
        public string ReceiptDate { get; set; }
        public string HoldReason { get; set; }
        public string SoNumber { get; set; }
        public string CustomerPoNumber { get; set; }
        public string AltItemNumber { get; set; }
        public string ItemNumber { get; set; }
        public string Style1 { get; set; }
        public string Style2 { get; set; }
        public string Style3 { get; set; }
        public string Remark1 { get; set; }
        public string HuId { get; set; }
        public string UnitName { get; set; }
        public int? Qty { get; set; }
        public decimal? Length { get; set; }
        public decimal? Width { get; set; }
        public decimal? Height { get; set; }
        public decimal? Weight { get; set; }
        public string LotNumber1 { get; set; }
        public string LotNumber2 { get; set; }
        public string LotDate { get; set; }
        public string Attribute1 { get; set; }
        public string CreateUser { get; set; }
        public string CreateDate { get; set; }
        public string UpdateUser { get; set; }
        public string UpdateDate { get; set; }
        public decimal? CBM { get; set; }
    }
    
    public class ReceiptWorkInfo
    {
        public string WhCode { get; set; }
        public string ReceiptId { get; set; }
        public string LoadId { get; set; }
        public string HuId { get; set; }
        public string WorkType { get; set; }
        public string WorkId { get; set; }
        public string CreateUser { get; set; }
        public string CreateDate { get; set; }
        public string UpdateUser { get; set; }
        public string UpdateDate { get; set; }
    }
}
