using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WMS.BLLClass
{
    public class PhotoMasterResult
    {
        public string Action { get; set; }
        public int? PhotoId { get; set; }
        public string Number { get; set; }
        public string Number2 { get; set; }
        public string Number3 { get; set; }
        public string Number4 { get; set; }
        public string UnitName { get; set; }
        public int? Qty { get; set; }
        public int? RegQty { get; set; }

        public string HuId { get; set; }
        public string LocationId { get; set; }
        public string HoldReason { get; set; }
        public string TCRStatus { get; set; }
        public string TCRCheckUser { get; set; }
        public DateTime? TCRCheckDate { get; set; }
        public string TCRProcessMode { get; set; }
        public string SettlementMode { get; set; }
        public decimal? SumPrice { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public string Status { get; set; }
        public string CheckStatus1 { get; set; }
        public string CheckUser1 { get; set; }
        public DateTime? CheckDate1 { get; set; }
        public string KRemark1 { get; set; }
        public string CheckStatus2 { get; set; }
        public string CheckUser2 { get; set; }
        public DateTime? CheckDate2 { get; set; }
        public string CRemark1 { get; set; }
        public string CreateUser { get; set; }
        public DateTime? CreateDate { get; set; }
        public string ClientCode { get; set; }
        public string ContainerNumber { get; set; }
        public DateTime? BeginPackDate { get; set; }
        public DateTime? ShipDate { get; set; }
        public string ContainerType { get; set; }

        public string OrderSource { get; set; }
        public string UpdateUser { get; set; }
        public DateTime? UpdateDate { get; set; }
        public string UserCode { get; set; }
        public string UserNameCN { get; set; }
        public DateTime? UploadDate { get; set; }

        public string Location { get; set; }

        public string Style1 { get; set; }
        public string Style2 { get; set; }
        public string Style3 { get; set; }
    }

    public class TCRProcessResult
    {
        public int? Id { get; set; }
        public string TCRProcessMode { get; set; }
        public string CreateUser { get; set; }
        public DateTime? CreateDate { get; set; }
    }

    public class PhotoMasterSearch : BaseSearch
    {
        public string WhCode { get; set; }
        public string Number { get; set; }
        public string Number2 { get; set; }
        public string Number3 { get; set; }
        public string UserCode { get; set; }
        public string HuId { get; set; }
        public string TCRStatus { get; set; }
        public string HoldReason { get; set; }
        public string KRemark1 { get; set; }
        public string CRemark1 { get; set; }
        public string CheckStatus1 { get; set; }
        public string CheckStatus2 { get; set; }
        public DateTime? BeginCreateDate { get; set; }
        public DateTime? EndCreateDate { get; set; }
        public string ClientCode { get; set; }
        public string ContainerNumber { get; set; }
        public string HoldReason1 { get; set; }
        public string HoldReasonType { get; set; }
        public string HoldReasonTypeNot { get; set; }
        public string PhotoType { get; set; }
         
    }

    public class PhotoMasterApiSearch : BaseSearch
    {
        
        public string Number2 { get; set; }
        public DateTime? CreateDate { get; set; }

        public string PhotoType { get; set; }

        public string WhCode { get; set; }
        public string status { get; set; }

    }

    public class UploadPhotoApi  
    {

        public int id { get; set; }
        public string userId { get; set; }
        public string fileId { get; set; }
        public string whCode { get; set; }
        public string number { get; set; }
        public string number2 { get; set; }
        public string numberType { get; set; }
        public string remark { get; set; }

        public string type { get; set; }

    }

    public class HostPW
    {
        public string host { get; set; }
        public string ud { get; set; }
        public string pw { get; set; }

    }
}
