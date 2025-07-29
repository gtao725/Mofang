using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WMS.BLLClass
{
    public class LocationResult
    {
        public string WhCode { get; set; }
        public string LocationId { get; set; }
        public int? MaxPltQty { get; set; }
        public string ZoneName { get; set; }
        public string Location { get; set; }
        public string LocationColumn { get; set; }
        public int? LocationRow { get; set; }
        public int? LocationFloor { get; set; }
        public int? LocationPcs { get; set; }
        public int LocationTypeId { get; set; }

        public string CreateUser { get; set; }

    }
}
