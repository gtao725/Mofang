using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WMS.BLLClass
{
    public class LoadMasterResult
    {
        public int Id { get; set; }
        public string LoadId { get; set; }
        public string ClientCode { get; set; }
        public string CustomerOutPoNumber { get; set; }
        public string CustomerOutPoNumberShow { get; set; }
        public string ShipMode { get; set; }
        public string Status0 { get; set; }
        public string Status1 { get; set; }
        public string Status2 { get; set; }
        public string Status3 { get; set; }
        public string ShipStatus { get; set; }
        public int? ProcessId { get; set; }
        public string ProcessName { get; set; }
        public DateTime? ReleaseDate { get; set; }
        public DateTime? BeginPickDate { get; set; }
        public DateTime? EndPickDate { get; set; }
        public DateTime? BeginPackDate { get; set; }
        public DateTime? EndPackDate { get; set; }
        public DateTime? BeginSortDate { get; set; }
        public DateTime? EndSortDate { get; set; }
        public DateTime? ShipDate { get; set; }
        public int? OutBoundOrderId { get; set; }

        public string CreateUser { get; set; }
        public DateTime? CreateDate { get; set; }
        public string ContainerNumber { get; set; }
        public string SealNumber { get; set; }
        public DateTime? ETD { get; set; }
        public string ContainerType { get; set; }

        public string ContainerName { get; set; }
        public string VesselName { get; set; }
        public string VesselNumber { get; set; }
        public string CarriageName { get; set; }
        public string Port { get; set; }
        public string DeliveryPlace { get; set; }
        public string BillNumber { get; set; }
        public string PortSuitcase { get; set; }    
        public string Action1 { get; set; }
        public string Action2 { get; set; }
        public string Action3 { get; set; }
        public string Remark { get; set; }
        public int? SumQty { get; set; }
        public int? DSSumQty { get; set; }
        public int? EchQty { get; set; }
        public decimal? SumCBM { get; set; }
        public decimal? SumWeight { get; set; }

        public int? ClientId { get; set; }
        public string ETDShow { get; set; }
    }

    public class LoadMasterSearch : BaseSearch
    {
        public string WhCode { get; set; }
        public string LoadId { get; set; }

        public string ClientCode { get; set; }
        public string CustomerOutPoNumber { get; set; }

        public string ContainerNumber { get; set; }
        public string ShipMode { get; set; }

        public string Status0 { get; set; }
        public string Status1 { get; set; }

        public string Status3 { get; set; }
        public string ShipStatus { get; set; }
        public DateTime? BeginShipDate { get; set; }
        public DateTime? EndShipDate { get; set; }

        public DateTime? BeginCreateDate { get; set; }
        public DateTime? EndCreateDate { get; set; }
    }


    public class LoadInsert
    {
        public string WhCode { get; set; }
        public string ShipMode { get; set; }
        public int ProcessId { get; set; }
        public string ProcessName { get; set; }
        public string CreateUser { get; set; }
        public DateTime? CreateDate { get; set; }

        public LoadContainerInsert LoadContainerInsert;

    }

    public class LoadContainerInsert
    {
        public string VesselName { get; set; }
        public string VesselNumber { get; set; }
        public string CarriageName { get; set; }
        public DateTime? ETD { get; set; }
        public string ContainerType { get; set; }
        public string Port { get; set; }
        public string DeliveryPlace { get; set; }
        public string BillNumber { get; set; }
        public string ContainerNumber { get; set; }
        public string SealNumber { get; set; }

    }

    public class LoadCreateRuleInsert
    {
        public string WhCode { get; set; }
        public string ClientCode { get; set; }
        public string OrderSource { get; set; }
        public string OrderType { get; set; }
        public string UserName { get; set; }
        public int LoadCount { get; set; }
        public int RuleId { get; set; }
        public DateTime? BeginDate { get; set; }
        public DateTime? EndDate { get; set; }

    }

}
