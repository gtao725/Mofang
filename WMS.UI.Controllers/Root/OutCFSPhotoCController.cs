using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Configuration;

namespace WMS.UI.Controllers
{
    public class OutCFSPhotoCController : Controller
    {
        WCF.InBoundService.InBoundServiceClient inboundcf = new WCF.InBoundService.InBoundServiceClient();
        WCF.RootService.RootServiceClient cf = new WCF.RootService.RootServiceClient();

        Helper Helper = new Helper();
        EIP.EIP EIP = new EIP.EIP();

        [DefaultRequest]
        public ActionResult Index()
        {
            ViewBag.PhotoServerAddress = ConfigurationManager.AppSettings["PhotoServerAddress"].ToString();
            ViewBag.PhotoSaveAddress = ConfigurationManager.AppSettings["PhotoSaveAddress"].ToString();
            ViewBag.WMSUrl = ConfigurationManager.AppSettings["WMSUrl"].ToString();
            ViewBag.userName = Session["userName"].ToString();
            ViewBag.whCode = Session["whCode"].ToString();
            ViewData["WhClientList"] = from r in inboundcf.WhClientListSelect(Session["whCode"].ToString())
                                       select new SelectListItem()
                                       {
                                           Text = r.ClientCode,     //text
                                           Value = r.ClientCode.ToString()
                                       };
            return View();
        }

        [DefaultRequest]
        public ActionResult Index1()
        {
            ViewBag.PhotoServerAddress = ConfigurationManager.AppSettings["PhotoServerAddress"].ToString();
            ViewBag.PhotoSaveAddress = ConfigurationManager.AppSettings["PhotoSaveAddress"].ToString();
            ViewBag.WMSUrl = ConfigurationManager.AppSettings["WMSUrl"].ToString();
            ViewBag.userName = Session["userName"].ToString();
            ViewBag.whCode = Session["whCode"].ToString();
            ViewData["WhClientList"] = from r in inboundcf.WhClientListSelect(Session["whCode"].ToString())
                                       select new SelectListItem()
                                       {
                                           Text = r.ClientCode,     //text
                                           Value = r.ClientCode.ToString()
                                       };
            return View();
        }

        //出货照片列表
        [HttpGet]
        public ActionResult List()
        {
            WCF.RootService.PhotoMasterSearch entity = new WCF.RootService.PhotoMasterSearch();
            entity.pageIndex = Convert.ToInt32(Request["eip_page"]);
            entity.pageSize = Convert.ToInt32(Request["eip_page_size"]);
            entity.WhCode = Session["whCode"].ToString();
            entity.ClientCode = Request["ClientCode"];
            entity.Number = Request["number"];
            entity.ContainerNumber = Request["number2"];
            entity.UserCode = Request["userCode"];
            entity.CheckStatus2 = Request["checkStatus2"];
            entity.KRemark1 = Request["KRemark1"];

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

            int total = 0;
            List<WCF.RootService.PhotoMasterResult> list = cf.OutCFSPhotoMasterList(entity, out total).ToList();

            Dictionary<string, string> fieldsName = new Dictionary<string, string>();
            fieldsName.Add("Action", "操作");
            fieldsName.Add("Number", "Load号");
            fieldsName.Add("ClientCode", "客户名");
            fieldsName.Add("BeginPackDate", "装箱时间");
            fieldsName.Add("ShipDate", "封箱时间");
            fieldsName.Add("PhotoId", "照片ID");
            fieldsName.Add("ContainerNumber", "集装箱号");
            fieldsName.Add("ContainerType", "箱型");
            fieldsName.Add("Location", "库区");
            fieldsName.Add("UserCode", "工号");
            fieldsName.Add("UserNameCN", "理货员");
            fieldsName.Add("Status", "是否上传");
            fieldsName.Add("UploadDate", "上传时间");
            fieldsName.Add("CheckStatus1", "客服审核状态");
            fieldsName.Add("CheckUser1", "客服审核人");
            fieldsName.Add("CheckDate1", "客服审核时间");
            fieldsName.Add("KRemark1", "客服备注");
            fieldsName.Add("CheckStatus2", "仓库审核状态");
            fieldsName.Add("CheckUser2", "仓库审核人");
            fieldsName.Add("CheckDate2", "仓库审核时间");
            fieldsName.Add("CRemark1", "仓库备注");

            return Content(EIP.EipListJson(list, total, fieldsName, "Action:80,Number:110,Number2:100,BeginPackDate:120,ShipDate:120,UploadDate:120,Status:70,ContainerType:50,UserCode:50,UserNameCN:50,CheckDate1:110,CheckDate2:110,Location:50,default:90"));
        }


        //照片上传完成
        [HttpGet]
        public void OutCFSPhotoCComplete()
        {
            WCF.RootService.PhotoMaster entity = new WCF.RootService.PhotoMaster();
            if (Request["fileId"] != null)
            {
                entity.PhotoId = Convert.ToInt32(Request["fileId"]);
            }
            else
            {
                entity.PhotoId = 0;
            }
            entity.WhCode = Request["whCode"];
            entity.Number = Request["number"];
            entity.Number2 = Request["number2"];
            entity.CreateUser = Request["user"];
            string result = cf.OutCFSPhotoCComplete(entity);

        }


        //修改TCR
        [HttpGet]
        public ActionResult PhotoMasterEdit()
        {
            WCF.RootService.PhotoMaster entity = new WCF.RootService.PhotoMaster();

            entity.Id = Convert.ToInt32(Request["id"]);
            entity.CRemark1 = Request["edit_Kremark1"];

            string result = cf.PhotoMasterEdit1(entity);
            if (result == "Y")
            {
                return Helper.RedirectAjax("ok", "处理成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", result, null, "");
            }
        }
        //清除照片
        [HttpGet]
        public void PhotoMasterDelete()
        {
            int id = Convert.ToInt32(Request["id"]);
            string LoadId = Request["LoadId"];
            string PhotoId = Request["PhotoId"];
            string result = cf.PhotoMasterDelById(id, Session["userName"].ToString());
        }

        //照片审核
        [HttpGet]
        public void OutCFSPhotoCShenheComplete()
        {
            WCF.RootService.PhotoMaster entity = new WCF.RootService.PhotoMaster();
            entity.PhotoId = Convert.ToInt32(Request["photoid"]);
            entity.WhCode = Request["whCode"];
            entity.Number = Request["number"];
            entity.Number2 = Request["number2"];
            entity.CheckUser2 = Request["user"];
            entity.Id = Convert.ToInt32(Request["Id"] == null ? "0" : Request["Id"]);
            string result = cf.CFSPhotoCShenheComplete(entity);

        }

    }
}
