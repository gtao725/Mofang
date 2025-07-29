using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SFExpress;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml;
using WMS.BLLClass;
using ZTOExpress;

namespace WMS.Express
{
    public class ExpressManager
    {
        #region 顺丰快递

        string errorMessage = "";

        /// <summary>
        /// 获取顺丰快递单或 母单号
        /// </summary>
        /// <param name="entity">顺丰实体</param>
        /// <returns>快递单获取结果</returns>
        public ExpressResult GetExpress(SFExpressModel entity)
        {
            //1. 拼接顺丰对接XML
            string xml = "";
            //顺丰是否保价
            if (entity.issureFlag == "1")
            {
                xml = "<?xml version='1.0' encoding='UTF-8'?>" +
                    "<Request service='OrderService' lang='zh-CN'>" +
                    "<Head>" + entity.CompanyCode + "</Head>" +
                    "<Body><Order" +
                    " orderid='" + entity.OrderId + "'" +
                    " is_gen_bill_no='1'" +
                    " j_company='" + entity.j_company + "'" +
                    " j_contact='" + entity.j_contact + "'" +
                    " j_tel='" + entity.j_tel + "'" +
                    " j_province='" + entity.j_province + "'" +
                    " j_city='" + entity.j_city + "'" +
                    " j_county='" + entity.j_county + "'" +
                    " j_address='" + entity.j_address + "'" +
                    " d_province='" + entity.d_Province + "'" +
                    " d_city='" + entity.d_city + "'" +
                    " d_company='" + entity.d_company + "'" +
                    " d_contact='" + entity.d_contact + "'" +
                    " d_tel='" + entity.d_tel + "'" +
                    " d_address='" + entity.d_address + "'" +
                    " express_type='" + entity.express_type + "'" +
                    " parcel_quantity='1'" +
                    " cargo_total_weight='" + entity.Weight + "'" +
                    " pay_method='" + entity.payMethod + "'" +
                    //跨境件需传以下俩个参数
                    //" declared_value = '1'" +
                    //" declared_value_currency = 'CNY'" +
                    " routelabelService='1'" +
                    " routelabelForReturn='1'" +
                    " is_unified_waybill_no='1'" +
                    " custid='" + entity.custid + "'" + ">" +
                    "<Cargo name='" + entity.j_name + "'" + ">" +
                    "</Cargo>" +
                    "<AddedService name='INSURE' value='" + entity.issureMoney + "'" + ">" +
                    "</AddedService>" +
                    "</Order></Body></Request>";
            }
            else
            {
                //是否到付
                if (entity.payMethod == "2")
                {
                    xml = "<?xml version='1.0' encoding='UTF-8'?>" +
                   "<Request service='OrderService' lang='zh-CN'>" +
                   "<Head>" + entity.CompanyCode + "</Head>" +
                   "<Body><Order" +
                   " orderid='" + entity.OrderId + "'" +
                   " is_gen_bill_no='1'" +
                   " j_company='" + entity.j_company + "'" +
                   " j_contact='" + entity.j_contact + "'" +
                   " j_tel='" + entity.j_tel + "'" +
                   " j_province='" + entity.j_province + "'" +
                   " j_city='" + entity.j_city + "'" +
                   " j_county='" + entity.j_county + "'" +
                   " j_address='" + entity.j_address + "'" +
                   " d_province='" + entity.d_Province + "'" +
                   " d_city='" + entity.d_city + "'" +
                   " d_company='" + entity.d_company + "'" +
                   " d_contact='" + entity.d_contact + "'" +
                   " d_tel='" + entity.d_tel + "'" +
                   " d_address='" + entity.d_address + "'" +
                   " express_type='2'" +
                   " parcel_quantity='1'" +
                   " cargo_total_weight='" + entity.Weight + "'" +
                   " pay_method='" + entity.payMethod + "'" +
                    //跨境件需传以下俩个参数
                    //" declared_value = '1'" +
                    //" declared_value_currency = 'CNY'" +
                   " routelabelService='1'" +
                   " routelabelForReturn='1'" +
                   " is_unified_waybill_no='1'" +
                   " " + ">" +
                   "<Cargo name='" + entity.j_name + "'" + ">" +
                   "</Cargo>" +
                   "</Order></Body></Request>";
                }
                else
                {
                    //非到付
                    xml = "<?xml version='1.0' encoding='UTF-8'?>" +
                    "<Request service='OrderService' lang='zh-CN'>" +
                    "<Head>" + entity.CompanyCode + "</Head>" +
                    "<Body><Order" +
                    " orderid='" + entity.OrderId + "'" +
                    " is_gen_bill_no='1'" +
                    " j_company='" + entity.j_company + "'" +
                    " j_contact='" + entity.j_contact + "'" +
                    " j_tel='" + entity.j_tel + "'" +
                    " j_province='" + entity.j_province + "'" +
                    " j_city='" + entity.j_city + "'" +
                    " j_county='" + entity.j_county + "'" +
                    " j_address='" + entity.j_address + "'" +
                    " d_province='" + entity.d_Province + "'" +
                    " d_city='" + entity.d_city + "'" +
                    " d_company='" + entity.d_company + "'" +
                    " d_contact='" + entity.d_contact + "'" +
                    " d_tel='" + entity.d_tel + "'" +
                    " d_address='" + entity.d_address + "'" +
                    " express_type='" + entity.express_type + "'" +
                    " parcel_quantity='1'" +
                    " cargo_total_weight='" + entity.Weight + "'" +
                    //跨境件需传以下俩个参数
                    //" declared_value = '1'" +
                    //" declared_value_currency = 'CNY'" +
                    " routelabelService='1'" +
                    " routelabelForReturn='1'" +
                    " is_unified_waybill_no='1'" +
                    " custid='" + entity.custid + "'" + ">" +
                    "<Cargo name='" + entity.j_name + "'" + ">" +
                    "</Cargo>" +
                    "</Order></Body></Request>";
                }
            }


            //2.webservice 传递数据后 获取返回的xml
            string srcString = SFApiConnect(xml);

            //string srcString = SFApiConnectTS(xml);


            //3.处理返回的xml 并填充   快递结果类ExpressResult
            ExpressResult expressResult = new ExpressResult();      // 快递结果类ExpressResult
            XmlDocument doc = new XmlDocument();
            //创建XML文档对象 
            if (!string.IsNullOrEmpty(srcString))
            {
                //加载xml字符串 
                doc.LoadXml(srcString);

                //查询状态信息 
                XmlNode res = doc.SelectSingleNode(@"Response/Head");
                string status = res.InnerText.ToString();

                if (status == "OK")
                {
                    //XmlNode OrderResponse1 = doc.SelectSingleNode(@"Response/Body");

                    XmlNode OrderResponse = doc.SelectSingleNode(@"Response/Body/OrderResponse");

                    //如果出现filter_result 表示异常
                    if (OrderResponse.Attributes["filter_result"] != null)
                    {
                        string filter_result = OrderResponse.Attributes["filter_result"].Value;
                        if (filter_result == "3")
                        {
                            if (OrderResponse.Attributes["remark"] != null)
                            {
                                string remark = OrderResponse.Attributes["remark"].Value;
                                string remarkResult = "";
                                if (remark == "1")
                                {
                                    remarkResult = "快件不可以收派：收方超范围";
                                }
                                else if (remark == "2")
                                {
                                    remarkResult = "快件不可以收派：派方超范围";
                                }
                                else if (remark == "3")
                                {
                                    remarkResult = "快件不可以收派：其它原因";
                                }

                                expressResult.Status = "ERR";
                                expressResult.ErrorCode = "不可以收派";
                                expressResult.Message = "快递单获取失败！" + remarkResult;
                                return expressResult;
                            }
                        }
                    }

                    //为实体赋值
                    expressResult.Status = status;

                    expressResult.OrderId = OrderResponse.Attributes["orderid"].Value;
                    expressResult.MailNo = OrderResponse.Attributes["mailno"].Value;
                    //expressResult.FormCode = "021VH";  //默认值,先不用系统的

                    if (OrderResponse.Attributes["destcode"] != null)
                        expressResult.DestCode = OrderResponse.Attributes["destcode"].Value;

                    if (expressResult.DestCode != null)
                    {
                        expressResult.Message = "快递单获取成功！";
                    }
                    else
                    {

                        expressResult.Message = "快递单获取成功!快递单无法获取CODE！";
                    }

                    XmlNode OrderResponseDetail = doc.SelectSingleNode(@"Response/Body/OrderResponse/rls_info/rls_detail");

                    if (OrderResponseDetail != null)
                    {
                        PackHeadJsonEntity detailModel = new PackHeadJsonEntity();

                        if (OrderResponseDetail.Attributes.Count > 0)
                        {
                            if (OrderResponseDetail.Attributes["proCode"] != null)
                            {
                                detailModel.proCode = OrderResponseDetail.Attributes["proCode"].Value;
                            }
                            else
                            {
                                detailModel.proCode = "";
                            }

                            if (OrderResponseDetail.Attributes["proName"] != null)
                            {
                                detailModel.proName = OrderResponseDetail.Attributes["proName"].Value;
                            }
                            else
                            {
                                detailModel.proName = "";
                            }

                            if (OrderResponseDetail.Attributes["destRouteLabel"] != null)
                            {
                                detailModel.destRouteLabel = OrderResponseDetail.Attributes["destRouteLabel"].Value;
                            }
                            else
                            {
                                detailModel.destRouteLabel = "";
                            }

                            if (OrderResponseDetail.Attributes["destTeamCode"] != null)
                            {
                                detailModel.destTeamCode = OrderResponseDetail.Attributes["destTeamCode"].Value;
                            }
                            else
                            {
                                detailModel.destTeamCode = "";
                            }

                            if (OrderResponseDetail.Attributes["codingMapping"] != null)
                            {
                                detailModel.codingMapping = OrderResponseDetail.Attributes["codingMapping"].Value;
                            }
                            else
                            {
                                detailModel.codingMapping = "";
                            }

                            if (OrderResponseDetail.Attributes["xbFlag"] != null)
                            {
                                detailModel.xbFlag = OrderResponseDetail.Attributes["xbFlag"].Value;
                            }
                            else
                            {
                                detailModel.xbFlag = "";
                            }

                            if (OrderResponseDetail.Attributes["codingMappingOut"] != null)
                            {
                                detailModel.codingMappingOut = OrderResponseDetail.Attributes["codingMappingOut"].Value;
                            }
                            else
                            {
                                detailModel.codingMappingOut = "";
                            }

                            if (OrderResponseDetail.Attributes["printIcon"] != null)
                            {
                                detailModel.printIcon = OrderResponseDetail.Attributes["printIcon"].Value;
                            }
                            else
                            {
                                detailModel.printIcon = "";
                            }

                            if (OrderResponseDetail.Attributes["twoDimensionCode"] != null)
                            {
                                detailModel.twoDimensionCode = OrderResponseDetail.Attributes["twoDimensionCode"].Value;
                            }
                            else
                            {
                                detailModel.twoDimensionCode = "";
                            }
                        }

                        expressResult.sFGetDetailModel = detailModel;
                    }
                    else
                    {
                        expressResult.sFGetDetailModel = null;
                    }

                }
                else if (status == "ERR")
                {
                    XmlNode OrderResponse = doc.SelectSingleNode(@"Response/ERROR");
                    //为实体赋值
                    errorMessage = OrderResponse.Attributes["code"].Value + "$" + OrderResponse.InnerText.ToString();

                    //为实体赋值
                    expressResult.Status = status;
                    expressResult.ErrorCode = OrderResponse.Attributes["code"].Value;

                    if (expressResult.ErrorCode == "8196")
                    {
                        expressResult.Message = "快递单获取失败！8196" + "$信息异常，手机号黑名单请更换联系方式！";
                    }
                    else
                    {
                        expressResult.Message = "快递单获取失败！" + errorMessage;
                    }


                    ExpressResult expressResult1 = SearchSFExpress(entity);      //查询顺丰快递单
                    if (expressResult1.Status == "OK")
                    {
                        expressResult = expressResult1;
                    }
                }
            }

            return expressResult;
        }


        /// <summary>
        /// 查询顺丰快递单
        /// </summary>
        /// <param name="entity">顺丰实体</param>
        /// <returns>快递单获取结果</returns>
        public ExpressResult SearchSFExpress(SFExpressModel entity)
        {
            StringBuilder strXML = new StringBuilder();
            strXML.Append("<?xml version='1.0' encoding='UTF-8'?><Request service='OrderSearchService' lang='zh-CN'>");//申请服务及语言
            strXML.Append("<Head>" + entity.CompanyCode + "</Head>"); //客户编码
            strXML.Append("<Body> <OrderSearch   orderid='" + entity.OrderId + "' />");
            strXML.Append("</Body></Request>");

            string xml = strXML.ToString();

            //2.webservice 传递数据后 获取返回的xml
            string srcString = SFApiConnect(xml);

            ExpressResult expressResult = new ExpressResult();
            XmlDocument doc = new XmlDocument();
            //创建XML文档对象 
            if (!string.IsNullOrEmpty(srcString))
            {
                //加载xml字符串 
                doc.LoadXml(srcString);

                //查询状态信息 
                XmlNode res = doc.SelectSingleNode(@"Response/Head");
                string status = res.InnerText.ToString();

                if (status == "OK")
                {
                    XmlNode OrderResponse = doc.SelectSingleNode(@"Response/Body/OrderResponse");

                    //为实体赋值
                    expressResult.Status = status;

                    expressResult.OrderId = OrderResponse.Attributes["orderid"].Value;
                    expressResult.MailNo = OrderResponse.Attributes["mailno"].Value;
                    //expressResult.FormCode = "021VH";  //默认值,先不用系统的

                    if (OrderResponse.Attributes["destcode"] != null)
                        expressResult.DestCode = OrderResponse.Attributes["destcode"].Value;

                    if (expressResult.DestCode != null && !string.IsNullOrEmpty(expressResult.DestCode))
                    {
                        expressResult.Message = "快递单获取成功！";
                    }
                    else
                    {
                        expressResult.Status = "ERR";
                        expressResult.Message = "快递单地址获取CODE错误！";
                    }

                    expressResult.sFGetDetailModel = null;
                }
                else if (status == "ERR")
                {
                    XmlNode OrderResponse = doc.SelectSingleNode(@"Response/ERROR");
                    //为实体赋值
                    expressResult.Status = status;
                    expressResult.ErrorCode = OrderResponse.Attributes["code"].Value;
                    expressResult.Message = "快递单获取失败！" + errorMessage + expressResult.ErrorCode + "$" + OrderResponse.InnerText.ToString();
                }
            }

            return expressResult;
        }

        /// <summary>
        /// 获取顺丰子单号
        /// </summary>
        /// <param name="entity">顺丰实体</param>
        /// <returns>快递单获取结果</returns>
        public ExpressResult GetZDExpress(SFExpressModel entity)
        {
            //1. 拼接顺丰对接XML
            string xml = "<?xml version='1.0' encoding='UTF-8'?>" +
                "<Request service='OrderZDService' lang='zh-CN'>" +
                "<Head>" + entity.CompanyCode + "</Head>" +
                "<Body><OrderZD" +
                " orderid='" + entity.OrderId + "'" +
                " parcel_quantity='" + entity.parcel_quantity + "' />" +
                "</Body></Request>";

            //2.webservice 传递数据后 获取返回的xml
            string srcString = SFApiConnect(xml);

            ExpressResult expressResult = new ExpressResult();
            List<ZDExpressResult> ZDExpressResultList = new List<ZDExpressResult>();

            XmlDocument doc = new XmlDocument();
            //创建XML文档对象 
            if (!string.IsNullOrEmpty(srcString))
            {
                //加载xml字符串 
                doc.LoadXml(srcString);

                //查询状态信息 
                XmlNode res = doc.SelectSingleNode(@"Response/Head");
                string status = res.InnerText.ToString();

                if (status == "OK")
                {
                    //为实体赋值
                    expressResult.Status = status;

                    //解析子母单成功数据
                    //数据实例：
                    //<Response service="OrderZDService"><Head> OK </Head><Body><OrderZDResponse><OrderZDResponse main_mailno = "444003078326" mailno_zd = "003971777577,003971777568" orderid = "TE201500104" /> </ OrderZDResponse ></ Body ></ Response >

                    //得到OrderZDResponse 节点
                    XmlNode OrderResponse = doc.SelectSingleNode(@"Response/Body/OrderZDResponse");

                    //获取 OrderZDResponse节点下的name及value
                    XmlNodeList OrderList = OrderResponse.ChildNodes;

                    string main_mailno = "", orderid = "";
                    foreach (XmlNode OrderZDResponse in OrderList)
                    {
                        orderid = OrderZDResponse.Attributes["orderid"].Value;          //订单
                        main_mailno = OrderZDResponse.Attributes["main_mailno"].Value;   //母单号
                        string mailno_zd = OrderZDResponse.Attributes["mailno_zd"].Value;      //子单号

                        string[] mailno = mailno_zd.Split(',');     //子单号进行赋值操作  
                        for (int i = 0; i < mailno.Count(); i++)
                        {
                            ZDExpressResult zdExpressResult = new ZDExpressResult();
                            zdExpressResult.MailNo_ZD = mailno[i].ToString();
                            ZDExpressResultList.Add(zdExpressResult);
                        }
                    }

                    expressResult.OrderId = orderid;
                    expressResult.MailNo = main_mailno;
                    expressResult.FormCode = "021VH";  //默认值,先不用系统的              
                    expressResult.ZDExpressResult = ZDExpressResultList;    //子单号列表
                    expressResult.Message = "子母单获取成功！";

                }
                else if (status == "ERR")
                {
                    //为实体赋值
                    XmlNode OrderResponse = doc.SelectSingleNode(@"Response/ERROR");
                    //为实体赋值
                    expressResult.Status = status;
                    expressResult.ErrorCode = OrderResponse.Attributes["code"].Value;
                    expressResult.Message = "子母单获取失败！" + errorMessage + expressResult.ErrorCode + "$" + OrderResponse.InnerText.ToString();
                }
            }

            return expressResult;
        }

        //顺丰API连接
        private static string SFApiConnect(string xml)
        {
            SFExpressService.CommonExpressServiceService sf = new SFExpressService.CommonExpressServiceService();
            string Checkword = "5dggDYWqBCtmcGWRhmhbb7qhAkcVhrwr";
            //string Checkword = "1gEpwT45JFUkU4yB";
            string verifyCode = MD5ToBase64String(xml + Checkword);     //生成verifyCode
            string srcString = sf.sfexpressService(xml, verifyCode);
            return srcString;
        }

        private static string SFApiConnectTS(string xml)
        {
            SFExpressServiceTS.CommonExpressServiceService sf = new SFExpressServiceTS.CommonExpressServiceService();
            string Checkword = "263TWOY3DDAQE39X";
            //string Checkword = "1gEpwT45JFUkU4yB";
            string verifyCode = MD5ToBase64String(xml + Checkword);     //生成verifyCode
            string srcString = sf.sfexpressService(xml, verifyCode);
            return srcString;
        }

        /// <summary>
        /// 订单确认/取消 接口
        /// </summary>
        /// <param name="entity">顺丰实体 其中dealtype=1是订单确认，dealtype=2是订单取消</param>
        /// <returns>响应结果</returns>
        public ExpressResult OrderConfirm(SFExpressModel entity)
        {
            string xmlWhere = "";
            if (entity.Weight > 0)
            {
                xmlWhere = "<OrderConfirmOption weight = '" + entity.Weight + "' volume ='" + entity.Volume + "'/> ";
            }
            //1. 拼接顺丰对接XML
            string xml = "<?xml version='1.0' encoding='UTF-8'?>" +
                "<Request service='OrderConfirmService' lang='zh-CN'>" +
                "<Head>" + entity.CompanyCode + "</Head>" +
                "<Body><OrderConfirm" +
                " orderid='" + entity.OrderId + "'" +
                " mailno='" + entity.MailNo + "'" +
                " dealtype='" + entity.Dealtype + "' >" +
                xmlWhere +
                "</OrderConfirm></ Body></Request>";

            //2.webservice 传递数据后 获取返回的xml
            string srcString = SFApiConnect(xml);

            ExpressResult expressResult = new ExpressResult();
            List<ZDExpressResult> ZDExpressResultList = new List<ZDExpressResult>();

            XmlDocument doc = new XmlDocument();
            //创建XML文档对象 
            if (!string.IsNullOrEmpty(srcString))
            {
                //加载xml字符串 
                doc.LoadXml(srcString);

                //查询状态信息 
                XmlNode res = doc.SelectSingleNode(@"Response/Head");
                string status = res.InnerText.ToString();

                if (status == "OK")
                {
                    //为实体赋值
                    expressResult.Status = status;

                    //解析子母单成功数据
                    //数据实例：
                    //<Response service="OrderConfirmService">< Head > OK </ Head >< Body >< OrderConfirmResponse orderid = "TE201500104" mailno = "444003078326" res_status = "2" /> </ Body > </ Response >

                    //得到 节点信息
                    XmlNode OrderResponse = doc.SelectSingleNode(@"Response/Body/OrderConfirmResponse");

                    //为实体赋值
                    expressResult.Status = status;
                    expressResult.OrderId = OrderResponse.Attributes["orderid"].Value;
                    expressResult.MailNo = OrderResponse.Attributes["mailno"].Value;
                    expressResult.Message = "订单确认成功！";

                }
                else if (status == "ERR")
                {
                    XmlNode OrderResponse = doc.SelectSingleNode(@"Response/ERROR");
                    //为实体赋值
                    expressResult.Status = status;
                    expressResult.ErrorCode = OrderResponse.Attributes["code"].Value;
                    expressResult.Message = expressResult.ErrorCode + "-" + OrderResponse.InnerText.ToString();
                }
            }

            return expressResult;
        }

        /// <summary>
        /// 顺丰路由查询
        /// </summary>
        /// <param name="entity">顺丰实体</param>
        /// <returns>响应结果</returns>
        public ExpressResult RouteResponseSF(RouteResponseSearch entity)
        {

            //1. 拼接顺丰对接XML
            string xml = "<?xml version='1.0' encoding='UTF-8'?>" +
                "<Request service='RouteService' lang='zh-CN'>" +
                "<Head>" + entity.CompanyCode + "</Head>" +
                "<Body><RouteRequest" +
                " tracking_type='1'" +
                " method_type='1'" +
                " tracking_number='" + entity.TrackingNumber + "' />" +
                "</ Body></Request>";

            //2.webservice 传递数据后 获取返回的xml
            string srcString = SFApiConnect(xml);

            ExpressResult expressResult = new ExpressResult();
            List<ZDExpressResult> ZDExpressResultList = new List<ZDExpressResult>();

            XmlDocument doc = new XmlDocument();
            //创建XML文档对象 
            if (!string.IsNullOrEmpty(srcString))
            {
                //加载xml字符串 
                doc.LoadXml(srcString);

                //查询状态信息 
                XmlNode res = doc.SelectSingleNode(@"Response/Head");
                string status = res.InnerText.ToString();

                if (status == "OK")
                {
                    //解析成功数据
                    //数据实例：
                    //<Response service="RouteService">< Head > OK </ Head >< Body >< RouteResponse mailno = "444003077898" >< Route accept_time = "2015-01-04 10:11:26" accept_address = "深圳" remark = "已收件" opcode = "50" /> < Route accept_time = "2015-01-05 17:41:50" remark = "此件签单返还的单号为 123638813180" opcode = "922" /> </ RouteResponse > </ Body ></ Response >

                    //得到 节点信息
                    XmlNode OrderResponse = doc.SelectSingleNode(@"Response/Body/RouteResponse");

                    List<RouteResponseResult> RouteResponseResultList = new List<RouteResponseResult>();

                    foreach (XmlNode RouteResponseNode in OrderResponse)
                    {
                        expressResult.MailNo = RouteResponseNode.Attributes["mailno"].Value;

                        RouteResponseResult routeResponseResult = new RouteResponseResult();

                        routeResponseResult.accept_time = RouteResponseNode.Attributes["accept_time"].Value;
                        routeResponseResult.accept_address = RouteResponseNode.Attributes["accept_address"].Value;
                        routeResponseResult.remark = RouteResponseNode.Attributes["remark"].Value;

                        RouteResponseResultList.Add(routeResponseResult);
                    }
                    //为实体赋值
                    expressResult.Status = status;
                    expressResult.RouteResponseResult = RouteResponseResultList;
                    expressResult.Message = "查询成功！";

                }
                else if (status == "ERR")
                {
                    XmlNode OrderResponse = doc.SelectSingleNode(@"Response/ERROR");
                    //为实体赋值
                    expressResult.Status = status;
                    expressResult.ErrorCode = OrderResponse.Attributes["code"].Value;
                    expressResult.Message = expressResult.ErrorCode + "-" + OrderResponse.InnerText.ToString();
                }
            }

            return expressResult;
        }


        #endregion



        #region 圆通快递

        /// <summary>
        /// 获取圆通快递单
        /// </summary>
        /// <param name="entity">圆通实体</param>
        /// <returns>快递单获取结果</returns>
        public ExpressResult GetExpress(YTExpressModel entity)
        {
            //1. 拼接圆通对接XML

            string xml = "<RequestOrder>" +
                "<clientID>" + entity.CompanyCode + "</clientID>" +
                "<logisticProviderID>YTO</logisticProviderID>" +
                " <customerId>" + entity.custid + "</customerId>" +
                " <txLogisticID>" + entity.OrderId + "</txLogisticID>" +
                " <tradeNo>1</tradeNo>" +
                " <totalServiceFee>1</totalServiceFee>" +
                " <codSplitFee>1</codSplitFee>" +
                " <orderType>1</orderType>" +
                " <flag>1</flag>" +
                " <sender>" +
                " <name>" + entity.j_contact + "</name><postCode></postCode>" +
                " <phone>" + entity.j_tel + "</phone>" +
                " <mobile></mobile>" +
                " <prov>" + entity.j_province + "</prov>" +
                " <city>" + entity.j_city + "," + entity.j_county + "</city>" +
                " <address>" + entity.j_address + "</address>" +
                " </sender>" +
                " <receiver>" +
                " <name>" + entity.d_contact + "</name>" +
                " <postCode>0</postCode>" +
                " <phone>" + entity.d_tel + "</phone>" +
                " <prov>" + entity.d_Province + "</prov>" +
                " <city>" + entity.d_city + "</city>" +
                " <address>" + entity.d_address + "</address>" +
                " </receiver>" +
                " <sendStartTime>" + DateTime.Now + "</sendStartTime>" +
                " <sendEndTime>" + DateTime.Now + "</sendEndTime>" +
                " <goodsValue></goodsValue>" +
                " <items>" +
                " <item>" +
                " <itemName>" + entity.item_name + "</itemName>" +
                " <number>" + entity.parcel_quantity + "</number>" +
                " <itemValue>0</itemValue>" +
                " </item>" +
                " </items>" +
                " <insuranceValue></insuranceValue>" +
                " <special></special>" +
                " <remark></remark>" +
                " </RequestOrder>";

            //2.传递数据后 获取返回的xml

            string srcString = YTApiConnect(xml, entity.custid, entity.Checkword);

            //3.处理返回的xml 并填充快递结果类ExpressResult

            ExpressResult expressResult = new ExpressResult();
            XmlDocument doc = new XmlDocument();

            //创建XML文档对象 
            if (!string.IsNullOrEmpty(srcString))
            {
                //加载xml字符串 
                doc.LoadXml(srcString);

                //查询状态信息 
                XmlNode res = doc.SelectSingleNode(@"Response/success");
                string status = res.InnerText.ToString();

                if (status == "true")
                {
                    XmlNode txLogisticID = doc.SelectSingleNode(@"Response/txLogisticID");
                    XmlNode mailno = doc.SelectSingleNode(@"Response/mailNo");
                    XmlNode Address = doc.SelectSingleNode(@"Response/distributeInfo/shortAddress");

                    //为实体赋值
                    expressResult.Status = "OK";
                    expressResult.OrderId = txLogisticID.InnerText;
                    expressResult.MailNo = mailno.InnerText;
                    string shortAddress = Address.InnerText.ToString();

                    if (shortAddress + "" != "")
                    {
                        expressResult.DestCode = shortAddress;
                        expressResult.Message = "快递单获取成功！";
                    }
                    else
                    {
                        expressResult.Status = "ERR";
                        expressResult.Message = "快递单地址获取CODE错误！";
                    }

                }
                else
                {
                    expressResult.Status = "ERR";
                    expressResult.Message = doc.SelectSingleNode(@"Response/reason").InnerText.ToString();
                }

            }

            return expressResult;
        }


        /// <summary>
        /// 圆通路由查询
        /// </summary>
        /// <param name="entity">圆通实体</param>
        /// <returns>返回结果</returns>
        public ExpressResult RouteResponseYT(RouteResponseSearch entity)
        {
            StringBuilder strXML = new StringBuilder();
            strXML.Append("<?xml version=\"1.0\"?>");//申请服务及语言
            strXML.Append("<ufinterface>"); // 
            strXML.Append("<Result>"); // 
            strXML.Append("<WaybillCode>"); // 
            strXML.Append("<Number>" + entity.TrackingNumber + "</Number>"); // 
            strXML.Append("</WaybillCode></Result></ufinterface>");

            string STUrl = "http://58.32.246.70:8002";
            string app_key = "FxLoGI";
            string Format = "XML";
            string method = "yto.Marketing.WaybillTrace";
            string user_id = "Oceaneast";
            string Secret_Key = "CSaSsX";
            string timestamp = DateTime.Now.ToString();

            string Paramet = "app_key=" + app_key + "&format=" + Format + "&method="
                + method + "&timestamp=" + timestamp.Replace("/", "-")
                             + "&user_id=" + user_id + "&v=1.01";
            string Data = Proces(Paramet, Secret_Key) + "&param=" + strXML.ToString();

            byte[] bytes = Encoding.UTF8.GetBytes(Data);
            WebClient webClient = new WebClient();
            webClient.Headers.Add("Content-Type", "application/x-www-form-urlencoded;charset=utf-8");//采取POST方式必须加的header，如果改为GET方式的话就去掉这句话即可   
            byte[] responseData = webClient.UploadData(STUrl, "POST", bytes);//得到返回字符流   
            string srcString = Encoding.UTF8.GetString(responseData);//解码  

            ExpressResult expressResult = new ExpressResult();
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(srcString);//加载xml字符串      

            try
            {
                XmlNodeList RouteResponseList = doc.SelectNodes("ufinterface/Result/WaybillProcessInfo");
                XmlNode Waybill_No = doc.SelectSingleNode("ufinterface/Result/WaybillProcessInfo/Waybill_No");

                expressResult.MailNo = Waybill_No.InnerXml;

                List<RouteResponseResult> RouteResponseResultList = new List<RouteResponseResult>();

                //需要数组显示节点信息
                foreach (XmlNode RouteResponseNode in RouteResponseList)
                {
                    XmlNode Upload_Time = RouteResponseNode.SelectSingleNode("Upload_Time");
                    XmlNode ProcessInfo = RouteResponseNode.SelectSingleNode("ProcessInfo");

                    RouteResponseResult routeResponseResult = new RouteResponseResult();
                    routeResponseResult.accept_time = Upload_Time.InnerXml;
                    routeResponseResult.accept_address = ProcessInfo.InnerXml;
                    RouteResponseResultList.Add(routeResponseResult);
                }

                expressResult.Status = "OK";
                expressResult.RouteResponseResult = RouteResponseResultList;
                expressResult.Message = "查询成功！";
            }
            catch (Exception e)
            {
                expressResult.Status = "ERR";
                XmlNode Result = doc.SelectSingleNode("Response/success");
                XmlNode reason = doc.SelectSingleNode("Response/reason");
                if (Result.InnerXml == "false")
                {
                    expressResult.Message = reason.InnerXml;
                }
                else
                {
                    expressResult.Message = "未获取到路由信息！";
                }
                expressResult.Status = Result.InnerXml;
            }


            return expressResult;
        }

        //圆通API连接
        private static string YTApiConnect(string xml, string custid, string Checkword)
        {
            string STUrl = "http://openapi.yto.net.cn/service/e_ord_apply/v1/IBdzTp";

            string clientId = custid;
            MD5 md5Hasher = MD5.Create();
            string postData = "logistics_interface=" + HttpUtility.UrlEncode(xml, Encoding.UTF8)
                                + "&data_digest=" + HttpUtility.UrlEncode(Convert.ToBase64String(md5Hasher.ComputeHash(Encoding.UTF8.GetBytes(xml + Checkword))), Encoding.UTF8)
                                + "&clientId=" + HttpUtility.UrlEncode(clientId, Encoding.UTF8);

            byte[] bytes = Encoding.UTF8.GetBytes(postData);
            WebClient webClient = new WebClient();
            webClient.Headers.Add("Content-Type", "application/x-www-form-urlencoded;charset=utf-8");//采取POST方式必须加的header，如果改为GET方式的话就去掉这句话即可   
            byte[] responseData = webClient.UploadData(STUrl, "POST", bytes);//得到返回字符流   
            string srcString = Encoding.UTF8.GetString(responseData);//解码  
            return srcString;
        }

        //MD5加密
        public static string MD5ToBase64String(string str)
        {
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] MD5 = md5.ComputeHash(System.Text.Encoding.UTF8.GetBytes(str));//MD5(注意UTF8编码)
            string result = Convert.ToBase64String(MD5, 0, MD5.Length);//Base64                                                        
            return result;
        }

        private static string Proces(string DataParam, string Secret_Key)
        {
            //Secret_Key=私钥
            string[] ArrayParameters = DataParam.Split('&');
            Array.Sort(ArrayParameters);

            StringBuilder sbParamet = new StringBuilder();
            sbParamet.Append(Secret_Key);
            for (int i = 0; i < ArrayParameters.Length; i++)
            {
                if (ArrayParameters[i].Split('=').Length != 2)
                {
                    throw new Exception("参数格式不正确");
                }
                string ParName = (ArrayParameters[i].Split('='))[0].Trim();
                string ParValue = (ArrayParameters[i].Split('='))[1].Trim();
                sbParamet.Append(ParName + ParValue);
            }
            string a = sbParamet.ToString(); //待加密的拼接字符串
            string Sign = UserMd5(a);
            return "sign=" + Sign + "&" + DataParam; //生成的sign

        }

        static string UserMd5(string str)
        {
            string cl = str;
            string pwd = "";
            System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create();
            byte[] s = md5.ComputeHash(Encoding.UTF8.GetBytes(cl));
            for (int i = 0; i < s.Length; i++)
            {
                pwd = pwd + s[i].ToString("X");
            }
            return pwd;
        }

        #endregion



        #region 中通快递 

        //获取快递单号
        public ExpressResult GetExpress(ZTOExpressModel entity)
        {
            ZTOExpressModelEntity modelEntity = new ZTOExpressModelEntity();
            modelEntity.partnerOrderCode = entity.partnerOrderCode;
            modelEntity.partnerType = "2";
            modelEntity.orderType = entity.orderType;

            accountInfo acc = new accountInfo();
            acc.accountId = entity.companyid;
            acc.accountPassword = entity.companypwd;

            if (entity.orderType == "1")
            {
                acc.type = "1";
            }
            else if (entity.orderType == "4")
            {
                acc.type = "74";
            }

            modelEntity.accountInfo = acc;
            modelEntity.senderInfo = entity.senderInfo;
            modelEntity.receiveInfo = entity.receiveInfo;

            modelEntity.remark = "宜欧物流";

            ZopClient client = new ZopClient(entity.key, entity.appserect);
            ZopPublicRequest request = new ZopPublicRequest();
            string ss = JsonConvert.SerializeObject(modelEntity);

            request.jsonBody = ss;
            request.body = ss;

            //request.url = "https://japi-test.zto.com/zto.open.createOrder";
            request.url = "https://japi.zto.com/zto.open.createOrder";

            string s = client.execute(request);

            ZTOExpressResult expResult = JsonConvert.DeserializeObject<ZTOExpressResult>(s);

            ExpressResult expressResult = new ExpressResult();
            if (expResult.status.ToLower() == "true")
            {
                PackHeadJsonEntityZTO detailModel = new PackHeadJsonEntityZTO();
                detailModel.bagAddr = expResult.result.bigMarkInfo.bagAddr;

                //为实体赋值
                expressResult.Status = "OK";
                expressResult.OrderId = expResult.result.orderCode;
                expressResult.MailNo = expResult.result.billCode;

                expressResult.DestCode = expResult.result.bigMarkInfo.mark;
                expressResult.Message = "快递单获取成功！";

                expressResult.zTOGetDetailModel = detailModel;
            }
            else
            {
                expressResult.Status = "ERR";
                expressResult.Message = expResult.statusCode + expResult.message;
            }

            return expressResult;
        }



        #endregion


        #region 顺丰快递API

        public ExpressResult GetSFExpressAPI(SFExpressAPIModel entity)
        {
            //string url = "https://fapi.sf-express.com/fopApiServices/access/sandbox/enter";
            //string partnerId = "DSXD";
            //string md5key = "dajcoWAOaW";


            string url = "https://fapi.sf-express.com/fopApiServices/access/enter";
            string partnerId = "FOP_OCEANEAST";
            string md5key = "UvwkcNtAR2KwoWRc";

            string json = JsonConvert.SerializeObject(entity);
            string serCode = "FOP_RECE_LTL_CREATE_ORDER";

            long timespamp = CurrentTimeMillis();

            //因业务报文中可能包含加号、空格等特殊字符，需要urlEnCode处理
            string toVerifyText = UrlEncode((json + timespamp + md5key), Encoding.UTF8);

            //进行Md5加密
            MD5 md = new MD5CryptoServiceProvider();
            byte[] digest = md.ComputeHash(UnicodeEncoding.UTF8.GetBytes(toVerifyText));

            //通过BASE64生成数字签名
            string result = Convert.ToBase64String(digest);

            string postData = "serviceCode=" + serCode + "&partnerID=" + partnerId + "&requestID=" + entity.orderId + "&timestamp=" + timespamp + "&msgDigest=" + result + "&msgData=" + json;

            string s = SFClient.post(url, postData);

            ExpressResult expressResult = new ExpressResult();

            JObject obj = JObject.Parse(s);
            string resMag = obj["apiResultCode"].ToString();
            if (resMag == "A1000")
            {
                try
                {
                    string s1 = obj["apiResultData"].ToString().Replace(@"\", "");

                    apiResultData expResult = JsonConvert.DeserializeObject<apiResultData>(s1);
                    if (expResult.success)
                    {
                        //为实体赋值
                        expressResult.Status = "OK";

                        SFExpressAPIResult1 OrderResponse = JsonConvert.DeserializeObject<SFExpressAPIResult1>(s1);

                        expressResult.OrderId = OrderResponse.obj.orderId;
                        expressResult.MailNo = OrderResponse.obj.waybillNo;
                        expressResult.DestCode = OrderResponse.obj.destCode;

                        if (!string.IsNullOrEmpty(expressResult.DestCode))
                        {
                            expressResult.Message = "快递单获取成功！";
                        }
                        else
                        {
                            expressResult.Status = "ERR";
                            expressResult.Message = "快递单地址获取CODE错误！";
                        }

                        PackHeadJsonEntity detailModel = new PackHeadJsonEntity();
                        if (OrderResponse.obj.rlsInfo != null)
                        {
                            detailModel = OrderResponse.obj.rlsInfo.rlsDetail;
                        }
                        else
                        {
                            detailModel = null;
                        }

                        expressResult.sFGetDetailModel = detailModel;
                    }
                    else
                    {
                        expressResult.Status = "ERR";
                        expressResult.Message = expResult.errorCode + expResult.errorMessage;
                    }
                }
                catch (Exception ex)
                {
                    string q = ex.ToString();
                }

            }
            else
            {
                expressResult.Status = "ERR";
                expressResult.Message = obj["apiErrorMsg"].ToString();
            }

            return expressResult;

        }

        public ExpressResult GetZDSFExpressAPI(SFExpressAPIZDModel entity)
        {
            //string url = "https://fapi.sf-express.com/fopApiServices/access/sandbox/enter";
            //string partnerId = "DSXD";
            //string md5key = "dajcoWAOaW";

            string url = "https://fapi.sf-express.com/fopApiServices/access/enter";
            string partnerId = "FOP_OCEANEAST";
            string md5key = "UvwkcNtAR2KwoWRc";

            string json = JsonConvert.SerializeObject(entity);
            string serCode = "FOP_RECE_LTL_APPEND_SUB_WAYBILL";

            long timespamp = CurrentTimeMillis();

            //因业务报文中可能包含加号、空格等特殊字符，需要urlEnCode处理
            string toVerifyText = UrlEncode((json + timespamp + md5key), Encoding.UTF8);

            //进行Md5加密
            MD5 md = new MD5CryptoServiceProvider();
            byte[] digest = md.ComputeHash(UnicodeEncoding.UTF8.GetBytes(toVerifyText));

            //通过BASE64生成数字签名
            string result = Convert.ToBase64String(digest);

            string postData = "serviceCode=" + serCode + "&partnerID=" + partnerId + "&requestID=" + entity.orderId + "&timestamp=" + timespamp + "&msgDigest=" + result + "&msgData=" + json;

            string s = SFClient.post(url, postData);

            ExpressResult expressResult = new ExpressResult();

            JObject obj = JObject.Parse(s);
            string resMag = obj["apiResultCode"].ToString();
            if (resMag == "A1000")
            {
                try
                {
                    string s1 = obj["apiResultData"].ToString().Replace(@"\", "");

                    apiResultData expResult = JsonConvert.DeserializeObject<apiResultData>(s1);
                    if (expResult.success)
                    {
                        //为实体赋值
                        expressResult.Status = "OK";

                        SFExpressAPIResult1 OrderResponse = JsonConvert.DeserializeObject<SFExpressAPIResult1>(s1);

                        expressResult.OrderId = OrderResponse.obj.orderId;
                        expressResult.MailNo = OrderResponse.obj.waybillNo;

                        List<ZDExpressResult> ZDExpressResultList = new List<ZDExpressResult>();
                        ZDExpressResult zdExpressResult = new ZDExpressResult();
                        zdExpressResult.MailNo_ZD = OrderResponse.obj.appendedSubWaybillNos;      //子单号
                        ZDExpressResultList.Add(zdExpressResult);

                        expressResult.ZDExpressResult = ZDExpressResultList;    //子单号列表
                        expressResult.Message = "子母单获取成功！";
                    }
                    else
                    {
                        expressResult.Status = "ERR";
                        expressResult.Message = expResult.errorCode + expResult.errorMessage;
                    }
                }
                catch (Exception ex)
                {
                    string q = ex.ToString();
                }
            }
            else
            {
                expressResult.Status = "ERR";
                expressResult.Message = obj["apiErrorMsg"].ToString();
            }

            return expressResult;

        }


        private static readonly DateTime Jan1st1970 = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static long CurrentTimeMillis()
        {
            return (long)(DateTime.UtcNow - Jan1st1970).TotalMilliseconds;
        }


        public static string UrlEncode(string str, Encoding encoding)
        {
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < str.Length; i++)
            {
                string t = str[i].ToString();
                string k = HttpUtility.UrlEncode(t, encoding);
                if (t == k)
                {
                    sb.Append(k);
                }
                else
                {
                    sb.Append(k.ToUpper());
                }
            }
            return sb.ToString();
        }


        //通过传递顺丰快递单号取得PDF下载URL
        public ExpressResult GetSFExpressDownUrlAPI(SFExpressDownUrlModel entity,string orderId)
        {
            //string url = "https://sfapi-sbox.sf-express.com/std/service";
            //string partnerId = "0216997073";
            //string md5key = "5dggDYWqBCtmcGWRhmhbb7qhAkcVhrwr";


            string url = "https://bspgw.sf-express.com/std/service";
            string partnerId = "YOGJWQZ849YE";
            string md5key = "vn3ttOclpoKPPTqcNw6jb3Arxeg5VXlp";

            string json = JsonConvert.SerializeObject(entity);
            string serCode = "COM_RECE_CLOUD_PRINT_WAYBILLS";

            long timespamp = CurrentTimeMillis();

            //因业务报文中可能包含加号、空格等特殊字符，需要urlEnCode处理，字符集编码统一使用UTF-8
            string toVerifyText = UrlEncode((json + timespamp + md5key), Encoding.UTF8);

            //进行Md5加密
            MD5 md = new MD5CryptoServiceProvider();
            byte[] digest = md.ComputeHash(UnicodeEncoding.UTF8.GetBytes(toVerifyText));

            //通过BASE64生成数字签名
            string result = Convert.ToBase64String(digest);

            string postData = "serviceCode=" + serCode + "&partnerID=" + partnerId + "&requestID=" + orderId + "&timestamp=" + timespamp + "&msgDigest=" + result + "&msgData=" + json;

            string s = SFClient.post(url, postData);

            ExpressResult expressResult = new ExpressResult();

            JObject obj = JObject.Parse(s);
            string resMag = obj["apiResultCode"].ToString();
            if (resMag == "A1000")
            {
                try
                {
                    string s1 = obj["apiResultData"].ToString().Replace(@"\", "");

                    JObject jodata = JObject.Parse(obj["apiResultData"].ToString());
                    string success = jodata["success"].ToString(); //是否成功
                    string requestId = jodata["requestId"].ToString(); //订单id
                    string errorMessage = jodata["errorMessage"].ToString(); //信息

                    if (success == "true")
                    {

                    }
                    else
                    {

                    }
                }
                catch (Exception ex)
                {
                    string q = ex.ToString();
                }

            }
            else
            {
                expressResult.Status = "ERR";
                expressResult.Message = obj["apiErrorMsg"].ToString();
            }

            return expressResult;

        }

        #endregion

    }
}
