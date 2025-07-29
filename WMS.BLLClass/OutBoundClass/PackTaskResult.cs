using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WMS.BLLClass
{
    public class PackTaskResult
    {
        public string WhCode { get; set; }
        public string LoadId { get; set; }
        public int? SortGroupId { get; set; }
        public string SortGroupNumber { get; set; }
        public string CustomerPoNumber { get; set; }
        public string OutPoNumber { get; set; }
        public string customer_Po { get; set; }
        public string outorder_number_alt { get; set; }
        public int? BarcodePrintCount { get; set; }
        public string Json { get; set; }
        public Byte[] outorder_number_alt_Bar { get; set; }
        public Byte[] customer_Po_Bar { get; set; }
        public string Remark { get; set; }
    }

    public class PackTaskSearch : BaseSearch
    {
        public string WhCode { get; set; }
        public string LoadId { get; set; }
        public string ClientCode { get; set; }
        public string CustomerOutPoNumber { get; set; }
        public string AltCustomerOutPoNumber { get; set; }
        public string SortGroupNumber { get; set; }
        public string AltItemNumber { get; set; }
        public string ExpressNumber { get; set; }
        public string UpdateUser { get; set; }
        public DateTime? BeginCreateDate { get; set; }
        public DateTime? EndCreateDate { get; set; }
        public string Status { get; set; }
        public string ExpressNumberIsNull { get; set; }

        public DateTime? BeginOrderDate { get; set; }
        public DateTime? EndOrderDate { get; set; }
    }

    public class PackTaskSearchResult
    {
        public int? PackTaskId { get; set; }
        public string LoadId { get; set; }
        public string SortGroupNumber { get; set; }
        public string CustomerOutPoNumber { get; set; }
        public string AltCustomerOutPoNumber { get; set; }
        public string OutPoNumber { get; set; }
        public string ClientCode { get; set; }
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
        public int? Qty { get; set; }
        
        public string d_addressDetail { get; set; }

        public string SinglePlaneTemplate { get; set; }
        public string PackingListTemplate { get; set; }
        public DateTime? CreateDate { get; set; }
        public DateTime? UpdateDate { get; set; }
        public string UpdateUser { get; set; }
        public string AltItemNumber { get; set; }
        public string PrintDate { get; set; }
        public DateTime? OrderDate { get; set; }

        public string OrderType { get; set; }
        public string OrderSource { get; set; }

        public string ProcessName { get; set; }
        public string ItemName { get; set; }

    }

    public class PackDetailSearchResult
    {
        public int PackDetailId { get; set; }
        public string PackNumber { get; set; }
        public string AltItemNumber { get; set; }
        public int? PlanQty { get; set; }
        public int? Qty { get; set; }
        public string CreateUser { get; set; }
        public DateTime? CreateDate { get; set; }

    }


    public class PackPackScanNumberResult
    {
        public int PackScanNumberId { get; set; }
        public string ScanNumber { get; set; }

    }


    public class PackTaskCryReport
    {
        public string WhCode { get; set; }
        public string ClientCode { get; set; }
        public string LoadId { get; set; }
        public string CustomerPoNumber { get; set; }
        public string OutPoNumber { get; set; }
        public string PingTaiNumber { get; set; }
        public Byte[] PingTaiNumberBar { get; set; }
        public string CustomerPo { get; set; }
        public string j_company { get; set; }
        public string j_contact { get; set; }
        public string j_tel { get; set; }
        public string j_province { get; set; }
        public string j_city { get; set; }
        public string j_county { get; set; }
        public string j_address { get; set; }
        public string d_company { get; set; }
        public string d_contact { get; set; }
        public string d_tel { get; set; }
        public string d_address { get; set; }
        public string dest_code { get; set; }
        public int? ItemId { get; set; }
        public string AltItemNumber { get; set; }
        public string UnitName { get; set; }
        public string Description { get; set; }
        public string Remark1 { get; set; }
        public string PackNumber { get; set; }
        public string PackCarton { get; set; }
        public decimal? Length { get; set; }
        public decimal? Width { get; set; }
        public decimal? Height { get; set; }
        public decimal? Weight { get; set; }
        public int? Qty { get; set; }
        public int? PackGroupId { get; set; }
        public DateTime? CreateDate { get; set; }
        public decimal? Price { get; set; }
        public decimal? TotalPrice { get; set; }
        public string EAN { get; set; }

        //订单类型
        public string BusinessMode { get; set; }
        //证书收件人
        public string CustomerName { get; set; }
        //证书编号
        public string CustomerRef { get; set; }

    }


    public class PackTaskCryReportExpress
    {
        public string WhCode { get; set; }
        public string ClientCode { get; set; }
        public string LoadId { get; set; }
        public string CustomerOutPoNumber { get; set; }
        public string CustomerPo { get; set; }
        public string j_company { get; set; }
        public string j_contact { get; set; }
        public string j_tel { get; set; }
        public string j_province { get; set; }
        public string j_city { get; set; }
        public string j_county { get; set; }
        public string j_address { get; set; }
        public string d_company { get; set; }
        public string d_contact { get; set; }
        public string d_tel { get; set; }
        public string d_Province { get; set; }
        public string d_city { get; set; }
        public string d_address { get; set; }
        public string express_code { get; set; }
        public string express_type_zh { get; set; }
        public string custid { get; set; }
        public string cod { get; set; }
        public string j_name { get; set; }
        public string form_code { get; set; }
        public string dest_code { get; set; }
        public string AirFlag { get; set; }
        public int? PackGroupId { get; set; }
        public string ExpressNumber { get; set; }
        public string Remark { get; set; }
        public Byte[] ExpressNumberBar { get; set; }
        public decimal? Weight { get; set; }

        public string DN { get; set; }
        public Byte[] DNBar { get; set; }

        public string proCode { get; set; }
        public Byte[] proCodeBar { get; set; }
        public string proName { get; set; }
        public string destRouteLabel { get; set; }
        public string destTeamCode { get; set; }
        public string codingMapping { get; set; }
        public string twoDimensionCode { get; set; }
        public Byte[] QRCode { get; set; }
        public string xbFlag { get; set; }
        public string codingMappingOut { get; set; }
        public string printIcon { get; set; }
        public Byte[] printIconBar { get; set; }
        public string ExpressNumberParent { get; set; }
        public string ExpressNumberParentShow { get; set; }
        public string bagAddr { get; set; }
        public int? packHeadId { get; set; }

        public string SinglePlaneTemplate { get; set; }
        public string OutPoNumber { get; set; }

        public string PayMethodShow { get; set; }
        public int? PackQty { get; set; } //包裹总数量add by yangxin 2024-05-29 子母单
    }

    public class PackTaskDeleteResult
    {
        public string WhCode { get; set; }
        public string LoadId { get; set; }
        public string OutPoNumber { get; set; }
        public int? PackTaskId { get; set; }
        public int? PackHeadId { get; set; }
        public int? PackDetailId { get; set; }
    }


    public class PackTaskCryReportYunPrintData
    {
        //寄件人信息
        public string j_company { get; set; }
        public string j_contact { get; set; }
        public string j_tel { get; set; }
        public string j_province { get; set; }
        public string j_city { get; set; }
        public string j_county { get; set; }
        public string j_address { get; set; }


        public string WhCode { get; set; }
        public string LoadId { get; set; }
        public string ClientCode { get; set; }

        public string OutPoNumber { get; set; }
        public string CustomerOutPoNumber { get; set; }
        public string CustomerPo { get; set; }
        public string express_code { get; set; }
        public int? PackGroupId { get; set; }

        //云打印对象字段
        //面单Base64图片数据
        public string Base64CryDate { get; set; }
        //运单号
        public string TrackNo { get; set; }
        //EdiJson
        public string EdiJson { get; set; }
        //签名
        public string Sign { get; set; }


        //订单信息-产品明细
        public string ProductInfo { get; set; }

        //是否送装服务
        public string ServiceCode { get; set; }

        //订单来源-抖音 京东
        public string OutOrderSourceToCloudPrint { get; set; }
        //包裹总数量，菜鸟快运类型子母单必填项  add by yangxin 2024-05-28
        public int PackQty { get; set; }

        public string ECustomerPo { get; set; }

    }

}
