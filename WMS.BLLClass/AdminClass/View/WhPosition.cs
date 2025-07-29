using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WMS.BLLClass
{
    public class WhPositionResult
    {
        public int? Id { get; set; }
        public int? CompanyId { get; set; }
        public string PositionName { get; set; }
        public string PositionNameCN { get; set; }
    }
    /// <summary>
    /// 职位查询对象
    /// </summary>
    public class WhPositionSearch : BaseSearch
    {
        public string UserName { get; set; }
        public int? CompanyId { get; set; }
        public string PositionName { get; set; }
        public string PositionNameCN { get; set; }
    }
}
