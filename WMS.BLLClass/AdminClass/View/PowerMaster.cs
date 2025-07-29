using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WMS.BLLClass
{
    //权限管理结果类
    public class PowerMaster
    {
        //结果属性
        public int? CompanyId { get; set; }
        public string UserName;
        public string UserNameCN;
        public string PositionName;
        public string PositionNameCN;
        public string PowerType;
        public string PowerName;
        public string PowerDescription;
    }

    //权限管理查询条件类
    public class PowerMasterSearch : BaseSearch
    {
        //查询属性
        public int? CompanyId { get; set; }
        public string UserName;
        public string UserNameCN;
        public string PositionName;
        public string PositionNameCN;
        public string PowerType;
        public string PowerName;
        public string PowerDescription;

    }
}
