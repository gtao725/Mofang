using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WMS.BLLClass
{
    public class WhLocationResult
    {
        public int? Id { get; set; }
        public string WhCode { get; set; }
        public string LocationId { get; set; }
        public string Location { get; set; }
        public int? MaxPltQty { get; set; }

        public string Status { get; set; }
        public int? ZoneId { get; set; }
        public string ZoneName { get; set; }
        public int? LocationTypeId { get; set; }
        public string LocationDescription { get; set; }
        public string CreateUser { get; set; }
        public DateTime? CreateDate { get; set; }
    }

    public class WhLocationSearch : BaseSearch
    {
        public string WhCode { get; set; }
        public string LocationId { get; set; }
        public string Location { get; set; }
        public int? ZoneId { get; set; }
        public string LocationTypeId { get; set; }
    }
}
