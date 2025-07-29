using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WMS.BLLClass
{

    //保税区入库订单类
    public class BsInBoundOrderModel
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



    //保税区Load类
    public class BsLoadModel
    {
        public string WhCode { get; set; }
        public string ClientCode { get; set; }
        public string BsLoadId { get; set; }
        public int ProcessId { get; set; }
        public string ProcessName { get; set; }
        public string CreateUser { get; set; }


        public List<BsOutOrderModel> BsOutOrderList;
    }

    //保税区出货订单类
    public class BsOutOrderModel
    {
        public string WhCode { get; set; }
        public string ClientCode { get; set; }
        public string outorder_number { get; set; }



        public List<BsOutOrderDetailModel> BsOutOrderDetailList;
    }

    //保税区出货订单明细类
    public class BsOutOrderDetailModel
    {
        public string WhCode { get; set; }
        public string ClientCode { get; set; }
        public string SoNumber { get; set; }
        public string CustomerPoNumber { get; set; }
        public string outorder_number { get; set; }
        public string item_number { get; set; }
        public int? item_id { get; set; }

        public string Style1 { get; set; }
        public string Style2 { get; set; }
        public string Style3 { get; set; }
        public string lot_number { get; set; }
        public string uom { get; set; }
        public int qty { get; set; }

        public int? LoadSeq { get; set; }

    }


}
