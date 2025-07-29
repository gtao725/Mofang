using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WMS.BLLClass
{
    public class BoschPackEntity
    {
        public string OutOrderNumber { get; set; }
        public string HeTongNo { get; set; }
        public string DNNo { get; set; }
        public string PackingListTemplate { get; set; }
        public int? PrintCount { get; set; }
        public string TransportType { get; set; }
        public string express_code { get; set; }

        public List<BoschPackEntityDetail> BoschPackEntityDetail;
    }

    public class BoschPackEntityDetail
    {
        public string LineNumber { get; set; }
        public string ItemNumber { get; set; }
        public string Description { get; set; }
        public string ClientItemNumber { get; set; }
        public string Length { get; set; }
        public string SaleNo { get; set; }
        public string SaleNoZd { get; set; }
    }

    public class BoschPackTaskCryReport
    {
        public string WhCode { get; set; }
        public string LoadId { get; set; }
        public string CustomerPoNumber { get; set; }
        public string OutPoNumber { get; set; }
        public string PingTaiNumber { get; set; }
     
        public string d_company { get; set; }
        public string d_contact { get; set; }
        public string d_tel { get; set; }
        public string d_address { get; set; }
        public string dest_code { get; set; }
        public string AltItemNumber { get; set; }
        public string UnitName { get; set; }
        public string PackNumber { get; set; }
        public string PackCarton { get; set; }
        public string Length { get; set; }
        public string Width { get; set; }
        public string Height { get; set; }
        public string Weight { get; set; }
        public string Description { get; set; }
        public int? Qty { get; set; }
        public DateTime? CreateDate { get; set; }
        public string Json { get; set; }
        public string OutOrderNumber { get; set; }
        public string HeTongNo { get; set; }
        public string DNNo { get; set; }
        public string SaleNo { get; set; }
        public string SaleNoZd { get; set; }
        public string PrintCount { get; set; }
        public string PackingListTemplate { get; set; }
        public string LineNumber { get; set; }
        public string ItemNumber { get; set; }
        public string EngLishDescription { get; set; }
        public string ClientItemNumber { get; set; }
        public string ItemLength { get; set; }
    }

    public class BoschPackTaskSearchResult
    {
        public int? PackTaskId { get; set; }
        public string LoadId { get; set; }
        public string SortGroupNumber { get; set; }
        public string AltCustomerOutPoNumber { get; set; }
        public string CustomerOutPoNumber { get; set; }
        public string OutPoNumber { get; set; }
        public string ExpressCode { get; set; }
        public int? PackHeadId { get; set; }
        public int? PackGroupId { get; set; }
        public string PackNumber { get; set; }
        public string ExpressNumber { get; set; }
        public string ExpressStatus { get; set; }
        public string ExpressMessage { get; set; }
        public decimal? Length { get; set; }
        public decimal? Width { get; set; }
        public decimal? Height { get; set; }
        public decimal? Weight { get; set; }
        public string PackCarton { get; set; }
        public string Status { get; set; }
        public int? planQty { get; set; }
        public int? packQty { get; set; }
        public int? packNowQty { get; set; }
        public string d_addressDetail { get; set; }
        public string SinglePlaneTemplate { get; set; }
        public string PackingListTemplate { get; set; }
        public DateTime? CreateDate { get; set; }
        public DateTime? UpdateDate { get; set; }
        public string AltItemNumber { get; set; }
        public string HeTongNo { get; set; }
        public string DNNo { get; set; }
        public string SaleNo { get; set; }
        public string SaleNoZd { get; set; }
        public string LineNumber { get; set; }
        public string ItemNumber { get; set; }
        public string EngLishDescription { get; set; }
        public string ClientItemNumber { get; set; }
        public string ItemLength { get; set; }

        public string Json { get; set; }
    }
}
