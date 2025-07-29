using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WMS.BLLClass
{
    public class RecLossTypeSearch : BaseSearch
    {
        public string RecLossType { get; set; }
        public string WhCode { get; set; }
        public string Status { get; set; }
    }

    public class RecLossSearch : BaseSearch
    {
        public string RecLossName { get; set; }
        public string WhCode { get; set; }
        public string Status { get; set; }
        public int? RecLossTypeId { get; set; }
    }

    public class RecLossResult
    {
        public int? Id { get; set; }
        public int? RecSort { get; set; }
        public string RecLossName { get; set; }
        public string RecLossDescription { get; set; }
        public string RecLossType { get; set; }
        public int? RecLossTypeId { get; set; }
        public decimal? Price { get; set; }
        public string Status { get; set; }
        public string CreateUser { get; set; }
        public DateTime? CreateDate { get; set; }
        public string UpdateUser { get; set; }
        public DateTime? UpdateDate { get; set; }
    }

    public class AddValueServiceResult
    {
        public string ReceiptId { get; set; }
        public string WhCode { get; set; }
        public string RecLossName { get; set; }
        public string RecLossDescription { get; set; }
        public string RecLossType { get; set; }
        public decimal? Price { get; set; }
        public int? Qty { get; set; }
    }
    public class RecConsumerGoodsModel
    {
        public string ReceiptId { get; set; }
        public string WhCode { get; set; }
        public int RecLossId { get; set; }
 
        public string RecLossName { get; set; }
        public string RecLossDescription { get; set; }
        public int  Qty { get; set; }
        public float Price { get; set; }
        public string CreateUser { get; set; }
    }

    
}
