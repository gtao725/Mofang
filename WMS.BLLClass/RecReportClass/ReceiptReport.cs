using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WMS.BLLClass
{
    public class ReceiptReportResult
    {
        public int Id { get; set; }
        public string WhCode { get; set; }
        public string ReceiptId { get; set; }
        public int ClientId { get; set; }
        public string ClientCode { get; set; }
        public DateTime ReceiptDate { get; set; }
        public string Status { get; set; }
        public string SoNumber { get; set; }
        public string CustomerPoNumber { get; set; }
        public string PoNumber { get; set; }
        public string AltItemNumber { get; set; }
        public string Style1 { get; set; }
        public string ItemNumber { get; set; }
        public string HuId { get; set; }
        public int UnitId { get; set; }
        public string UnitName { get; set; }
        public int Qty { get; set; }
        public decimal? Length { get; set; }
        public decimal? Width { get; set; }
        public decimal? Height { get; set; }
        public decimal? Weight { get; set; }
        public decimal? CBM { get; set; }
        public string LotNumber1 { get; set; }
        public string LotNumber2 { get; set; }
        public DateTime LotDate { get; set; }

    }

    public class ReceiptReportSearch : BaseSearch
    {
        public string WhCode { get; set; }
        public string ReceiptId { get; set; }
        public int? ClientId { get; set; }
    }
}
