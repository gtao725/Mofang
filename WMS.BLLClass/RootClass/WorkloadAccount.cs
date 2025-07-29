using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WMS.BLLClass
{
    public class WorkloadAccountSearch : BaseSearch
    {
        public string WhCode { get; set; }
        public string ReceiptId { get; set; }
        public string LoadId { get; set; }
        public string UserCode { get; set; }
        public string WorkType { get; set; }
        public DateTime? BeginCreateDate { get; set; }
        public DateTime? EndCreateDate { get; set; }
    }

    public class WorkloadAccountResult
    {
        public string Action { get; set; }
        public string WhCode { get; set; }
        public string ReceiptId { get; set; }
        public string ClientCode { get; set; }
        public string LoadId { get; set; }
        public DateTime? ReceiptDate { get; set; }
        public string UserCode { get; set; }
        public string UserNameCN { get; set; }
        public string WorkType { get; set; }
        public decimal? cbm { get; set; }

        public decimal? baQty { get; set; }
        public string UpdateUser { get; set; }
    }
}
