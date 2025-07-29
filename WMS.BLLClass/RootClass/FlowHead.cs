using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WMS.BLLClass
{
    public class FlowHeadResult
    {
        public int? Id { get; set; }
        public string FlowName { get; set; }
        public string Type { get; set; }
        public string TypeName { get; set; }
        public string InterceptFlag { get; set; }
        public int? FlowOrderBy { get; set; }
        public int? FieldOrderById { get; set; }
        public string OrderByDescription { get; set; }
        public string Remark { get; set; }
        public string CreateUser { get; set; }
        public DateTime? CreateDate { get; set; }
        public string UpdateUser { get; set; }
        public DateTime? UpdateDate { get; set; }
        public string InTemplate { get; set; }
        public string PZTemplate { get; set; }
        public string OutTemplate { get; set; }
        public string InTemplateShow { get; set; }
        public string PZTemplateShow { get; set; }
        public string OutTemplateShow { get; set; }

        public int? CheckAllHuWeightFlag { get; set; }
        public string CheckAllHuWeightShow { get; set; }
        public int? UrlEdiId { get; set; }

        public int? UrlEdiId2 { get; set; }

        public int? UrlEdiId3 { get; set; }
        public string UrlNameShow { get; set; }

        public string UrlNameShow2 { get; set; }

        public string UrlNameShow3 { get; set; }

        public int? ClientId { get; set; }

        public int? RId { get; set; }
    }

    public class FlowHeadSearch : BaseSearch
    {
        public string FlowName { get; set; }
        public string Type { get; set; }
        public string WhCode { get; set; }
        public int? ClientId { get; set; }
    }

}
