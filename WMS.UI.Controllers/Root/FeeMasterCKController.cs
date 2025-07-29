using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace WMS.UI.Controllers
{
    public class FeeMasterCKController : Controller
    {
        WCF.InBoundService.InBoundServiceClient cf = new WCF.InBoundService.InBoundServiceClient();
        WCF.RootService.RootServiceClient cf1 = new WCF.RootService.RootServiceClient();

        Helper Helper = new Helper();
        EIP.EIP EIP = new EIP.EIP();
        [DefaultRequest]
        //页面显示 （通用）
        public ActionResult Index()
        {
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

            string soNumber = Request["soNumber"];
            string[] soNumberList = null;
            if (!string.IsNullOrEmpty(soNumber))
            {
                string po_temp = soNumber.Replace("\r\n", "@");    //把so中的空格 替换为@符号
                soNumberList = po_temp.Split('@');           //把SO 按照@分割，放在数组
            }

            int total = 0;
            string str = "";
            List<WCF.RootService.FeeMaster> list = cf1.FeeMaseterList(entity, soNumberList, out total, out str).ToList();

            Dictionary<string, string> fieldsName = new Dictionary<string, string>();
            fieldsName.Add("Id", "操作");
            fieldsName.Add("FeeNumber", "系统编号");
            fieldsName.Add("Type", "类型");
            fieldsName.Add("ClientCode", "客户名");
            fieldsName.Add("SoNumber", "SO号");
            fieldsName.Add("Status", "状态");
            fieldsName.Add("LocationId", "收货区域");

            fieldsName.Add("Description", "备注");

            fieldsName.Add("OperationBeginDate", "开始操作时间");
            fieldsName.Add("OperationEndDate", "结束操作时间");

            fieldsName.Add("CreateUser", "创建人");
            fieldsName.Add("CreateDate", "创建时间");

            //fieldsName.Add("YuShouFee", "预收款");
            //fieldsName.Add("YuShouUser", "预收人");
            //fieldsName.Add("JieSuanFee", "结算款");
            //fieldsName.Add("JieSuanUser", "结算人");
            //fieldsName.Add("BillingType", "开票类型");

            //fieldsName.Add("InvoiceType", "发票机号");
            //fieldsName.Add("NoNumber", "发票代码");
            //fieldsName.Add("InvoiceNumber", "发票号码");
            //fieldsName.Add("InvoiceTopContent", "开票抬头");

            return Content(EIP.EipListJson(list, total, fieldsName, "Id:110,FeeNumber:120,ClientCode:110,Status:70,CreateDate:110,CreateUser:50,LocationId:70,OperationBeginDate:130,OperationEndDate:130,default:80"));
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
            List<WCF.RootService.FeeDetail> list = cf1.FeeDetailList(entity, out total, out str).ToList();

            Dictionary<string, string> fieldsName = new Dictionary<string, string>();
            fieldsName.Add("Id", "操作");
            fieldsName.Add("FeeNumber", "系统编号");
            fieldsName.Add("SoNumber", "SO号");
            fieldsName.Add("CustomerPoNumber", "PO号");
            fieldsName.Add("AltItemNumber", "款号");
            fieldsName.Add("HuId", "托盘");
            fieldsName.Add("LocationId", "库位");
            fieldsName.Add("TCRProcessMode", "TCR处理方式");
            fieldsName.Add("Qty", "数量");
            fieldsName.Add("OperationQty", "实际托盘个数或挂衣数");
            fieldsName.Add("OperationUser", "操作人");
            fieldsName.Add("OperationHours", "操作小时");
            fieldsName.Add("OtherFee", "其他费");

            fieldsName.Add("Description", "备注");
            fieldsName.Add("CreateUser", "创建人");
            fieldsName.Add("CreateDate", "创建时间");
            fieldsName.Add("UpdateUser", "修改人");
            fieldsName.Add("UpdateDate", "修改时间");

            return Content(EIP.EipListJson(list, total, fieldsName, "Id:70,FeeNumber:60,SoNumber:100,TCRProcessMode:100,Qty:50,Description:120,CreateDate:120,UpdateDate:120,OperationQty:120,default:80", null, "", 200, str));
        }

        //修改费用明细
        public ActionResult EditFeeDetail()
        {
            WCF.RootService.FeeDetail entity = new WCF.RootService.FeeDetail();
            entity.Id = Convert.ToInt32(Request["id"].Trim());

            try
            {
                entity.OperationQty = Convert.ToInt32(Request["Editdetail_qty"].Trim());
            }
            catch
            {
                return Helper.RedirectAjax("err", "操作数量必须为数值类型！", null, "");
            }

            entity.OperationUser = Session["userName"].ToString();

            string result = cf1.FeeDetailCKEdit(entity);
            if (result == "Y")
            {
                return Helper.RedirectAjax("ok", "修改成功！", result, "");
            }
            else
            {
                return Helper.RedirectAjax("err", result, null, "");
            }
        }

        //确认状态
        public ActionResult confirmFeeDetailCK()
        {
            WCF.RootService.FeeMaster entity = new WCF.RootService.FeeMaster();
            entity.FeeNumber = Request["feeNumber"].Trim();
            entity.WhCode = Session["whCode"].ToString();
            entity.OperationBeginDate = Convert.ToDateTime(Request["BeginCreateDateMin"].Trim() + " " + Request["BeginCreateDateMin1"].Trim() + ":" + Request["BeginCreateDateMin2"].Trim());
            entity.OperationEndDate = Convert.ToDateTime(Request["EndCreateDateMin"].Trim() + " " + Request["EndCreateDateMin1"].Trim() + ":" + Request["EndCreateDateMin2"].Trim());
            entity.Description = Session["userNameCN"].ToString();

            if (entity.OperationEndDate <= entity.OperationBeginDate)
            {
                return Helper.RedirectAjax("err", "结束操作时间不能小于开始时间", null, "");
            }

            string result = cf1.ConfirmFeeMasterCK(entity);
            if (result == "Y")
            {
                return Helper.RedirectAjax("ok", "确认成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", result, null, "");
            }
        }


        //确认状态
        public ActionResult EditFeeMasterBeginEndDate()
        {
            WCF.RootService.FeeMaster entity = new WCF.RootService.FeeMaster();
            entity.FeeNumber = Request["feeNumber"].Trim();
            entity.WhCode = Session["whCode"].ToString();
            entity.OperationBeginDate = Convert.ToDateTime(Request["edit_BeginCreateDateMin"].Trim() + " " + Request["edit_BeginCreateDateMin1"].Trim() + ":" + Request["edit_BeginCreateDateMin2"].Trim());
            entity.OperationEndDate = Convert.ToDateTime(Request["edit_EndCreateDateMin"].Trim() + " " + Request["edit_EndCreateDateMin1"].Trim() + ":" + Request["edit_EndCreateDateMin2"].Trim());

            if (entity.OperationEndDate <= entity.OperationBeginDate)
            {
                return Helper.RedirectAjax("err", "结束操作时间不能小于开始时间", null, "");
            }

            string result = cf1.FeeMasterCKEditBeginEndDate(entity);
            if (result == "Y")
            {
                return Helper.RedirectAjax("ok", "修改操作时间成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", result, null, "");
            }
        }

        [HttpGet]
        public ActionResult AddWarhouseLoss()
        {
            WCF.RootService.FeeDetail entity = new WCF.RootService.FeeDetail();
            entity.WhCode = Session["whCode"].ToString();
            entity.FeeNumber = Request["feeNumber"].Trim();

            try
            {
                entity.OtherFee = Convert.ToDecimal(Request["warhouseLossFee"].Trim());
            }
            catch
            {
                return Helper.RedirectAjax("err", "耗材总费用必须为数值类型", null, "");
            }

            entity.UpdateUser = Session["userName"].ToString();

            string result = cf1.FeeDetailAddCKLoss(entity);
            if (result == "Y")
            {
                return Helper.RedirectAjax("ok", "添加成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", result, null, "");
            }
        }

        [HttpGet]
        public ActionResult DelFeeDetailLoss()
        {
            string result = cf1.FeeDetailDelCKLoss(Convert.ToInt32(Request["id"]), Session["userName"].ToString());
            if (result == "Y")
            {
                return Helper.RedirectAjax("ok", "删除成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", result, null, "");
            }
        }

    }
}
