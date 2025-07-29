using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WMS.BLLClass
{
    public class ContractFormSearch : BaseSearch 
    {
        public string ContractName { get; set; }

        public string WhCode { get; set; }

        public string Type { get; set; }

        public string ChargeName { get; set; }

    }

    public class ContractFormOutSearch : BaseSearch
    {
        public string ContractName { get; set; }

        public string WhCode { get; set; }

        public string ChargeName { get; set; }

    }
}
