using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WMS.BLLClass
{
    /// <summary>
    /// 用户职位查询结果对象
    /// </summary>
    public class WhUserWhPositionResult
    {
        public int? Id { get; set; }
        public int? UId { get; set; }
        public int? PId { get; set; }
        public string Action { get; set; }
        public int? CompanyId { get; set; }
        public string UserNameCN { get; set; }
        public string UserName { get; set; }
        public string PositionName { get; set; }
        public string PositionNameCN { get; set; }
    }
    /// <summary>
    /// 用户职位查询对象
    /// </summary>
    public class WhUserWhPositionSearch : BaseSearch
    {
        public int? CompanyId { get; set; }
        public string UserNameCN { get; set; }
        public string UserName { get; set; }
        public string PositionNameCN { get; set; }
    }


}
