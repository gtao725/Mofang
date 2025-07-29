using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WMS.BLLClass
{
    public class BusinessFlowHeadSearch : BaseSearch
    {
        public int BusinessFlowGroupId { get; set; }
    }
    public class BusinessFlowHeadResult
    {
        public int? Id { get; set; }
        public int? FunctionId { get; set; }
        public string FunctionName { get; set; }
        public string Description { get; set; }
        public string SelectRuleDescription { get; set; }
        public int? BusinessFlowHeadId { get; set; }
        public int? FlowRuleId { get; set; }
        public int? GroupId { get; set; }
    }

    public class BusinessFlowHeadInsert
    {
        public int? busGroupId { get; set; }

        public List<FlowRuleModel> FlowRuleModel;
    }

    public class FlowRuleModel
    {
        public int? Id { get; set; }
        public string FunctionName { get; set; }
        public int? BusinessFlowHeadId { get; set; }
        public int? GroupId { get; set; }
    }
}
