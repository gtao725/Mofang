using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WMS.BLLClass
{
    public class WhPallateResult
    {
        public int? Id { get; set; }
        public string WhCode { get; set; }
        public string HuId { get; set; }
        public int? TypeId { get; set; }
        public string TypeName { get; set; }
        public string Status { get; set; }
        public string CreateUser { get; set; }
        public DateTime? CreateDate { get; set; }
    }

    public class WhPallateSearch : BaseSearch
    {
        public string WhCode { get; set; }
        public string HuId { get; set; }
        public string TypeId { get; set; }
    }
}
