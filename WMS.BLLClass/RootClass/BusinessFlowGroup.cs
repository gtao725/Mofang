using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WMS.BLLClass
{
    public class BusinessFlowGroupSearch : BaseSearch
    {
        public string FlowName { get; set; }
        public string Type { get; set; }
        public string WhCode { get; set; }
        public int FlowHeadId { get; set; }

    }
    public class BusinessFlowGroupResult
    {
        public string FlowName { get; set; }

        public int? Id { get; set; }
        public string Remark { get; set; }

        public string CreateUser { get; set; }

        public DateTime? CreateDate { get; set; }
        public string UpdateUser { get; set; }

        public DateTime? UpdateDate { get; set; }
    }
}
