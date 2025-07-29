using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WMS.BLLClass
{
    public class FlowRuleSearch : BaseSearch
    {
        public string FunctionName { get; set; }

    }

    public class FlowRuleResult
    {
        public int? Id { get; set; }
        public int FunctionId { get; set; }
        public string RuleType { get; set; }
        public string FunctionName { get; set; }
        public string StatusName { get; set; }
        public string Description { get; set; }
        public int? GroupId { get; set; }
        public int? FunctionFlag { get; set; }
        public int? RequiredFlag { get; set; }
        public int? RelyId { get; set; }
        public int? BusinessObjectGroupId { get; set; }
        public string SelectRuleDescription { get; set; }
        public int? RollbackFlag { get; set; }
        public string RollbackFlagShow { get; set; }

        public string FlowName { get; set; }
    }
}
