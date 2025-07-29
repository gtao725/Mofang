using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WMS.BLLClass
{
  
    //电商入库订单类
    public class EclInBoundOrderModel
    {
        public string WhCode { get; set; }
        public int? ClientId { get; set; }
        public string ClientCode { get; set; }
        public string SoNumber { get; set; }
        public string OrderType { get; set; }
        public int? ProcessId { get; set; }
        public string ProcessName { get; set; }

        public List<InBoundOrderDetailInsert> InBoundOrderDetailInsert;
    }



    //电商Load类
    public class EclLoadModel
    {
        public string WhCode { get; set; }
        public string ClientCode { get; set; }
        public string EclLoadId { get; set; }
        public int ProcessId { get; set; }
        public string ProcessName { get; set; }
        public string CreateUser { get; set; }
        

        public List<EclOutOrderModel> EclOutOrderList;
    }

    //电商出货订单类
    public class EclOutOrderModel
    {
        public string OrderSource { get; set; }
        public string OrderType { get; set; }
        public string WhCode { get; set; }
        public string ClientCode { get; set; }
        public string outorder_number { get; set; }
        public string outorder_number_alt { get; set; }
        public int ProcessId { get; set; }
        public string ProcessName { get; set; }
        public string buy_name { get; set; }
        public string buy_company { get; set; }
        public string address { get; set; }
        public string CreateUser { get; set; }

        public List<EclOutOrderDetailModel> EclOutOrderDetailList;
    }

    //电商出货订单明细类
    public class EclOutOrderDetailModel
    {
        public string WhCode { get; set; }
        public string ClientCode { get; set; }
        public string so_number { get; set; }
        public string outorder_number { get; set; }
        public string Style1 { get; set; }
        public string Style2 { get; set; }
        public string item_number { get; set; }
        public int? item_id { get; set; }
        public string lot_number { get; set; }
        public string uom { get; set; }
        public int qty { get; set; }
        public decimal? price { get; set; }

    }

    public class EclOutOrderModelResult
    {
        public string outorder_number { get; set; }
        public string outorder_number_alt { get; set; }
        public string status { get; set; }
        public string remark { get; set; }
    }

}
