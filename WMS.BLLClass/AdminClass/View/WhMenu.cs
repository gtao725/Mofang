using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WMS.BLLClass
{
    /// <summary>
    /// 用户查询对象
    /// </summary>
    public class WhMenuResult
    {
        public int? Id { get; set; }
        public string Action { get; set; }
        public int? CompanyId { get; set; }
        public string MenuName { get; set; }
        public string MenuNameCN { get; set; }
        public string MenuUrl { get; set; }
        public string MenuIcon { get; set; }
        public int? MenuSort { get; set; }
        public int? PowerId { get; set; }
        public string PowerName { get; set; }
        public int? ParentMenuId { get; set; }
        public string ParentMenuName { get; set; }
        public string UserName { get; set; }
        public string PositionPowerName { get; set; }
        public string CreateUser { get; set; }
        public DateTime? CreateDate { get; set; }

    }

    public class WhMenuSearch : BaseSearch
    {
        public int? MenuId { get; set; }
        public string MenuNameCN { get; set; }
        public string ParentId { get; set; }
        public int? CompanyId { get; set; }
        public int? PowerId { get; set; }
        public string PowerName { get; set; }
        public string PowerType { get; set; }
    }
}
