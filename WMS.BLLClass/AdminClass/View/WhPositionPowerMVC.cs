using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WMS.BLLClass
{
    //权限管理结果类
    public class WhPositionPowerMVCResult
    {
        //结果属性
        public int? Id { get; set; }
        public string Action { get; set; }
        public int? CompanyId { get; set; }
        public int? ParentId { get; set; }
        public string AreaName { get; set; }
        public string ControllerName { get; set; }
        public string ActionName { get; set; }

        public int? HttpMethod { get; set; }
        public int? PowerId { get; set; }

        public string PowerName { get; set; }
        public string Description { get; set; }
    }

    //权限管理查询条件类
    public class WhPositionPowerMVCSearch : BaseSearch
    {
        //查询属性
        public int? CompanyId { get; set; }
        public string UserName;
        public int? UserId;
        public int? PowerId;
        public string AreaName;
    }
}
