using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WMS.BLLClass
{
    public class WhPowerResult
    {
        public int? Id { get; set; }
        public int? CompanyId { get; set; }
        public string PowerName { get; set; }

        public string PowerDescription { get; set; }
        public string PowerType { get; set; }
    }
    public class WhPowerSearch : BaseSearch
    {
        public string PositionName { get; set; }
        public int? CompanyId { get; set; }
        public string PowerName { get; set; }
        public string PowerType { get; set; }
    }
}
