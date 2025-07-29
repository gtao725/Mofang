using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WMS.UI.Controllers.Model
{
    public class MenuTreeModel
    {
        public int? id { get; set; } 
        public string text { get; set; }
        public string iconCls { get; set; }
        public string attributes { get; set; }
        public string state { get; set; }
        public List<MenuTreeModel> children { get; set; }
    }
}
