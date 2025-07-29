using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WMS.BLLClass
{
    public class BaseSearch
    {
        //分页属性
        public int pageSize {get;set; }
        public int pageIndex {get;set; }

        //排序属性
        public string[] OrderByColumn {get;set; }
        public bool[] Sort {get;set; }
    }
}
