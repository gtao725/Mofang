using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WMS.BLLClass
{
    public class UnTransferExpressNumberResult
    {
        public string LoadId { get; set; }
        public string SortGroupNumber { get; set; }
        public string PackGroupNumber { get; set; }
        public string ExpressNumber { get; set; }
        public decimal? Weight { get; set; }
        public string PackCarton { get; set; }
        public string Status { get; set; }
        public string ClientCode { get; set; }
        public string CreateUser { get; set; }
        public DateTime? CreateDate { get; set; }

    }

    public class UnTransferExpressNumberSearch : BaseSearch
    {
        public string WhCode { get; set; }
        public string ClientCode { get; set; }
        public string ExpressNumber { get; set; }
        public DateTime? CreateDateBegin { get; set; }
        public DateTime? CreateDateEnd { get; set; }
    }
}
