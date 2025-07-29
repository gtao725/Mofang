using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WMS.BLLClass
{
    public class DamcoGrnRuleSearch : BaseSearch 
    {
        public string WhCode { get; set; }
        public string ClientCode { get; set; }

        public string MailTo { get; set; }

    }
}
