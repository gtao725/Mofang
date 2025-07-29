using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WMS.BLLClass
{
    //快递获取结果
    public class ExpressResult
    {
        public string Status { get; set; }
        public string OrderId { get; set; }
        public string MailNo { get; set; }
        public string FormCode { get; set; }
        public string DestCode { get; set; }
        public string Message { get; set; }
        public string ErrorCode { get; set; }

        public PackHeadJsonEntity sFGetDetailModel;

        public PackHeadJsonEntityZTO zTOGetDetailModel;

        public List<ZDExpressResult> ZDExpressResult;

        public List<RouteResponseResult> RouteResponseResult;
    }

    public class ZDExpressResult
    {
        public string MailNo_ZD { get; set; }
    }

    //路由结果实体
    public class RouteResponseResult
    {
        public string accept_time { get; set; }
        public string accept_address { get; set; }

        public string remark { get; set; }
    }

    public class RouteResponseSearch
    {
        public string CompanyCode { get; set; }
        public string TrackingNumber { get; set; }
    }

    //顺丰下单后反馈明细实体
    public class PackHeadJsonEntity
    {
        //时效类型图标
        public string proCode { get; set; }

        //时效类型文字，当图标为空时 加载文字
        public string proName { get; set; }

        //目的地,为空打印 DestCode
        public string destRouteLabel { get; set; }

        //单元区域编码,为空 则留空
        public string destTeamCode { get; set; }

        //进港信息,为空 则留空
        public string codingMapping { get; set; }

        //A标,为空 则留空
        public string xbFlag { get; set; }

        //出港信息,为空 则留空
        public string codingMappingOut { get; set; }

        //图标区域,为空 则留空
        public string printIcon { get; set; }

        //二维码
        public string twoDimensionCode { get; set; }


    }

    //顺丰快递实体
    public class SFExpressModel
    {
        public string OrderId { get; set; }
        public string CompanyCode { get; set; }
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
        public string express_type { get; set; }
        public int? parcel_quantity { get; set; }
        public string custid { get; set; }
        public decimal? cod { get; set; }
        public string j_name { get; set; }
        public string MailNo { get; set; }

        //订单确认/取消 接口所用
        public int? Dealtype { get; set; }
        public Decimal? Weight { get; set; }
        public Decimal? Volume { get; set; }
        public string issureFlag { get; set; }
        public decimal? issureMoney { get; set; }
        public string installServiceFlag { get; set; }
        public string payMethod { get; set; }
    }


    //圆通快递实体
    public class YTExpressModel
    {
        public string OrderId { get; set; }
        public string CompanyCode { get; set; }
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

        public string custid { get; set; }
        public int? parcel_quantity { get; set; }
        public string item_name { get; set; }

        public string Checkword { get; set; }
    }


    //中通快递实体
    public class ZTOExpressModel
    {
        //客户订单号
        public string partnerOrderCode { get; set; }
        //companyid
        public string companyid { get; set; }
        public string companypwd { get; set; }
        public string appserect { get; set; }
        //key
        public string key { get; set; }

        //1：普通件 4：星联全网件
        public string orderType { get; set; }

        public senderInfo senderInfo;

        public receiveInfo receiveInfo;

        //增值服务：含保价，尊享等
        public List<orderVasList> orderVasList;

    }

    //中通下单后反馈明细实体
    public class PackHeadJsonEntityZTO
    {
        public string bagAddr { get; set; }
    }

    //中通快递实体
    public class ZTOExpressModelEntity
    {
        //对接文档地址：https://op.zto.cn/#/index
        //实体文档地址：https://op.zto.cn/#/interface?resourceGroup=20&apiName=zto.open.createOrder

        public string partnerType { get; set; }
        public string orderType { get; set; }
        public string partnerOrderCode { get; set; }

        public accountInfo accountInfo;

        public senderInfo senderInfo;

        public receiveInfo receiveInfo;

        public List<orderVasList> orderVasList;

        public summaryInfo summaryInfo;
        public string siteCode { get; set; }
        public string siteName { get; set; }
        public string remark { get; set; }

    }

    //中通快递实体
    public class accountInfo
    {
        public string accountId { get; set; }
        public string type { get; set; }
        public string accountPassword { get; set; }
    }

    //中通快递实体
    public class senderInfo
    {
        public string senderPhone { get; set; }
        public string senderName { get; set; }
        public string senderAddress { get; set; }
        public string senderDistrict { get; set; }
        public string senderMobile { get; set; }
        public string senderProvince { get; set; }
        public string senderCity { get; set; }

    }

    //中通快递实体
    public class receiveInfo
    {
        public string receiverDistrict { get; set; }
        public string receiverMobile { get; set; }
        public string receiverProvince { get; set; }
        public string receiverCity { get; set; }
        public string receiverAddress { get; set; }
        public string receiverPhone { get; set; }
        public string receiverName { get; set; }

    }

    //中通快递实体
    public class orderVasList
    {
        public string vasType { get; set; }
        public string vasAmount { get; set; }

    }

    //中通快递实体
    public class summaryInfo
    {
        public string quantity { get; set; }
        public string premium { get; set; }
        public string price { get; set; }
        public string otherCharges { get; set; }
        public string freight { get; set; }
        public string packCharges { get; set; }
        public string startTime { get; set; }
        public string endTime { get; set; }
        public string orderSum { get; set; }

    }


    //中通快递实体
    public class ZTOExpressResult
    {
        public string message { get; set; }
        public string statusCode { get; set; }
        public string status { get; set; }

        public result result;

    }

    //中通快递实体
    public class result
    {
        public bigMarkInfo bigMarkInfo;
        public string siteCode { get; set; }
        public string SiteName { get; set; }
        public string orderCode { get; set; }
        public string billCode { get; set; }
        public string partnerOrderCode { get; set; }

        public signBillInfo signBillInfo;

    }

    //中通快递实体
    public class bigMarkInfo
    {
        public string bagAddr { get; set; }
        public string mark { get; set; }
    }
    //中通快递实体
    public class signBillInfo
    {
        public string siteCode { get; set; }

    }


    //顺丰快递API实体
    public class SFExpressAPIModel
    {
        //订单号
        public string orderId { get; set; }

        //使用预留的运单号时传入。只接受传入母单号。
        public string waybillNo { get; set; }
        //使用预留的运单号时 填0。是否生成运单号：默认为1 生成运单号
        public int isGenBillNo { get; set; }
        //使用预留的运单号时，该字段为传入子单号。
        public string subWaybills { get; set; }

        //月结账号
        public string customId { get; set; }


        //寄件人信息
        public string sendCompany { get; set; }
        public string sendContact { get; set; }
        public string sendMobile { get; set; }
        public string sendTel { get; set; }
        public string sendProvince { get; set; }
        public string sendCity { get; set; }
        public string sendCounty { get; set; }
        public string sendAddress { get; set; }


        //收件人信息
        public string deliveryCompany { get; set; }
        public string deliveryContact { get; set; }
        public string deliveryProvince { get; set; }
        public string deliveryCity { get; set; }
        public string deliveryCounty { get; set; }
        public string deliveryAddress { get; set; }
        public string deliveryMobile { get; set; }
        public string deliveryTel { get; set; }



        public int parcelQuantity { get; set; }

        //支付方式 1.寄方付2.收方付3.第三方付
        public int payMethod { get; set; }

        //是否下call，0-不下call，不会通知小哥上门
        public int isDoCall { get; set; }

        public string cargoName { get; set; }
        //货物名称
        public List<cargoList> cargoList;

        public List<AdditionServices> AdditionServices;
    }


    //顺丰快递API实体
    public class SFExpressDownUrlModel
    {
        public string templateCode { get; set; }
        public string version { get; set; }
        public string fileType { get; set; }
        public string sync { get; set; }
        public string customTemplateCode { get; set; }



        public List<documents> documents;

    }
    public class documents
    {
        public string masterWaybillNo { get; set; }
        public string waybillNoCheckType { get; set; }
        public string waybillNoCheckValue { get; set; }
        public customData customData { get; set; }
    }

    public class customData
    {
        public string Remark { get; set; }
    }

    public class cargoList
    {
        public string name { get; set; }
    }

    public class AdditionServices
    {
        public string name { get; set; }

        public value5 value5;
    }

    public class value5
    {
        public int serviceType { get; set; }

        public List<serviceItemInfos> serviceItemInfos;
    }

    public class serviceItemInfos
    {
        public int count { get; set; }
        public string standServiceCode { get; set; }
        public string standServiceName { get; set; }
        public string cusServiceCode { get; set; }
        public string cusServiceName { get; set; }
    }


    public class SFExpressAPIResult
    {
        public string apiResponseID { get; set; }
        public string apiResultCode { get; set; }

        public apiResultData apiResultData;

    }

    public class apiResultData
    {
        public string errorCode { get; set; }
        public string errorMessage { get; set; }
        public Boolean success { get; set; }

    }

    public class SFExpressAPIResult1
    {
        public SFExpressAPIobj obj { get; set; }
    }

    public class SFExpressAPIobj
    {
        public string orderId { get; set; }
        public string destCode { get; set; }
        public string waybillNo { get; set; }

        public string appendedSubWaybillNos { get; set; }
        public string subWaybillNos { get; set; }
        public SFExpressAPIrlsInfo rlsInfo { get; set; }
    }

    public class SFExpressAPIrlsInfo
    {
        public PackHeadJsonEntity rlsDetail { get; set; }
    }


    public class SFExpressAPIZDModel
    {
        //订单号
        public string orderId { get; set; }

        public int count { get; set; }

    }
}
