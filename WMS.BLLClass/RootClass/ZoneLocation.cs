using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WMS.BLLClass
{
    public class ZoneResult
    {
        public string WhCode { get; set; }
        public string ZoneName { get; set; }
        public string Description { get; set; }
        public string UpZoneName { get; set; }
        public string RegFlag { get; set; }
        public decimal? ZoneCBM { get; set; }
        public string CreateUser { get; set; }

    }

    public class ZoneLocationResult
    {
        public int? Id { get; set; }
        public string WhCode { get; set; }
        public string Location { get; set; }
    }

    public class ZoneLocationSearch : BaseSearch
    {
        public int? ZoneId { get; set; }

        public string WhCode { get; set; }

        public string Location { get; set; }
    }
}
