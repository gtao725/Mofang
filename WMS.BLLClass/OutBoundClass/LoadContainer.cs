using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WMS.BLLClass
{
    public class LoadContainerSearch : BaseSearch
    {
        public string WhCode { get; set; }
        public string LoadId { get; set; }
        public string ClientCode { get; set; }
        public string BillNumber { get; set; }
        public string ContainerNumber { get; set; }
        public string ContainerNumberSix { get; set; }
        public string SealNumber { get; set; }
        public DateTime? BeginETD { get; set; }
        public DateTime? EndETD { get; set; }
        public DateTime? BeginCreateDate { get; set; }
        public DateTime? EndCreateDate { get; set; }
        public string CreateUser { get; set; }
        public string ChuCangFS { get; set; }

        public string Status0 { get; set; }
        public string Status1 { get; set; }
        public string Status3 { get; set; }
        public string ShipStatus { get; set; }
        public string LoadChargeStatus { get; set; }
        public string VesselName { get; set; }

    }

    public class LoadContainerResult
    {
        public int Id { get; set; }
        public string Action1 { get; set; }
        public string Action2 { get; set; }
        public string Action3 { get; set; }
        public string Action4 { get; set; }
        public int? LoadMasterId { get; set; }
        public string ChuCangFS { get; set; }
        public string LoadId { get; set; }
        public string ClientCode { get; set; }
        public string Status0 { get; set; }
        public string Status1 { get; set; }
        public int? ProcessId { get; set; }
        public string ProcessName { get; set; }
        public string VesselName { get; set; }
        public string VesselNumber { get; set; }
        public string CarriageName { get; set; }
        public DateTime? ETD { get; set; }
        public string ContainerType { get; set; }
        public string ContainerName { get; set; }
        public string PortSuticase { get; set; }
        public string Port { get; set; }
        public string DeliveryPlace { get; set; }
        public string BillNumber { get; set; }
        public string ContainerNumber { get; set; }
        public string SealNumber { get; set; }
        public string ContainerSource { get; set; }
        public string CreateUserName { get; set; }
        public string CreateUser { get; set; }
        public DateTime? CreateDate { get; set; }
        public int? LoadContainerHuDetailId { get; set; }
        public int? SumQty { get; set; }
        public int? DSSumQty { get; set; }
        public int? EchQty { get; set; }
        public decimal? SumCBM { get; set; }
        public decimal? SumWeight { get; set; }

        public string Status3 { get; set; }
        public string ShipStatus { get; set; }
        public string WeightFlag { get; set; }
        public string Remark { get; set; }

        public DateTime? ReleaseDate { get; set; }
        public DateTime? BeginPickDate { get; set; }
        public DateTime? EndPickDate { get; set; }
        public DateTime? BeginPackDate { get; set; }
        public DateTime? EndPackDate { get; set; }
        public DateTime? BeginSortDate { get; set; }
        public DateTime? EndSortDate { get; set; }
        public DateTime? ShipDate { get; set; }

        public string LoadChargeStatus { get; set; }
        public string ClientContractNameOut { get; set; }

        public int? PlanQty { get; set; }
        public string ShippingOrigin { get; set; }

        public string ETDShow { get; set; }
    }

    public class CreateUserResult
    {
        public string CreateUserName { get; set; }
        public string CreateUser { get; set; }

    }


    public class LoadHuIdExtendSearch : BaseSearch
    {
        public string WhCode { get; set; }
        public string LoadId { get; set; }
        public string HuId { get; set; }
    }


    public class ImportLoadContainerResult
    {
        public string WhCode { get; set; }
        public string VesselName { get; set; }
        public string VesselNumber { get; set; }
        public string CarriageName { get; set; }
        public DateTime? ETD { get; set; }
        public string ContainerType { get; set; }
        public string ContainerName { get; set; }
        public string Port { get; set; }
        public string DeliveryPlace { get; set; }
        public string BillNumber { get; set; }
        public string ContainerNumber { get; set; }
        public string SealNumber { get; set; }

        public string DischageCode { get; set; }
        public string PortSuitcase { get; set; }
        public string SoNumber { get; set; }
        public string CaoZuoUser { get; set; }
        public string SystemNumber { get; set; }
        public string LadingNumber { get; set; }
        public int? Qty { get; set; }
        public decimal? CBM { get; set; }
        public decimal? Weight { get; set; }
        public string CreateUserName { get; set; }
        public string CreateUser { get; set; }
        public DateTime? CreateDate { get; set; }
        public string WeightFlag { get; set; }

    }

    public class LoadChargeRuleResult
    {
        public int Id { get; set; }
        public int? FunctionId { get; set; }
        public string FunctionName { get; set; }
        public string FunctionUnitName { get; set; }
        public string TypeName { get; set; }
        public int? SOFeeFlag { get; set; }
        public int? CustomerFlag { get; set; }
        public int? WarehouseFlag { get; set; }
        public int? BargainingFlag { get; set; }
        public string Description { get; set; }
        public string BargainingDescription { get; set; }
        public int? DaiDianId { get; set; }
        public string ClientAutoCharge { get; set; }

    }

    public class LoadChargeDetailResult
    {
        public int Id { get; set; }
        public string ClientCode { get; set; }
        public string WhCode { get; set; }
        public string LoadId { get; set; }
        public string ChargeType { get; set; }
        public string ChargeName { get; set; }
        public string ChargeUnitName { get; set; }
        public string LadderNumber { get; set; }
        public string QtyCbm { get; set; }
        public string Remark { get; set; }
        public string Description { get; set; }
        public decimal? Price { get; set; }
        public decimal? PriceTotal { get; set; }
        public DateTime? ETD { get; set; }
        public string ETDShow { get; set; }
        public string SoNumber { get; set; }
        public string UnitName { get; set; }
        public string ContainerNumber { get; set; }
        public string ContainerType { get; set; }
        public decimal? CBM { get; set; }
        public string SoStatus { get; set; }
        public string CreateUser { get; set; }

        public int LoadChargeRuleId { get; set; }

        public string TaxInclusiveFlag { get; set; }
    }


    public class LoadChargeDetailInsert
    {
        public int loadChargeRuleId { get; set; }
        public string qtycbm { get; set; }
        public string unitName { get; set; }
        public decimal price { get; set; }
        public string loadId { get; set; }
        public string whCode { get; set; }
        public string userName { get; set; }
        public string soNumber { get; set; }
        public int daiDianId { get; set; }
    }

    public class LoadChargeRuleSearch : BaseSearch
    {
        public string WhCode { get; set; }
        public string LoadId { get; set; }
        public int? SOFeeFlag { get; set; }
        public string TypeName { get; set; }
    }

    public class LoadChargeDetailSearch : BaseSearch
    {
        public string WhCode { get; set; }
        public string ClientCode { get; set; }
        public string SoNumber { get; set; }
        public string BillNumber { get; set; }
        public string ContainerNumber { get; set; }
        public string CreateUser { get; set; }
        public string SoStatus { get; set; }

        public DateTime? BeginETD { get; set; }
        public DateTime? EndETD { get; set; }
    }

}
