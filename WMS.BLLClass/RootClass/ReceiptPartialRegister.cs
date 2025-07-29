using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WMS.BLLClass
{
    public class ReceiptPartialRegisterSearch : BaseSearch
    {
        public string ReceiptId { get; set; }
        public string WhCode { get; set; }
        public string ClientCode { get; set; }
        public DateTime? BeginCreateDate { get; set; }
        public DateTime? EndCreateDate { get; set; }
    }

    public class ReceiptPartialRegisterResult
    {
        public int Id { get; set; }
        public int? PhotoId { get; set; }
        public DateTime? UploadDate { get; set; }
        public string ClientCode { get; set; }
        public string ReceiptId { get; set; }
        public string Status { get; set; }
        public int? Qty { get; set; }
        public string CreateUser { get; set; }
        public DateTime? CreateDate { get; set; }


    }

    public class ReceiptPartialUnPreceiptSearch : BaseSearch
    {
        public string ReceiptId { get; set; }
        public string ClientCode { get; set; }
        
        public string WhCode { get; set; }
        public string PoNumber { get; set; }
        public string SoNumber { get; set; }
        public string itemNumber { get; set; }

    }

    public class ReceiptPartialUnReceiptResult
    {
        public int Id { get; set; }
        public string ClientCode { get; set; }
        public string ReceiptId { get; set; }
        public string SoNumber { get; set; }
        public string PoNumber { get; set; }
        public string itemNumber { get; set; }
        public int? ItemId { get; set; }
        public int? UnQty { get; set; }
        public int? RegisteredQty { get; set; }
        public string CreateUser { get; set; }
        public DateTime? CreateDate { get; set; }


    }

    public class ReceiptPartialRegisteredDetailResult
    {
        public int Id { get; set; }
        public string ClientCode { get; set; }
        public string ReceiptId { get; set; }
        public string SoNumber { get; set; }
        public string PoNumber { get; set; }
        public string itemNumber { get; set; }
        public int? ItemId { get; set; }
        public int? RegisteredQty { get; set; }
        public string Reason { get; set; }
        public string CreateUser { get; set; }
        public DateTime? CreateDate { get; set; }


    }
}
