using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WMS.BLLClass
{
    public class SerialNumberInSearch : BaseSearch
    {
        public string ClientCode { get; set; }
        public string WhCode { get; set; }
        public string ReceiptId { get; set; }
        public string SoNumber { get; set; }
        public string CustomerPoNumber { get; set; }
        public string AltItemNumber { get; set; }
        public string CartonId { get; set; }
        public string HuId { get; set; }
        public DateTime? BeginCreateDate { get; set; }
        public DateTime? EndCreateDate { get; set; }
    }


    public class SerialNumberDetailSearch : BaseSearch
    {
        public int HeadId { get; set; }
        public string SNType { get; set; }
        public int PCS { get; set; }
        public string UPC { get; set; }
        public DateTime? CreateDate { get; set; }

    }
    public class SerialNumberDetailOut 
    {
        public int Id { get; set; }
        public int HeadId { get; set; }
        public string SNType { get; set; }
        public int? PCS { get; set; }
        public string UPC { get; set; }
        public DateTime? CreateDate { get; set; }

    }
    
    public class SerialNumberInOut
    {
        public int Id { get; set; }
        public string WhCode { get; set; }
        public string ReceiptId { get; set; }
        public Nullable<int> ClientId { get; set; }
        public string ClientCode { get; set; }
        public string SoNumber { get; set; }
        public string CustomerPoNumber { get; set; }
        public string AltItemNumber { get; set; }
        public Nullable<int> PoId { get; set; }
        public Nullable<int> ItemId { get; set; }
        public string CartonId { get; set; }
        public string HuId { get; set; }
        public Nullable<decimal> Length { get; set; }
        public Nullable<decimal> Width { get; set; }
        public Nullable<decimal> Height { get; set; }
        public Nullable<decimal> Weight { get; set; }
        public string LotNumber1 { get; set; }
        public string LotNumber2 { get; set; }
        public Nullable<System.DateTime> LotDate { get; set; }
        public Nullable<int> ToOutStatus { get; set; }
        public string CreateUser { get; set; }
        public Nullable<System.DateTime> CreateDate { get; set; }
        public string UpdateUser { get; set; }
        public Nullable<System.DateTime> UpdateDate { get; set; }
    }

    public class SerialNumberOutSearch : BaseSearch
    {
        public string ClientCode { get; set; }
        public string WhCode { get; set; }
        public string LoadId { get; set; }
        public string SoNumber { get; set; }
        public string CustomerPoNumber { get; set; }
        public string AltItemNumber { get; set; }
        public string CartonId { get; set; }
        public string HuId { get; set; }
        public DateTime? BeginCreateDate { get; set; }
        public DateTime? EndCreateDate { get; set; }
    }
}
