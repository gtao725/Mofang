using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WMS.BLLClass
{
    public class InVentoryResult
    {
        public int HuMasterId { get; set; }
        public int HuDetailId { get; set; }
        public string ReceiptId { get; set; }
        public int? ClientId { get; set; }
        public string ClientCode { get; set; }
        public string WhCode { get; set; }
        public string Location { get; set; }
        public DateTime ReceiptDate { get; set; }
        public string SoNumber { get; set; }
        public string CustomerPoNumber { get; set; }
        public string AltItemNumber { get; set; }
        public int? ItemId { get; set; }
        public string Style1 { get; set; }
        public string Style2 { get; set; }
        public string Style3 { get; set; }
        public string UnitName { get; set; }
        public int Qty { get; set; }
        public int? PlanQty { get; set; }
        public decimal? Length { get; set; }
        public decimal? Width { get; set; }
        public decimal? Height { get; set; }
        public decimal? Weight { get; set; }
        public decimal? CBM { get; set; }
        public int? Doi { get; set; }
        public string HuId { get; set; }

        public string Type { get; set; }
        public string Status { get; set; }

        public string TypeShow { get; set; }
        public string StatusShow { get; set; }
        public string LocationShow { get; set; }
        public int? HoldId { get; set; }
        public string HoldReason { get; set; }
        public string LotNumber1 { get; set; }
        public string LotNumber2 { get; set; }
        public DateTime? LotDate { get; set; }
        public int? LocationTypeId { get; set; }
        public string PickDetailShow { get; set; }
        public string UnitNameShow { get; set; }
    }

    public class InVentorySearch : BaseSearch
    {
        public string WhCode { get; set; }
        public string SoNumber { get; set; }
        public string CustomerPoNumber { get; set; }
        public string AltItemNumber { get; set; }

        public string LotNumber1 { get; set; }

        public string LotNumber2 { get; set; }

        public DateTime? LotDate { get; set; }

        public int? ClientId { get; set; }
        public string ReceiptId { get; set; }
        public string HuId { get; set; }
        public string Type { get; set; }

        public string HoldReason { get; set; }

        public string LocationId { get; set; }

        public string LocationId1 { get; set; }
        public int LocationTypeId { get; set; }

        public DateTime? BeginReceiptDate { get; set; }

        public DateTime? EndReceiptDate { get; set; }
        public int Qty { get; set; }
    }
    public class ZoneNameSearch
    {
        public string ZoneName { get; set; }
        public int? ZoneId { get; set; }

        public int? ZoneOrderBy { get; set; }
        public int? OnlySkuFlag { get; set; }
        public int? MaxLocationIdQty { get; set; }

        public int? MaxPallateQty { get; set; }
        public int? InvClearUpSkuMaxQty { get; set; }

        public int? SumQty { get; set; }
        public int? OnHandQty { get; set; }

        public int? ItemId { get; set; }
        public string res { get; set; }
    }


    public class InvMoveDetailAdd : BaseSearch
    {
        public string MoveNumEdit { get; set; }
        public string Location { get; set; }
        public string WhCode { get; set; }
        public string MoveNum { get; set; }
        public string ClientCode { get; set; }
        public string SoNumber { get; set; }
        public string CustomerPoNumber { get; set; }
        public string AltItemNumber { get; set; }
        public int ItemId { get; set; }
        public string HuId { get; set; }
        public int ZoneId { get; set; }

        public string ZoneName { get; set; }
        public int DesZoneId { get; set; }
        public string DesZoneName { get; set; }
        public string CreateUser { get; set; }
        public Nullable<System.DateTime> CreateDate { get; set; }
        public Nullable<System.DateTime> BeginDate { get; set; }
        public Nullable<System.DateTime> EndDate { get; set; }

        public int UpZoneId { get; set; }
    }


    public class InvMoveDetailSearch : BaseSearch
    {
        public string MoveNumEdit { get; set; }
        public string Location { get; set; }
        public string WhCode { get; set; }
        public string MoveNum { get; set; }
        public string ClientCode { get; set; }
        public string SoNumber { get; set; }
        public string CustomerPoNumber { get; set; }
        public string AltItemNumber { get; set; }
        public int ItemId { get; set; }
        public string HuId { get; set; }
        public int ZoneId { get; set; }
        public string ZoneName { get; set; }
        public int DesZoneId { get; set; }
        public string DesZoneName { get; set; }
        public string CreateUser { get; set; }
        public Nullable<System.DateTime> CreateDate { get; set; }
        public Nullable<System.DateTime> BeginDate { get; set; }
        public Nullable<System.DateTime> EndDate { get; set; }
        public int DetailFlag { get; set; }
    }

    public class InvMoveDetailResult
    {
        public string MoveNumEdit { get; set; }
        public string Location { get; set; }
        public string WhCode { get; set; }
        public string MoveNum { get; set; }
        public string ClientCode { get; set; }
        public string SoNumber { get; set; }
        public string CustomerPoNumber { get; set; }
        public string AltItemNumber { get; set; }
        public int ItemId { get; set; }
        public string HuId { get; set; }
        public int ZoneId { get; set; }
        public string ZoneName { get; set; }
        public int DesZoneId { get; set; }
        public string DesZoneName { get; set; }
        public string CreateUser { get; set; }
        public Nullable<System.DateTime> CreateDate { get; set; }
        public Nullable<System.DateTime> BeginDate { get; set; }
        public Nullable<System.DateTime> EndDate { get; set; }
        public int DetailFlag { get; set; }
    }



    public class ItemZone
    {
        public string ZoneName { get; set; }
        public string LocationId { get; set; }
        public int? ZoneId { get; set; }
        public int? OnhandQty { get; set; }
        public int? RemainingQty { get; set; }


    }
}
