using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WMS.BLLClass
{
    public class HoldMasterSearch : BaseSearch
    {
        public int ClientId { get; set; }

        public string WhCode { get; set; }

        public string ClientCode { get; set; }
 
    }
}
