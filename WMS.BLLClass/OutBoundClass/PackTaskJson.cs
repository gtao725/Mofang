using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WMS.BLLClass
{
    public class PackTaskJsonEntity
    {
        public string WhCode { get; set; }

        //OMS的outorder_number
        public string CustomerOutPoNumber { get; set; }

        //OMS的outorder_number_alt
        public string AltCustomerOutPoNumber { get; set; }

        //OMS的customer_po
        public string customerPo { get; set; }

        //客户名
        public string ClientCode { get; set; }

        //运输类型 是物流 还是快递
        public string TransportType { get; set; }

        //寄件人信息
        public string j_company { get; set; }
        public string j_contact { get; set; }
        public string j_tel { get; set; }
        public string j_province { get; set; }
        public string j_city { get; set; }
        public string j_county { get; set; }
        public string j_address { get; set; }

        //收件人信息
        public string d_company { get; set; }
        public string d_contact { get; set; }
        public string d_tel { get; set; }
        public string d_Province { get; set; }
        public string d_city { get; set; }
        public string d_address { get; set; }

        //快递公司代码 SF YT
        public string express_code { get; set; }
        //快递单类型
        public string express_type { get; set; }
        //快递单类型中文
        public string express_type_zh { get; set; }
        //包裹数 默认1
        public int parcel_quantity { get; set; }
        //寄件方公司月结帐号
        public string companyCode { get; set; }
        //公司客户的月结卡号
        public string custid { get; set; }
        //货到付款金额
        public string cod { get; set; }
        //货物名称 品名
        public string j_name { get; set; }
        //寄件地code
        public string form_code { get; set; }
        //收件地code
        public string dest_code { get; set; }
        //是否航空  
        public string AirFlag { get; set; }

        //是否允许多个包装 0不允许 1允许
        public int? PackMoreFlag { get; set; }
        //是否是单品包装 0不是 1是单品
        public int? SingleFlag { get; set; }
        //是否子母单 0不是 1是
        public int? ZdFlag { get; set; }
        //电子面单模版名称
        public string SinglePlaneTemplate { get; set; }
        //随箱单模版名称
        public string PackingListTemplate { get; set; }

        //订单创建时间
        public DateTime? OrderCreateDate { get; set; }
        //密钥
        public string Checkword { get; set; }

        public string issureFlag { get; set; }
        public decimal? issureMoney { get; set; }
        public string ExpressNumber { get; set; }

        //寄件方公司月结密码
        public string companyPwd { get; set; }

        //订单类型
        public string BusinessMode { get; set; }
        //证书收件人
        public string CustomerName { get; set; }
        //证书编号
        public string CustomerRef { get; set; }



        //云打印对象字段
        //面单Base64图片数据
        public string Base64CryDate { get; set; }
        //运单号
        public string TrackNo { get; set; }
        //EdiJson
        public string EdiJson { get; set; }
        //签名
        public string Sign { get; set; }

        public string installServiceFlag { get; set; }

        public string OutOrderSourceToCloudPrint { get; set; }
        public string payMethod { get; set; }
        public string ServiceCode { get; set; }

        //门店名称
        public string CompanyName { get; set; }

        //博士所需对象

        public BoschPackEntity boschPackEntity;

    }


}
