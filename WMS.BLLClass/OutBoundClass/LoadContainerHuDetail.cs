using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WMS.BLLClass
{
    public class LoadContainerHuDetailResult
    {
        public string Action { get; set; }
        public string ClientCode { get; set; }
        public string SoNumber { get; set; }
        public int? Qty { get; set; }
        public int? PlanQty { get; set; }
        public int? AcQty { get; set; }
        public int? SelectQty { get; set; }
        public decimal? CBM { get; set; }
        public decimal? SelectCBM { get; set; }
        public DateTime? InvDate { get; set; }
        public int? Date { get; set; }
        public string ContainerNumber { get; set; }
        public long sequence { get; set; }
        public string CustomerPoNumber { get; set; }
        public string AltItemNumber { get; set; }
        public int? ItemId { get; set; }
        public string Style1 { get; set; }
        public string Style2 { get; set; }

        public string Stragg { get; set; }
    }

    public class LoadContainerHuDetailSearch : BaseSearch
    {
        public string SoNumber { get; set; }
        public string PoNumber { get; set; }
        public string AltNumber { get; set; }
        public string Style1 { get; set; }
        public string Style2 { get; set; }
        public string ClientCode { get; set; }
        public string WhCode { get; set; }
        public int LoadContainerHuDetailId { get; set; }

    }


    public class LoadContainerHuDetailResult1
    {
        public string WhCode { get; set; }
        public int? ClientId { get; set; }
        public string ClientCode { get; set; }
        public string SoNumber { get; set; }
        public string CustomerPoNumber { get; set; }
        public string AltItemNumber { get; set; }
        public int? Qty { get; set; }
        public decimal? Length { get; set; }
        public decimal? Width { get; set; }
        public decimal? Height { get; set; }
        public decimal? Weight { get; set; }
        public int? ItemId { get; set; }
        public string UnitName { get; set; }
        public int? UnitId { get; set; }
        public string LotNumber1 { get; set; }
        public string LotNumber2 { get; set; }
        public DateTime? LotDate { get; set; }
        public int? sequence { get; set; }
    }

    public class LoadContainerHuDetailResult2
    {
        public string Action { get; set; }
        public int Id { get; set; }
        public string ProcessName { get; set; }
        public string ClientCode { get; set; }
        public string SoNumber { get; set; }
        public string CustomerPoNumber { get; set; }
        public string AltItemNumber { get; set; }
        public int? Qty { get; set; }
        public decimal? Length { get; set; }
        public decimal? Width { get; set; }
        public decimal? Height { get; set; }
        public decimal? Weight { get; set; }
        public int? ItemId { get; set; }
        public string UnitName { get; set; }
        public int? UnitId { get; set; }
        public string LotNumber1 { get; set; }
        public string LotNumber2 { get; set; }
        public DateTime? LotDate { get; set; }
        public string CreateUser { get; set; }
        public DateTime? CreateDate { get; set; }

        public int? Sequence { get; set; }
        public decimal? CBM { get; set; }

        public string Style1 { get; set; }
        public string Style2 { get; set; }

        public string Style3 { get; set; }
    }
}
