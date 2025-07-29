using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WMS.BLLClass
{
    public class BusinessObjectSearch : BaseSearch
    {
        public string ObjectDes { get; set; }
        public string ObjectName { get; set; }
        public string ObjectType { get; set; }

    }

    public class BusinessObjectResult
    {
        public int? Id { get; set; }
        public string ObjectDes { get; set; }
        public string ObjectType { get; set; }

    }

}
