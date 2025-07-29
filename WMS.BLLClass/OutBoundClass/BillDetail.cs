using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WMS.BLLClass
{
    public class BillMasterSearch : BaseSearch
    {
        public string WhCode { get; set; }
        public string CreateUser { get; set; }
        public DateTime? BeginCreateDate { get; set; }
        public DateTime? EndCreateDate { get; set; }
    }

    public class BillDetailSearch : BaseSearch
    {
        public string BLNumber { get; set; }
        public string WhCode { get; set; }
        public string AgentCode { get; set; }
        public string ClientCode { get; set; }
        public string FeeStatus { get; set; }
        public int? NotLoadFlag { get; set; }
        public string LoadId { get; set; }

        public string SoNumber { get; set; }
        public string OceScmType { get; set; }

        //报表是装箱费 还是特费
        public string ReportType { get; set; }

        //报表是马士基 还是第三方
        public string ReportAgentType { get; set; }
        public string ContainerNumber { get; set; }
        public DateTime? BeginCreateDate { get; set; }
        public DateTime? EndCreateDate { get; set; }
        public DateTime? BeginETD { get; set; }
        public DateTime? EndETD { get; set; }
    }

    public class BillDetailResult
    {
        public string Action { get; set; }
        public string LoadId { get; set; }
        public string ContainerNumber { get; set; }
        public string ContainerType { get; set; }
        public string AgentCode { get; set; }
        public string ClientCode { get; set; }
        public string Status { get; set; }
        public string FeeStatus { get; set; }
        public int? SumQty { get; set; }
        public decimal? SumCBM { get; set; }
        public DateTime? CreateDate { get; set; }

    }


    public class BillDetailRepostResult
    {
        public int Id { get; set; }
        public string ClientCode { get; set; }
        public string LoadId { get; set; }
        public string ChargeName { get; set; }
        public string origin { get; set; }
        public string invoice_name { get; set; }
        public string invoice_no { get; set; }
        public string due_date { get; set; }
        public string consignee_name { get; set; }
        public DateTime? sending_date { get; set; }
        public DateTime? etd { get; set; }
        public string etd_show { get; set; }
        public string booking_no { get; set; }
        public string cbl { get; set; }
        public string container_no { get; set; }
        public string container_size { get; set; }
        public string tixiangdian { get; set; }
        public string jingangdian { get; set; }
        public string portsurtcase { get; set; }
        public decimal? Qty { get; set; }
        public decimal? quantity { get; set; }
        public string charge_item { get; set; }
        public string charge_code { get; set; }

        //税率
        public decimal? TaxRate { get; set; }
        public double? unit_price { get; set; }
        public double? invoice_amount { get; set; }
        public double? no_vat_amount { get; set; }
        public string currency { get; set; } 
        public string vat_rate { get; set; }
        public string fapiao_type { get; set; }
        public string booking_damco_pic { get; set; }  
        public string remark { get; set; }
        public string fcr { get; set; }
        public string application_form { get; set; }
        public string oce { get; set; }

        //仓库装箱收入
        public double warhouse_pack_fee { get; set; }
        //运输收入
        public double truck_pack_fee { get; set; }
        //仓库代垫收入
        public double warhouse_daidian_fee { get; set; }
        //运输代垫收入
        public double truck_daidian_fee { get; set; }
        //卸货费
        public double warhouse_unload_fee { get; set; }
        //超期堆存费
        public double demurrageCharge_fee { get; set; }
        //修改改挂收入
        public double other_fee { get; set; }
        //堆场收入
        public double yard_fee { get; set; }

        //堆场中控收入
        public double yardZhongKong_fee { get; set; }

        //运输收入(外包)
        public double truck_pack_outSourcing_fee { get; set; }
        //货代收入
        public double agent_fee { get; set; }
        //差额
        public string difference { get; set; }
        public string locationId { get; set; }
        public string customerName { get; set; }
        public string createUser { get; set; }

        public int? DaiDianId { get; set; }
    }

}
