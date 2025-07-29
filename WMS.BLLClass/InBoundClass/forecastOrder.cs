using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WMS.BLLClass
{
    public class ExcelImportInBoundSearch : BaseSearch
    {
        public string WhCode { get; set; }
        public string SystemNumber { get; set; }
        public string Supplier { get; set; }
        public string SoNumber { get; set; }
        public string PoNumber { get; set; }
        public string ItemNumber { get; set; }
        public string ReceiptId { get; set; }
        public DateTime? BeginDate { get; set; }
        public DateTime? EndDate { get; set; }

        public string Labeling { get; set; }

        public string PlatHeavyCargo { get; set; }

        public string TruckWaitingTime { get; set; }

        public DateTime? BeginConsDate { get; set; }
        public DateTime? EndConsDate { get; set; }

        public string ASOS { get; set; }
        public string ClothesHanger { get; set; }
        public string Hudson { get; set; }

        public string ShipeeziSO { get; set; }

        public string RatioorBulk { get; set; }

    }


    public class ExcelImportInBoundSo
    {

        public string SoNumber { get; set; }
        public string WhCode { get; set; }
        public string ClientCode { get; set; }

    }


    public class KmartReceiptDelay
    {
        public string WhCode { get; set; }
        public string Id { get; set; }
        public string ReceiptId { get; set; }
        public string PlaceOfDeparture { get; set; }
        public string PlaceOfDelivery { get; set; }
        public DateTime? ConsDate { get; set; }
        public string Supplier { get; set; }
        public string SoNumber { get; set; }
        public string TruckNumber { get; set; }
        public string Appointment { get; set; }
        public DateTime? RegistrationDate { get; set; }
        public DateTime? BeginDate { get; set; }
        public DateTime? EndDate { get; set; }
        public double? TruckwaitingTime { get; set; }
        public string WaitingDescription { get; set; }
        public double? UnloadingEfficiency { get; set; }
        public string UnloadingDescription { get; set; }
        public double? CombinedWaitingTime { get; set; }

        public string CombinedDescription { get; set; }
        public string ReasonCode1 { get; set; }
        public string ReasonCode2 { get; set; }
        public string ReasonCode3 { get; set; }
        public string Reason1 { get; set; }
        public string Reason2 { get; set; }
        public string Reason3 { get; set; }

        public string TransportType { get; set; }
        public string BkDate { get; set; }

        public DateTime? BkDateBegin { get; set; }
        public DateTime? BkDateEnd { get; set; }

    }


    public class KmartHeavyPalletSearch : BaseSearch
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

        public DateTime? BeginConsDate { get; set; }
        public DateTime? EndConsDate { get; set; }

    }

    public class KmartHeavyPalletResult
    {
        public string Show { get; set; }
        public string ClientCode { get; set; }
        public string WhCode { get; set; }
        public DateTime? ConsDate { get; set; }
        public DateTime? DCDD { get; set; }
        public string Supplier { get; set; }
        public string SoNumber { get; set; }
        public string PoNumber { get; set; }
        public string ItemNumber { get; set; }
        
        public string ReceiptId { get; set; }
        public string HuId { get; set; }
        public string LocationId { get; set; }
        public string PlaceOfDelivery { get; set; }
        public int? BookedCarton { get; set; }
        public int Qty { get; set; }
        public decimal? recCBM { get; set; }
        public decimal? Length { get; set; }
        public decimal? Width { get; set; }
        public decimal? Height { get; set; }
        public DateTime? ReceiptDate { get; set; }
        public string UpdateUser { get; set; }

        public string Show1{ get; set; }

    }


    public class ExcelImportOutBoundSearch : BaseSearch
    {
        public string WhCode { get; set; }
        public string ContainerNumber { get; set; }
        public string SoNumber { get; set; }
        public string PoNumber { get; set; }
        public string ItemNumber { get; set; }
        
    }

}
