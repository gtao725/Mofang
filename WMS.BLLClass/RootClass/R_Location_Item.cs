using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WMS.BLLClass
{
    public class R_Location_ItemSearch : BaseSearch
    {
        public string WhCode { get; set; }
        public string ClientCode { get; set; }
        public string LocationId { get; set; }
        public string AltItemNumber { get; set; }
        public string Status { get; set; }
    }

    public class R_Location_ItemResult
    {
        public int Id { get; set; }
        public string ClientCode { get; set; }
        public string LocationId { get; set; }
        public string AltItemNumber { get; set; }
        public string EAN { get; set; }
        public string UnitName { get; set; }
        public string Status { get; set; }
        public string StatusShow { get; set; }
        public string LotNumber1 { get; set; }
        public string LotNumber2 { get; set; }
        public DateTime? LotDate { get; set; }
        public int? MinQty { get; set; }
        public int? MaxQty { get; set; }
        public string CreateUser { get; set; }
        public DateTime? CreateDate { get; set; }
        public string UpdateUser { get; set; }
        public DateTime? UpdateDate { get; set; }
        public string WhCode { get; set; }
        public int? ItemId { get; set; }
        public int? InvQty { get; set; }
    }

}
