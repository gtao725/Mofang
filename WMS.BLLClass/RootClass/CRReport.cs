using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WMS.BLLClass 
{
    public class CRReportSearch : BaseSearch
    {
        public string WhCode { get; set; }
        public string Type { get; set; }
        public string TemplateName { get; set; }

        public string Description { get; set; }
    }

    public class CRReportEdit : BaseSearch
    {
        public int Id { get; set; }
        public string WhCode { get; set; }
        public string Url { get; set; }
        public string TemplateName { get; set; }

        public string Description { get; set; }
    }

    public class UrlEdiSearch : BaseSearch
    {
        public string WhCode { get; set; }
        public string Url { get; set; }

    }
}
