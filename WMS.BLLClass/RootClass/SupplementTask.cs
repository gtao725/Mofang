using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WMS.BLLClass
{
    public class SupplementTaskResult
    {
        public int Id { get; set; }
        public string SupplementNumber { get; set; }
        public string Status { get; set; }
        public string CreateUser { get; set; }
        public DateTime? CreateDate { get; set; }
        public string UpdateUser { get; set; }
        public DateTime? UpdateDate { get; set; }
    }

    public class SupplementTaskSearch : BaseSearch
    {

        public string SupplementNumber { get; set; }
        public string WhCode { get; set; }
        public DateTime? BeginCreateDate { get; set; }
        public DateTime? EndCreateDate { get; set; }
    }
}
