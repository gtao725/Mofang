using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WMS.BLLClass
{
    public class HuDetailResult
    {
        public int Id { get; set; }
        public string WhCode { get; set; }
        public string HuId { get; set; }
        public int? ClientId { get; set; }
        public string ClientCode { get; set; }

        public string SoNumber { get; set; }
        public string CustomerPoNumber { get; set; }
        public string AltItemNumber { get; set; }
        public int? ItemId { get; set; }
        public string UnitName { get; set; }
        public int? UnitId { get; set; }
        public DateTime ReceiptDate { get; set; }
        public int? Qty { get; set; }
        public int? PlanQty { get; set; }
        public decimal? Length { get; set; }
        public decimal? Width { get; set; }
        public decimal? Height { get; set; }
        public decimal? Weight { get; set; }
        public string LotNumber1 { get; set; }
        public string LotNumber2 { get; set; }
        public DateTime? LotDate { get; set; }
        public string Location { get; set; }
        public int HuMasterId { get; set; }
        public string UserName { get; set; }

        public int LocationTypeId { get; set; }
        public int? LocationTypeDetailId { get; set; }

        public int? HuIdOrderBy { get; set; }
    }
    public class HuDetailRemained : HuDetailResult
    {
        public string Style1 { get; set; }
        public string Style2 { get; set; }
        public string Style3 { get; set; }
        public List<SerialNumberModel> SerialNumberModel { get; set; }

    }

    public class HuInfo 
    {
        public int Id { get; set; }

        public string HuId { get; set; }
        
        public string WhCode { get; set; }
        public string Status { get; set; }
        public string Location { get; set; }
        public string HoldReason { get; set; }

        public int?  ClientId { get; set; }
        public string ClientCode { get; set; }

    }



    public class SerialNumberModel
    {
        public string CartonId { get; set; }
        public List<SerialNumberDetailModel> SerialNumberDetail { get; set; }
    }
    public class SerialNumberDetailModel
    {
        public string SNType { get; set; }
        public int PCS { get; set; }
        public string UPC { get; set; }
    }

    public class HuDetailInsert
    {
        public string ReceiptId { get; set; }
        public string WhCode { get; set; }
        public string HuId { get; set; }
        public int? ClientId { get; set; }
        public string ClientCode { get; set; }
        public string SoNumber { get; set; }
        public string CustomerPoNumber { get; set; }
        public string AltItemNumber { get; set; }
        public int? ItemId { get; set; }
        public string UnitName { get; set; }
        public int UnitFlag { get; set; }
        public int? UnitId { get; set; }
        public DateTime ReceiptDate { get; set; }
        public int? Qty { get; set; }
        public decimal? Length { get; set; }
        public decimal? Width { get; set; }
        public decimal? Height { get; set; }
        public decimal? Weight { get; set; }
        public string LotNumber1 { get; set; }
        public string LotNumber2 { get; set; }
        public DateTime? LotDate { get; set; }
        public string Location { get; set; }
        public string UserName { get; set; }
    }

    public class EditHuDetailLotEntity
    {
        public string WhCode { get; set; }
        public string HuId { get; set; }
        public string ClientCode { get; set; }
        
        public string ClientSystemNumber { get; set; }

        public string AltItemNumber { get; set; }
        public string LotNumber1 { get; set; }
        public string LotNumber2 { get; set; }
        public DateTime? LotDate { get; set; }
        public string UserName { get; set; }
        public int Qty { get; set; }
        public string NewLotNumber1 { get; set; }
        public string NewLotNumber2 { get; set; }
        public DateTime? NewLotDate { get; set; }
    }


    public class HuDetailResult1
    {
        public int Id { get; set; }
        public string WhCode { get; set; }
        public string HuId { get; set; }
        public int? ClientId { get; set; }
        public string ClientCode { get; set; }

        public string SoNumber { get; set; }
        public string CustomerPoNumber { get; set; }
        public string AltItemNumber { get; set; }
        public int? ItemId { get; set; }
        public string UnitName { get; set; }
        public int? UnitId { get; set; }
        public DateTime ReceiptDate { get; set; }
        public int? Qty { get; set; }
        public int? PlanQty { get; set; }
        public decimal? Length { get; set; }
        public decimal? Width { get; set; }
        public decimal? Height { get; set; }
        public decimal? Weight { get; set; }
        public string LotNumber1 { get; set; }
        public string LotNumber2 { get; set; }
        public DateTime? LotDate { get; set; }
        public string Location { get; set; }
        public int HuMasterId { get; set; }
        public string UserName { get; set; }

        public int LocationTypeId { get; set; }
        public int? LocationTypeDetailId { get; set; }

        public int? HuIdOrderBy { get; set; }
        public string Type { get; set; }
        public string Status { get; set; }
    }
 

    public class HuMasterResult
    {
 
        public string WhCode { get; set; }
        public string HuId { get; set; }
 
        public decimal? HuLength { get; set; }
        public decimal? HuWidth { get; set; }
        public decimal? HuHeight { get; set; }
        public decimal? HuWeight { get; set; }
         
    }

    public class HuMasterResult1
    {
        public string Show { get; set; }

        public string WhCode { get; set; }
        public string ClientCode { get; set; }

        public string SoNumber { get; set; }
        public string PoNumber { get; set; }
        public string ItemNumber { get; set; }

        public string ReceiptId { get; set; }
        public string HuId { get; set; }
        public string LocationId { get; set; }

        public decimal? HuLength { get; set; }
        public decimal? HuWidth { get; set; }
        public decimal? HuHeight { get; set; }
        public decimal? HuWeight { get; set; }

        public decimal? recCBM { get; set; }
        public decimal? Length { get; set; }
        public decimal? Width { get; set; }
        public decimal? Height { get; set; }
        public int Qty { get; set; }
        public string UpdateUser { get; set; }
        public DateTime? ReceiptDate { get; set; }
    }


    public class HuMasterSearch1 : BaseSearch
    {
        public string WhCode { get; set; }
        public string HuId { get; set; }

        public string ClientCode { get; set; }
        public string SoNumber { get; set; }
        public string CustomerPoNumber { get; set; }
        public string AltItemNumber { get; set; }
        public string ReceiptId { get; set; }
        public DateTime? BeginReceiptDate { get; set; }
        public DateTime? EndReceiptDate { get; set; }

    }

}
