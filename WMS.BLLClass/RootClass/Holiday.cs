using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WMS.BLLClass
{
    public class HolidaySearch : BaseSearch
    {
        public string HolidayName { get; set; }
        public string WhCode { get; set; }
    }

    public class ContractFormExtendSearch : BaseSearch
    {
        public string ChargeName { get; set; }
        public string WhCode { get; set; }
    }

}
