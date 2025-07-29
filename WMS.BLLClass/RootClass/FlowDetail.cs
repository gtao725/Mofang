using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WMS.BLLClass
{
    public class FlowDetailResult
    {
        public int? FlowHeadId { get; set; }
        public int? FlowRuleId { get; set; }
        public int? FunctionId { get; set; }
        public string FunctionName { get; set; }
        public int? StatusId { get; set; }
        public string StatusName { get; set; }
        public string Description { get; set; }
        public int? GroupId { get; set; }
        public int? BusinessObjectGroupId { get; set; }
        public string SelectRuleDescription { get; set; }
        public int? RollbackFlag { get; set; }
        public string RollbackFlagShow { get; set; }
        public string Type { get; set; }
        public string Mark { get; set; }
        public string LoadId { get; set; }
    }


    public class FlowDetailSearch : BaseSearch
    {
        public int? FlowHeadId { get; set; }

    }


    public class FlowHeadInsert
    {
        public int? FlowHeadId { get; set; }

        public List<FlowDetailModel> FlowDetailModel;
    }


    public class FlowDetailModel
    {
        public int? FlowRuleId { get; set; }
        public string FunctionName { get; set; }
        public int? GroupId { get; set; }
        public int? BusinessObjectGroupId { get; set; }
        public int? RollbackFlag { get; set; }
        public int? StatusId { get; set; }
        public string StatusName { get; set; }
        public string Type { get; set; }
        public string Mark { get; set; }

    }
}
