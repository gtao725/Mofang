using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WMS.BLLClass
{
    public class ReceiptResult
    {
        public int Id { get; set; }
        public string WhCode { get; set; }
        public string ReceiptId { get; set; }
        public string TransportType { get; set; }
        public string TransportTypeExtend { get; set; }
        public int ClientId { get; set; }
        public string ClientCode { get; set; }
        public DateTime ReceiptDate { get; set; }
        public string Status { get; set; }
        public string SoNumber { get; set; }
        public string CustomerPoNumber { get; set; }
        public string PoNumber { get; set; }
        public string AltItemNumber { get; set; }
        public string Style1 { get; set; }
        public string Style2 { get; set; }
        public string Style3 { get; set; }

        public string ItemNumber { get; set; }
        public string HuId { get; set; }
        public int UnitId { get; set; }
        public string UnitName { get; set; }
        public string UnitNameShow { get; set; }
        public int Qty { get; set; }
        public decimal? Length { get; set; }
        public decimal? Width { get; set; }
        public decimal? Height { get; set; }
        public decimal? Weight { get; set; }
        public decimal? CBM { get; set; }
        public string LotNumber1 { get; set; }
        public string LotNumber2 { get; set; }
        public DateTime LotDate { get; set; }
        public string HoldReason { get; set; }
        public string LotFlagShow { get; set; }

        public string Custom1 { get; set; }
        public string Custom2 { get; set; }
        public string Custom3 { get; set; }

        public int LotFlag { get; set; }

        public string CreateUser { get; set; }

        public decimal? HuWeight { get; set; }
        public decimal? HuCbm { get; set; }
    }


    public class ReceiptSearch : BaseSearch
    {
        public string WhCode { get; set; }
        public string ReceiptId { get; set; }
        public string HuId { get; set; }
        public string SoNumber { get; set; }
        public string CustomerPoNumber { get; set; }
        public string AltItemNumber { get; set; }
        public string ClientCode { get; set; }
        public DateTime? ReceiptDateBegin { get; set; }
        public DateTime? ReceiptDateEnd { get; set; }
        public string CreateUser { get; set; }
    }


    public class ReceiptInsert
    {
        public string WhCode { get; set; }
        public int RegId { get; set; }
        public string ReceiptId { get; set; }
        public int ClientId { get; set; }
        public string ClientCode { get; set; }
        public DateTime ReceiptDate { get; set; }
        public string Status { get; set; }
        public string SoNumber { get; set; }
        public string CustomerPoNumber { get; set; }
        public int PoId { get; set; }
        public string HuId { get; set; }
        public decimal? HuLength { get; set; }
        public decimal? HuWidth { get; set; }
        public decimal? HuHeight { get; set; }
        public decimal? HuWeight { get; set; }
        public string Location { get; set; }
        public string TransportType { get; set; } 
        public string TransportTypeExtend { get; set; }
        public string TransportTypeEdit { get; set; }

        public int LotFlag { get; set; }
        public int? ProcessId { get; set; }
        public string CreateUser { get; set; }
        public DateTime CreateDate { get; set; }
        public string Remark { get; set; }

        //是否允许托盘重复使用,0:不可重复使用,1:可重复使用
        public int HuIdMultiplexingFlag { get; set; }

        public List<RecModeldetail> RecModeldetail;

        public HoldMasterModel HoldMasterModel;

        public List<WorkloadAccountModel> WorkloadAccountModel;

    }


    public class RecModeldetail
    {
        public string AltItemNumber { get; set; }
        public int ItemId { get; set; }
        public int UnitId { get; set; }
        public string UnitName { get; set; }
        public int Qty { get; set; }
        public decimal? Length { get; set; }
        public decimal? Width { get; set; }
        public decimal? Height { get; set; }
        public decimal? Weight { get; set; }
        public string LotNumber1 { get; set; }
        public string LotNumber2 { get; set; }
        public DateTime? LotDate { get; set; }
        public string Attribute1 { get; set; }

        public string Custom1 { get; set; }
        public string Custom2 { get; set; }
        public string Custom3 { get; set; }

        public List<SerialNumberInModel> SerialNumberInModel;
    }


    public class HoldMasterModel
    {
        public int HoldId { get; set; }
        public string HoldReason { get; set; }
    }

    public class WorkloadAccountModel
    {
        public string WorkType { get; set; }
        public string UserCode { get; set; }

    }

    public class WorkloadAccountModelCN
    {
        public string WorkType { get; set; }
        public string UserCode { get; set; }
        public string UserNameCN { get; set; }
    }


    public class SerialNumberInModel
    {
        public string CartonId { get; set; }
    }


    public class CheckRecModel
    {
        public int PoId { get; set; }
        public int ItemId { get; set; }
        public string UnitName { get; set; }
        public int Qty { get; set; }
    }


    public class EclRecModel
    {
        public string SoNumber { get; set; }
        public string CustomerPoNumber { get; set; }
        public string ClientCode { get; set; }
        public string AltItemNumber { get; set; }
        public string UnitName { get; set; }
        public string LotNumber1 { get; set; }
        public string LotNumber2 { get; set; }
        public DateTime? LotDate { get; set; }
        public int Qty { get; set; }
        public string ReceiptId { get; set; }

    }
    public class RecSoModel
    {
        public string WhCode { get; set; }
        public string ReceiptId { get; set; }
        public string SoNumber { get; set; }
        public string CustomerPoNumber { get; set; }
        public decimal? RecCBM { get; set; }
        public int RecQty { get; set; }

    }

}
