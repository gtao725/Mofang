using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WMS.BLLClass
{
    public class TransferDetailCeModel: BaseSearch
    {
        public int TransferTaskId { get; set; }
        public string TransferNumber { get; set; }
        public string LoadId { get; set; }
        public string CustomerOutPoNumber { get; set; }
        public string OutPoNumber { get; set; }
        public string PackNumber { get; set; }
        
        public string express_code { get; set; }
        public string express_type_zh { get; set; }
        public string ExpressNumber { get; set; }
        public string StatusDes { get; set; }
        public int TransferHeadId { get; set; }
    }

    public class TransferDetailCe 
    {
        public int TransferTaskId { get; set; }
        public string TransferNumber { get; set; }
        public string LoadId { get; set; }
        public string CustomerOutPoNumber { get; set; }
        public string OutPoNumber { get; set; }
        public string PackNumber { get; set; }
        public string express_code { get; set; }
        public string express_type_zh { get; set; }
        public string ExpressNumber { get; set; }
        public string StatusDes { get; set; }
        public int TransferHeadId { get; set; }
    }
}
