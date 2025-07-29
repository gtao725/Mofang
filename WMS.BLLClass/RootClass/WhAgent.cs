using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WMS.BLLClass
{
    public class WhAgentResult
    {
        public int? Id { get; set; }
        public string Action { get; set; }
        public string WhCode { get; set; }
        public string AgentName { get; set; }
        public string AgentCode { get; set; }
        public string AgentType { get; set; }
      
    }

    public class WhAgentSearch : BaseSearch
    {
        public int? Id { get; set; }
        public string AgentName { get; set; }
        public string WhCode { get; set; }
        public string AgentCode { get; set; }
        public string AgentType { get; set; }
        public int? ClientId { get; set; }
    }
}
