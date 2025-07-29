using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WMS.BLLClass
{
    public class ReceiptRegisterDetailResult
    {
        public string Action { get; set; }
        public int Id { get; set; }
        public int InOrderDetailId { get; set; }
        public string WhCode { get; set; }
        public string ReceiptId { get; set; }
        public string SoNumber { get; set; }
        public string PoNumber { get; set; }
        public string ItemNumber { get; set; }
        public string CustomerPoNumber { get; set; }
        public string AltItemNumber { get; set; }
        public string Style1 { get; set; }
        public string Style2 { get; set; }
        public string Style3 { get; set; }
        public int? RegQty { get; set; }

        public string UnitName { get; set; }
    }

    public class ReceiptRegisterDetailSearch : BaseSearch
    {
        public string ReceiptId { get; set; }

        public string WhCode { get; set; }
    }

    public class ReceiptRegisterDetailEdit
    {
        public int Id { get; set; }
        public int InOrderDetailId { get; set; }
        public int RegQty { get; set; }
        public int DiffQty { get; set; }
        public string UpdateUser { get; set; }
        public DateTime UpdateDate { get; set; }

        public string ReceiptId { get; set; }
        public string WhCode { get; set; }
    }
}
