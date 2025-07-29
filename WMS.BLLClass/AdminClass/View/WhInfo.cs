using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WMS.BLLClass
{
    public class WhInfoResult
    {
        public int? Id { get; set; }
        public string WhCode { get; set; }
        public string WhName { get; set; }
        public int CompanyId { get; set; }
        public int? UserId { get; set; }
    }

    public class WhInfoSearch : BaseSearch
    {
        public string UserName { get; set; }
        public int CompanyId { get; set; }
    }

    public class WhInfoWhUserResult
    {
        public int? Id { get; set; }

        public string Action { get; set; }
        public string WhCode { get; set; }
        public string WhName { get; set; }
        public int CompanyId { get; set; }
    }

    public class WhInfoResult1
    {
        public int? Id { get; set; }
        public string WhCode { get; set; }
        public string WhName { get; set; }
        public int? NoHuIdFlag { get; set; }
        public string NoHuIdFlagShow { get; set; }
    }

    public class WhInfoSearch1 : BaseSearch
    {
        public string WhName { get; set; }
        public int CompanyId { get; set; }
    }
}
