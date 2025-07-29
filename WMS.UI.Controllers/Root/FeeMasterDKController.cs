using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace WMS.UI.Controllers
{
    public class FeeMasterDKController : Controller
    {
        WCF.InBoundService.InBoundServiceClient cf = new WCF.InBoundService.InBoundServiceClient();
        WCF.RootService.RootServiceClient cf1 = new WCF.RootService.RootServiceClient();

        Helper Helper = new Helper();
        EIP.EIP EIP = new EIP.EIP();
        [DefaultRequest]
        //页面显示 （通用）
        public ActionResult Index()
        {
            ViewBag.CrSaveAddress = ConfigurationManager.AppSettings["CrAddress98"].ToString();

            ViewData["WhClientList"] = from r in cf.WhClientListSelect(Session["whCode"].ToString())
                                       select new SelectListItem()
                                       {
                                           Text = r.ClientCode,     //text
                                           Value = r.Id.ToString()
                                       };

            ViewData["zoneLocationList"] = from r in cf.RecZoneSelect(Session["whCode"].ToString(), 0)
                                           select new SelectListItem()
                                           {
                                               Text = r.ZoneName,     //text
                                               Value = r.ZoneName.ToString()
                                           };

            return View();
        }


        //通过选择的客户得到收货区域
        [HttpPost]
        public ActionResult GetRecZoneNameList()
        {
            IEnumerable<WCF.InBoundService.WhZoneResult> list = cf.RecZoneSelect(Session["whCode"].ToString(), Convert.ToInt32(Request["txt_WhClient"]));
            if (list != null)
            {
                var sql = from r in list
                          select new
                          {
                              Value = r.ZoneName.ToString(),
                              Text = r.ZoneName    //text
                          };
                return Json(sql);
            }
            else
            {
                return null;
            }
        }

        //得到收货区域
        [HttpPost]
        public ActionResult GetRecZoneNameList1()
        {
            IEnumerable<WCF.InBoundService.WhZoneResult> list = cf.RecZoneSelect(Session["whCode"].ToString(), 0);
            if (list != null)
            {
                var sql = from r in list
                          select new
                          {
                              Value = r.ZoneName.ToString(),
                              Text = r.ZoneName    //text
                          };
                return Json(sql);
            }
            else
            {
                return null;
            }
        }

        //查询
        public ActionResult List()
        {
            WCF.RootService.FeeMasterSearch entity = new WCF.RootService.FeeMasterSearch();
            entity.pageIndex = Convert.ToInt32(Request["eip_page"]);
            entity.pageSize = Convert.ToInt32(Request["eip_page_size"]);
            entity.WhCode = Session["whCode"].ToString();
            entity.FeeNumber = Request["FeeNumber"].Trim();
            entity.Status = Request["status"].Trim();
            entity.ClientCode = Request["clientCode"].Trim();
            entity.Type = Request["type"].Trim();
            entity.InvoiceNumberOrderBy = Request["InvoiceNumberOrderBy"].Trim();

            if (Request["BeginCreateDate"] != "")
            {
                entity.BeginCreateDate = Convert.ToDateTime(Request["BeginCreateDate"]);
            }
            else
            {
                entity.BeginCreateDate = null;
            }

            if (Request["EndCreateDate"] != "")
            {
                entity.EndCreateDate = Convert.ToDateTime(Request["EndCreateDate"]).AddDays(1);
            }
            else
            {
                entity.EndCreateDate = null;
            }

            if (Request["BeginFeeCreateDate"] != "")
            {
                entity.BeginFeeCreateDate = Convert.ToDateTime(Request["BeginFeeCreateDate"]);
            }
            else
            {
                entity.BeginFeeCreateDate = null;
            }

            if (Request["EndFeeCreateDate"] != "")
            {
                entity.EndFeeCreateDate = Convert.ToDateTime(Request["EndFeeCreateDate"]).AddDays(0);
            }
            else
            {
                entity.EndFeeCreateDate = null;
            }

            string soNumber = Request["soNumber"];
            string[] soNumberList = null;
            if (!string.IsNullOrEmpty(soNumber))
            {
                string po_temp = soNumber.Replace("\r\n", "@");    //把so中的空格 替换为@符号
                soNumberList = po_temp.Split('@');           //把SO 按照@分割，放在数组
            }

            int total = 0;
            string str = "";
            List<WCF.RootService.FeeMaster> list = cf1.FeeMaseterList(entity, soNumberList, out total,out str).ToList();

            Dictionary<string, string> fieldsName = new Dictionary<string, string>();
            fieldsName.Add("Id", "操作");
            fieldsName.Add("FeeNumber", "系统编号");
            fieldsName.Add("Type", "类型");
            fieldsName.Add("ClientCode", "客户名");
            fieldsName.Add("ClientCodeCN", "客户中文名");            
            fieldsName.Add("SoNumber", "SO号");
            fieldsName.Add("Status", "状态");
            fieldsName.Add("LocationId", "收货区域");

            fieldsName.Add("LoadId", "Load号");
            fieldsName.Add("LuruFee", "录入总价");

            fieldsName.Add("YuShouFee", "预收款");
            fieldsName.Add("YuShouUser", "预收人");

            fieldsName.Add("Description", "备注");
            fieldsName.Add("ReceiptId", "收货批次号");
            fieldsName.Add("OperationBeginDate", "开始操作时间");
            fieldsName.Add("OperationEndDate", "结束操作时间");

            fieldsName.Add("CreateUser", "创建人");
            fieldsName.Add("CreateDate", "创建时间");

            fieldsName.Add("JieSuanFee", "结算款");
            fieldsName.Add("JieSuanUser", "结算人");
            fieldsName.Add("BillingType", "开票类型");
            fieldsName.Add("InvoiceType", "发票机号");
            fieldsName.Add("NoNumber", "发票代码");
            fieldsName.Add("InvoiceNumber", "发票号码");
            fieldsName.Add("InvoiceTopContent", "开票抬头");
            fieldsName.Add("UpdateDate", "收费时间");

            return Content(EIP.EipListJson(list, total, fieldsName, "Id:90,FeeNumber:140,ClientCode:120,Status:65,CreateDate:130,CreateUser:50,LocationId:70,OperationBeginDate:130,OperationEndDate:130,UpdateDate:130,Type:70,SoNumber:140,LoadId:140,default:75", null, "", 200, str));
        }

        //显示费用明细
        public ActionResult FeeDetailList()
        {
            WCF.RootService.FeeDetailSearch entity = new WCF.RootService.FeeDetailSearch();
            entity.pageIndex = Convert.ToInt32(Request["eip_page"]);
            entity.pageSize = Convert.ToInt32(Request["eip_page_size"]);
            entity.WhCode = Session["whCode"].ToString();
            entity.FeeNumber = Request["feeNumber"];
            entity.SoNumber = Request["searchDetail_soNumber"];
            entity.PoNumber = Request["searchDetail_poNumber"];
            entity.HuId = Request["searchDetail_huId"];
            entity.LocationId = Request["searchDetail_locationId"];

            int total = 0;
            string str = "";
            List<WCF.RootService.FeeDetail> list = cf1.FeeDetailList(entity, out total,out str).ToList();

            Dictionary<string, string> fieldsName = new Dictionary<string, string>();
            fieldsName.Add("Id", "操作");
            fieldsName.Add("FeeNumber", "系统编号");
            fieldsName.Add("SoNumber", "SO号");
            fieldsName.Add("CustomerPoNumber", "PO号");
            fieldsName.Add("AltItemNumber", "款号");
            fieldsName.Add("HuId", "托盘");
            fieldsName.Add("LocationId", "库位");
            fieldsName.Add("TCRProcessMode", "TCR处理方式");

            fieldsName.Add("PeopleNormalFee", "人员监管费");
            fieldsName.Add("PeopleFujiaFee", "人员监管附加费");
            fieldsName.Add("EquipmentUseFee", "设备使用附加费");
            fieldsName.Add("TruckFee", "车辆管理费");
            fieldsName.Add("DaDanFee", "打单费");
            fieldsName.Add("OtherFee", "其他费");

            fieldsName.Add("Price", "单价");
            fieldsName.Add("Qty", "录入数量");
            fieldsName.Add("OperationQty", "实际托盘个数或挂衣数");

            fieldsName.Add("ChangDiFee", "场地费");
            fieldsName.Add("ChangDiHours", "场地预估小时");
            fieldsName.Add("OperationHours", "实际操作小时");

            fieldsName.Add("PeopleFee", "监管费单价");
            fieldsName.Add("PeopleNormalHours", "正常班小时");
            fieldsName.Add("PeopleNormalNightHours", "夜班小时");
            fieldsName.Add("PeopleWeekendHours", "周末班小时");
            fieldsName.Add("PeopleStatutoryHolidayHours", "节假日班小时");

            fieldsName.Add("OperationUser", "操作人");
            fieldsName.Add("Description", "备注");
            fieldsName.Add("CreateUser", "创建人");
            fieldsName.Add("CreateDate", "创建时间");
            fieldsName.Add("UpdateUser", "修改人");
            fieldsName.Add("UpdateDate", "修改时间");

            return Content(EIP.EipListJson(list, total, fieldsName, "Id:60,SoNumber:120,TCRProcessMode:100,Price:60,Qty:60,TotalPrice:60,Description:120,CreateDate:120,UpdateDate:120,OperationQty:120,default:80", null, "", 200, str));
        }

        //显示费用结算过程明细
        public ActionResult JSFeeDetailList()
        {

            int total = 0;
            string str = "";
            List<WCF.RootService.FeeDetailResult1> list = cf1.getOperationFeeList(Request["feeNumber"], Session["whCode"].ToString(),out total).ToList();

            Dictionary<string, string> fieldsName = new Dictionary<string, string>();
            fieldsName.Add("HSTotalPrice", "含税总价");
            fieldsName.Add("TotalPrice", "总价");
            fieldsName.Add("TCRProcessMode", "TCR处理方式");

            fieldsName.Add("OperationQtyFee", "操作数量费用");
            fieldsName.Add("PeopleNormalFee", "人员监管费");
            fieldsName.Add("PeopleFujiaFee", "人员监管附加费");
            fieldsName.Add("EquipmentUseFee", "设备使用附加费");
            fieldsName.Add("TruckFee", "车辆管理费");
            fieldsName.Add("DaDanFee", "打单费");
            fieldsName.Add("OtherFee", "其他费");

            fieldsName.Add("Price", "单价");
            fieldsName.Add("Qty", "录入数量");
            fieldsName.Add("OperationQty", "操作数量");
         
            fieldsName.Add("ChangDiFee", "场地费");
            fieldsName.Add("OperationHours", "实际操作小时");

            fieldsName.Add("PeopleNormalHours", "正常班小时");
            fieldsName.Add("PeopleNormalNightHours", "夜班小时");
            fieldsName.Add("PeopleWeekendHours", "周末班小时");
            fieldsName.Add("PeopleStatutoryHolidayHours", "节假日班小时");  

            return Content(EIP.EipListJson(list, total, fieldsName, "TCRProcessMode:100,Price:60,Qty:60,OperationQty:60,default:80", null, "", 200, null));
        }

        //道口预收款
        public ActionResult EditFeeMaster()
        {
            WCF.RootService.FeeMaster entity = new WCF.RootService.FeeMaster();
            entity.Id = Convert.ToInt32(Request["id"].Trim());
            entity.LuruFee = Convert.ToDecimal(Request["Hid_luruFee"].Trim());
            try
            {
                entity.YuShouFee = Convert.ToDecimal(Request["edit_yushouFee"].Trim());
            }
            catch (Exception)
            {
                return Helper.RedirectAjax("err", "预收款需为数值类型！", null, "");
            }

            entity.YuShouUser = Session["userNameCN"].ToString();

            string result = cf1.FeeMaseterDKEdit(entity);
            if (result == "Y")
            {
                return Helper.RedirectAjax("ok", "修改成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", result, null, "");
            }
        }

        //确认TCR结算
        public ActionResult EditFeeMasterDK() 
        {
            WCF.RootService.FeeMaster entity = new WCF.RootService.FeeMaster();
            entity.FeeNumber = Request["feeNumber"].Trim();
            entity.WhCode = Session["whCode"].ToString();

            try
            {
                entity.JieSuanFee = Convert.ToDecimal(Request["OperationFee"].Trim());
            }
            catch (Exception)
            {
                return Helper.RedirectAjax("err", "结算款需为数值类型！", null, "");
            }

            entity.JieSuanUser = Session["userNameCN"].ToString();

            entity.BillingType = Request["BillingType"].Trim();
            entity.InvoiceType = Request["InvoiceType"].Trim();
            entity.NoNumber = Request["NoNumber"].Trim();
            entity.InvoiceNumber = Request["InvoiceNumber"].Trim();
            entity.InvoiceTopContent = Request["InvoiceTopContent"].Trim();

            string result = cf1.FeeMaseterDKJiesuanEdit(entity,0);
            if (result == "Y")
            {
                return Helper.RedirectAjax("ok", "确认成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", result, null, "");
            }
        }

        //确认出货提箱费用结算
        public ActionResult confirmOutLoadFee()
        {
            WCF.RootService.FeeMaster entity = new WCF.RootService.FeeMaster();
            entity.FeeNumber = Request["feeNumber"].Trim();
            entity.WhCode = Session["whCode"].ToString();

            try
            {
                entity.JieSuanFee = Convert.ToDecimal(Request["OperationFee_load"].Trim());
            }
            catch (Exception)
            {
                return Helper.RedirectAjax("err", "结算款需为数值类型！", null, "");
            }

            entity.JieSuanUser = Session["userNameCN"].ToString();

            entity.BillingType = Request["BillingType_load"].Trim();
            entity.InvoiceType = Request["InvoiceType_load"].Trim();
            entity.NoNumber = Request["NoNumber_load"].Trim();
            entity.InvoiceNumber = Request["InvoiceNumber_load"].Trim();
            entity.InvoiceTopContent = Request["InvoiceTopContent_load"].Trim();

            string result = cf1.FeeMaseterDKJiesuanEdit(entity,1);
            if (result == "Y")
            {
                return Helper.RedirectAjax("ok", "确认成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", result, null, "");
            }
        }
        


        //修改发票信息
        public ActionResult EditFeeMasterInvoice() 
        {
            WCF.RootService.FeeMaster entity = new WCF.RootService.FeeMaster();
            entity.Id = Convert.ToInt32(Request["id"].Trim());

            entity.BillingType = Request["edit_BillingType"].Trim();
            entity.InvoiceType = Request["edit_InvoiceType"].Trim();
            entity.NoNumber = Request["edit_NoNumber"].Trim();
            entity.InvoiceNumber = Request["edit_InvoiceNumber"].Trim();
            entity.InvoiceTopContent = Request["edit_InvoiceTopContent"].Trim();

            string result = cf1.FeeMaseterInvoiceEdit(entity);
            if (result == "Y")
            {
                return Helper.RedirectAjax("ok", "修改成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", result, null, "");
            }
        }

        [HttpPost]
        public void getOperationFee()
        {
            WCF.RootService.FeeMaster entity = new WCF.RootService.FeeMaster();
            entity.FeeNumber = Request["feeNumber"].Trim();
            entity.WhCode = Session["whCode"].ToString();

            cf1.AgainTCRFeeCalculate(entity);

            string result = cf1.getOperationFee(Request["feeNumber"].Trim(), Session["whCode"].ToString());
            Response.Write(result);
        }



    }
}
