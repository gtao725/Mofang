using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WMS.BLLClass
{
    public class InBoundOrderExntedSearch : BaseSearch
    {
        public string WhCode { get; set; }
        public string ClientCode { get; set; }       
        public string SoNumber { get; set; }
        public string AltItemNumber { get; set; }
        public string CartonId { get; set; }
        public DateTime? BeginCreateDate { get; set; }
        public DateTime? EndCreateDate { get; set; }

    }
}
