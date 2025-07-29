using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WMS.BLLClass
{
    public class WhZoneResult
    {
        public int? Id { get; set; }

        public string ZoneName { get; set; }

        public string Description { get; set; }

        public int? ParentId { get; set; }

        public string ParentZoneName { get; set; }

        public int? RegFlag { get; set; }
        public string RegShow { get; set; }

        public decimal? ZoneCBM { get; set; }

        public int? ClientId { get; set; }

    }
    public class WhZoneSearch : BaseSearch
    {
        public string ZoneName { get; set; }
        public string WhCode { get; set; }
        public int? ZoneId { get; set; }
        public int? UpId { get; set; }
    }

    public class ZoneExtendResult
    {
        public string WhCode { get; set; }
        public int? Id { get; set; }
        public int? ZoneId { get; set; }
        public string ZoneName { get; set; }
        public int? ZoneOrderBy { get; set; }
        public int? OnlySkuFlag { get; set; }
        public string OnlySkuShow { get; set; }
        public int? MaxLocationIdQty { get; set; }
        public int? MaxPallateQty { get; set; }
        public string CreateUser { get; set; }
    }
}
