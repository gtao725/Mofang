using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WMS.BLLClass
{
    public class LoadCreateRuleResult
    {
        public int Id { get; set; }
        public string RuleName { get; set; }
        public string Description { get; set; }
        public int? OrderQty { get; set; }
        public int? Qty { get; set; }
        public string Status { get; set; }
        public string ShipMode { get; set; }
        public string CreateUser { get; set; }
        public DateTime? CreateDate { get; set; }
        public string ClientCode { get; set; }
        public string OrderSource { get; set; }
        public string OrderType { get; set; }
        public string OutPoNumber { get; set; }
    }


    public class LoadCreateRuleSearch : BaseSearch
    {
        public int Id { get; set; }
        public string WhCode { get; set; }
        public string RuleName { get; set; }
        public string Status { get; set; }
        public string FlowName { get; set; }
        public string ClientCode { get; set; }
        public string OrderSource { get; set; }
        public DateTime? BeginDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
