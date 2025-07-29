using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WMS.BLLClass
{
    public class RFFlowRuleSearch : BaseSearch
    {
        public string FunctionName { get; set; }

    }

    public class RFFlowRuleResult
    {
        public int Id { get; set; }
        public int FunctionId { get; set; }
        public string RuleType { get; set; }
        public string FunctionName { get; set; }
        public string Description { get; set; }
        public int? GroupId { get; set; }
        public int? FunctionFlag { get; set; }
        public int? RequiredFlag { get; set; }
        public int? RelyId { get; set; }
        public int? BusinessObjectHeadId { get; set; }
        public string SelectRuleDescription { get; set; }
    }
}
