using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WMS.BLLClass
{
    public class WhUserResult
    {
        public int? Id { get; set; }
        public int? CompanyId { get; set; }
        public string UserName { get; set; }
        public string PassWord { get; set; }
        public string UserNameCN { get; set; }
        public string UserCode { get; set; }
        public string Status { get; set; }
        public string CreateUser { get; set; }
        public DateTime? CreateDate { get; set; }
        public int? WhCodeId { get; set; }
        public string WhName { get; set; }

        public string PositionNameCN { get; set; }

        public int? CheckFlag { get; set; }
    }


    public class WhUserSearch : BaseSearch
    {
        public int? CompanyId { get; set; }
        public string UserNameCN { get; set; }
        public string UserName { get; set; }
        public string UserCode { get; set; }
        public string PositionNameCN { get; set; }
        public string WhCodeName { get; set; }
    }

}
