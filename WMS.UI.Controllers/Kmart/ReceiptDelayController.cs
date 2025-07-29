using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace WMS.UI.Controllers
{
    public class ReceiptDelayController : Controller
    {
        WCF.InBoundService.InBoundServiceClient cf = new WCF.InBoundService.InBoundServiceClient();
        Helper Helper = new Helper();
        EIP.EIP EIP = new EIP.EIP();
        [DefaultRequest]
        //页面显示 （通用）
        public ActionResult Index()
        {
            ViewData["ReasonList"] = from r in cf.KmartReasontSelect()
                                       select new SelectListItem()
                                       {
                                           Text = r.Comments,    //text
                                           Value = r.ReasonCode
                                       };

            return View();
        }

        //查询收货异常报表
        [HttpGet]
        public ActionResult List()
        {
            WCF.InBoundService.ExcelImportInBoundSearch entity = new WCF.InBoundService.ExcelImportInBoundSearch();
            entity.pageIndex = Convert.ToInt32(Request["eip_page"]);
            entity.pageSize = Convert.ToInt32(Request["eip_page_size"]);
            entity.WhCode = Session["whCode"].ToString();
            entity.SoNumber = Request["SoNumber"].ToString();
            entity.WhCode = Session["whCode"].ToString();
            entity.ReceiptId=Request["ReceiptId"].ToString();

            if (!string.IsNullOrEmpty(Request["BeginDate"]))
            {
                entity.BeginDate = Convert.ToDateTime(Request["BeginDate"]);
            }
            if (!string.IsNullOrEmpty(Request["EndDate"]))
            {
                entity.EndDate = Convert.ToDateTime(Request["EndDate"]);
            }

            if (!string.IsNullOrEmpty(Request["BeginConsDate"]))
            {
                entity.BeginConsDate = Convert.ToDateTime(Request["BeginConsDate"]);
            }
            if (!string.IsNullOrEmpty(Request["EndConsDate"]))
            {
                entity.EndConsDate = Convert.ToDateTime(Request["EndConsDate"]);
            }

            entity.TruckWaitingTime = Request["TruckWaitingTime"].ToString();

            int total = 0;
            string str = "";
            List<WCF.InBoundService.KmartReceiptDelay> list = cf.KmartReceiptDelayList(entity, out total, out str).ToList();

            Dictionary<string, string> fieldsName = new Dictionary<string, string>();
            fieldsName.Add("Id", "操作");
            fieldsName.Add("ReceiptId", "收货批次");
            fieldsName.Add("PlaceOfDeparture", "POL 起运港");
            fieldsName.Add("ConsDate", "Cons Date 送货截止");
            fieldsName.Add("Supplier", "Supplier 供应商");
            fieldsName.Add("SoNumber", "Booking number 进仓号码");
            fieldsName.Add("TruckNumber", "Truck Number 车牌号");
            fieldsName.Add("TransportType", "Truck type  车型");
            fieldsName.Add("Appointment", "Appointment  是否预约");
            fieldsName.Add("BkDate", "预约日期");
            fieldsName.Add("RegistrationDate", "换单日期");
            fieldsName.Add("BeginDate", "Unloading Start Date & Time 开始收货");
            fieldsName.Add("EndDate", "Unloading Finish Date & Time  结束收货");
            fieldsName.Add("TruckwaitingTime", "Truck waiting time");
            fieldsName.Add("WaitingDescription", "ISC Dashboard Category(Waiting)");
            fieldsName.Add("UnloadingEfficiency", "Unloading efficiency");
            fieldsName.Add("UnloadingDescription", "ISC Dashboard Category(Unloading)");
            fieldsName.Add("CombinedWaitingTime", "Combined Waiting Time");
            fieldsName.Add("CombinedDescription", "ISC Dashboard Category");
            fieldsName.Add("ReasonCode1", "ReasonCode1");
            fieldsName.Add("ReasonCode2", "ReasonCode2");
            fieldsName.Add("ReasonCode3", "ReasonCode3");
            fieldsName.Add("Reason1", "Reason1");
            fieldsName.Add("Reason2", "Reason2");
            fieldsName.Add("Reason3", "Reason3");


            return Content(EIP.EipListJson(list, total, fieldsName, "Id:60,default:140", null, "", 50, str));


        }

        //修改Forecast信息
        [HttpGet]
        public ActionResult SaveReason()
        {
            WCF.InBoundService.ReceiptRegisterExtend entity = new WCF.InBoundService.ReceiptRegisterExtend();
            entity.Id = Convert.ToInt32(Request["Id"]);
            entity.ReceiptId = Request["ReceiptId"] ;
            entity.WhCode = Session["whCode"].ToString();
            entity.HoldReason1 = Request["edit_ReasonCode1"];
            entity.HoldReason2 = Request["edit_ReasonCode2"];
            entity.HoldReason3 = Request["edit_ReasonCode3"];
            entity.CreateDate = DateTime.Now;
            entity.CreateUser = Session["userName"].ToString();
            entity.UpdateUser = Session["userName"].ToString();
            entity.UpdateDate = DateTime.Now;
            string result = cf.KmartDelayReason(entity);
            if (result == "Y")
            {
                return Helper.RedirectAjax("ok", "信息修改成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", result, null, "");
            }


 
        }



      

    }
}
