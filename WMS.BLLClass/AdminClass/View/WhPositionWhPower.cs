using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WMS.BLLClass
{
    /// <summary>
    /// 职位权限查询结果对象
    /// </summary>
    public class WhPositionWhPowerResult
    {
        public int? Id { get; set; }
        public string Action { get; set; }
        public string PositionName { get; set; }
        public string PositionNameCN { get; set; }
        public int? CompanyId { get; set; }
        public string PowerName { get; set; }
        public string PowerDescription { get; set; }
        public string PowerType { get; set; }
    }
    /// <summary>
    /// 职位权限查询对象
    /// </summary>
    public class WhPositionWhPowerSearch : BaseSearch
    {
        public string PositionName { get; set; }
        public string PositionNameCN { get; set; }
        public int? CompanyId { get; set; }
        public string PowerName { get; set; }
        public string PowerType { get; set; }
    }
}
