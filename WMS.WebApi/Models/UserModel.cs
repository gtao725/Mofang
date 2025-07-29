using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WMS.BLLClass;

namespace WMS.WebApi.Models
{
    public class UserModel
    {

 
        public int CompanyId { get; set; }
        public string WhCode { get; set; }
        public string UserName { get; set; }

        public string UserNameCN { get; set; }
        public List<WhInfoResult> WhInfo { get; set; }
    }


}